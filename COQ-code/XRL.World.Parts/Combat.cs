using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts
{
	[Serializable]
	public class Combat : IPart
	{
		[NonSerialized]
		public MissileWeapon LastFired;

		[NonSerialized]
		public static int TrackShieldBlock = 0;

		[NonSerialized]
		private static List<MissileWeapon> MissileWeaponPartList = new List<MissileWeapon>();

		[NonSerialized]
		private static bool MissileWeaponPartListInUse;

		public override int Priority => 90000;

		public override void Attach()
		{
			ParentObject.Flags.SetBit(2, value: true);
		}

		public override void Remove()
		{
			ParentObject.Flags.SetBit(2, value: false);
		}

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != GetAdjacentNavigationWeightEvent.ID && ID != GetDefenderHitDiceEvent.ID && ID != GetNavigationWeightEvent.ID)
			{
				return ID == PooledEvent<CommandEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(BeginTakeActionEvent E)
		{
			ParentObject.ClearShieldBlocks();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetDefenderHitDiceEvent E)
		{
			GameObject shield = ParentObject.GetShield(CanBlockWithShield, E.Attacker);
			if (shield == null)
			{
				return true;
			}
			if (!ParentObject.CanMoveExtremities())
			{
				return true;
			}
			Shield part = shield.GetPart<Shield>();
			if (part == null)
			{
				return true;
			}
			BlockedWithShield(shield, part);
			int Chance;
			if (ParentObject.HasEffect<ShieldWall>() && part.WornOn == "Hand")
			{
				Chance = 100;
			}
			else
			{
				Chance = 25 * (1 + ParentObject.GetIntProperty("ImprovedBlock"));
				if (ParentObject.HasSkill("Shield_Block"))
				{
					Chance += 25;
				}
				if (ParentObject.HasSkill("Shield_DeftBlocking"))
				{
					Chance += 25;
				}
			}
			if (BeforeShieldBlockEvent.Check(E.Attacker, E.Defender, E.Weapon, shield, part, ref Chance) && Chance.in100())
			{
				E.ShieldBlocked = true;
				if (TrackShieldBlock > 0)
				{
					TrackShieldBlock++;
				}
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You block with your shield! (+" + part.AV + " AV)", 'g');
				}
				ParentObject.ParticleText("*block (" + part.AV.Signed() + " AV)", IComponent<GameObject>.ConsequentialColorChar(ParentObject));
				ParentObject.PlayWorldSound(shield.GetPropertyOrTag("BlockSound").Coalesce("Sounds/Melee/multiUseBlock/sfx_melee_naturalWeapon_fist_blocked"), 0.5f, 0f, Combat: false, shield.HasIntProperty("BlockSoundDelay") ? ((float)shield.GetIntProperty("BlockSoundDelay") / 1000f) : 0f);
				E.AV += part.AV;
				if (ParentObject.HasSkill("Shield_StaggeringBlock") && GameObject.Validate(E.Attacker))
				{
					int chance = 20;
					if (ParentObject.HasStat("Strength"))
					{
						chance = ParentObject.Stat("Strength") * 2 - 35;
					}
					if (chance.in100())
					{
						if (ParentObject.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("You stagger " + E.Attacker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " with your shield block!", 'g');
						}
						else if (E.Attacker.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("You are staggered by " + ParentObject.poss("block") + "!", 'r');
						}
						E.Attacker.ApplyEffect(new Stun(Stat.Random(1, 2), 12));
					}
				}
				AfterShieldBlockEvent.Send(E.Attacker, E.Defender, E.Weapon, shield, part);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetNavigationWeightEvent E)
		{
			if (E.Actor != ParentObject)
			{
				if (E.IgnoreCreatures)
				{
					E.MinWeight(2);
				}
				else if (!ParentObject.CanBePositionSwapped() && (!E.Flying || ParentObject.IsFlying))
				{
					E.MinWeight(100);
				}
				else if (E.Actor != null)
				{
					E.Uncacheable = true;
					if (ParentObject.IsHostileTowards(E.Actor))
					{
						if (ParentObject != E.Actor.Target)
						{
							if (E.Flying && !ParentObject.IsFlying)
							{
								E.MinWeight(5);
							}
							else
							{
								E.MinWeight(E.Autoexploring ? 30 : 95);
							}
						}
					}
					else
					{
						GameObject target = ParentObject.Target;
						if (target != null)
						{
							if (ParentObject.isAdjacentTo(target) || E.Actor.Target == target)
							{
								E.MinWeight(95);
							}
							else
							{
								E.MinWeight(3);
							}
						}
						else
						{
							E.MinWeight(ParentObject.Brain?.Goals?.Items?.Count() ?? 2);
						}
					}
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
		{
			if (E.Actor != ParentObject)
			{
				E.MinWeight(2);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CommandEvent E)
		{
			if (E.Command == Flight.SWOOP_ATTACK_COMMAND_NAME)
			{
				SwoopAttack(ParentObject);
			}
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public static bool ThrowWeapon(GameObject Attacker, GameObject Defender = null, Cell TargetCell = null, MissilePath Path = null, IList<GameObject> Weapons = null)
		{
			if (!Attacker.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			if (Attacker.OnWorldMap())
			{
				return Attacker.Fail("You cannot throw things on the world map.");
			}
			if (Weapons == null)
			{
				Weapons = Attacker.GetThrownWeapons();
			}
			if (Weapons.IsNullOrEmpty())
			{
				return Attacker.Fail("You do not have a thrown weapon equipped.");
			}
			int count = Weapons.Count;
			GameObject gameObject = null;
			GameObject gameObject2 = null;
			GameObject gameObject3 = null;
			if (TargetCell == null && Attacker.IsPlayer())
			{
				Physics physics = ((Defender?.CurrentCell != null && !Attacker.IsConfused) ? Defender.Physics : ((Sidebar.CurrentTarget?.CurrentCell == null || Attacker.IsConfused) ? Attacker.Physics : Sidebar.CurrentTarget.Physics));
				FireType FireType = FireType.Normal;
				int num = 9999;
				int num2 = 9999;
				foreach (GameObject Weapon in Weapons)
				{
					GetThrownWeaponRangeEvent.GetFor(out var MaxRange, out var MidRange, Weapon, Attacker);
					if (MaxRange < num)
					{
						num = MaxRange;
					}
					if (MidRange < num2)
					{
						num2 = MidRange;
					}
				}
				switch (Weapons.Count)
				{
				case 3:
					gameObject3 = Weapons[2];
					goto case 2;
				case 2:
					gameObject2 = Weapons[1];
					goto case 1;
				case 1:
					gameObject = Weapons[0];
					Weapons = null;
					break;
				default:
					Weapons = new List<GameObject>(Weapons);
					break;
				}
				Path = MissileWeapon.ShowPicker(physics.CurrentCell.X, physics.CurrentCell.Y, Snap: true, AllowVis.Any, num, BowOrRifle: false, MidRange: num2, Projectile: gameObject ?? Weapons[0], FireType: ref FireType);
				if (Path == null)
				{
					return false;
				}
				TargetCell = physics.CurrentZone.GetCell((int)(Path.X1 / 3f), (int)(Path.Y1 / 3f));
			}
			if (TargetCell == null)
			{
				return false;
			}
			int value = (int)(1000.0 / (double)count);
			bool flag = false;
			if (gameObject != null)
			{
				GameObject weapon = gameObject;
				Cell targetCell = TargetCell;
				MissilePath mPath = Path;
				int? energyCost = value;
				if (Attacker.PerformThrow(weapon, targetCell, Defender, mPath, 0, null, null, energyCost))
				{
					flag = true;
				}
			}
			if (gameObject2 != null)
			{
				GameObject weapon2 = gameObject2;
				Cell targetCell2 = TargetCell;
				MissilePath mPath2 = Path;
				int? energyCost = value;
				if (Attacker.PerformThrow(weapon2, targetCell2, Defender, mPath2, 0, null, null, energyCost))
				{
					flag = true;
				}
			}
			if (gameObject3 != null)
			{
				GameObject weapon3 = gameObject3;
				Cell targetCell3 = TargetCell;
				MissilePath mPath3 = Path;
				int? energyCost = value;
				if (Attacker.PerformThrow(weapon3, targetCell3, Defender, mPath3, 0, null, null, energyCost))
				{
					flag = true;
				}
			}
			if (Weapons != null)
			{
				foreach (GameObject Weapon2 in Weapons)
				{
					Cell targetCell4 = TargetCell;
					MissilePath mPath4 = Path;
					int? energyCost = value;
					if (Attacker.PerformThrow(Weapon2, targetCell4, Defender, mPath4, 0, null, null, energyCost))
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
			return true;
		}

		public static bool FireMissileWeapon(GameObject Attacker, GameObject AimedAt = null, Cell TargetCell = null, FireType FireType = FireType.Normal, string Skill = null, int Rapid = 0, int SweepShots = 0, int SweepWidth = 90, float? EnergyMultiplier = null, bool SkipPastMaxRange = false)
		{
			bool flag = !Attacker.IsPlayer();
			if (!Attacker.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			if (Attacker.OnWorldMap())
			{
				return Attacker.Fail("You cannot fire missile weapons on the world map.");
			}
			if (!Attacker.FireEvent("CanFireMissileWeapon"))
			{
				return false;
			}
			if (!Attacker.TryGetPart<Combat>(out var Part))
			{
				return false;
			}
			List<GameObject> list = ((!Skill.IsNullOrEmpty()) ? Attacker.GetMissileWeapons(null, (MissileWeapon mw) => mw.Skill == Skill) : Attacker.GetMissileWeapons());
			if (list == null || list.Count == 0)
			{
				return Attacker.Fail("You do not have " + (Skill.IsNullOrEmpty() ? "a" : "an appropriate") + " missile weapon equipped!");
			}
			List<MissileWeapon> list2;
			if (MissileWeaponPartListInUse)
			{
				list2 = new List<MissileWeapon>(list.Count);
			}
			else
			{
				MissileWeaponPartList.Clear();
				list2 = MissileWeaponPartList;
				MissileWeaponPartListInUse = true;
			}
			try
			{
				GameObject gameObject = Attacker.Target;
				foreach (GameObject item in list)
				{
					if (item.TryGetPart<MissileWeapon>(out var Part2) && !list2.Contains(Part2) && (!flag || AIWantUseWeaponEvent.Check(item, Attacker, gameObject)))
					{
						list2.Add(Part2);
					}
				}
				if (SkipPastMaxRange && gameObject != null)
				{
					int num = Attacker.DistanceTo(gameObject);
					int i = 0;
					for (int num2 = list2.Count; i < num2; i++)
					{
						if (num > list2[i].MaxRange)
						{
							list2.Remove(list2[i]);
							i = -1;
							num2--;
							if (num2 <= 0)
							{
								return false;
							}
						}
					}
				}
				if (list2.Count > 1 && !CanFireAllMissileWeaponsEvent.Check(Attacker, list))
				{
					if (Part.LastFired != null)
					{
						list2.Remove(Part.LastFired);
					}
					MissileWeapon randomElement = list2.GetRandomElement();
					list2.Clear();
					list2.Add(randomElement);
				}
				if (TargetCell == null && gameObject != null && !Attacker.IsPlayer())
				{
					TargetCell = gameObject.CurrentCell;
				}
				MissilePath missilePath = null;
				if (TargetCell == null)
				{
					if (!Attacker.IsPlayer())
					{
						return false;
					}
					TargetCell = ((gameObject == null || Attacker.IsConfused) ? Attacker.CurrentCell : gameObject.CurrentCell);
					if (TargetCell == null)
					{
						return true;
					}
					bool flag2 = false;
					int num3 = 0;
					foreach (MissileWeapon item2 in list2)
					{
						if (!flag2 && (item2.Skill == "Rifle" || item2.Skill == "Bow"))
						{
							flag2 = true;
						}
						if (item2.MaxRange > num3)
						{
							num3 = item2.MaxRange;
						}
					}
					GameObject Projectile = null;
					GameObject gameObject2 = null;
					string Blueprint = null;
					GetMissileWeaponProjectileEvent.GetFor(list[0], ref Projectile, ref Blueprint);
					if (Projectile == null && !Blueprint.IsNullOrEmpty())
					{
						gameObject2 = GameObject.Create(Blueprint);
						if (gameObject2 != null)
						{
							MissileWeapon.SetupProjectile(gameObject2, Attacker);
							Projectile = gameObject2;
						}
					}
					try
					{
						missilePath = MissileWeapon.ShowPicker(TargetCell.X, TargetCell.Y, Snap: true, AllowVis.Any, num3, flag2, Projectile ?? Attacker, ref FireType);
						if (missilePath == null)
						{
							return false;
						}
						TargetCell = missilePath.Path.Last();
						if (FireType == FireType.Mark)
						{
							Rifle_DrawABead part2 = Attacker.GetPart<Rifle_DrawABead>();
							GameObject combatTarget = TargetCell.GetCombatTarget(Attacker, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, Projectile);
							part2.SetMark(combatTarget);
							Attacker.UseEnergy(1000, "Physical Skill", null, null);
							FireType = FireType.Normal;
							AutoAct.Setting = "ReopenMissileUI";
							return true;
						}
					}
					catch (Exception x)
					{
						MetricsManager.LogException("CommandFireMissileWeapon", x);
					}
					finally
					{
						gameObject2?.Obliterate();
					}
				}
				else if (Attacker.CurrentZone != TargetCell.ParentZone)
				{
					return false;
				}
				int num4 = 0;
				float num5 = 1f;
				if (EnergyMultiplier.HasValue)
				{
					num5 = EnergyMultiplier.Value;
				}
				else if (list2.Count > 1)
				{
					num5 /= (float)list2.Count;
				}
				float num6 = 1f;
				if (list2.Count > 1)
				{
					num6 /= (float)list2.Count;
				}
				if (AimedAt == null)
				{
					AimedAt = TargetCell.GetCombatTarget(Attacker, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, null, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true);
				}
				if (AimedAt != null)
				{
					if (gameObject == null)
					{
						gameObject = AimedAt;
					}
					Event @event = Event.New("TargetedForMissileWeapon");
					@event.SetParameter("Attacker", Attacker);
					@event.SetParameter("Defender", AimedAt);
					@event.SetParameter("MissileWeapons", list2);
					if (!AimedAt.FireEvent(@event))
					{
						return false;
					}
					if (AimedAt.HasEffect((RifleMark fx) => fx.Marker == Attacker))
					{
						num4 += 2;
					}
				}
				if (!BeforeFireMissileWeaponsEvent.Check(Attacker, AimedAt, TargetCell, missilePath, list2))
				{
					return false;
				}
				bool flag3 = Sidebar.CurrentTarget == null;
				if (AimedAt != null && AimedAt != Attacker && Attacker.IsPlayer())
				{
					if (!flag3 && !AimedAt.IsCreature)
					{
						GameObject currentTarget = Sidebar.CurrentTarget;
						if (currentTarget != null && currentTarget.IsCreature)
						{
							goto IL_05d5;
						}
					}
					if (AimedAt.IsVisible() && !list2.Any((MissileWeapon part) => part.ParentObject.HasTagOrProperty("NoMissileSetTarget")))
					{
						Sidebar.CurrentTarget = AimedAt;
						flag3 = false;
					}
				}
				goto IL_05d5;
				IL_05d5:
				Attacker.FireEvent("FiringMissile");
				foreach (MissileWeapon item3 in list2)
				{
					Event event2 = Event.New("CommandFireMissile");
					event2.SetParameter("EnergyMultiplier", num5);
					if (num6 != 1f)
					{
						event2.SetParameter("AnimationDelayMultiplier", num6);
					}
					event2.SetParameter("AimLevel", num4);
					event2.SetParameter("Owner", Attacker);
					event2.SetParameter("TargetCell", TargetCell);
					event2.SetParameter("AimedAt", AimedAt);
					event2.SetParameter("Path", missilePath);
					event2.SetParameter("FireType", FireType);
					event2.SetFlag("TargetUnset", flag3);
					if (SweepShots > 0)
					{
						event2.SetParameter("EnergyMultiplier", 0f);
						if (Rapid > 0)
						{
							event2.SetParameter("AnimationDelayMultiplier", num6 / (float)(SweepShots * Rapid));
						}
						else
						{
							event2.SetParameter("AnimationDelayMultiplier", num6 / (float)SweepShots);
						}
						int num7 = -SweepWidth / 2;
						int num8 = SweepWidth / SweepShots;
						if (50.in100())
						{
							num7 = -num7;
							num8 = -num8;
						}
						event2.SetParameter("SweepShots", SweepShots);
						event2.SetParameter("SweepWidth", SweepWidth);
						int num9 = 0;
						while (num9 < SweepShots)
						{
							if (Attacker != null)
							{
								CommandReloadEvent.Execute(Attacker, FreeAction: true);
							}
							event2.SetParameter("FlatVariance", num7);
							event2.SetParameter("SweepShot", num9 + 1);
							if (Rapid > 0)
							{
								event2.SetParameter("RapidShots", Rapid);
								for (int j = 0; j < Rapid; j++)
								{
									event2.SetParameter("RapidShot", j + 1);
									item3.ParentObject.FireEvent(event2);
								}
							}
							else
							{
								item3.ParentObject.FireEvent(event2);
							}
							num9++;
							num7 += num8;
						}
					}
					else if (Rapid > 0)
					{
						event2.SetParameter("EnergyMultiplier", 0f);
						event2.SetParameter("AnimationDelayMultiplier", num6 / (float)Rapid);
						event2.SetParameter("RapidShots", Rapid);
						for (int k = 0; k < Rapid; k++)
						{
							event2.SetParameter("RapidShot", k + 1);
							item3.ParentObject.FireEvent(event2);
						}
					}
					else
					{
						event2.SetParameter("FlatVariance", 0);
						item3.ParentObject.FireEvent(event2);
					}
					Part.LastFired = item3;
					if (flag)
					{
						AIAfterMissileEvent.Send(item3.ParentObject, Attacker, gameObject);
					}
					Attacker.FireEvent("FiredMissileWeapon");
				}
			}
			finally
			{
				if (list2 == MissileWeaponPartList)
				{
					MissileWeaponPartListInUse = false;
				}
			}
			return true;
		}

		public static int GetShieldBlocksPerTurn(GameObject Actor)
		{
			if (!Actor.HasSkill("Shield_SwiftBlocking"))
			{
				return 1;
			}
			return 2;
		}

		public int GetShieldBlocksPerTurn()
		{
			return GetShieldBlocksPerTurn(ParentObject);
		}

		public bool CanBlockWithShield(GameObject Shield)
		{
			return CanBlockWithShield(ParentObject, Shield);
		}

		public static bool CanBlockWithShield(GameObject Actor, GameObject Shield)
		{
			if (Actor.HasEffect<ShieldWall>())
			{
				return true;
			}
			if (Shield.TryGetPart<Shield>(out var Part))
			{
				return Part.Blocks < GetShieldBlocksPerTurn(Actor);
			}
			return false;
		}

		public void BlockedWithShield(GameObject ShieldObject, Shield Shield = null)
		{
			BlockedWithShield(ParentObject, ShieldObject, Shield);
		}

		public static void BlockedWithShield(GameObject Actor, GameObject ShieldObject, Shield Shield = null)
		{
			if (Shield == null)
			{
				Shield = ShieldObject.GetPart<Shield>();
			}
			if (Shield != null)
			{
				Shield.Blocks++;
			}
		}

		public static bool PerformMeleeAttack(GameObject Attacker, GameObject Defender, int EnergyCost = 1000, int HitModifier = 0, int PenModifier = 0, int PenCapModifier = 0, string Properties = null, bool IgnoreFlight = false)
		{
			if (!IgnoreFlight && !Attacker.FlightMatches(Defender))
			{
				IComponent<GameObject>.EmitMessage(Attacker, Attacker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " cannot reach " + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
				return false;
			}
			if (!Attacker.PhaseMatches(Defender))
			{
				IComponent<GameObject>.EmitMessage(Attacker, Attacker.Poss("attack") + " passes through " + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + "!");
				return false;
			}
			if (!Attacker.CanMoveExtremities("Attack", ShowMessage: true))
			{
				return true;
			}
			if (Attacker.Body == null)
			{
				return false;
			}
			List<BodyPart> list = new List<BodyPart>();
			List<GameObject> list2 = Event.NewGameObjectList();
			int PossibleWeapons;
			BodyPart PrimaryWeaponPart;
			GameObject mainWeapon = Attacker.Body.GetMainWeapon(out PossibleWeapons, out PrimaryWeaponPart, Defender, NeedPrimary: true, FailDownFromPrimary: true, list);
			GetMeleeAttacksEvent E = GetMeleeAttacksEvent.HandleFrom(Attacker);
			GetMeleeAttackChanceEvent E2 = PooledEvent<GetMeleeAttackChanceEvent>.FromPool();
			bool flag = GameObject.Validate(Defender);
			list.ShuffleInPlace();
			int num = list.IndexOf(PrimaryWeaponPart);
			if (num > 0)
			{
				BodyPart value = list[0];
				list[0] = list[num];
				list[num] = value;
			}
			int num2 = 0;
			int num3 = list.Count;
			while (num2 < num3)
			{
				GameObject firstValidWeapon = list[num2].GetFirstValidWeapon(Defender);
				if (firstValidWeapon == null || list2.Contains(firstValidWeapon) || !firstValidWeapon.FireEvent("CanMeleeAttack"))
				{
					num3--;
					list.RemoveAt(num2);
				}
				else
				{
					list2.Add(firstValidWeapon);
					num2++;
				}
			}
			int num4 = 0;
			int count = list.Count;
			int num5 = E.Attacks.Count;
			while (flag && num4 < count)
			{
				BodyPart bodyPart = list[num4];
				GameObject weapon = list2[num4];
				bool flag2 = num4 == 0;
				E2.HandleFor(Attacker, weapon, -1, 0, 1.0, Properties, bodyPart, PrimaryWeaponPart, flag2, Intrinsic: true);
				bool consecutive = false;
				int num6 = E2.GetFinalizedChance();
				while (num6 > 0 && num6.in100())
				{
					for (int i = 0; i < E2.Attempts && flag; i++)
					{
						MeleeAttackWithWeapon(Attacker, Defender, weapon, bodyPart, Properties, HitModifier, PenModifier, PenCapModifier, 0, 0, flag2);
						flag = GameObject.Validate(Defender);
					}
					consecutive = true;
					num6 -= 100;
				}
				for (int j = 0; j < num5; j++)
				{
					MeleeAttack meleeAttack = E.Attacks[j];
					if ((meleeAttack.Primary.HasValue && meleeAttack.Primary.Value != flag2) || !meleeAttack.IsValidFor(bodyPart))
					{
						continue;
					}
					string text = Properties;
					if (!meleeAttack.Properties.IsNullOrEmpty())
					{
						text = (text.IsNullOrEmpty() ? meleeAttack.Properties : (text + "," + meleeAttack.Properties));
					}
					GetMeleeAttackChanceEvent getMeleeAttackChanceEvent = E2;
					int chance = meleeAttack.Chance;
					string properties = text;
					bool primary = flag2;
					getMeleeAttackChanceEvent.HandleFor(Attacker, weapon, chance, 0, 1.0, properties, bodyPart, PrimaryWeaponPart, primary, Intrinsic: false, consecutive);
					num6 = E2.GetFinalizedChance();
					while (num6 > 0 && num6.in100())
					{
						for (int k = 0; k < E2.Attempts && flag; k++)
						{
							MeleeAttackWithWeapon(Attacker, Defender, weapon, bodyPart, text, meleeAttack.HitModifier + HitModifier, meleeAttack.PenModifier + PenModifier, meleeAttack.PenModifier + PenCapModifier, 0, 0, flag2, Intrinsic: false);
							flag = GameObject.Validate(Defender);
						}
						consecutive = true;
						num6 -= 100;
					}
					E.RemoveAttackAt(0);
					num5--;
					j--;
				}
				num4++;
			}
			PooledEvent<GetMeleeAttacksEvent>.ResetTo(ref E);
			PooledEvent<GetMeleeAttackChanceEvent>.ResetTo(ref E2);
			if (mainWeapon != null && mainWeapon.GetWeaponSkill() == "ShortBlades" && Attacker.HasSkill("ShortBlades_Expertise"))
			{
				EnergyCost = EnergyCost * 3 / 4;
			}
			Attacker.UseEnergy(EnergyCost, "Combat Melee", null, null);
			Event @event = Event.New("AIMessage");
			@event.SetParameter("Message", "Attacked");
			@event.SetParameter("By", Attacker);
			Defender.FireEvent(@event);
			return true;
		}

		public static bool BeginAttack(GameObject Attacker, GameObject Defender, Cell AttackerCell, Cell DefenderCell)
		{
			if (!Defender.HasStat("Hitpoints"))
			{
				return false;
			}
			if (Defender.IsPlayerLed())
			{
				AttackerCell.ParentZone.MarkActive(DefenderCell.ParentZone);
			}
			if (Attacker.HasRegisteredEvent("BeginAttack"))
			{
				Event @event = Event.New("BeginAttack");
				@event.SetParameter("TargetObject", Defender);
				@event.SetParameter("TargetCell", DefenderCell);
				if (!Attacker.FireEvent(@event))
				{
					return false;
				}
			}
			if (DefenderCell.HasObjectWithRegisteredEvent("ObjectAttacking"))
			{
				Event event2 = Event.New("ObjectAttacking");
				event2.SetParameter("Object", Attacker);
				event2.SetParameter("TargetObject", Defender);
				event2.SetParameter("TargetCell", DefenderCell);
				if (!DefenderCell.FireEvent(event2))
				{
					return false;
				}
			}
			if (Attacker.IsPlayer() && IComponent<GameObject>.Visible(Defender))
			{
				Sidebar.CurrentTarget = Defender;
			}
			return true;
		}

		public static bool AttackDirection(GameObject Attacker, string Direction)
		{
			if (Attacker.Physics == null)
			{
				return false;
			}
			if (!Attacker.CheckFrozen())
			{
				return false;
			}
			Cell cellFromDirection = Attacker.CurrentCell.GetCellFromDirection(Direction, BuiltOnly: false);
			if (cellFromDirection == null)
			{
				return false;
			}
			return AttackCell(Attacker, cellFromDirection);
		}

		public static bool AttackObject(GameObject Attacker, GameObject Defender, string Properties = null, int HitModifier = 0, int PenModifier = 0, int PenCapModifier = 0, bool IgnoreFlight = false)
		{
			Cell cell = Attacker.CurrentCell;
			Cell cell2 = Defender?.CurrentCell;
			if (cell == null || cell2 == null)
			{
				return false;
			}
			if (!BeginAttack(Attacker, Defender, cell, cell2))
			{
				return false;
			}
			return PerformMeleeAttack(Attacker, Defender, 1000, HitModifier, PenModifier, PenCapModifier, Properties, IgnoreFlight);
		}

		public static bool AttackCell(GameObject Attacker, Cell Cell, string Properties = null, int HitModifier = 0, int PenModifier = 0, int PenCapModifier = 0, bool IgnoreFlight = false)
		{
			Cell cell = Attacker.CurrentCell;
			GameObject combatTarget = Cell.GetCombatTarget(Attacker, IgnoreFlight, IgnoreAttackable: false, IgnorePhase: false, 5);
			if (cell == null || combatTarget == null)
			{
				return false;
			}
			if (!BeginAttack(Attacker, combatTarget, cell, Cell))
			{
				return false;
			}
			return PerformMeleeAttack(Attacker, combatTarget, 1000, HitModifier, PenModifier, PenCapModifier, Properties, IgnoreFlight);
		}

		private static MeleeAttackResult MeleeAttackWithWeaponInternal(GameObject Attacker, GameObject Defender, GameObject Weapon, BodyPart BodyPart, string Properties = null, int HitModifier = 0, int PenModifier = 0, int PenCapModifier = 0, int AdjustDamageResult = 0, int AdjustDamageDieSize = 0, bool Primary = false, bool Intrinsic = true)
		{
			MeleeAttackResult result = default(MeleeAttackResult);
			int Penetrations = 0;
			int num = 0;
			string text = "1d1";
			string text2 = "Strength";
			Damage damage = new Damage(0);
			damage.AddAttribute("Melee");
			if (Weapon == null || !Weapon.TryGetPart<MeleeWeapon>(out var Part))
			{
				return result;
			}
			text = Part.BaseDamage;
			num = Part.MaxStrengthBonus;
			text2 = Part.Stat;
			if (text2.Contains(","))
			{
				int num2 = int.MinValue;
				foreach (string item in text2.CachedCommaExpansion())
				{
					int num3 = Attacker.Stat(item);
					if (num3 > num2)
					{
						text2 = item;
						num2 = num3;
					}
				}
			}
			if (Part.HasTag("WeaponUnarmed"))
			{
				damage.AddAttribute("Unarmed");
			}
			if (!Part.Attributes.IsNullOrEmpty())
			{
				damage.AddAttributes(Part.Attributes);
			}
			if (AdjustDamageResult != 0)
			{
				text = DieRoll.AdjustResult(text, AdjustDamageResult);
			}
			if (AdjustDamageDieSize != 0)
			{
				text = DieRoll.AdjustDieSize(text, AdjustDamageDieSize);
			}
			bool flag = false;
			damage.AddAttribute(text2);
			if (Statistic.IsMental(text2))
			{
				damage.AddAttribute("Mental");
				flag = true;
			}
			string text3 = Part?.Skill ?? "Unarmed";
			PlaySwingSound(Attacker, Weapon, text3);
			BeforeMeleeAttackEvent.Send(Attacker, Defender, Weapon, text3, text2);
			int num4 = Stat.Random(1, 20);
			string skill = text3;
			string stat = text2;
			int @for = GetToHitModifierEvent.GetFor(Attacker, Defender, Weapon, HitModifier, null, null, skill, stat);
			int value = num4 + @for;
			Event @event = Event.New("GetDefenderDV");
			@event.SetParameter("Weapon", Weapon);
			@event.SetParameter("Damage", damage);
			@event.SetParameter("Attacker", Attacker);
			@event.SetParameter("Defender", Defender);
			@event.SetParameter("NaturalHitResult", num4);
			@event.SetParameter("Result", value);
			@event.SetParameter("Skill", text3);
			@event.SetParameter("Stat", text2);
			@event.SetParameter("DV", Stats.GetCombatDV(Defender));
			Defender.FireEvent(@event);
			@event.ID = "WeaponGetDefenderDV";
			Weapon?.FireEvent(@event);
			@event.ID = "AttackerGetDefenderDV";
			Attacker.FireEvent(@event);
			value = @event.GetIntParameter("Result");
			num4 = @event.GetIntParameter("NaturalHitResult");
			bool flag2 = Properties.Contains("Critical");
			if (!flag2)
			{
				int num5 = GetCriticalThresholdEvent.GetFor(Attacker, Defender, Weapon, null, text3);
				int for2 = GetSpecialEffectChanceEvent.GetFor(Attacker, Weapon, "Melee Critical", 5, Defender);
				if (for2 != 5)
				{
					num5 -= (for2 - 5) / 5;
				}
				if (num4 >= num5)
				{
					flag2 = true;
				}
			}
			bool flag3 = true;
			bool flag4 = flag2 || value > @event.GetIntParameter("DV") || Properties.Contains("Autohit");
			if (flag4)
			{
				@event.ID = "DefenderBeforeHit";
				if (!Defender.FireEvent(@event))
				{
					flag4 = false;
				}
			}
			if (@event.HasIntParameter("NoMissMessage"))
			{
				flag3 = false;
			}
			if (!flag4)
			{
				if (CombatJuice.enabled)
				{
					CombatJuice.playPrefabAnimation(Defender, "CombatJuice/CombatJuiceMissAnimationPrefab");
					CombatJuice.playWorldSound(Attacker, Weapon?.GetTagOrStringProperty("MissSound") ?? "sfx_melee_generic_miss");
					CombatJuice.punch(Attacker, Defender);
				}
				else
				{
					Defender.ParticleBlip("&K\t", 10, 0L);
				}
				if (Attacker.IsPlayer())
				{
					if (flag3)
					{
						if (IComponent<GameObject>.TerseMessages)
						{
							IComponent<GameObject>.AddPlayerMessage("You miss!", 'r');
						}
						else if (Weapon != null)
						{
							IComponent<GameObject>.AddPlayerMessage("{{r|You miss with " + Attacker.its_(Weapon) + "!}} [" + value + " vs " + @event.GetIntParameter("DV") + "]");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("{{r|You miss!}} [" + value + " vs " + @event.GetIntParameter("DV") + "]");
						}
					}
				}
				else if (Defender.IsPlayer())
				{
					if (AutoAct.IsInterruptable() && Defender.IsRelevantHostile(Attacker))
					{
						AutoAct.Interrupt("you are under attack by " + Attacker.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null), null, Attacker, IsThreat: true);
					}
					if (flag3)
					{
						if (IComponent<GameObject>.TerseMessages)
						{
							IComponent<GameObject>.AddPlayerMessage(Attacker.Does("miss", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you!");
						}
						else if (Weapon != null)
						{
							IComponent<GameObject>.AddPlayerMessage(Attacker.Does("miss", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you with " + Attacker.its_(Weapon) + "! [" + value + " vs " + @event.GetIntParameter("DV") + "]");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(Attacker.Does("miss", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you! [" + value + " vs " + @event.GetIntParameter("DV") + "]");
						}
					}
				}
				else if (AutoAct.IsInterruptable())
				{
					if (Attacker.IsPlayerLedAndPerceptible() && !Attacker.IsTrifling && (!IComponent<GameObject>.Visible(Defender) || The.Player.IsRelevantHostile(Defender)))
					{
						AutoAct.Interrupt("you " + Attacker.GetPerceptionVerb() + " " + Attacker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: true, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " fighting" + (Attacker.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(Attacker))), null, Attacker, IsThreat: true);
					}
					else if (Defender.IsPlayerLedAndPerceptible() && !Defender.IsTrifling && (!IComponent<GameObject>.Visible(Attacker) || The.Player.IsRelevantHostile(Attacker)))
					{
						AutoAct.Interrupt("you " + Defender.GetPerceptionVerb() + " " + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: true, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " fighting" + (Defender.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(Defender))), null, Defender, IsThreat: true);
					}
				}
				Event event2 = Event.New("AttackerMeleeMiss");
				event2.SetParameter("Weapon", Weapon);
				event2.SetParameter("Attacker", Attacker);
				event2.SetParameter("Defender", Defender);
				Attacker.FireEvent(event2);
				Event event3 = Event.New("DefenderAfterAttackMissed");
				event3.SetParameter("Attacker", Attacker);
				event3.SetParameter("Defender", Defender);
				event3.SetParameter("Weapon", Weapon);
				Defender.FireEvent(event3);
				if (Weapon != null)
				{
					event3.ID = "WeaponAfterAttackMissed";
					Weapon.FireEvent(event3);
				}
			}
			else
			{
				Defender.FireEvent("DefenderAttackHit");
				if (!Defender.HasStat("AV"))
				{
					return result;
				}
				DefendMeleeHitEvent.Send(Attacker, Defender, Weapon, damage, value);
				int AV = (flag ? Stats.GetCombatMA(Defender) : Stats.GetCombatAV(Defender));
				int PenetrationBonus = 0;
				bool ShieldBlocked = false;
				GetAttackerHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, Attacker, Defender, Weapon);
				GetWeaponHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, Attacker, Defender, Weapon);
				GetDefenderHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, Attacker, Defender, Weapon);
				int StatBonus = ((Part == null || !Part.HasTag("WeaponIgnoreStrength")) ? Attacker.StatMod(text2) : 0);
				int PenetrationBonus2 = PenModifier;
				int MaxPenetrationBonus = PenCapModifier;
				if (Part != null)
				{
					PenetrationBonus2 += Part.PenBonus;
					MaxPenetrationBonus += Part.PenBonus;
				}
				string hand = (Primary ? "Primary" : "Secondary");
				GetWeaponMeleePenetrationEvent.Process(ref Penetrations, ref StatBonus, ref num, ref PenetrationBonus2, ref MaxPenetrationBonus, AV, flag2, Properties, hand, Attacker, Defender, Weapon);
				GetAttackerMeleePenetrationEvent.Process(ref Penetrations, ref StatBonus, ref num, ref PenetrationBonus2, ref MaxPenetrationBonus, AV, flag2, Properties, hand, Attacker, Defender, Weapon);
				GetDefenderMeleePenetrationEvent.Process(ref Penetrations, ref StatBonus, ref num, ref PenetrationBonus2, ref MaxPenetrationBonus, AV, flag2, Properties, hand, Attacker, Defender, Weapon);
				BaseSkill baseSkill = null;
				bool flag5 = false;
				int num6 = 0;
				int num7 = 0;
				if (flag2)
				{
					Attacker.PlayWorldSound("Sounds/Damage/sfx_damage_critical", 0.5f, 0f, Combat: true);
					num6 = 1;
					num7 = 1;
					flag5 = true;
					if (baseSkill == null)
					{
						baseSkill = Skills.GetGenericSkill(text3, Attacker);
					}
					if (baseSkill != null)
					{
						int weaponCriticalModifier = baseSkill.GetWeaponCriticalModifier(Attacker, Defender, Weapon);
						if (weaponCriticalModifier != 0)
						{
							num6 += weaponCriticalModifier;
							num7 += weaponCriticalModifier;
						}
					}
					Event event4 = Event.New("WeaponCriticalModifier");
					event4.SetParameter("Attacker", Attacker);
					event4.SetParameter("Defender", Defender);
					event4.SetParameter("Weapon", Weapon);
					event4.SetParameter("Skill", text3);
					event4.SetParameter("Stat", text2);
					event4.SetParameter("PenBonus", num6);
					event4.SetParameter("CapBonus", num7);
					event4.SetFlag("AutoPen", flag5);
					Weapon?.FireEvent(event4);
					event4.ID = "AttackerCriticalModifier";
					Attacker.FireEvent(event4);
					num6 = event4.GetIntParameter("PenBonus");
					num7 = event4.GetIntParameter("CapBonus");
					flag5 = event4.HasFlag("AutoPen");
				}
				Penetrations = (result.Penetrations = Penetrations + Stat.RollDamagePenetrations(AV, StatBonus + PenetrationBonus2 + PenetrationBonus + num6, num + MaxPenetrationBonus + PenetrationBonus + num7));
				bool flag6 = false;
				Event event5 = Event.New("AttackerHit", 5, 0, 2);
				event5.SetParameter("Penetrations", Penetrations);
				event5.SetParameter("Damage", damage);
				event5.SetParameter("Attacker", Attacker);
				event5.SetParameter("Defender", Defender);
				event5.SetParameter("Weapon", Weapon);
				event5.SetParameter("Properties", Properties);
				event5.SetFlag("Critical", flag2);
				if (!Attacker.FireEvent(event5))
				{
					return result;
				}
				if (event5.HasFlag("DidSpecialEffect"))
				{
					flag6 = true;
				}
				Properties = event5.GetStringParameter("Properties", "") ?? Properties;
				Event event6 = Event.New("DefenderHit", 4, 0, 2);
				event6.SetParameter("Penetrations", event5.GetIntParameter("Penetrations"));
				event6.SetParameter("Damage", damage);
				event6.SetParameter("Attacker", Attacker);
				event6.SetParameter("Defender", Defender);
				event6.SetParameter("Weapon", Weapon);
				event6.SetFlag("Critical", flag2);
				if (!Defender.FireEvent(event6))
				{
					return result;
				}
				if (event6.HasFlag("DidSpecialEffect"))
				{
					flag6 = true;
				}
				Event event7 = Event.New("WeaponHit", 4, 0, 2);
				event7.SetParameter("Penetrations", event6.GetIntParameter("Penetrations"));
				event7.SetParameter("Damage", damage);
				event7.SetParameter("Attacker", Attacker);
				event7.SetParameter("Defender", Defender);
				event7.SetParameter("Weapon", Weapon);
				event7.SetFlag("Critical", flag2);
				event7.SetParameter("Properties", Properties);
				if (Weapon != null && !Weapon.FireEvent(event7))
				{
					return result;
				}
				if (event7.HasFlag("DidSpecialEffect"))
				{
					flag6 = true;
				}
				Properties = event7.GetStringParameter("Properties", "") ?? Properties;
				bool defenderIsCreature = Defender.HasTag("Creature");
				string blueprint = Defender.Blueprint;
				WeaponUsageTracking.TrackMeleeWeaponHit(Attacker, Weapon, defenderIsCreature, blueprint);
				if (Attacker.HasRegisteredEvent("WieldedWeaponHit"))
				{
					Event event8 = Event.New("WieldedWeaponHit", 4, 0, 2);
					event8.SetParameter("Penetrations", event7.GetIntParameter("Penetrations"));
					event8.SetParameter("Damage", damage);
					event8.SetParameter("Attacker", Attacker);
					event8.SetParameter("Defender", Defender);
					event8.SetParameter("Weapon", Weapon);
					event8.SetFlag("Critical", flag2);
					if (!Attacker.FireEvent(event8))
					{
						return result;
					}
					if (event8.HasFlag("DidSpecialEffect"))
					{
						flag6 = true;
					}
				}
				if (!event7.HasParameter("Penetrations"))
				{
					return result;
				}
				Penetrations = event7.GetIntParameter("Penetrations");
				if (Penetrations <= 0 && Properties.Contains("Autopen"))
				{
					Penetrations = 1;
				}
				else if (Penetrations > 1 && Properties.Contains("MaxPens1"))
				{
					Penetrations = 1;
				}
				else if (Penetrations <= 0 && flag5 && Attacker != null && Attacker.IsPlayer())
				{
					Penetrations = 1;
				}
				result.Penetrations = Penetrations;
				Cell value2 = Defender.CurrentCell;
				if (Penetrations > 0)
				{
					if (Penetrations < 2)
					{
						Attacker?.PlayWorldSound("Sounds/Damage/sfx_damage_penetrate_low");
					}
					else if (Penetrations < 4)
					{
						Attacker?.PlayWorldSound("Sounds/Damage/sfx_damage_penetrate_med");
					}
					else
					{
						Attacker?.PlayWorldSound("Sounds/Damage/sfx_damage_penetrate_high");
					}
					damage.AddAttribute(text3);
					if (Weapon != null && flag2)
					{
						damage.AddAttribute("Critical");
						Event event9 = Event.New("CriticalHit");
						event9.SetParameter("Attacker", Attacker);
						event9.SetParameter("Defender", Defender);
						event9.SetParameter("BaseDamage", text);
						event9.SetParameter("Weapon", Weapon);
						event9.SetParameter("Skill", text3);
						event9.SetParameter("Stat", text2);
						event9.ID = "AttackerCriticalHit";
						Attacker.FireEvent(event9);
						event9.ID = "WeaponCriticalHit";
						if (Weapon != null)
						{
							Weapon.FireEvent(event9);
						}
						else
						{
							Attacker.FireEvent(event9);
						}
						event9.ID = "DefenderCriticalHit";
						Defender.FireEvent(event9);
						text = event9.GetStringParameter("BaseDamage");
						if (event9.HasFlag("DidSpecialEffect"))
						{
							flag6 = true;
						}
					}
					int num8 = 0;
					if (damage.HasAttribute("Mental") && Defender.Brain == null)
					{
						if (Attacker.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("Your mental attack does not affect " + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
						}
						flag6 = true;
					}
					else
					{
						DieRoll cachedDieRoll = text.GetCachedDieRoll();
						for (int i = 0; i < Penetrations; i++)
						{
							num8 += cachedDieRoll.Resolve();
						}
					}
					if (num8 > 0 || flag6)
					{
						if (flag2)
						{
							Attacker.ParticleText("*critical hit*", IComponent<GameObject>.ConsequentialColorChar(Attacker));
						}
						string resultColor = Stat.GetResultColor(Penetrations);
						if (!Options.ShowMonsterHPHearts)
						{
							Defender.ParticleBlip(resultColor + "\u0003", 10, 0L);
						}
						damage.Amount += num8;
						Event event10 = Event.New("DealDamage");
						event10.SetParameter("Penetrations", Penetrations);
						event10.SetParameter("Damage", damage);
						event10.SetParameter("Attacker", Attacker);
						event10.SetParameter("Defender", Defender);
						event10.SetParameter("Weapon", Weapon);
						event10.SetParameter("Properties", Properties);
						event10.SetParameter("Cell", value2);
						event10.SetFlag("Critical", flag2);
						if (event10.HasFlag("DidSpecialEffect"))
						{
							flag6 = true;
						}
						Event event11 = Event.New("WeaponDealDamage");
						event11.SetParameter("Penetrations", Penetrations);
						event11.SetParameter("Damage", damage);
						event11.SetParameter("Attacker", Attacker);
						event11.SetParameter("Defender", Defender);
						event11.SetParameter("Weapon", Weapon);
						event11.SetParameter("Properties", Properties);
						event11.SetParameter("Cell", value2);
						event11.SetFlag("Critical", flag2);
						if (event11.HasFlag("DidSpecialEffect"))
						{
							flag6 = true;
						}
						Event event12 = Event.New("TakeDamage");
						event12.SetParameter("Penetrations", Penetrations);
						event12.SetParameter("Damage", damage);
						event12.SetParameter("Owner", Attacker);
						event12.SetParameter("Attacker", Attacker);
						event12.SetParameter("Defender", Defender);
						event12.SetParameter("Weapon", Weapon);
						event12.SetParameter("Properties", Properties);
						event12.SetParameter("Message", "");
						event12.SetParameter("Cell", value2);
						event12.SetFlag("NoDamageMessage", State: true);
						event12.SetFlag("Critical", flag2);
						if (CombatJuice.enabled)
						{
							if (damage.IsBludgeoningDamage())
							{
								if (Penetrations <= 1)
								{
									CombatJuice.playPrefabAnimation(Defender, "CombatJuice/CombatJuiceBashAnimationPrefabLow");
								}
								else if (Penetrations <= 3)
								{
									CombatJuice.playPrefabAnimation(Defender, "CombatJuice/CombatJuiceBashAnimationPrefabMedium");
								}
								else
								{
									CombatJuice.playPrefabAnimation(Defender, "CombatJuice/CombatJuiceBashAnimationPrefabHigh");
								}
							}
							else if (Penetrations <= 1)
							{
								CombatJuice.playPrefabAnimation(Defender, "CombatJuice/CombatJuiceSlashAnimationPrefabLow");
							}
							else if (Penetrations <= 3)
							{
								CombatJuice.playPrefabAnimation(Defender, "CombatJuice/CombatJuiceSlashAnimationPrefabMedium");
							}
							else
							{
								CombatJuice.playPrefabAnimation(Defender, "CombatJuice/CombatJuiceSlashAnimationPrefabHigh");
							}
							if (Weapon.HasTagOrStringProperty("HitSound"))
							{
								CombatJuice.playWorldSound(Attacker, Weapon.GetTagOrStringProperty("HitSound"), 0.5f, 0f, 0f, Weapon.HasIntProperty("HitSoundDelay") ? ((float)Weapon.GetIntProperty("HitSoundDelay") / 1000f) : 0.135f);
							}
							CombatJuice.punch(Attacker, Defender);
						}
						bool flag7 = false;
						if (Attacker.FireEvent(event10) || flag6)
						{
							WeaponUsageTracking.TrackMeleeWeaponDamage(Attacker, Weapon, defenderIsCreature, blueprint, damage);
							if (Weapon == null || Weapon.FireEvent(event11))
							{
								StringBuilder stringBuilder = Event.NewStringBuilder();
								if (Attacker.IsPlayer())
								{
									stringBuilder.Append("{{g|You");
									if (flag2)
									{
										stringBuilder.Append(" critically");
									}
									stringBuilder.Append(" hit");
									if (!IComponent<GameObject>.TerseMessages)
									{
										stringBuilder.Append(" {{").Append(resultColor).Append("|(x")
											.Append(Penetrations)
											.Append(")}}");
									}
									if (damage.Amount > 0)
									{
										stringBuilder.Append(" for ").Append(damage.Amount).Append(" damage");
									}
									if (!IComponent<GameObject>.TerseMessages)
									{
										stringBuilder.Append(" with ");
										if (Weapon == null)
										{
											stringBuilder.Append("your bare hands");
										}
										else
										{
											Attacker.its_(Weapon, stringBuilder);
										}
										stringBuilder.Append("! [").Append(value).Append(']');
									}
									stringBuilder.Append("}}");
								}
								else if (Defender.IsPlayer())
								{
									stringBuilder.Append("%T");
									if (flag2)
									{
										stringBuilder.Append(" critically");
									}
									stringBuilder.Append(Attacker.GetVerb("hit"));
									if (!IComponent<GameObject>.TerseMessages)
									{
										stringBuilder.Append(" {{").Append(resultColor).Append("|(x")
											.Append(Penetrations)
											.Append(")}}");
									}
									if (damage.Amount > 0)
									{
										stringBuilder.Append(" for ").Append(damage.Amount).Append(" damage");
									}
									if (!IComponent<GameObject>.TerseMessages)
									{
										if (Weapon == null)
										{
											stringBuilder.Append(" barehanded");
										}
										else
										{
											stringBuilder.Append(" with ");
											Attacker.its_(Weapon, stringBuilder);
										}
										stringBuilder.Append('.');
										stringBuilder.Append(" [").Append(value).Append(']');
									}
								}
								event12.SetParameter("Message", stringBuilder.ToString());
								if (flag2 && Defender.GetIntProperty("Bleeds") > 0)
								{
									Defender.Bloodsplatter();
								}
								if (Defender.FireEvent(event12))
								{
									flag7 = true;
									if (event12.HasFlag("DidSpecialEffect"))
									{
										flag6 = true;
									}
									if (Defender.IsValid())
									{
										if (flag2)
										{
											if (baseSkill == null)
											{
												baseSkill = Skills.GetGenericSkill(text3, Attacker);
											}
											baseSkill?.WeaponMadeCriticalHit(Attacker, Defender, Weapon, Properties);
											Event event13 = Event.New("AfterCriticalHit");
											event13.SetParameter("Attacker", Attacker);
											event13.SetParameter("Defender", Attacker);
											event13.SetParameter("Weapon", Weapon);
											event13.SetParameter("Skill", text3);
											event13.SetParameter("Stat", text2);
											event13.SetParameter("Properties", Properties);
											event13.SetParameter("Cell", value2);
											event13.ID = "AttackerAfterCriticalHit";
											Attacker.FireEvent(event13);
											event13.ID = "WeaponAfterCriticalHit";
											if (Weapon != null)
											{
												Weapon.FireEvent(event13);
											}
											else
											{
												Attacker.FireEvent(event13);
											}
											event13.ID = "DefenderAfterCriticalHit";
											Defender.FireEvent(event13);
											if (event13.HasFlag("DidSpecialEffect"))
											{
												flag6 = true;
											}
										}
										Event event14 = Event.New("AttackerAfterDamage");
										event14.SetParameter("Penetrations", Penetrations);
										event14.SetParameter("Damage", damage);
										event14.SetParameter("Attacker", Attacker);
										event14.SetParameter("Defender", Defender);
										event14.SetParameter("Weapon", Weapon);
										event14.SetParameter("Message", "");
										event14.SetParameter("Properties", Properties);
										event14.SetParameter("Cell", value2);
										event14.SetFlag("Critical", flag2);
										Attacker.FireEvent(event14);
										event14.ID = "WeaponAfterDamage";
										Weapon?.FireEvent(event14);
										if (event14.HasFlag("DidSpecialEffect"))
										{
											flag6 = true;
										}
									}
								}
								if (event12.HasFlag("DidSpecialEffect"))
								{
									flag6 = true;
								}
							}
						}
						if (!flag7 && !flag6)
						{
							if (!damage.SuppressionMessageDone && Attacker.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("You fail to deal damage with your attack! [" + value + "]", 'r');
							}
							if (Defender.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage(Attacker.Does("fail", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " to deal damage with " + Attacker.its + " attack! [" + value + "]");
							}
						}
						if (Options.ShowMonsterHPHearts)
						{
							Defender.ParticleBlip(Defender.GetHPColor() + "\u0003", 10, 0L);
						}
					}
				}
				else
				{
					if (CombatJuice.enabled)
					{
						CombatJuice.playPrefabAnimation(Defender, "CombatJuice/CombatJuiceBlockAnimationPrefab");
						CombatJuice.punch(Attacker, Defender);
					}
					if (Attacker.IsPlayer())
					{
						if (IComponent<GameObject>.TerseMessages)
						{
							IComponent<GameObject>.AddPlayerMessage("You don't penetrate " + Defender.poss("armor") + ".", 'r');
						}
						else if (Weapon != null)
						{
							IComponent<GameObject>.AddPlayerMessage("You don't penetrate " + Defender.poss("armor") + " with " + Attacker.its_(Weapon) + ". {{y|[" + value + "]}}", 'r');
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("You don't penetrate " + Defender.poss("armor") + ". {{y|[" + value + "]}}", 'r');
						}
					}
					else if (Defender.IsPlayer())
					{
						if (IComponent<GameObject>.TerseMessages)
						{
							IComponent<GameObject>.AddPlayerMessage(Attacker.Does("don't", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " penetrate your armor.");
						}
						else if (Weapon != null)
						{
							IComponent<GameObject>.AddPlayerMessage(Attacker.Does("don't", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " penetrate your armor with " + Attacker.its_(Weapon) + "! [" + value + "]");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(Attacker.Does("don't", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " penetrate your armor! [" + value + "]");
						}
					}
					if (ShieldBlocked)
					{
						if (!CombatJuice.enabled)
						{
							Defender.ParticleBlip("&G\a", 10, 0L);
						}
						PlayBlockedSound(Attacker, Weapon, text3);
					}
					else
					{
						if (!CombatJuice.enabled)
						{
							Defender.ParticleBlip("&K\a", 10, 0L);
						}
						PlayBlockedSound(Attacker, Weapon, text3);
					}
				}
				Event event15 = Event.New("AttackerAfterAttack");
				event15.SetParameter("Penetrations", Penetrations);
				event15.SetParameter("Damage", damage);
				event15.SetParameter("Attacker", Attacker);
				event15.SetParameter("Defender", Defender);
				event15.SetParameter("Weapon", Weapon);
				event15.SetParameter("Skill", text3);
				event15.SetParameter("Stat", text2);
				event15.SetParameter("Properties", Properties);
				event15.SetParameter("Cell", value2);
				event15.SetFlag("Critical", flag2);
				Attacker.FireEvent(event15);
				event15.ID = "DefenderAfterAttack";
				Defender.FireEvent(event15);
				if (Weapon != null)
				{
					event15.ID = "WeaponAfterAttack";
					Weapon.FireEvent(event15);
				}
			}
			result.Attacks++;
			if (flag4)
			{
				result.Hits++;
			}
			if (flag2)
			{
				result.Criticals++;
			}
			result.Damage += damage.Amount;
			result.Penetrations += Penetrations;
			return result;
		}

		public static MeleeAttackResult MeleeAttackWithWeapon(GameObject Attacker, GameObject Defender, GameObject Weapon, BodyPart BodyPart, string Properties = null, int HitModifier = 0, int PenModifier = 0, int PenCapModifier = 0, int AdjustDamageResult = 0, int AdjustDamageDieSize = 0, bool Primary = false, bool Intrinsic = true)
		{
			MeleeAttackResult result = default(MeleeAttackResult);
			if (Weapon == null)
			{
				MetricsManager.LogEditorWarning("No weapon supplied to MeleeAttackWithWeapon");
				return result;
			}
			GetWeaponMeleeAttacksEvent getWeaponMeleeAttacksEvent = GetWeaponMeleeAttacksEvent.HandleFrom(Attacker, Weapon, Primary, Intrinsic);
			int num = -1;
			do
			{
				if (!GameObject.Validate(Defender))
				{
					return result;
				}
				if (!GameObject.Validate(Weapon) || (Weapon.Equipped != Attacker && !Attacker.IsADefaultBehavior(Weapon)))
				{
					return result;
				}
				string text = Properties ?? "";
				if (num >= 0)
				{
					MeleeAttack meleeAttack = getWeaponMeleeAttacksEvent.Attacks[num];
					if (!meleeAttack.IsValidFor(BodyPart) || !meleeAttack.Chance.in100())
					{
						continue;
					}
					if (!meleeAttack.Properties.IsNullOrEmpty())
					{
						text = (text.IsNullOrEmpty() ? meleeAttack.Properties : (text + "," + meleeAttack.Properties));
					}
				}
				result += MeleeAttackWithWeaponInternal(Attacker, Defender, Weapon, BodyPart, text, HitModifier, PenModifier, PenCapModifier, AdjustDamageResult, AdjustDamageDieSize);
			}
			while (++num < getWeaponMeleeAttacksEvent.Attacks.Count);
			return result;
		}

		public static bool SwoopAttack(GameObject Attacker, string Direction = null)
		{
			if (Direction == null)
			{
				if (Attacker.IsPlayer())
				{
					Direction = Attacker.Physics.PickDirectionS("Swoop");
				}
				if (Direction == null)
				{
					return false;
				}
			}
			Attacker.PlayWorldSound("Sounds/Abilities/sfx_ability_swoop");
			Attacker.Physics.DidX("swoop", "down to attack", null, null, null, Attacker);
			Flight.SuspendFlight(Attacker);
			TrackShieldBlock = 1;
			bool flag = false;
			try
			{
				bool num = AttackDirection(Attacker, Direction);
				bool flag2 = TrackShieldBlock > 1;
				Flight.DesuspendFlight(Attacker);
				TrackShieldBlock = 0;
				flag = true;
				if (!num)
				{
					return false;
				}
				int num2 = Flight.GetSwoopFallChance(Attacker);
				if (flag2)
				{
					num2 *= 2;
				}
				if (num2.in100())
				{
					Flight.Fall(Attacker);
				}
				else
				{
					Attacker.UseEnergy(1000, "Swoop Return", null, null);
				}
			}
			finally
			{
				if (!flag)
				{
					Flight.DesuspendFlight(Attacker);
					TrackShieldBlock = 0;
				}
			}
			return true;
		}

		private static void PlaySwingSound(GameObject Attacker, GameObject Weapon, string Skill)
		{
			string text = Weapon?.GetTagOrStringProperty("SwingSound");
			if (text.IsNullOrEmpty() && !Skill.IsNullOrEmpty())
			{
				bool flag = Weapon?.IsNatural() ?? false;
				text = Skill switch
				{
					"LongBlades" => flag ? "sfx_melee_naturalWeapon_longblade_swing" : "sfx_melee_longBlade_metal_swing", 
					"Axe" => flag ? "sfx_melee_naturalWeapon_axe_swing" : "sfx_melee_axe_oneHanded_metal_swing", 
					"ShortBlades" => flag ? "sfx_melee_naturalWeapon_shortblade_swing" : "sfx_melee_shortSword_metal_swing", 
					"Unarmed" => "sfx_melee_naturalWeapon_fist_swing", 
					_ => flag ? "sfx_melee_naturalWeapon_cudgel_swing" : "sfx_melee_cudgel_wood_swing", 
				};
			}
			Attacker.PlayWorldSound(text.Coalesce("sfx_melee_cudgel_wood_swing"), 0.5f, 0f, Combat: true);
		}

		private static void PlayBlockedSound(GameObject Attacker, GameObject Weapon, string Skill)
		{
			string text = Weapon?.GetTagOrStringProperty("BlockedSound");
			if (text.IsNullOrEmpty() && !Skill.IsNullOrEmpty())
			{
				bool flag = Weapon?.IsNatural() ?? false;
				text = Skill switch
				{
					"LongBlades" => flag ? "sfx_melee_naturalWeapon_longblade_blocked" : "sfx_melee_metal_blocked", 
					"Axe" => flag ? "sfx_melee_naturalWeapon_axe_blocked" : "sfx_melee_metal_blocked", 
					"ShortBlades" => flag ? "sfx_melee_naturalWeapon_shortblade_blocked" : "sfx_melee_metal_blocked", 
					"Unarmed" => "sfx_melee_naturalWeapon_fist_blocked", 
					_ => flag ? "sfx_melee_naturalWeapon_cudgel_blocked" : "sfx_melee_cudgel_wood_blocked", 
				};
			}
			Attacker.PlayWorldSound(text.Coalesce("sfx_melee_generic_miss"), 1f, 0.2f, Combat: true);
		}
	}
}
