using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AIGetRetreatAbilityListEvent : IAIMoveCommandListEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(AIGetRetreatAbilityListEvent), null, CountPool, ResetPool);

		private static List<AIGetRetreatAbilityListEvent> Pool;

		private static int PoolCounter;

		public Cell AvoidCell;

		public AIGetRetreatAbilityListEvent()
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

		public static void ResetTo(ref AIGetRetreatAbilityListEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static AIGetRetreatAbilityListEvent FromPool()
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

		public override void Reset()
		{
			base.Reset();
			AvoidCell = null;
		}

		public static List<AICommandList> GetFor(GameObject Actor, GameObject Target = null, Cell TargetCell = null, Cell AvoidCell = null, int Distance = -1, int StandoffDistance = 0)
		{
			AIGetRetreatAbilityListEvent aIGetRetreatAbilityListEvent = FromPool();
			if (Distance == -1)
			{
				Distance = ((GameObject.Validate(ref Actor) && TargetCell != null) ? Actor.DistanceTo(TargetCell) : 0);
			}
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetRetreatAbilityList"))
			{
				Event @event = Event.New("AIGetRetreatAbilityList");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("TargetCell", TargetCell);
				@event.SetParameter("AvoidCell", AvoidCell);
				@event.SetParameter("Distance", Distance);
				@event.SetParameter("StandoffDistance", StandoffDistance);
				@event.SetParameter("List", aIGetRetreatAbilityListEvent.List);
				flag = Actor.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, IAICommandListEvent.CascadeLevel))
			{
				aIGetRetreatAbilityListEvent.Actor = Actor;
				aIGetRetreatAbilityListEvent.Target = Target;
				aIGetRetreatAbilityListEvent.TargetCell = TargetCell;
				aIGetRetreatAbilityListEvent.AvoidCell = AvoidCell;
				aIGetRetreatAbilityListEvent.Distance = Distance;
				aIGetRetreatAbilityListEvent.StandoffDistance = StandoffDistance;
				flag = Actor.HandleEvent(aIGetRetreatAbilityListEvent);
			}
			return aIGetRetreatAbilityListEvent.List;
		}
	}
}
