namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class GetRunningBehaviorEvent : PooledEvent<GetRunningBehaviorEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Actor;

		public string AbilityName;

		public string Verb;

		public string EffectDisplayName;

		public string EffectMessageName;

		public int EffectDuration;

		public bool SpringingEffective;

		public Templates.StatCollector Stats;

		public int Priority;

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
			AbilityName = null;
			Verb = null;
			EffectDisplayName = null;
			EffectMessageName = null;
			EffectDuration = 0;
			SpringingEffective = false;
			Stats = null;
			Priority = 0;
		}

		public static void Retrieve(GameObject Actor, out string AbilityName, out string Verb, out string EffectDisplayName, out string EffectMessageName, out int EffectDuration, out bool SpringingEffective, Templates.StatCollector Stats = null)
		{
			AbilityName = null;
			Verb = null;
			EffectDisplayName = null;
			EffectMessageName = null;
			EffectDuration = 0;
			SpringingEffective = false;
			bool flag = true;
			int num = 0;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetRunningBehavior"))
			{
				Event @event = Event.New("GetRunningBehavior");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("AbilityName", AbilityName);
				@event.SetParameter("Verb", Verb);
				@event.SetParameter("EffectDisplayName", EffectDisplayName);
				@event.SetParameter("EffectMessageName", EffectMessageName);
				@event.SetParameter("EffectDuration", EffectDuration);
				@event.SetParameter("Stats", Stats);
				@event.SetFlag("SpringingEffective", SpringingEffective);
				@event.SetParameter("Priority", num);
				flag = Actor.FireEvent(@event);
				AbilityName = @event.GetStringParameter("AbilityName");
				Verb = @event.GetStringParameter("Verb");
				EffectDisplayName = @event.GetStringParameter("EffectDisplayName");
				EffectMessageName = @event.GetStringParameter("EffectMessageName");
				EffectDuration = @event.GetIntParameter("EffectDuration");
				SpringingEffective = @event.HasFlag("SpringingEffective");
				num = @event.GetIntParameter("Priority");
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetRunningBehaviorEvent>.ID, CascadeLevel))
			{
				GetRunningBehaviorEvent getRunningBehaviorEvent = PooledEvent<GetRunningBehaviorEvent>.FromPool();
				getRunningBehaviorEvent.Actor = Actor;
				getRunningBehaviorEvent.AbilityName = AbilityName;
				getRunningBehaviorEvent.Verb = Verb;
				getRunningBehaviorEvent.EffectDisplayName = EffectDisplayName;
				getRunningBehaviorEvent.EffectMessageName = EffectMessageName;
				getRunningBehaviorEvent.EffectDuration = EffectDuration;
				getRunningBehaviorEvent.SpringingEffective = SpringingEffective;
				getRunningBehaviorEvent.Stats = Stats;
				getRunningBehaviorEvent.Priority = num;
				flag = Actor.HandleEvent(getRunningBehaviorEvent);
				AbilityName = getRunningBehaviorEvent.AbilityName;
				Verb = getRunningBehaviorEvent.Verb;
				EffectDisplayName = getRunningBehaviorEvent.EffectDisplayName;
				EffectMessageName = getRunningBehaviorEvent.EffectMessageName;
				EffectDuration = getRunningBehaviorEvent.EffectDuration;
				SpringingEffective = getRunningBehaviorEvent.SpringingEffective;
				num = getRunningBehaviorEvent.Priority;
			}
		}
	}
}
