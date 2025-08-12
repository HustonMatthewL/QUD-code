namespace XRL.World
{
	[GameEvent(Base = true)]
	public abstract class IChargeStorageEvent : IChargeEvent
	{
		public int Transient;

		public bool UnlimitedTransient;

		public override bool Dispatch(IEventHandler Handler)
		{
			if (!base.Dispatch(Handler))
			{
				return false;
			}
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Transient = 0;
			UnlimitedTransient = false;
		}

		public override Event GenerateRegisteredEvent(string ID)
		{
			Event @event = base.GenerateRegisteredEvent(ID);
			@event.SetParameter("Transient", Transient);
			@event.SetFlag("UnlimitedTransient", UnlimitedTransient);
			return @event;
		}

		public override void SyncFromRegisteredEvent(Event E, bool AllFields = false)
		{
			base.SyncFromRegisteredEvent(E, AllFields);
			Transient = E.GetIntParameter("Transient");
			UnlimitedTransient = E.HasFlag("UnlimitedTransient");
		}
	}
}
