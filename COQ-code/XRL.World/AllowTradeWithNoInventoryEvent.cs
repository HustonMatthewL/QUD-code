namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AllowTradeWithNoInventoryEvent : PooledEvent<AllowTradeWithNoInventoryEvent>
	{
		public GameObject Actor;

		public GameObject Trader;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Actor = null;
			Trader = null;
		}

		public static bool Check(GameObject Actor, GameObject Trader)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Trader) && Trader.HasRegisteredEvent("AllowTradeWithNoInventory"))
			{
				Event @event = Event.New("AllowTradeWithNoInventory");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Trader", Trader);
				flag = Trader.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Trader) && Trader.WantEvent(PooledEvent<AllowTradeWithNoInventoryEvent>.ID, MinEvent.CascadeLevel))
			{
				AllowTradeWithNoInventoryEvent allowTradeWithNoInventoryEvent = PooledEvent<AllowTradeWithNoInventoryEvent>.FromPool();
				allowTradeWithNoInventoryEvent.Actor = Actor;
				allowTradeWithNoInventoryEvent.Trader = Trader;
				flag = Trader.HandleEvent(allowTradeWithNoInventoryEvent);
			}
			return !flag;
		}
	}
}
