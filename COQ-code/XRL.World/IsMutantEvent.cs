namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class IsMutantEvent : PooledEvent<IsMutantEvent>
	{
		public GameObject Object;

		public bool IsMutant;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			IsMutant = false;
		}

		public static bool Check(GameObject Object)
		{
			bool flag = Object?.genotypeEntry?.IsMutant ?? (Object.IsCreature && !Object.HasTagOrProperty("NonMutant"));
			bool flag2 = true;
			if (flag2 && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("IsMutant"))
			{
				Event @event = Event.New("IsMutant");
				@event.SetParameter("Object", Object);
				@event.SetFlag("IsMutant", flag);
				flag2 = Object.FireEvent(@event);
				flag = @event.HasFlag("IsMutant");
			}
			if (flag2 && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<IsMutantEvent>.ID, MinEvent.CascadeLevel))
			{
				IsMutantEvent isMutantEvent = PooledEvent<IsMutantEvent>.FromPool();
				isMutantEvent.Object = Object;
				isMutantEvent.IsMutant = flag;
				flag2 = Object.HandleEvent(isMutantEvent);
				flag = isMutantEvent.IsMutant;
			}
			return flag;
		}
	}
}
