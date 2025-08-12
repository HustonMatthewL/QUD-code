namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class DamageDieSizeAdjustedEvent : PooledEvent<DamageDieSizeAdjustedEvent>
	{
		public GameObject Object;

		public IPart Part;

		public int Amount;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Part = null;
			Amount = 0;
		}

		public static void Send(GameObject Object, IPart Part, int Amount)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("DamageDieSizeAdjusted"))
			{
				Event @event = Event.New("DamageDieSizeAdjusted");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Part", Part);
				@event.SetParameter("Amount", Amount);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<DamageDieSizeAdjustedEvent>.ID, MinEvent.CascadeLevel))
			{
				DamageDieSizeAdjustedEvent damageDieSizeAdjustedEvent = PooledEvent<DamageDieSizeAdjustedEvent>.FromPool();
				damageDieSizeAdjustedEvent.Object = Object;
				damageDieSizeAdjustedEvent.Part = Part;
				damageDieSizeAdjustedEvent.Amount = Amount;
				flag = Object.HandleEvent(damageDieSizeAdjustedEvent);
			}
		}
	}
}
