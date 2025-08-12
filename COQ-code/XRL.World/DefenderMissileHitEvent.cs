using XRL.World.Parts;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class DefenderMissileHitEvent : PooledEvent<DefenderMissileHitEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Launcher;

		public GameObject Attacker;

		public GameObject Defender;

		public GameObject Owner;

		public GameObject Projectile;

		public Projectile ProjectilePart;

		public GameObject AimedAt;

		public GameObject ApparentTarget;

		public MissilePath MissilePath;

		public FireType Type;

		public int AimLevel;

		public int NaturalHitResult;

		public int HitResult;

		public bool PathInvolvesPlayer;

		public GameObject MessageAsFrom;

		public bool Done;

		public bool PenetrateCreatures;

		public bool PenetrateWalls;

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
			Launcher = null;
			Attacker = null;
			Defender = null;
			Owner = null;
			Projectile = null;
			ProjectilePart = null;
			AimedAt = null;
			ApparentTarget = null;
			MissilePath = null;
			Type = FireType.Normal;
			AimLevel = 0;
			NaturalHitResult = 0;
			HitResult = 0;
			PathInvolvesPlayer = false;
			MessageAsFrom = null;
			Done = false;
			PenetrateCreatures = false;
			PenetrateWalls = false;
		}

		public static bool Check(GameObject Launcher, GameObject Attacker, GameObject Defender, GameObject Owner, GameObject Projectile, Projectile ProjectilePart, GameObject AimedAt, GameObject ApparentTarget, MissilePath MissilePath, FireType Type, int AimLevel, int NaturalHitResult, int HitResult, bool PathInvolvesPlayer, GameObject MessageAsFrom, ref bool Done, ref bool PenetrateCreatures, ref bool PenetrateWalls)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Defender) && Defender.HasRegisteredEvent("DefenderMissileHit"))
			{
				Event @event = Event.New("DefenderMissileHit");
				@event.SetParameter("Launcher", Launcher);
				@event.SetParameter("Attacker", Attacker);
				@event.SetParameter("Defender", Defender);
				@event.SetParameter("Owner", Owner);
				@event.SetParameter("Projectile", Projectile);
				@event.SetParameter("ProjectilePart", ProjectilePart);
				@event.SetParameter("AimedAt", AimedAt);
				@event.SetParameter("ApparentTarget", ApparentTarget);
				@event.SetParameter("MissilePath", MissilePath);
				@event.SetParameter("Type", Type);
				@event.SetParameter("AimLevel", AimLevel);
				@event.SetParameter("NaturalHitResult", NaturalHitResult);
				@event.SetParameter("HitResult", HitResult);
				@event.SetFlag("PathInvolvesPlayer", PathInvolvesPlayer);
				@event.SetParameter("MessageAsFrom", MessageAsFrom);
				@event.SetFlag("Done", Done);
				@event.SetFlag("PenetrateCreatures", PenetrateCreatures);
				@event.SetFlag("PenetrateWalls", PenetrateWalls);
				flag = Defender.FireEvent(@event);
				Done = @event.HasFlag("Done");
				PenetrateCreatures = @event.HasFlag("PenetrateCreatures");
				PenetrateWalls = @event.HasFlag("PenetrateWalls");
			}
			if (flag && GameObject.Validate(ref Defender) && Defender.WantEvent(PooledEvent<DefenderMissileHitEvent>.ID, CascadeLevel))
			{
				DefenderMissileHitEvent defenderMissileHitEvent = PooledEvent<DefenderMissileHitEvent>.FromPool();
				defenderMissileHitEvent.Launcher = Launcher;
				defenderMissileHitEvent.Attacker = Attacker;
				defenderMissileHitEvent.Defender = Defender;
				defenderMissileHitEvent.Owner = Owner;
				defenderMissileHitEvent.Projectile = Projectile;
				defenderMissileHitEvent.ProjectilePart = ProjectilePart;
				defenderMissileHitEvent.AimedAt = AimedAt;
				defenderMissileHitEvent.ApparentTarget = ApparentTarget;
				defenderMissileHitEvent.MissilePath = MissilePath;
				defenderMissileHitEvent.Type = Type;
				defenderMissileHitEvent.AimLevel = AimLevel;
				defenderMissileHitEvent.NaturalHitResult = NaturalHitResult;
				defenderMissileHitEvent.HitResult = HitResult;
				defenderMissileHitEvent.PathInvolvesPlayer = PathInvolvesPlayer;
				defenderMissileHitEvent.MessageAsFrom = MessageAsFrom;
				defenderMissileHitEvent.Done = Done;
				defenderMissileHitEvent.PenetrateCreatures = PenetrateCreatures;
				defenderMissileHitEvent.PenetrateWalls = PenetrateWalls;
				flag = Defender.HandleEvent(defenderMissileHitEvent);
				Done = defenderMissileHitEvent.Done;
				PenetrateCreatures = defenderMissileHitEvent.PenetrateCreatures;
				PenetrateWalls = defenderMissileHitEvent.PenetrateWalls;
			}
			return flag;
		}
	}
}
