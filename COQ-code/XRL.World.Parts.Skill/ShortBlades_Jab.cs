using System;

namespace XRL.World.Parts.Skill
{
	[Serializable]
	public class ShortBlades_Jab : BaseSkill
	{
		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == PooledEvent<GetMeleeAttackChanceEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetMeleeAttackChanceEvent E)
		{
			if (!E.Primary && E.Weapon?.GetWeaponSkill() == "ShortBlades" && !E.Properties.HasDelimitedSubstring(',', "Flurrying"))
			{
				E.Attempts++;
			}
			return base.HandleEvent(E);
		}
	}
}
