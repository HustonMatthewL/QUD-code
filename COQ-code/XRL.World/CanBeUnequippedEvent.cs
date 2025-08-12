namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class CanBeUnequippedEvent : PooledEvent<CanBeUnequippedEvent>
	{
		public GameObject Object;

		public GameObject Equipper;

		public GameObject Actor;

		public bool Forced;

		public bool SemiForced;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Equipper = null;
			Actor = null;
			Forced = false;
			SemiForced = false;
		}

		public static bool Check(GameObject Object, GameObject Equipper = null, GameObject Actor = null, bool Forced = false, bool SemiForced = false)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CanBeUnequipped"))
			{
				Event @event = Event.New("CanBeUnequipped");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Equipper", Equipper);
				@event.SetParameter("Actor", Actor);
				@event.SetFlag("Forced", Forced);
				@event.SetFlag("SemiForced", SemiForced);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CanBeUnequippedEvent>.ID, MinEvent.CascadeLevel))
			{
				CanBeUnequippedEvent canBeUnequippedEvent = PooledEvent<CanBeUnequippedEvent>.FromPool();
				canBeUnequippedEvent.Object = Object;
				canBeUnequippedEvent.Equipper = Equipper;
				canBeUnequippedEvent.Actor = Actor;
				canBeUnequippedEvent.Forced = Forced;
				canBeUnequippedEvent.SemiForced = SemiForced;
				flag = Object.HandleEvent(canBeUnequippedEvent);
			}
			return flag;
		}
	}
}
