namespace XRL.World
{
	[GameEvent(Cascade = 15, Cache = Cache.Pool)]
	public class BodyPositionChangedEvent : PooledEvent<BodyPositionChangedEvent>
	{
		public new static readonly int CascadeLevel = 15;

		public GameObject Object;

		public string To;

		public bool Involuntary;

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
			Object = null;
			To = null;
			Involuntary = false;
		}

		public static BodyPositionChangedEvent FromPool(GameObject Object, string To = null, bool Involuntary = false)
		{
			BodyPositionChangedEvent bodyPositionChangedEvent = PooledEvent<BodyPositionChangedEvent>.FromPool();
			bodyPositionChangedEvent.Object = Object;
			bodyPositionChangedEvent.To = To;
			bodyPositionChangedEvent.Involuntary = Involuntary;
			return bodyPositionChangedEvent;
		}

		public static void Send(GameObject Object, string To = null, bool Involuntary = false)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BodyPositionChanged"))
			{
				Event @event = Event.New("BodyPositionChanged");
				@event.SetParameter("Object", Object);
				@event.SetParameter("To", To);
				@event.SetFlag("Involuntary", Involuntary);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<BodyPositionChangedEvent>.ID, CascadeLevel))
			{
				flag = Object.HandleEvent(FromPool(Object, To, Involuntary));
			}
		}
	}
}
