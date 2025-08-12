using XRL.World.Anatomy;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class PreferDefaultBehaviorEvent : PooledEvent<PreferDefaultBehaviorEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Actor;

		public GameObject Target;

		public BodyPart Part;

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
			Target = null;
			Part = null;
		}

		public static bool Check(GameObject Actor, GameObject Target, BodyPart Part)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("PreferDefaultBehavior"))
			{
				Event @event = Event.New("PreferDefaultBehavior");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("Part", Part);
				flag = Actor.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<PreferDefaultBehaviorEvent>.ID, CascadeLevel))
			{
				PreferDefaultBehaviorEvent preferDefaultBehaviorEvent = PooledEvent<PreferDefaultBehaviorEvent>.FromPool();
				preferDefaultBehaviorEvent.Actor = Actor;
				preferDefaultBehaviorEvent.Target = Target;
				preferDefaultBehaviorEvent.Part = Part;
				flag = Actor.HandleEvent(preferDefaultBehaviorEvent);
			}
			return !flag;
		}
	}
}
