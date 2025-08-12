namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class CanStartConversationEvent : PooledEvent<CanStartConversationEvent>
	{
		public GameObject Actor;

		public GameObject Object;

		public bool Physical;

		public bool Mental;

		public string FailureMessage;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Actor = null;
			Object = null;
			Physical = false;
			Mental = false;
			FailureMessage = null;
		}

		public static CanStartConversationEvent FromPool(GameObject Actor, GameObject Object, bool Physical = false, bool Mental = false)
		{
			CanStartConversationEvent canStartConversationEvent = PooledEvent<CanStartConversationEvent>.FromPool();
			canStartConversationEvent.Actor = Actor;
			canStartConversationEvent.Object = Object;
			canStartConversationEvent.Physical = Physical;
			canStartConversationEvent.Mental = Mental;
			canStartConversationEvent.FailureMessage = null;
			return canStartConversationEvent;
		}

		public static bool Check(GameObject Actor, GameObject Object, out string FailureMessage, bool Physical = false, bool Mental = false)
		{
			FailureMessage = null;
			if (Actor.HasRegisteredEvent("CanStartConversation") || Object.HasRegisteredEvent("CanStartConversation"))
			{
				Event @event = Event.New("CanStartConversation");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Object", Object);
				@event.SetFlag("Physical", Physical);
				@event.SetFlag("Mental", Mental);
				try
				{
					if (!Actor.FireEvent(@event))
					{
						return false;
					}
					if (!Object.FireEvent(@event))
					{
						return false;
					}
				}
				finally
				{
					FailureMessage = @event.GetStringParameter("FailureMessage");
				}
			}
			bool flag = Actor.WantEvent(PooledEvent<CanStartConversationEvent>.ID, MinEvent.CascadeLevel);
			bool flag2 = Object.WantEvent(PooledEvent<CanStartConversationEvent>.ID, MinEvent.CascadeLevel);
			if (flag || flag2)
			{
				CanStartConversationEvent canStartConversationEvent = FromPool(Actor, Object, Physical, Mental);
				try
				{
					if (flag && !Actor.HandleEvent(canStartConversationEvent))
					{
						return false;
					}
					if (flag2 && !Object.HandleEvent(canStartConversationEvent))
					{
						return false;
					}
				}
				finally
				{
					FailureMessage = canStartConversationEvent.FailureMessage;
				}
			}
			return true;
		}
	}
}
