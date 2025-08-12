using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class NaturalEquipment : IPart
	{
		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeginBeingUnequippedEvent>.ID && ID != PooledEvent<CanBeUnequippedEvent>.ID)
			{
				return ID == PooledEvent<GetSlotsRequiredEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(CanBeUnequippedEvent E)
		{
			if (!E.Forced)
			{
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeginBeingUnequippedEvent E)
		{
			if (!E.Forced && ParentObject.Equipped != null)
			{
				E.AddFailureMessage("You can't remove " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetSlotsRequiredEvent E)
		{
			if (E.Object == ParentObject)
			{
				E.AllowReduction = false;
			}
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}
	}
}
