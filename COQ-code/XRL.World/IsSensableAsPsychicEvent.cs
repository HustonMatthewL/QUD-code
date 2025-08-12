namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class IsSensableAsPsychicEvent : PooledEvent<IsSensableAsPsychicEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Object;

		public bool Sensable;

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
			Sensable = false;
		}

		public static bool Check(GameObject Object)
		{
			bool flag = false;
			bool flag2 = true;
			if (flag2 && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("IsSensableAsPsychic"))
			{
				Event @event = Event.New("IsSensableAsPsychic");
				@event.SetParameter("Object", Object);
				@event.SetFlag("Sensable", flag);
				flag2 = Object.FireEvent(@event);
				flag = @event.HasFlag("Sensable");
			}
			if (flag2 && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<IsSensableAsPsychicEvent>.ID, CascadeLevel))
			{
				IsSensableAsPsychicEvent isSensableAsPsychicEvent = PooledEvent<IsSensableAsPsychicEvent>.FromPool();
				isSensableAsPsychicEvent.Object = Object;
				isSensableAsPsychicEvent.Sensable = flag;
				flag2 = Object.HandleEvent(isSensableAsPsychicEvent);
				flag = isSensableAsPsychicEvent.Sensable;
			}
			return flag;
		}
	}
}
