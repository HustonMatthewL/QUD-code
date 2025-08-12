namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetCleaveAmountEvent : PooledEvent<GetCleaveAmountEvent>
	{
		public GameObject Object;

		public GameObject Actor;

		public GameObject Target;

		public int BaseAmount;

		public int Amount;

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
			BaseAmount = 0;
			Amount = 0;
		}

		public static int GetFor(GameObject Object, GameObject Actor, GameObject Target, int BaseAmount = 1)
		{
			bool flag = true;
			int num = BaseAmount;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetCleaveAmount"))
			{
				Event @event = Event.New("GetCleaveAmount");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("BaseAmount", BaseAmount);
				@event.SetParameter("Amount", num);
				flag = Object.FireEvent(@event);
				num = @event.GetIntParameter("Amount");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetCleaveAmountEvent>.ID, MinEvent.CascadeLevel))
			{
				GetCleaveAmountEvent getCleaveAmountEvent = PooledEvent<GetCleaveAmountEvent>.FromPool();
				getCleaveAmountEvent.Object = Object;
				getCleaveAmountEvent.Actor = Actor;
				getCleaveAmountEvent.Target = Target;
				getCleaveAmountEvent.BaseAmount = BaseAmount;
				getCleaveAmountEvent.Amount = num;
				flag = Object.HandleEvent(getCleaveAmountEvent);
				num = getCleaveAmountEvent.Amount;
			}
			return num;
		}
	}
}
