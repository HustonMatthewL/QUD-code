using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts
{
	[Serializable]
	public class Garbage : IPart
	{
		public static readonly string RIFLE_INTERACTION = "RifleThroughGarbage";

		public int? Depth;

		public int Level;

		public override bool SameAs(IPart Part)
		{
			Garbage garbage = Part as Garbage;
			if (garbage.Depth != Depth)
			{
				return false;
			}
			if (garbage.Level != Level)
			{
				return false;
			}
			return base.SameAs(Part);
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && (ID != EnteredCellEvent.ID || Depth.HasValue) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
			{
				return ID == ObjectEnteringCellEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetInventoryActionsEvent E)
		{
			if (IComponent<GameObject>.ThePlayer.HasPart<TrashRifling>())
			{
				E.AddAction("Rifle", "rifle", RIFLE_INTERACTION, null, 'r', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == RIFLE_INTERACTION && AttemptRifle(E.Actor, E.Auto, E.FromCell, E.Generated))
			{
				E.RequestInterfaceExit();
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EnteredCellEvent E)
		{
			int valueOrDefault = Depth.GetValueOrDefault();
			if (!Depth.HasValue)
			{
				valueOrDefault = ParentObject.CurrentCell.ParentZone.GetZoneZ();
				Depth = valueOrDefault;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(ObjectEnteringCellEvent E)
		{
			if (E.Type != "Charge")
			{
				AttemptRifle(E.Object, Automatic: true, E.Cell);
			}
			return base.HandleEvent(E);
		}

		public bool AttemptRifle(GameObject Actor, bool Automatic = false, Cell FromCell = null, List<GameObject> Tracking = null)
		{
			TrashRifling part = Actor.GetPart<TrashRifling>();
			if (part == null || ((Automatic || !Actor.IsPlayer()) && !part.IsActive()))
			{
				return false;
			}
			Customs_TrashDivining part2 = Actor.GetPart<Customs_TrashDivining>();
			Tinkering_Scavenger part3 = Actor.GetPart<Tinkering_Scavenger>();
			Cell cell = ParentObject.GetCurrentCell();
			GameObject gameObject = null;
			bool result = false;
			if (Automatic)
			{
				if (ParentObject.IsImportant())
				{
					return false;
				}
				if (Actor.IsPlayer())
				{
					if (Actor.ArePerceptibleHostilesNearby(logSpot: false, popSpot: false, null, null, null, Options.AutoexploreIgnoreEasyEnemies, Options.AutoexploreIgnoreDistantEnemies))
					{
						return false;
					}
				}
				else if (Actor.Target != null)
				{
					return false;
				}
			}
			if (!Actor.CheckFrozen(Telepathic: false, Telekinetic: true, Silent: true))
			{
				if (!Automatic && Actor.IsPlayer())
				{
					Actor.CheckFrozen();
				}
				return false;
			}
			Cell cell2 = FromCell ?? Actor.GetCurrentCell();
			string text = null;
			if (cell != null && cell != cell2)
			{
				text = ((!Actor.IsPlayer() && !IComponent<GameObject>.Visible(Actor)) ? ("to their " + Directions.GetExpandedDirection(cell2.GetDirectionFromCell(cell))) : Directions.GetDirectionDescription(Actor, cell2.GetDirectionFromCell(cell)));
			}
			ParentObject.PlayWorldSound("Sounds/Interact/sfx_interact_garbage_rifle");
			bool flag = true;
			if (part2 != null)
			{
				if (gameObject == null)
				{
					gameObject = ParentObject.RemoveOne();
				}
				if ((Actor.IsPlayer() || (Actor.IsPlayerLed() && The.Player.HasSkill("Customs_TrashDivining"))) && GetSkillEffectChanceEvent.GetFor(Actor, gameObject, part2, 5).in100())
				{
					string text2 = null;
					IBaseJournalEntry randomUnrevealedNote = JournalAPI.GetRandomUnrevealedNote();
					if (randomUnrevealedNote is JournalMapNote)
					{
						text2 = "Rifling through " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ((text == null) ? "" : (" " + text)) + ", " + Actor.does("piece", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " together clues and" + Actor.GetVerb("determine") + " the location of:\n\n";
						flag = false;
					}
					else
					{
						text2 = "Rifling through " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ((text == null) ? "" : (" " + text)) + ", " + Actor.does("piece", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " together clues and" + Actor.GetVerb("arrive") + " at the following conclusion:\n\n";
						flag = false;
					}
					text2 += randomUnrevealedNote.Text;
					EmitMessage(text2, ' ', FromDialog: false, Options.PopupJournalNote);
					randomUnrevealedNote.Reveal(ParentObject.DisplayName);
				}
			}
			if (part3 != null)
			{
				if (gameObject == null)
				{
					gameObject = ParentObject.RemoveOne();
				}
				int num = Stat.Random(1, 100);
				int num2 = Tier.Constrain((Depth.Value - 10) / 4 + Level);
				if (num > 75)
				{
					if (num <= 99)
					{
						GameObject gameObject2 = GameObjectFactory.create(PopulationManager.RollOneFrom("Scrap " + num2).Blueprint);
						cell.AddObject(gameObject2);
						if (Actor.IsPlayer())
						{
							IComponent<GameObject>.EmitMessage(Actor, "You rifle through " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ((text == null) ? "" : (" " + text)) + ", and find " + gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".", ' ', !Automatic, !Options.ShowScavengeItemAsMessage);
						}
						result = true;
						flag = false;
					}
					else
					{
						GameObject gameObject3 = GameObjectFactory.create(PopulationManager.RollOneFrom("Junk " + num2).Blueprint);
						cell.AddObject(gameObject3);
						if (Actor.IsPlayer())
						{
							EmitMessage("You rifle through " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ((text == null) ? "" : (" " + text)) + ", and find " + gameObject3.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".", ' ', !Automatic, !Options.ShowScavengeItemAsMessage);
						}
						result = true;
						flag = false;
					}
				}
			}
			if (gameObject != null)
			{
				if (Actor.IsPlayer())
				{
					if (flag)
					{
						IComponent<GameObject>.EmitMessage(Actor, "{{K|You rifle through " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ((text == null) ? "" : (" " + text)) + ", but you find nothing.}}", ' ', !Automatic);
					}
				}
				else if (IComponent<GameObject>.Visible(Actor))
				{
					IComponent<GameObject>.AddPlayerMessage(Actor.Does("rifle", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " through " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ((text == null) ? "" : (" " + text)) + ".");
				}
				else if (IComponent<GameObject>.Visible(gameObject))
				{
					IComponent<GameObject>.AddPlayerMessage("Somebody rifles through " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ((text == null) ? "" : (" " + text)) + ".");
				}
				if (IComponent<GameObject>.Visible(gameObject))
				{
					gameObject.DustPuff();
				}
				gameObject.Destroy();
				Actor.UseEnergy(1000, "Skill", null, null);
			}
			return result;
		}
	}
}
