namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class BeforeMeleeAttackEvent : PooledEvent<BeforeMeleeAttackEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Actor;

		public GameObject Target;

		public GameObject Weapon;

		public string Skill;

		public string Stat;

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
			Weapon = null;
			Skill = null;
			Stat = null;
		}

		public static void Send(GameObject Actor, GameObject Target, GameObject Weapon, string Skill, string Stat)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("BeforeMeleeAttack"))
			{
				Event @event = Event.New("BeforeMeleeAttack");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("Weapon", Weapon);
				@event.SetParameter("Skill", Skill);
				@event.SetParameter("Stat", Stat);
				flag = Actor.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<BeforeMeleeAttackEvent>.ID, CascadeLevel))
			{
				BeforeMeleeAttackEvent beforeMeleeAttackEvent = PooledEvent<BeforeMeleeAttackEvent>.FromPool();
				beforeMeleeAttackEvent.Actor = Actor;
				beforeMeleeAttackEvent.Target = Target;
				beforeMeleeAttackEvent.Weapon = Weapon;
				beforeMeleeAttackEvent.Skill = Skill;
				beforeMeleeAttackEvent.Stat = Stat;
				flag = Actor.HandleEvent(beforeMeleeAttackEvent);
			}
		}
	}
}
