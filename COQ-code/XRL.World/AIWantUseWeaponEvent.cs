namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AIWantUseWeaponEvent : PooledEvent<AIWantUseWeaponEvent>
	{
		public GameObject Actor;

		public GameObject Object;

		public GameObject Target;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Actor = null;
			Object = null;
			Target = null;
		}

		public static bool Check(GameObject Object, GameObject Actor, GameObject Target)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AIWantUseWeapon"))
			{
				Event @event = Event.New("AIWantUseWeapon");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<AIWantUseWeaponEvent>.ID, MinEvent.CascadeLevel))
			{
				AIWantUseWeaponEvent aIWantUseWeaponEvent = PooledEvent<AIWantUseWeaponEvent>.FromPool();
				aIWantUseWeaponEvent.Object = Object;
				aIWantUseWeaponEvent.Actor = Actor;
				aIWantUseWeaponEvent.Target = Target;
				flag = Object.HandleEvent(aIWantUseWeaponEvent);
			}
			return flag;
		}
	}
}
