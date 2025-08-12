namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetCleaveMaxPenaltyEvent : PooledEvent<GetCleaveMaxPenaltyEvent>
	{
		public GameObject Object;

		public GameObject Actor;

		public GameObject Target;

		public int BaseAmount;

		public int Amount;

		public bool IsChargingStrike;

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
			IsChargingStrike = false;
		}

		public static int GetFor(GameObject Object, GameObject Actor, GameObject Target, int BaseAmount = 1, bool IsChargingStrike = false)
		{
			bool flag = true;
			int num = BaseAmount;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetCleaveMaxPenalty"))
			{
				Event @event = Event.New("GetCleaveMaxPenalty");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("BaseAmount", BaseAmount);
				@event.SetParameter("Amount", num);
				@event.SetParameter("IsChargingStrike", IsChargingStrike);
				flag = Object.FireEvent(@event);
				num = @event.GetIntParameter("Amount");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetCleaveMaxPenaltyEvent>.ID, MinEvent.CascadeLevel))
			{
				GetCleaveMaxPenaltyEvent getCleaveMaxPenaltyEvent = PooledEvent<GetCleaveMaxPenaltyEvent>.FromPool();
				getCleaveMaxPenaltyEvent.Object = Object;
				getCleaveMaxPenaltyEvent.Actor = Actor;
				getCleaveMaxPenaltyEvent.Target = Target;
				getCleaveMaxPenaltyEvent.BaseAmount = BaseAmount;
				getCleaveMaxPenaltyEvent.Amount = num;
				getCleaveMaxPenaltyEvent.IsChargingStrike = IsChargingStrike;
				flag = Object.HandleEvent(getCleaveMaxPenaltyEvent);
				num = getCleaveMaxPenaltyEvent.Amount;
			}
			return num;
		}
	}
}
