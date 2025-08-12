namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetGenderEvent : PooledEvent<GetGenderEvent>
	{
		public GameObject Object;

		public string Name;

		public bool AsIfKnown;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Name = null;
			AsIfKnown = false;
		}

		public static string GetFor(GameObject Object, string Name, bool AsIfKnown = false)
		{
			if (Object.HasRegisteredEvent("GetGender"))
			{
				Event @event = Event.New("GetGender");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Name", Name);
				@event.SetFlag("AsIfKnown", AsIfKnown);
				Object.FireEvent(@event);
				Name = @event.GetStringParameter("Name");
			}
			if (Object.WantEvent(PooledEvent<GetGenderEvent>.ID, MinEvent.CascadeLevel))
			{
				GetGenderEvent getGenderEvent = PooledEvent<GetGenderEvent>.FromPool();
				getGenderEvent.Object = Object;
				getGenderEvent.Name = Name;
				getGenderEvent.AsIfKnown = AsIfKnown;
				Object.HandleEvent(getGenderEvent);
				Name = getGenderEvent.Name;
			}
			return Name;
		}
	}
}
