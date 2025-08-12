using System;
using XRL.UI;
using XRL.World.ZoneParts;

namespace XRL.World.Parts
{
	[Serializable]
	public class MapReveal : IPart
	{
		public string Duration = "50";

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
			{
				return ID == InventoryActionEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetInventoryActionsEvent E)
		{
			E.AddAction("Read", "read", "ActivateMapReveal", null, 'r');
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "ActivateMapReveal")
			{
				if (!E.Actor.IsPlayer())
				{
					return false;
				}
				if (!ParentObject.Owner.IsNullOrEmpty())
				{
					if (Popup.ShowYesNoCancel(ParentObject.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " not owned by you, and using " + ParentObject.them + " will consume " + ParentObject.them + ". Are you sure you want to do so?") != 0)
					{
						return false;
					}
				}
				else if (!(ParentObject.InInventory?.Owner).IsNullOrEmpty() && Popup.ShowYesNoCancel(ParentObject.InInventory.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " not owned by you, and using " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " will consume " + ParentObject.them + ". Are you sure you want to do so?") != 0)
				{
					return false;
				}
				if (E.Item.IsTemporary || !IComponent<GameObject>.CheckRealityDistortionUsability(E.Actor, null, E.Actor, ParentObject, null, null))
				{
					Popup.Show(ParentObject.Does("seem", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " to be behaving as nothing more than an ordinary piece of paper.");
					E.Actor.UseEnergy(1000, "Item Failure", null, null);
					E.RequestInterfaceExit();
					return true;
				}
				Popup.Show(ParentObject.Itis + " a map of your surroundings!");
				int num = Duration.RollCached();
				AmbientOmniscience ambientOmniscience = E.Actor.CurrentZone.RequirePart<AmbientOmniscience>();
				ambientOmniscience.IsRealityDistortionBased = true;
				if (num > 0)
				{
					ambientOmniscience.Duration = num;
				}
				ParentObject.Destroy();
				E.RequestInterfaceExit();
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetItemElementsEvent E)
		{
			if (E.IsRelevantObject(ParentObject))
			{
				E.Add("travel", 10);
				E.Add("scholarship", 4);
				E.Add("time", 2);
			}
			return base.HandleEvent(E);
		}
	}
}
