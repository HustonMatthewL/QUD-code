using System;

namespace XRL.World.AI.GoalHandlers
{
	[Serializable]
	public class Land : IMovementGoal
	{
		public bool overridesCombat;

		public Land(bool overridesCombat = false)
		{
			this.overridesCombat = overridesCombat;
		}

		public override bool Finished()
		{
			return false;
		}

		public override bool CanFight()
		{
			return !overridesCombat;
		}

		public override void TakeAction()
		{
			if (!base.ParentObject.IsFlying)
			{
				Pop();
			}
			else if (AttemptToLandEvent.Check(base.ParentObject) && !base.ParentObject.IsFlying)
			{
				Pop();
			}
		}
	}
}
