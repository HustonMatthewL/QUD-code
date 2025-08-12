using XRL.World.Parts;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class BeforeProjectileHitEvent : PooledEvent<BeforeProjectileHitEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Projectile;

		public GameObject Launcher;

		public GameObject Attacker;

		public GameObject Object;

		public GameObject ApparentTarget;

		public Cell Cell;

		public TreatAsSolid ViaTreatAsSolid;

		public bool PenetrateCreatures;

		public bool PenetrateWalls;

		public bool Hit;

		public bool LightBased;

		public bool Prospective;

		public bool Recheck;

		public bool RecheckPhase;

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
			Projectile = null;
			Launcher = null;
			Attacker = null;
			Object = null;
			ApparentTarget = null;
			Cell = null;
			ViaTreatAsSolid = null;
			PenetrateCreatures = false;
			PenetrateWalls = false;
			Hit = false;
			LightBased = false;
			Prospective = false;
			Recheck = false;
			RecheckPhase = false;
		}

		public static bool Check(GameObject Projectile, GameObject Attacker, GameObject Object, out bool Recheck, out bool RecheckPhase, bool PenetrateCreatures = false, bool PenetrateWalls = false, GameObject Launcher = null, GameObject ApparentTarget = null, Cell Cell = null, TreatAsSolid ViaTreatAsSolid = null, bool LightBased = false, bool Prospective = false)
		{
			if (Cell == null)
			{
				Cell = Object?.CurrentCell;
			}
			Recheck = false;
			RecheckPhase = false;
			bool flag = true;
			bool flag2 = true;
			if (flag)
			{
				bool flag3 = GameObject.Validate(ref Projectile) && Projectile.HasRegisteredEvent("BeforeProjectileHit");
				bool flag4 = GameObject.Validate(ref Attacker) && Attacker.HasRegisteredEvent("BeforeProjectileHit");
				bool flag5 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BeforeProjectileHit");
				if (flag3 || flag4 || flag5)
				{
					Event @event = Event.New("BeforeProjectileHit");
					@event.SetParameter("Projectile", Projectile);
					@event.SetParameter("Launcher", Launcher);
					@event.SetParameter("Attacker", Attacker);
					@event.SetParameter("Object", Object);
					@event.SetParameter("ApparentTarget", ApparentTarget);
					@event.SetParameter("Cell", Cell);
					@event.SetParameter("ViaTreatAsSolid", ViaTreatAsSolid);
					@event.SetFlag("PenetrateCreatures", PenetrateCreatures);
					@event.SetFlag("PenetrateWalls", PenetrateWalls);
					@event.SetFlag("Hit", flag2);
					@event.SetFlag("LightBased", LightBased);
					@event.SetFlag("Prospective", Prospective);
					@event.SetFlag("Recheck", Recheck);
					@event.SetFlag("RecheckPhase", RecheckPhase);
					flag = (!flag3 || Projectile.FireEvent(@event)) && (!flag4 || Attacker.FireEvent(@event)) && (!flag5 || Object.FireEvent(@event));
					flag2 = @event.HasFlag("Hit");
					Recheck = @event.HasFlag("Recheck");
					RecheckPhase = @event.HasFlag("RecheckPhase");
				}
			}
			if (flag)
			{
				bool flag6 = GameObject.Validate(ref Projectile) && Projectile.WantEvent(PooledEvent<BeforeProjectileHitEvent>.ID, CascadeLevel);
				bool flag7 = GameObject.Validate(ref Attacker) && Attacker.WantEvent(PooledEvent<BeforeProjectileHitEvent>.ID, CascadeLevel);
				bool flag8 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<BeforeProjectileHitEvent>.ID, CascadeLevel);
				if (flag6 || flag7 || flag8)
				{
					BeforeProjectileHitEvent beforeProjectileHitEvent = PooledEvent<BeforeProjectileHitEvent>.FromPool();
					beforeProjectileHitEvent.Projectile = Projectile;
					beforeProjectileHitEvent.Launcher = Launcher;
					beforeProjectileHitEvent.Attacker = Attacker;
					beforeProjectileHitEvent.Object = Object;
					beforeProjectileHitEvent.ApparentTarget = ApparentTarget;
					beforeProjectileHitEvent.Cell = Cell;
					beforeProjectileHitEvent.ViaTreatAsSolid = ViaTreatAsSolid;
					beforeProjectileHitEvent.PenetrateCreatures = PenetrateCreatures;
					beforeProjectileHitEvent.PenetrateWalls = PenetrateWalls;
					beforeProjectileHitEvent.Hit = flag2;
					beforeProjectileHitEvent.LightBased = LightBased;
					beforeProjectileHitEvent.Prospective = Prospective;
					beforeProjectileHitEvent.Recheck = Recheck;
					beforeProjectileHitEvent.RecheckPhase = RecheckPhase;
					flag = (!flag6 || Projectile.HandleEvent(beforeProjectileHitEvent)) && (!flag7 || Attacker.HandleEvent(beforeProjectileHitEvent)) && (!flag8 || Object.HandleEvent(beforeProjectileHitEvent));
					flag2 = beforeProjectileHitEvent.Hit;
					Recheck = beforeProjectileHitEvent.Recheck;
					RecheckPhase = beforeProjectileHitEvent.RecheckPhase;
				}
			}
			return flag2;
		}
	}
}
