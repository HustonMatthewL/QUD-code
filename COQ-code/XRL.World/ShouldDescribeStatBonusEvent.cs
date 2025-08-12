namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class ShouldDescribeStatBonusEvent : PooledEvent<ShouldDescribeStatBonusEvent>
	{
		public GameObject Object;

		public IComponent<GameObject> Component;

		public string Stat;

		public int Amount;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Component = null;
			Stat = null;
			Amount = 0;
		}

		public static bool Check(GameObject Object, IComponent<GameObject> Component, string Stat, int Amount)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("ShouldDescribeStatBonus"))
			{
				Event @event = Event.New("ShouldDescribeStatBonus");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Component", Component);
				@event.SetParameter("Stat", Stat);
				@event.SetParameter("Amount", Amount);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<ShouldDescribeStatBonusEvent>.ID, MinEvent.CascadeLevel))
			{
				ShouldDescribeStatBonusEvent shouldDescribeStatBonusEvent = PooledEvent<ShouldDescribeStatBonusEvent>.FromPool();
				shouldDescribeStatBonusEvent.Object = Object;
				shouldDescribeStatBonusEvent.Component = Component;
				shouldDescribeStatBonusEvent.Stat = Stat;
				shouldDescribeStatBonusEvent.Amount = Amount;
				flag = Object.HandleEvent(shouldDescribeStatBonusEvent);
			}
			return flag;
		}
	}
}
