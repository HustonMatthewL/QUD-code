namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class AIAfterMissileEvent : PooledEvent<AIAfterMissileEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Object;

		public GameObject Actor;

		public GameObject Target;

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
			Actor = null;
			Target = null;
		}

		public static void Send(GameObject Object, GameObject Actor, GameObject Target)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AIAfterMissile"))
			{
				Event @event = Event.New("AIAfterMissile");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Actor", Actor);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIAfterMissile"))
			{
				Event event2 = Event.New("AIAfterMissile");
				event2.SetParameter("Object", Object);
				event2.SetParameter("Actor", Actor);
				event2.SetParameter("Target", Target);
				flag = Actor.FireEvent(event2);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<AIAfterMissileEvent>.ID, CascadeLevel))
			{
				AIAfterMissileEvent aIAfterMissileEvent = PooledEvent<AIAfterMissileEvent>.FromPool();
				aIAfterMissileEvent.Object = Object;
				aIAfterMissileEvent.Actor = Actor;
				aIAfterMissileEvent.Target = Target;
				flag = Actor.HandleEvent(aIAfterMissileEvent);
			}
		}
	}
}
