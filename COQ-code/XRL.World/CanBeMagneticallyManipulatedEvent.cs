namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class CanBeMagneticallyManipulatedEvent : PooledEvent<CanBeMagneticallyManipulatedEvent>
	{
		public GameObject Object;

		public bool Allow;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Allow = false;
		}

		public static bool Check(GameObject Object)
		{
			bool flag = true;
			bool flag2 = false;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CanBeMagneticallyManipulated"))
			{
				Event @event = Event.New("CanBeMagneticallyManipulated");
				@event.SetParameter("Object", Object);
				@event.SetFlag("Allow", flag2);
				flag = Object.FireEvent(@event);
				flag2 = @event.HasFlag("Allow");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CanBeMagneticallyManipulatedEvent>.ID, MinEvent.CascadeLevel))
			{
				CanBeMagneticallyManipulatedEvent canBeMagneticallyManipulatedEvent = PooledEvent<CanBeMagneticallyManipulatedEvent>.FromPool();
				canBeMagneticallyManipulatedEvent.Object = Object;
				canBeMagneticallyManipulatedEvent.Allow = flag2;
				flag = Object.HandleEvent(canBeMagneticallyManipulatedEvent);
				flag2 = canBeMagneticallyManipulatedEvent.Allow;
			}
			return flag2;
		}
	}
}
