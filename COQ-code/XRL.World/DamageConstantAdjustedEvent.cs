namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class DamageConstantAdjustedEvent : PooledEvent<DamageConstantAdjustedEvent>
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
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("DamageConstantAdjusted"))
			{
				Event @event = Event.New("DamageConstantAdjusted");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Part", Part);
				@event.SetParameter("Amount", Amount);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<DamageConstantAdjustedEvent>.ID, MinEvent.CascadeLevel))
			{
				DamageConstantAdjustedEvent damageConstantAdjustedEvent = PooledEvent<DamageConstantAdjustedEvent>.FromPool();
				damageConstantAdjustedEvent.Object = Object;
				damageConstantAdjustedEvent.Part = Part;
				damageConstantAdjustedEvent.Amount = Amount;
				flag = Object.HandleEvent(damageConstantAdjustedEvent);
			}
		}
	}
}
