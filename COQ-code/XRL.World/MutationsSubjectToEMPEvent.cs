namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class MutationsSubjectToEMPEvent : PooledEvent<MutationsSubjectToEMPEvent>
	{
		public GameObject Object;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
		}

		public static bool Check(GameObject Object)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("MutationsSubjectToEMP"))
			{
				Event @event = Event.New("MutationsSubjectToEMP");
				@event.SetParameter("Object", Object);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<MutationsSubjectToEMPEvent>.ID, MinEvent.CascadeLevel))
			{
				MutationsSubjectToEMPEvent mutationsSubjectToEMPEvent = PooledEvent<MutationsSubjectToEMPEvent>.FromPool();
				mutationsSubjectToEMPEvent.Object = Object;
				flag = Object.HandleEvent(mutationsSubjectToEMPEvent);
			}
			return !flag;
		}
	}
}
