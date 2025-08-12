using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AfterExamineFailureEvent : IExamineEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterExamineFailureEvent), null, CountPool, ResetPool);

		private static List<AfterExamineFailureEvent> Pool;

		private static int PoolCounter;

		public AfterExamineFailureEvent()
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

		public static void ResetTo(ref AfterExamineFailureEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static AfterExamineFailureEvent FromPool()
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

		public static void Send(GameObject Actor, GameObject Item, bool ConfusionBased = false)
		{
			bool flag = true;
			AfterExamineFailureEvent afterExamineFailureEvent = null;
			if (flag && (Actor.HasRegisteredEvent("AfterExamineFailure") || Item.HasRegisteredEvent("AfterExamineFailure")))
			{
				if (afterExamineFailureEvent == null)
				{
					afterExamineFailureEvent = FromPool();
					afterExamineFailureEvent.Actor = Actor;
					afterExamineFailureEvent.Item = Item;
					afterExamineFailureEvent.Setup();
				}
				Event @event = Event.New("AfterExamineFailure");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Item", Item);
				@event.SetFlag("Identify", afterExamineFailureEvent.Identify);
				@event.SetFlag("IdentifyIfDestroyed", afterExamineFailureEvent.IdentifyIfDestroyed);
				@event.SetFlag("ConfusionBased", ConfusionBased);
				flag = Actor.FireEvent(@event) && Item.FireEvent(@event);
				afterExamineFailureEvent.Identify = @event.HasFlag("Identify");
				afterExamineFailureEvent.IdentifyIfDestroyed = @event.HasFlag("IdentifyIfDestroyed");
			}
			if (flag)
			{
				bool flag2 = Actor.WantEvent(ID, MinEvent.CascadeLevel);
				bool flag3 = Item.WantEvent(ID, MinEvent.CascadeLevel);
				if (flag2 || flag3)
				{
					if (afterExamineFailureEvent == null)
					{
						afterExamineFailureEvent = FromPool();
						afterExamineFailureEvent.Actor = Actor;
						afterExamineFailureEvent.Item = Item;
						afterExamineFailureEvent.Setup();
					}
					afterExamineFailureEvent.ConfusionBased = ConfusionBased;
					flag = (!flag2 || Actor.HandleEvent(afterExamineFailureEvent)) && (!flag3 || Item.HandleEvent(afterExamineFailureEvent));
				}
			}
			afterExamineFailureEvent?.ProcessIdentify();
		}
	}
}
