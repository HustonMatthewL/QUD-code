namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class BeingConsumedEvent : PooledEvent<BeingConsumedEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Actor;

		public GameObject Object;

		public override int GetCascadeLevel()
		{
			return CascadeLevel;
		}

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Actor = null;
			Object = null;
		}

		public static void Send(GameObject Actor, GameObject Object)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BeingConsumed"))
			{
				Event @event = Event.New("BeingConsumed");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Object", Object);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<BeingConsumedEvent>.ID, CascadeLevel))
			{
				BeingConsumedEvent beingConsumedEvent = PooledEvent<BeingConsumedEvent>.FromPool();
				beingConsumedEvent.Actor = Actor;
				beingConsumedEvent.Object = Object;
				flag = Object.HandleEvent(beingConsumedEvent);
			}
		}
	}
}
