namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class SlamEvent : PooledEvent<SlamEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Actor;

		public GameObject Target;

		public Cell TargetCell;

		public int SlamMultiplier;

		public int SlamPower;

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
			TargetCell = null;
			SlamMultiplier = 0;
			SlamPower = 0;
		}

		public static bool Check(GameObject Actor, GameObject Target, Cell TargetCell, ref int SlamMultiplier, ref int SlamPower)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("Slam"))
			{
				Event @event = Event.New("Slam");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("TargetCell", TargetCell);
				@event.SetParameter("SlamMultiplier", SlamMultiplier);
				@event.SetParameter("SlamPower", SlamPower);
				flag = Actor.FireEvent(@event);
				SlamMultiplier = @event.GetIntParameter("SlamMultiplier");
				SlamPower = @event.GetIntParameter("SlamPower");
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<SlamEvent>.ID, CascadeLevel))
			{
				SlamEvent slamEvent = PooledEvent<SlamEvent>.FromPool();
				slamEvent.Actor = Actor;
				slamEvent.Target = Target;
				slamEvent.TargetCell = TargetCell;
				slamEvent.SlamMultiplier = SlamMultiplier;
				slamEvent.SlamPower = SlamPower;
				flag = Actor.HandleEvent(slamEvent);
				SlamMultiplier = slamEvent.SlamMultiplier;
				SlamPower = slamEvent.SlamPower;
			}
			return flag;
		}
	}
}
