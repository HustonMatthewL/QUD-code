using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class ExamineCriticalFailureEvent : IExamineEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(ExamineCriticalFailureEvent), null, CountPool, ResetPool);

		private static List<ExamineCriticalFailureEvent> Pool;

		private static int PoolCounter;

		public static readonly int PASSES = 2;

		public ExamineCriticalFailureEvent()
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

		public static void ResetTo(ref ExamineCriticalFailureEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static ExamineCriticalFailureEvent FromPool()
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

		public static bool Check(GameObject Actor, GameObject Item)
		{
			bool flag = true;
			ExamineCriticalFailureEvent examineCriticalFailureEvent = null;
			Event @event = null;
			bool flag2 = Actor.HasRegisteredEvent("ExamineCriticalFailure");
			bool flag3 = Item.HasRegisteredEvent("ExamineCriticalFailure");
			bool flag4 = Actor.WantEvent(ID, MinEvent.CascadeLevel);
			bool flag5 = Item.WantEvent(ID, MinEvent.CascadeLevel);
			for (int i = 1; i <= PASSES; i++)
			{
				if (flag && (flag2 || flag3))
				{
					if (examineCriticalFailureEvent == null)
					{
						examineCriticalFailureEvent = FromPool();
						examineCriticalFailureEvent.Actor = Actor;
						examineCriticalFailureEvent.Item = Item;
						examineCriticalFailureEvent.Setup();
					}
					if (@event == null)
					{
						@event = Event.New("ExamineCriticalFailure");
					}
					@event.SetParameter("Actor", Actor);
					@event.SetParameter("Item", Item);
					@event.SetParameter("Pass", i);
					@event.SetFlag("Identify", examineCriticalFailureEvent.Identify);
					@event.SetFlag("IdentifyIfDestroyed", examineCriticalFailureEvent.IdentifyIfDestroyed);
					flag = Actor.FireEvent(@event) && Item.FireEvent(@event);
					examineCriticalFailureEvent.Identify = @event.HasFlag("Identify");
					examineCriticalFailureEvent.IdentifyIfDestroyed = @event.HasFlag("IdentifyIfDestroyed");
				}
				if (flag && (flag4 || flag5))
				{
					if (examineCriticalFailureEvent == null)
					{
						examineCriticalFailureEvent = FromPool();
						examineCriticalFailureEvent.Actor = Actor;
						examineCriticalFailureEvent.Item = Item;
						examineCriticalFailureEvent.Setup();
					}
					examineCriticalFailureEvent.Pass = i;
					flag = (!flag4 || Actor.HandleEvent(examineCriticalFailureEvent)) && (!flag5 || Item.HandleEvent(examineCriticalFailureEvent));
				}
			}
			examineCriticalFailureEvent?.ProcessIdentify();
			return flag;
		}
	}
}
