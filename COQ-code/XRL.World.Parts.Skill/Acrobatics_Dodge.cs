using System;

namespace XRL.World.Parts.Skill
{
	[Serializable]
	public class Acrobatics_Dodge : BaseSkill
	{
		public override int Priority => int.MinValue;

		public override bool AddSkill(GameObject GO)
		{
			base.StatShifter.SetStatShift("DV", 2);
			return base.AddSkill(GO);
		}

		public override bool RemoveSkill(GameObject GO)
		{
			base.StatShifter.RemoveStatShifts();
			return base.RemoveSkill(GO);
		}
	}
}
