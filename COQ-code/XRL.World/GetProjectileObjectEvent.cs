namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetProjectileObjectEvent : PooledEvent<GetProjectileObjectEvent>
	{
		public GameObject Ammo;

		public GameObject Launcher;

		public GameObject Projectile;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Ammo = null;
			Launcher = null;
			Projectile = null;
		}

		public static GameObject GetFor(GameObject Ammo, GameObject Launcher)
		{
			bool flag = true;
			GameObject gameObject = null;
			if (flag && GameObject.Validate(ref Ammo) && Ammo.WantEvent(PooledEvent<GetProjectileObjectEvent>.ID, MinEvent.CascadeLevel))
			{
				GetProjectileObjectEvent getProjectileObjectEvent = PooledEvent<GetProjectileObjectEvent>.FromPool();
				getProjectileObjectEvent.Ammo = Ammo;
				getProjectileObjectEvent.Launcher = Launcher;
				getProjectileObjectEvent.Projectile = gameObject;
				flag = Ammo.HandleEvent(getProjectileObjectEvent);
				gameObject = getProjectileObjectEvent.Projectile;
			}
			if (flag && GameObject.Validate(ref Ammo) && Ammo.HasRegisteredEvent("GetProjectileObject"))
			{
				Event @event = Event.New("GetProjectileObject");
				@event.SetParameter("Ammo", Ammo);
				@event.SetParameter("Launcher", Launcher);
				@event.SetParameter("Projectile", gameObject);
				flag = Ammo.FireEvent(@event);
				gameObject = @event.GetGameObjectParameter("Projectile");
			}
			return gameObject;
		}
	}
}
