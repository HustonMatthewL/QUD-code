using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AIGetOffensiveAbilityListEvent : IAICommandListEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(AIGetOffensiveAbilityListEvent), null, CountPool, ResetPool);

		private static List<AIGetOffensiveAbilityListEvent> Pool;

		private static int PoolCounter;

		public AIGetOffensiveAbilityListEvent()
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

		public static void ResetTo(ref AIGetOffensiveAbilityListEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static AIGetOffensiveAbilityListEvent FromPool()
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

		public static List<AICommandList> GetFor(GameObject Actor, GameObject Target = null, int Distance = -1)
		{
			AIGetOffensiveAbilityListEvent aIGetOffensiveAbilityListEvent = FromPool();
			if (Target == null)
			{
				Target = Actor?.Target;
			}
			if (Distance == -1 && GameObject.Validate(ref Actor) && GameObject.Validate(ref Target))
			{
				Distance = Actor.DistanceTo(Target);
			}
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetOffensiveAbilityList"))
			{
				Event @event = Event.New("AIGetOffensiveAbilityList");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("Distance", Distance);
				@event.SetParameter("List", aIGetOffensiveAbilityListEvent.List);
				flag = Actor.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetOffensiveMutationList"))
			{
				Event event2 = Event.New("AIGetOffensiveMutationList");
				event2.SetParameter("User", Actor);
				event2.SetParameter("Target", Target);
				event2.SetParameter("Distance", Distance);
				event2.SetParameter("List", aIGetOffensiveAbilityListEvent.List);
				flag = Actor.FireEvent(event2);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, IAICommandListEvent.CascadeLevel))
			{
				aIGetOffensiveAbilityListEvent.Actor = Actor;
				aIGetOffensiveAbilityListEvent.Target = Target;
				aIGetOffensiveAbilityListEvent.Distance = Distance;
				flag = Actor.HandleEvent(aIGetOffensiveAbilityListEvent);
			}
			return aIGetOffensiveAbilityListEvent.List;
		}
	}
}
