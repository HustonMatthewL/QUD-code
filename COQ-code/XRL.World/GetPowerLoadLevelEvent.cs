namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetPowerLoadLevelEvent : PooledEvent<GetPowerLoadLevelEvent>
	{
		public GameObject Object;

		public int Level;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Level = 0;
		}

		public static int GetFor(GameObject Object, int Level = 100)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetPowerLoadLevel"))
			{
				Event @event = Event.New("GetPowerLoadLevel");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Level", Level);
				flag = Object.FireEvent(@event);
				Level = @event.GetIntParameter("Level");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetPowerLoadLevelEvent>.ID, MinEvent.CascadeLevel))
			{
				GetPowerLoadLevelEvent getPowerLoadLevelEvent = PooledEvent<GetPowerLoadLevelEvent>.FromPool();
				getPowerLoadLevelEvent.Object = Object;
				getPowerLoadLevelEvent.Level = Level;
				flag = Object.HandleEvent(getPowerLoadLevelEvent);
				Level = getPowerLoadLevelEvent.Level;
			}
			return Level;
		}
	}
}
