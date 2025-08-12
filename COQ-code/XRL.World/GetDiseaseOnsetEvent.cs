namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetDiseaseOnsetEvent : PooledEvent<GetDiseaseOnsetEvent>
	{
		public GameObject Object;

		public Effect Effect;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Effect = null;
		}

		public static Effect GetFor(GameObject Object)
		{
			bool flag = true;
			Effect effect = null;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetDiseaseOnset"))
			{
				Event @event = Event.New("GetDiseaseOnset");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Effect", effect);
				flag = Object.FireEvent(@event);
				effect = @event.GetParameter("Effect") as Effect;
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetDiseaseOnsetEvent>.ID, MinEvent.CascadeLevel))
			{
				GetDiseaseOnsetEvent getDiseaseOnsetEvent = PooledEvent<GetDiseaseOnsetEvent>.FromPool();
				getDiseaseOnsetEvent.Object = Object;
				getDiseaseOnsetEvent.Effect = effect;
				flag = Object.HandleEvent(getDiseaseOnsetEvent);
				effect = getDiseaseOnsetEvent.Effect;
			}
			return effect;
		}
	}
}
