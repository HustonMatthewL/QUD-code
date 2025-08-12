using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AIGetPassiveAbilityListEvent : IAICommandListEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(AIGetPassiveAbilityListEvent), null, CountPool, ResetPool);

		private static List<AIGetPassiveAbilityListEvent> Pool;

		private static int PoolCounter;

		public AIGetPassiveAbilityListEvent()
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

		public static void ResetTo(ref AIGetPassiveAbilityListEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static AIGetPassiveAbilityListEvent FromPool()
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
			AIGetPassiveAbilityListEvent aIGetPassiveAbilityListEvent = FromPool();
			if (Target == null)
			{
				Target = Actor?.Target;
			}
			if (Distance == -1 && GameObject.Validate(ref Actor) && GameObject.Validate(ref Target))
			{
				Distance = Actor.DistanceTo(Target);
			}
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetPassiveAbilityList"))
			{
				Event @event = Event.New("AIGetPassiveAbilityList");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("Distance", Distance);
				@event.SetParameter("List", aIGetPassiveAbilityListEvent.List);
				flag = Actor.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetPassiveMutationList"))
			{
				Event event2 = Event.New("AIGetPassiveMutationList");
				event2.SetParameter("User", Actor);
				event2.SetParameter("Target", Target);
				event2.SetParameter("Distance", Distance);
				event2.SetParameter("List", aIGetPassiveAbilityListEvent.List);
				flag = Actor.FireEvent(event2);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, IAICommandListEvent.CascadeLevel))
			{
				aIGetPassiveAbilityListEvent.Actor = Actor;
				aIGetPassiveAbilityListEvent.Target = Target;
				aIGetPassiveAbilityListEvent.Distance = Distance;
				flag = Actor.HandleEvent(aIGetPassiveAbilityListEvent);
			}
			return aIGetPassiveAbilityListEvent.List;
		}
	}
}
