namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class NotifyTargetImmuneEvent : PooledEvent<NotifyTargetImmuneEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Weapon;

		public GameObject Target;

		public GameObject Actor;

		public Damage Damage;

		public IComponent<GameObject> ImmunityFrom;

		public string Checking;

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
			Weapon = null;
			Target = null;
			Actor = null;
			Damage = null;
			ImmunityFrom = null;
			Checking = null;
		}

		public static void Send(GameObject Weapon, GameObject Target, GameObject Actor = null, Damage Damage = null, IComponent<GameObject> ImmunityFrom = null)
		{
			bool flag = true;
			if (flag)
			{
				bool flag2 = GameObject.Validate(ref Weapon) && Weapon.HasRegisteredEvent("NotifyTargetImmune");
				bool flag3 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("NotifyTargetImmune");
				if (flag2 || flag3)
				{
					Event @event = Event.New("NotifyTargetImmune");
					@event.SetParameter("Weapon", Weapon);
					@event.SetParameter("Target", Target);
					@event.SetParameter("Actor", Actor);
					@event.SetParameter("Damage", Damage);
					@event.SetParameter("ImmunityFrom", ImmunityFrom);
					if (flag && flag2)
					{
						@event.SetParameter("Checking", "Weapon");
						flag = Weapon.FireEvent(@event);
					}
					if (flag && flag3)
					{
						@event.SetParameter("Checking", "Actor");
						flag = Actor.FireEvent(@event);
					}
				}
			}
			if (!flag)
			{
				return;
			}
			bool flag4 = GameObject.Validate(ref Weapon) && Weapon.WantEvent(PooledEvent<NotifyTargetImmuneEvent>.ID, CascadeLevel);
			bool flag5 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<NotifyTargetImmuneEvent>.ID, CascadeLevel);
			if (flag4 || flag5)
			{
				NotifyTargetImmuneEvent notifyTargetImmuneEvent = PooledEvent<NotifyTargetImmuneEvent>.FromPool();
				notifyTargetImmuneEvent.Weapon = Weapon;
				notifyTargetImmuneEvent.Target = Target;
				notifyTargetImmuneEvent.Actor = Actor;
				notifyTargetImmuneEvent.Damage = Damage;
				notifyTargetImmuneEvent.ImmunityFrom = ImmunityFrom;
				if (flag && flag4)
				{
					notifyTargetImmuneEvent.Checking = "Weapon";
					flag = Weapon.HandleEvent(notifyTargetImmuneEvent);
				}
				if (flag && flag5)
				{
					notifyTargetImmuneEvent.Checking = "Actor";
					flag = Actor.HandleEvent(notifyTargetImmuneEvent);
				}
			}
		}
	}
}
