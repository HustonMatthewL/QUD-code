namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetAutoEquipPriorityEvent : PooledEvent<GetAutoEquipPriorityEvent>
	{
		public GameObject Object;

		public int Default;

		public int Priority;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Default = 0;
			Priority = 0;
		}

		public static int GetFor(GameObject Object, int Default = 10)
		{
			int num = Default;
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetAutoEquipPriority"))
			{
				Event @event = Event.New("GetAutoEquipPriority");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Default", Default);
				@event.SetParameter("Priority", num);
				flag = Object.FireEvent(@event);
				num = @event.GetIntParameter("Priority");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetAutoEquipPriorityEvent>.ID, MinEvent.CascadeLevel))
			{
				GetAutoEquipPriorityEvent getAutoEquipPriorityEvent = PooledEvent<GetAutoEquipPriorityEvent>.FromPool();
				getAutoEquipPriorityEvent.Object = Object;
				getAutoEquipPriorityEvent.Default = Default;
				getAutoEquipPriorityEvent.Priority = num;
				flag = Object.HandleEvent(getAutoEquipPriorityEvent);
				num = getAutoEquipPriorityEvent.Priority;
			}
			return num;
		}
	}
}
