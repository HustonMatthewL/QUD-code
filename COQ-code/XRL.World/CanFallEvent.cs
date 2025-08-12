namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class CanFallEvent : PooledEvent<CanFallEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Object;

		public bool CanFall;

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
			Object = null;
			CanFall = false;
		}

		public static bool Check(GameObject Object)
		{
			bool flag = true;
			if (GameObject.Validate(ref Object))
			{
				if (Object.HasPropertyOrTag("SuspendedPlatform"))
				{
					flag = false;
				}
				else if (Object.IsScenery)
				{
					flag = false;
				}
				else if (Object.IsFlying)
				{
					flag = false;
				}
				else if (Object.GetWeight() < 0.0)
				{
					flag = false;
				}
				else if (!Object.IsSubjectToGravity)
				{
					flag = false;
				}
			}
			bool flag2 = true;
			if (flag2 && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CanFall"))
			{
				Event @event = Event.New("CanFall");
				@event.SetParameter("Object", Object);
				@event.SetFlag("CanFall", flag);
				flag2 = Object.FireEvent(@event);
				flag = @event.HasFlag("CanFall");
			}
			if (flag2 && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CanFallEvent>.ID, CascadeLevel))
			{
				CanFallEvent canFallEvent = PooledEvent<CanFallEvent>.FromPool();
				canFallEvent.Object = Object;
				canFallEvent.CanFall = flag;
				flag2 = Object.HandleEvent(canFallEvent);
				flag = canFallEvent.CanFall;
			}
			return flag;
		}
	}
}
