using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class ExamineFailureEvent : IExamineEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(ExamineFailureEvent), null, CountPool, ResetPool);

		private static List<ExamineFailureEvent> Pool;

		private static int PoolCounter;

		public static readonly int PASSES = 2;

		public ExamineFailureEvent()
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

		public static void ResetTo(ref ExamineFailureEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static ExamineFailureEvent FromPool()
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

		public static bool Check(GameObject Actor, GameObject Item, bool ConfusionBased = false)
		{
			bool flag = true;
			ExamineFailureEvent examineFailureEvent = null;
			Event @event = null;
			Actor.HasRegisteredEvent("ExamineFailure");
			Item.HasRegisteredEvent("ExamineFailure");
			bool flag2 = Actor.WantEvent(ID, MinEvent.CascadeLevel);
			bool flag3 = Item.WantEvent(ID, MinEvent.CascadeLevel);
			for (int i = 1; i <= PASSES; i++)
			{
				if (flag)
				{
					if (examineFailureEvent == null)
					{
						examineFailureEvent = FromPool();
						examineFailureEvent.Actor = Actor;
						examineFailureEvent.Item = Item;
						examineFailureEvent.Setup();
					}
					if (@event == null)
					{
						@event = Event.New("ExamineFailure");
					}
					@event.SetParameter("Actor", Actor);
					@event.SetParameter("Item", Item);
					@event.SetFlag("Identify", examineFailureEvent.Identify);
					@event.SetFlag("IdentifyIfDestroyed", examineFailureEvent.IdentifyIfDestroyed);
					@event.SetFlag("ConfusionBased", ConfusionBased);
					flag = Actor.FireEvent(@event) && Item.FireEvent(@event);
					examineFailureEvent.Identify = @event.HasFlag("Identify");
					examineFailureEvent.IdentifyIfDestroyed = @event.HasFlag("IdentifyIfDestroyed");
				}
				if (flag && (flag2 || flag3))
				{
					if (examineFailureEvent == null)
					{
						examineFailureEvent = FromPool();
						examineFailureEvent.Actor = Actor;
						examineFailureEvent.Item = Item;
						examineFailureEvent.Setup();
					}
					examineFailureEvent.ConfusionBased = ConfusionBased;
					flag = (!flag2 || Actor.HandleEvent(examineFailureEvent)) && (!flag3 || Item.HandleEvent(examineFailureEvent));
				}
			}
			examineFailureEvent?.ProcessIdentify();
			return flag;
		}
	}
}
