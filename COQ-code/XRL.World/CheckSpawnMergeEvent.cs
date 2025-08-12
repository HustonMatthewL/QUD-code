namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class CheckSpawnMergeEvent : PooledEvent<CheckSpawnMergeEvent>
	{
		public GameObject Object;

		public GameObject Other;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Other = null;
		}

		public static bool Check(GameObject Object, GameObject Other)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CheckSpawnMerge"))
			{
				Event @event = Event.New("CheckSpawnMerge");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Other", Other);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckSpawnMergeEvent>.ID, MinEvent.CascadeLevel))
			{
				CheckSpawnMergeEvent checkSpawnMergeEvent = PooledEvent<CheckSpawnMergeEvent>.FromPool();
				checkSpawnMergeEvent.Object = Object;
				checkSpawnMergeEvent.Other = Other;
				flag = Object.HandleEvent(checkSpawnMergeEvent);
			}
			return flag;
		}
	}
}
