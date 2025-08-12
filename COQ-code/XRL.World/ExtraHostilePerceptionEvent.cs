namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class ExtraHostilePerceptionEvent : PooledEvent<ExtraHostilePerceptionEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Actor;

		public GameObject Hostile;

		public string PerceiveVerb;

		public bool TreatAsVisible;

		public override int GetCascadeLevel()
		{
			return CascadeLevel;
		}

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Actor = null;
			Hostile = null;
			PerceiveVerb = null;
			TreatAsVisible = false;
		}

		public static bool Check(GameObject Actor, out GameObject Hostile, out string PerceiveVerb, out bool TreatAsVisible)
		{
			Hostile = null;
			PerceiveVerb = "perceive";
			TreatAsVisible = false;
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("ExtraHostilePerception"))
			{
				Event @event = Event.New("ExtraHostilePerception");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Hostile", Hostile);
				@event.SetParameter("PerceiveVerb", PerceiveVerb);
				@event.SetFlag("TreatAsVisible", TreatAsVisible);
				flag = Actor.FireEvent(@event);
				Hostile = @event.GetGameObjectParameter("Hostile");
				PerceiveVerb = @event.GetStringParameter("PerceiveVerb");
				TreatAsVisible = @event.HasFlag("TreatAsVisible");
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<ExtraHostilePerceptionEvent>.ID, CascadeLevel))
			{
				ExtraHostilePerceptionEvent extraHostilePerceptionEvent = PooledEvent<ExtraHostilePerceptionEvent>.FromPool();
				extraHostilePerceptionEvent.Actor = Actor;
				extraHostilePerceptionEvent.Hostile = Hostile;
				extraHostilePerceptionEvent.PerceiveVerb = PerceiveVerb;
				extraHostilePerceptionEvent.TreatAsVisible = TreatAsVisible;
				flag = Actor.HandleEvent(extraHostilePerceptionEvent);
				Hostile = extraHostilePerceptionEvent.Hostile;
				PerceiveVerb = extraHostilePerceptionEvent.PerceiveVerb;
				TreatAsVisible = extraHostilePerceptionEvent.TreatAsVisible;
			}
			return Hostile != null;
		}
	}
}
