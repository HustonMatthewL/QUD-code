namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AfterPseudoRelicGeneratedEvent : PooledEvent<AfterPseudoRelicGeneratedEvent>
	{
		public GameObject Object;

		public string Element;

		public string Type;

		public string Subtype;

		public int Tier;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Element = null;
			Type = null;
			Subtype = null;
			Tier = 0;
		}

		public static void Send(GameObject Object, string Element, string Type, string Subtype, int Tier)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AfterPseudoRelicGenerated"))
			{
				Event @event = Event.New("AfterPseudoRelicGenerated");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Element", Element);
				@event.SetParameter("Type", Type);
				@event.SetParameter("Subtype", Subtype);
				@event.SetParameter("Tier", Tier);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<AfterPseudoRelicGeneratedEvent>.ID, MinEvent.CascadeLevel))
			{
				AfterPseudoRelicGeneratedEvent afterPseudoRelicGeneratedEvent = PooledEvent<AfterPseudoRelicGeneratedEvent>.FromPool();
				afterPseudoRelicGeneratedEvent.Object = Object;
				afterPseudoRelicGeneratedEvent.Element = Element;
				afterPseudoRelicGeneratedEvent.Type = Type;
				afterPseudoRelicGeneratedEvent.Subtype = Subtype;
				afterPseudoRelicGeneratedEvent.Tier = Tier;
				flag = Object.HandleEvent(afterPseudoRelicGeneratedEvent);
			}
		}
	}
}
