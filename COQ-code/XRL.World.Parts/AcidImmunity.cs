using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class AcidImmunity : IPart
	{
		public override bool SameAs(IPart p)
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == BeforeApplyDamageEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(BeforeApplyDamageEvent E)
		{
			if (E.Object == ParentObject && E.Damage.IsAcidDamage())
			{
				NotifyTargetImmuneEvent.Send(E.Weapon, E.Object, E.Actor, E.Damage, this);
				E.Damage.Amount = 0;
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}
	}
}
