using System;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class MakeFussOnTaken : IPart
	{
		public string Action = "found";

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
			{
				return ID == TakenEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(EquippedEvent E)
		{
			MakeFuss(E.Actor);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(TakenEvent E)
		{
			MakeFuss(E.Actor);
			return base.HandleEvent(E);
		}

		public void MakeFuss(GameObject Actor)
		{
			if (Actor == null || !Actor.IsPlayer())
			{
				return;
			}
			if (ParentObject.Understood())
			{
				CompleteQuestOnTaken part = ParentObject.GetPart<CompleteQuestOnTaken>();
				if (part == null || !The.Game.HasQuest(part.Quest) || The.Game.FinishedQuest(part.Quest))
				{
					Popup.Show("You have " + Action + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + "!");
				}
			}
			ParentObject.RemovePart(this);
		}
	}
}
