using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AfterExamineCriticalFailureEvent : IExamineEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterExamineCriticalFailureEvent), null, CountPool, ResetPool);

		private static List<AfterExamineCriticalFailureEvent> Pool;

		private static int PoolCounter;

		public AfterExamineCriticalFailureEvent()
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

		public static void ResetTo(ref AfterExamineCriticalFailureEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static AfterExamineCriticalFailureEvent FromPool()
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

		public static void Send(GameObject Actor, GameObject Item)
		{
			bool flag = true;
			AfterExamineCriticalFailureEvent afterExamineCriticalFailureEvent = null;
			if (flag && (Actor.HasRegisteredEvent("AfterExamineCriticalFailure") || Item.HasRegisteredEvent("AfterExamineCriticalFailure")))
			{
				if (afterExamineCriticalFailureEvent == null)
				{
					afterExamineCriticalFailureEvent = FromPool();
					afterExamineCriticalFailureEvent.Actor = Actor;
					afterExamineCriticalFailureEvent.Item = Item;
					afterExamineCriticalFailureEvent.Setup();
				}
				Event @event = Event.New("AfterExamineCriticalFailure");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Item", Item);
				@event.SetFlag("Identify", afterExamineCriticalFailureEvent.Identify);
				@event.SetFlag("IdentifyIfDestroyed", afterExamineCriticalFailureEvent.IdentifyIfDestroyed);
				flag = Actor.FireEvent(@event) && Item.FireEvent(@event);
				afterExamineCriticalFailureEvent.Identify = @event.HasFlag("Identify");
				afterExamineCriticalFailureEvent.IdentifyIfDestroyed = @event.HasFlag("IdentifyIfDestroyed");
			}
			if (flag)
			{
				bool flag2 = Actor.WantEvent(ID, MinEvent.CascadeLevel);
				bool flag3 = Item.WantEvent(ID, MinEvent.CascadeLevel);
				if (flag2 || flag3)
				{
					if (afterExamineCriticalFailureEvent == null)
					{
						afterExamineCriticalFailureEvent = FromPool();
						afterExamineCriticalFailureEvent.Actor = Actor;
						afterExamineCriticalFailureEvent.Item = Item;
						afterExamineCriticalFailureEvent.Setup();
					}
					flag = (!flag2 || Actor.HandleEvent(afterExamineCriticalFailureEvent)) && (!flag3 || Item.HandleEvent(afterExamineCriticalFailureEvent));
				}
			}
			afterExamineCriticalFailureEvent?.ProcessIdentify();
		}
	}
}
