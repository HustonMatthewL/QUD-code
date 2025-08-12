namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class CanMissilePassForcefieldEvent : PooledEvent<CanMissilePassForcefieldEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Actor;

		public GameObject Projectile;

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
			Projectile = null;
		}

		public static bool Check(GameObject Actor, GameObject Projectile)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("CanMissilePassForcefield"))
			{
				Event @event = Event.New("CanMissilePassForcefield");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Projectile", Projectile);
				flag = Actor.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<CanMissilePassForcefieldEvent>.ID, CascadeLevel))
			{
				CanMissilePassForcefieldEvent canMissilePassForcefieldEvent = PooledEvent<CanMissilePassForcefieldEvent>.FromPool();
				canMissilePassForcefieldEvent.Actor = Actor;
				canMissilePassForcefieldEvent.Projectile = Projectile;
				flag = Actor.HandleEvent(canMissilePassForcefieldEvent);
			}
			return !flag;
		}
	}
}
