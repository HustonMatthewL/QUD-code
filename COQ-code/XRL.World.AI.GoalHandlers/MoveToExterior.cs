using System;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers
{
	[Serializable]
	public class MoveToExterior : IMovementGoal
	{
		[NonSerialized]
		public InteriorPortal Part;

		[NonSerialized]
		public InteriorZone Interior;

		public MoveToExterior()
		{
		}

		public MoveToExterior(GameObject Target)
			: this()
		{
			Part = Target.GetPart<InteriorPortal>();
		}

		public override void Create()
		{
			Interior = base.CurrentZone as InteriorZone;
		}

		public override bool Finished()
		{
			if (Interior != null)
			{
				return base.CurrentZone != Interior;
			}
			return true;
		}

		public bool IsValidExit(GameObject Object)
		{
			return Object.HasPart(typeof(InteriorPortal));
		}

		public override void TakeAction()
		{
			if (Part == null || !GameObject.Validate(Part.ParentObject))
			{
				Part = base.CurrentZone.FindClosestObject(base.ParentObject, IsValidExit)?.GetPart<InteriorPortal>();
				if (Part == null)
				{
					FailToParent();
					return;
				}
			}
			if (base.ParentObject.DistanceTo(Part.ParentObject) > 1)
			{
				PushChildGoal(new MoveTo(Part.ParentObject, careful: false, overridesCombat: false, 1));
			}
			else if (!Part.TryExit(base.ParentObject))
			{
				FailToParent();
			}
		}
	}
}
