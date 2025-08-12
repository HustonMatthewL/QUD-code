namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class ShotCompleteEvent : PooledEvent<ShotCompleteEvent>
	{
		public GameObject Object;

		public GameObject Actor;

		public GameObject LoadedAmmo;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Actor = null;
			LoadedAmmo = null;
		}

		public static void Send(GameObject Object, GameObject Actor, GameObject LoadedAmmo)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("ShotComplete"))
			{
				Event @event = Event.New("ShotComplete");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("LoadedAmmo", LoadedAmmo);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<ShotCompleteEvent>.ID, MinEvent.CascadeLevel))
			{
				ShotCompleteEvent shotCompleteEvent = PooledEvent<ShotCompleteEvent>.FromPool();
				shotCompleteEvent.Object = Object;
				shotCompleteEvent.Actor = Actor;
				shotCompleteEvent.LoadedAmmo = LoadedAmmo;
				flag = Object.HandleEvent(shotCompleteEvent);
			}
		}
	}
}
