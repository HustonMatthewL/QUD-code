using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class MagneticPulse : BaseMutation
	{
		public static readonly string COMMAND_NAME = "CommandMagneticPulse";

		public int Cooldown = 100;

		public int PulseCharging;

		public MagneticPulse()
		{
			DisplayName = "Magnetic Pulse";
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void CollectStats(Templates.StatCollector stats)
		{
			stats.Set("Radius", base.Level);
			stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), Cooldown);
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID)
			{
				return ID == PooledEvent<CommandEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
		{
			if (E.Distance > 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.Add(COMMAND_NAME);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AIBoredEvent E)
		{
			if (PulseCharging == 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				CommandEvent.Send(E.Actor, COMMAND_NAME);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeginTakeActionEvent E)
		{
			if (PulseCharging > 0)
			{
				PulseCharging++;
				ParentObject.UseEnergy(1000, "Physical Mutation", null, null);
				if (PulseCharging >= 2)
				{
					EmitMagneticPulse(ParentObject, GetRadius(base.Level));
					PulseCharging = 0;
				}
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CommandEvent E)
		{
			if (E.Command == COMMAND_NAME)
			{
				PulseCharging = 1;
				ParentObject.UseEnergy(1000, "Physical Mutation", null, null);
				CooldownMyActivatedAbility(ActivatedAbilityID, Cooldown);
			}
			return base.HandleEvent(E);
		}

		public override string GetDescription()
		{
			return "You emit powerful magnetic pulses.";
		}

		public override string GetLevelText(int Level)
		{
			return string.Concat("After a one-round warmup, you emit a pulse with radius " + Level + " that attempts to pull metal objects toward you, including metal gear equipped on creatures.\n", "Cooldown: ", Cooldown.Things("round"), "\n");
		}

		public override bool Render(RenderEvent E)
		{
			if (PulseCharging == 1)
			{
				int num = XRLCore.CurrentFrame % 60;
				if (num > 35 && num < 45)
				{
					E.Tile = null;
					E.RenderString = ">";
					E.ColorString = "&C";
				}
			}
			return base.Render(E);
		}

		public static bool CanManipulate(GameObject Object)
		{
			if (!Object.IsNatural())
			{
				return CanBeMagneticallyManipulatedEvent.Check(Object);
			}
			return false;
		}

		private static int PullResistance(GameObject obj)
		{
			if (obj.HasStat("Strength"))
			{
				return obj.Stat("Strength");
			}
			return (int)Math.Round((double)obj.GetKineticResistance() * 0.1, MidpointRounding.AwayFromZero);
		}

		private static int PullStrengthInstance(int Radius)
		{
			return Radius * 10 - Stat.Random(1, 20);
		}

		private static int PullSquares(int PullStrength, int PullResistance)
		{
			return (PullStrength - PullResistance) / 5;
		}

		public static void EmitMagneticPulse(GameObject Actor, int Radius)
		{
			if (!GameObject.Validate(Actor))
			{
				return;
			}
			Cell cell = Actor.CurrentCell;
			if (cell == null)
			{
				return;
			}
			Actor?.PlayWorldSound("Sounds/Abilities/sfx_ability_magnetic_pulse");
			IComponent<GameObject>.XDidY(Actor, "emit", "a powerful magnetic pulse", "!", null, null, Actor);
			if (IComponent<GameObject>.Visible(Actor))
			{
				for (int num = Radius; num > 0; num -= 2)
				{
					float num2 = 1f;
					for (int i = 0; i < 360; i++)
					{
						float num3 = (float)Math.Sin((double)(float)i * 0.017) / num2;
						float num4 = (float)Math.Cos((double)(float)i * 0.017) / num2;
						float num5 = (float)num / num2;
						The.ParticleManager.Add("@", (float)cell.X + num3 * num5, (float)cell.Y + num4 * num5, 0f - num3, 0f - num4, (int)num5);
					}
				}
			}
			List<Cell> realAdjacentCells = cell.GetRealAdjacentCells(Radius);
			List<Tuple<GameObject, int>> list = new List<Tuple<GameObject, int>>(Radius + 2);
			int num6 = 0;
			foreach (Cell item in realAdjacentCells)
			{
				foreach (GameObject @object in item.Objects)
				{
					if (CanManipulate(@object) || @object.AnyInstalledCybernetics(CanManipulate))
					{
						list.Add(new Tuple<GameObject, int>(@object, -1));
					}
				}
				num6 += item.Objects.Count;
			}
			List<GameObject> list2 = new List<GameObject>(num6);
			foreach (Cell item2 in realAdjacentCells)
			{
				list2.AddRange(item2.Objects);
			}
			list2.Sort((GameObject a, GameObject b) => a.DistanceTo(Actor).CompareTo(b.DistanceTo(Actor)));
			foreach (GameObject affectedObject in list2)
			{
				List<Tuple<Cell, char>> lineTo = affectedObject.GetLineTo(Actor);
				GameObject randomElement = (from o in affectedObject.GetInventoryAndEquipment()
					where CanManipulate(o) && o.Implantee == null
					select o).GetRandomElement();
				GameObject gameObject = null;
				if (randomElement != null)
				{
					int pullStrength = PullStrengthInstance(Radius);
					int pullResistance = PullResistance(randomElement);
					int num7 = PullSquares(pullStrength, pullResistance);
					if (num7 > 0)
					{
						if (randomElement.Equipped != null && (randomElement.EquippedOn()?.Type == "Body" || !randomElement.UnequipAndRemove()))
						{
							if (!list.Any((Tuple<GameObject, int> e) => e.Item1 == affectedObject))
							{
								list.Add(new Tuple<GameObject, int>(affectedObject, num7));
							}
						}
						else
						{
							if (affectedObject.IsPlayer())
							{
								gameObject = randomElement;
							}
							randomElement.RemoveFromContext();
							affectedObject.CurrentCell.AddObject(randomElement);
							Cell cell2 = null;
							for (int j = 1; j <= num7 && j < lineTo.Count - 1 && !lineTo[j].Item1.IsSolid(); j++)
							{
								if (!randomElement.HasPart<Combat>() || lineTo[j].Item1.IsEmpty())
								{
									cell2 = lineTo[j].Item1;
								}
							}
							if (cell2 != null)
							{
								randomElement.DirectMoveTo(cell2);
							}
						}
					}
				}
				if (gameObject == null)
				{
					continue;
				}
				if (affectedObject.IsPlayer())
				{
					if (AutoAct.IsInterruptable())
					{
						AutoAct.Interrupt(gameObject.does("have", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " been ripped from your body", null, IComponent<GameObject>.Visible(Actor) ? Actor : affectedObject, IsThreat: true);
					}
					else
					{
						Popup.Show(gameObject.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " ripped from your body!");
					}
				}
				else if (affectedObject.IsPlayerLed() && !affectedObject.IsTrifling && IComponent<GameObject>.Visible(affectedObject))
				{
					if (AutoAct.IsInterruptable())
					{
						AutoAct.Interrupt("your companion, " + affectedObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, null) + "," + affectedObject.GetVerb("have") + " had " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " ripped from " + affectedObject.its + " body", null, IComponent<GameObject>.Visible(Actor) ? Actor : affectedObject, IsThreat: true);
					}
					else
					{
						Popup.Show("Your companion, " + affectedObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, null) + "," + affectedObject.GetVerb("have") + " had " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " ripped from " + affectedObject.its + " body!");
					}
				}
			}
			list.Sort((Tuple<GameObject, int> a, Tuple<GameObject, int> b) => a.Item1.DistanceTo(Actor).CompareTo(b.Item1.DistanceTo(Actor)));
			foreach (Tuple<GameObject, int> item3 in list)
			{
				int num8 = 0;
				if (item3.Item2 < 0)
				{
					int pullStrength2 = PullStrengthInstance(Radius);
					int pullResistance2 = PullResistance(item3.Item1);
					num8 = PullSquares(pullStrength2, pullResistance2);
				}
				else
				{
					num8 = item3.Item2;
				}
				if (num8 <= 0)
				{
					continue;
				}
				List<Tuple<Cell, char>> lineTo2 = item3.Item1.GetLineTo(Actor);
				Cell cell3 = null;
				for (int k = 1; k <= num8 && k < lineTo2.Count - 1 && lineTo2[k].Item1.IsEmptyOfSolidFor(item3.Item1, item3.Item1.IsCombatObject(NoBrainOnly: true)); k++)
				{
					cell3 = lineTo2[k].Item1;
				}
				if (cell3 != null && item3.Item1.DirectMoveTo(cell3) && IComponent<GameObject>.Visible(item3.Item1))
				{
					if (IComponent<GameObject>.Visible(Actor))
					{
						IComponent<GameObject>.AddPlayerMessage(item3.Item1.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " pulled toward " + Actor.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(item3.Item1.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " pulled toward something.");
					}
				}
			}
		}

		public int GetRadius(int Level)
		{
			return 1 + Level;
		}

		public override bool ChangeLevel(int NewLevel)
		{
			return base.ChangeLevel(NewLevel);
		}

		private void AddAbility(GameObject GO)
		{
			ActivatedAbilityID = AddMyActivatedAbility("Magnetic Pulse", COMMAND_NAME, "Physical Mutations", null, "æ");
		}

		public override bool Mutate(GameObject GO, int Level)
		{
			AddAbility(GO);
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			RemoveMyActivatedAbility(ref ActivatedAbilityID);
			return base.Unmutate(GO);
		}
	}
}
