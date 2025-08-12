using XRL.World.Parts;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetAmmoCountAvailableEvent : PooledEvent<GetAmmoCountAvailableEvent>
	{
		public GameObject Object;

		public MissileWeapon MissileWeapon;

		public int Count;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			MissileWeapon = null;
			Count = 0;
		}

		public void Register(int Amount)
		{
			if (Amount > 0 && (Count == 0 || Count > Amount))
			{
				Count = Amount;
			}
		}

		public static int GetFor(GameObject Object, MissileWeapon MissileWeapon = null)
		{
			if (MissileWeapon == null)
			{
				MissileWeapon = Object?.GetPart<MissileWeapon>();
			}
			bool flag = true;
			int num = 0;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetAmmoCountAvailable"))
			{
				Event @event = Event.New("GetAmmoCountAvailable");
				@event.SetParameter("Object", Object);
				@event.SetParameter("MissileWeapon", MissileWeapon);
				@event.SetParameter("Count", num);
				flag = Object.FireEvent(@event);
				num = @event.GetIntParameter("Count");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetAmmoCountAvailableEvent>.ID, MinEvent.CascadeLevel))
			{
				GetAmmoCountAvailableEvent getAmmoCountAvailableEvent = PooledEvent<GetAmmoCountAvailableEvent>.FromPool();
				getAmmoCountAvailableEvent.Object = Object;
				getAmmoCountAvailableEvent.MissileWeapon = MissileWeapon;
				getAmmoCountAvailableEvent.Count = num;
				flag = Object.HandleEvent(getAmmoCountAvailableEvent);
				num = getAmmoCountAvailableEvent.Count;
			}
			return num;
		}
	}
}
