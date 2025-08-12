using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AIGetMovementAbilityListEvent : IAIMoveCommandListEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(AIGetMovementAbilityListEvent), null, CountPool, ResetPool);

		private static List<AIGetMovementAbilityListEvent> Pool;

		private static int PoolCounter;

		public AIGetMovementAbilityListEvent()
		{
			base.ID = ID;
		}

		public static int CountPool()
		{
			if (Pool != null)
			{
				return Pool.Count;
			}
			return 0;
		}

		public static void ResetPool()
		{
			while (PoolCounter > 0)
			{
				Pool[--PoolCounter].Reset();
			}
		}

		public static void ResetTo(ref AIGetMovementAbilityListEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static AIGetMovementAbilityListEvent FromPool()
		{
			return MinEvent.FromPool(ref Pool, ref PoolCounter);
		}

		public override bool Dispatch(IEventHandler Handler)
		{
			if (!base.Dispatch(Handler))
			{
				return false;
			}
			return Handler.HandleEvent(this);
		}

		public static List<AICommandList> GetFor(GameObject Actor, GameObject Target = null, Cell TargetCell = null, int Distance = -1, int StandoffDistance = 0)
		{
			AIGetMovementAbilityListEvent aIGetMovementAbilityListEvent = FromPool();
			if (TargetCell == null)
			{
				TargetCell = Target?.CurrentCell;
			}
			if (Distance == -1)
			{
				Distance = ((GameObject.Validate(ref Actor) && TargetCell != null) ? Actor.DistanceTo(TargetCell) : 0);
			}
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetMovementAbilityList"))
			{
				Event @event = Event.New("AIGetMovementAbilityList");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("TargetCell", TargetCell);
				@event.SetParameter("Distance", Distance);
				@event.SetParameter("StandoffDistance", StandoffDistance);
				@event.SetParameter("List", aIGetMovementAbilityListEvent.List);
				flag = Actor.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetMovementMutationList"))
			{
				Event event2 = Event.New("AIGetMovementMutationList");
				event2.SetParameter("User", Actor);
				event2.SetParameter("Target", Target);
				event2.SetParameter("TargetCell", TargetCell);
				event2.SetParameter("Distance", Distance);
				event2.SetParameter("StandoffDistance", StandoffDistance);
				event2.SetParameter("List", aIGetMovementAbilityListEvent.List);
				flag = Actor.FireEvent(event2);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, IAICommandListEvent.CascadeLevel))
			{
				aIGetMovementAbilityListEvent.Actor = Actor;
				aIGetMovementAbilityListEvent.Target = Target;
				aIGetMovementAbilityListEvent.TargetCell = TargetCell;
				aIGetMovementAbilityListEvent.Distance = Distance;
				aIGetMovementAbilityListEvent.StandoffDistance = StandoffDistance;
				flag = Actor.HandleEvent(aIGetMovementAbilityListEvent);
			}
			return aIGetMovementAbilityListEvent.List;
		}
	}
}
