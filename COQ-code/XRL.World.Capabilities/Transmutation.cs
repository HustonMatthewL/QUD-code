using System.Collections.Generic;
using Qud.API;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.Anatomy;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World.Capabilities
{
	[HasWishCommand]
	public static class Transmutation
	{
		public const string TAG_SUPPRESS = "SuppressTransmutation";

		public static bool TransmuteObject(GameObject Object, GameObject Actor = null, GameObject Weapon = null, GameObject Projectile = null, string Blueprint = null, string Message = null, string Context = "Transmute", string Sound = "sfx_statusEffect_spacetimeWeirdness", bool Animate = false, bool MakePermanent = false)
		{
			if (!Object.IsReal || Object.IsNowhere())
			{
				return false;
			}
			if (Object.HasTagOrProperty("SuppressTransmutation"))
			{
				return false;
			}
			if (Blueprint.IsNullOrEmpty())
			{
				Blueprint = EncountersAPI.GetATransmutationBlueprintFor(Object);
			}
			if (Blueprint.IsNullOrEmpty() || Blueprint == Object.Blueprint)
			{
				return false;
			}
			if (!GameObjectFactory.Factory.Blueprints.TryGetValue(Blueprint, out var value))
			{
				MetricsManager.LogError("No blueprint by name '" + Blueprint + "' found to transmute into.");
				return false;
			}
			bool flag = Object.IsPlayer();
			if (flag && value.DescendsFrom("Gemstone"))
			{
				Achievement.TRANSMUTED_GEM.Unlock();
			}
			GameObject gameObject = GameObject.Create(value, 0, 0, null, null, null, Context);
			Cell currentCell = Object.CurrentCell;
			currentCell.PlayWorldSound(Sound);
			using (new IntPropertyMod(gameObject, "SuppressTransmutation", 1))
			{
				Object.SplitFromStack();
				string text = (Message.IsNullOrEmpty() ? null : GameText.VariableReplace(Message, Object, gameObject));
				if (Object.Brain != null && gameObject.Brain == null)
				{
					if (Animate)
					{
						AnimateObject.Animate(gameObject);
					}
					else if (!Object.Die(Actor, null, text, null, Accidental: false, Weapon, Projectile))
					{
						return false;
					}
				}
				if (!text.IsNullOrEmpty())
				{
					Object.Physics.EmitMessage(text, 'W');
				}
				if (flag)
				{
					gameObject.RemovePart(typeof(GivesRep));
					gameObject.RequirePart<Inventory>();
					Brain brain = gameObject.RequirePart<Brain>();
					brain.Allegiance.Clear();
					brain.Allegiance["Player"] = 100;
					brain.FactionFeelings.Clear();
					if (!gameObject.HasStat("Energy"))
					{
						try
						{
							Statistic statistic = new Statistic(GameObjectFactory.Factory.Blueprints["Creature"].Stats["Energy"]);
							statistic.Owner = gameObject;
							gameObject.Statistics["Energy"] = statistic;
						}
						catch
						{
						}
					}
				}
				gameObject.StripContents(KeepNatural: true, Silent: true);
				TransmuteBody(Object, gameObject);
				TransmuteInventory(Object, gameObject);
				TransmuteBrain(Object, gameObject);
				TransmuteName(Object, gameObject);
				if (flag)
				{
					gameObject.SystemMoveTo(currentCell, null);
					gameObject.MakeActive();
					if (flag)
					{
						The.Game.Player.Body = gameObject;
					}
				}
				Object.ReplaceWith(gameObject);
				Object.RemoveFromContext();
				if (!flag)
				{
					gameObject.SystemMoveTo(currentCell, null);
					gameObject.MakeActive();
				}
				if (MakePermanent)
				{
					gameObject.RemovePart(typeof(Temporary));
					gameObject.RemovePart(typeof(ExistenceSupport));
				}
				if (currentCell.IsVisible() && Options.UseParticleVFX && !Object.Render.Tile.IsNullOrEmpty() && !gameObject.Render.Tile.IsNullOrEmpty())
				{
					ParticleVFXTransmuted.Play(Object, gameObject);
				}
				return true;
			}
		}

		public static void TransmuteBody(GameObject Object, GameObject Target)
		{
			if (Object.Body == null)
			{
				return;
			}
			foreach (BodyPart item in Object.Body.LoopParts())
			{
				GameObject equipped = item.Equipped;
				if (equipped == null || equipped.IsNatural() || !item.TryUnequip(Silent: true, SemiForced: true))
				{
					continue;
				}
				Target.ReceiveObject(equipped, NoStack: true);
				if (Target.Body != null)
				{
					BodyPart firstPart = Target.Body.GetFirstPart(item.Type, item.Laterality, (BodyPart x) => x.Equipped == null);
					if (firstPart != null)
					{
						Target.EquipObject(equipped, firstPart, Silent: true, 0);
						continue;
					}
				}
				TransferObject(equipped, Object, Target);
			}
		}

		private static void TransferObject(GameObject Item, GameObject Object, GameObject Target)
		{
			if (Target.Inventory == null || !Target.ReceiveObject(Item))
			{
				InventoryActionEvent.Check(Object, Object, Item, "CommandDropObject", Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, Forced: true, Silent: true, 0, 0, 0, null, null, null, Object.GetDropInventory() ?? Target.GetDropInventory() ?? Item.GetDropInventory());
			}
		}

		public static void TransmuteInventory(GameObject Object, GameObject Target)
		{
			if (Object.Inventory == null)
			{
				return;
			}
			foreach (GameObject item in new List<GameObject>(Object.Inventory.Objects))
			{
				TransferObject(item, Object, Target);
			}
		}

		public static void TransmuteBrain(GameObject Object, GameObject Target)
		{
			if (Object.Brain == null || Target.Brain == null)
			{
				return;
			}
			Target.PartyLeader = Object.PartyLeader;
			if (!Object.Brain.PartyMembers.IsNullOrEmpty() && Target.Brain != null)
			{
				foreach (KeyValuePair<int, PartyMember> partyMember in Object.Brain.PartyMembers)
				{
					Target.Brain.PartyMembers[partyMember.Key] = partyMember.Value;
				}
			}
			AllegianceSet allegiance = Object.Brain.Allegiance;
			if (!allegiance.IsNullOrEmpty())
			{
				Object.Brain.Allegiance = Target.Brain.Allegiance;
				Target.Brain.Allegiance = allegiance;
			}
			foreach (GameObject item in Object.CurrentZone.YieldObjects())
			{
				if (item.PartyLeader != Object)
				{
					continue;
				}
				if (item.TryGetEffect<Proselytized>(out var Effect))
				{
					if (!Target.HasPart<Persuasion_Proselytize>())
					{
						continue;
					}
					Effect.Proselytizer = Target;
				}
				if (item.TryGetEffect<Beguiled>(out var Effect2))
				{
					if (!Target.HasPart<Beguiling>())
					{
						continue;
					}
					Effect2.Beguiler = Target;
				}
				item.PartyLeader = Target;
				item.Brain.Goals.Clear();
			}
		}

		public static void TransmuteName(GameObject Object, GameObject Target)
		{
			if (Object.HasProperName && Object.IsCombatObject())
			{
				string displayName = Object.Render.DisplayName;
				GameObjectBlueprint blueprint = Object.GetBlueprint();
				string oldValue = (blueprint.HasProperName() ? Object.GetSpecies() : blueprint.CachedDisplayNameStripped);
				GameObjectBlueprint blueprint2 = Target.GetBlueprint();
				string newValue = (blueprint2.HasProperName() ? Target.GetSpecies() : blueprint2.CachedDisplayNameStripped);
				displayName = displayName.Replace(oldValue, newValue);
				Target.Render.DisplayName = displayName;
				Target.HasProperName = true;
			}
		}

		[WishCommand("transmute", null)]
		public static void WishTransmute()
		{
			WishTransmute(null);
		}

		[WishCommand("transmute", null)]
		public static void WishTransmute(string Blueprint)
		{
			TransmuteObject(The.Player, null, null, null, Blueprint, "=subject.T= =verb:were= transmuted into =object.an=.");
		}
	}
}
