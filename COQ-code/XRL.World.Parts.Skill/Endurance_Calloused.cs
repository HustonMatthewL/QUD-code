using System;

namespace XRL.World.Parts.Skill
{
	[Serializable]
	public class Endurance_Calloused : BaseSkill
	{
		public override bool AddSkill(GameObject GO)
		{
			base.StatShifter.SetStatShift("AV", 1, baseValue: true);
			return base.AddSkill(GO);
		}

		public override bool RemoveSkill(GameObject GO)
		{
			base.StatShifter.RemoveStatShifts();
			return base.RemoveSkill(GO);
		}
	}
}
