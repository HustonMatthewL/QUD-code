namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetNotReadyToFireMessageEvent : PooledEvent<GetNotReadyToFireMessageEvent>
	{
		public GameObject Object;

		public string Message;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Message = null;
		}

		public static string GetFor(GameObject Object)
		{
			bool flag = true;
			string text = null;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetNotReadyToFireMessage"))
			{
				Event @event = Event.New("GetNotReadyToFireMessage");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Message", text);
				flag = Object.FireEvent(@event);
				text = @event.GetStringParameter("Message");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetNotReadyToFireMessageEvent>.ID, MinEvent.CascadeLevel))
			{
				GetNotReadyToFireMessageEvent getNotReadyToFireMessageEvent = PooledEvent<GetNotReadyToFireMessageEvent>.FromPool();
				getNotReadyToFireMessageEvent.Object = Object;
				getNotReadyToFireMessageEvent.Message = text;
				flag = Object.HandleEvent(getNotReadyToFireMessageEvent);
				text = getNotReadyToFireMessageEvent.Message;
			}
			return text;
		}
	}
}
