using XRL.World.Parts;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class MissilePenetrateEvent : PooledEvent<MissilePenetrateEvent>
	{
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

		public bool PathInvolvesPlayer;

		public GameObject MessageAsFrom;

		public int Penetrations;

		public string OutcomeMessageFragment;

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
			PathInvolvesPlayer = false;
			MessageAsFrom = null;
			Penetrations = 0;
			OutcomeMessageFragment = null;
		}

		public static void Process(GameObject Launcher, GameObject Attacker, GameObject Defender, GameObject Owner, GameObject Projectile, Projectile ProjectilePart, GameObject AimedAt, GameObject ApparentTarget, MissilePath MissilePath, FireType Type, int AimLevel, int NaturalHitResult, bool PathInvolvesPlayer, GameObject MessageAsFrom, ref int Penetrations, ref string OutcomeMessageFragment)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Launcher) && Launcher.HasRegisteredEvent("MissilePenetrate"))
			{
				Event @event = Event.New("MissilePenetrate");
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
				@event.SetParameter("PathInvolvesPlayer", PathInvolvesPlayer);
				@event.SetParameter("MessageAsFrom", MessageAsFrom);
				@event.SetParameter("Penetrations", Penetrations);
				@event.SetParameter("OutcomeMessageFragment", OutcomeMessageFragment);
				flag = Launcher.FireEvent(@event);
				Penetrations = @event.GetIntParameter("Penetrations");
				OutcomeMessageFragment = @event.GetStringParameter("OutcomeMessageFragment");
			}
			if (flag && GameObject.Validate(ref Launcher) && Launcher.WantEvent(PooledEvent<MissilePenetrateEvent>.ID, MinEvent.CascadeLevel))
			{
				MissilePenetrateEvent missilePenetrateEvent = PooledEvent<MissilePenetrateEvent>.FromPool();
				missilePenetrateEvent.Launcher = Launcher;
				missilePenetrateEvent.Attacker = Attacker;
				missilePenetrateEvent.Defender = Defender;
				missilePenetrateEvent.Owner = Owner;
				missilePenetrateEvent.Projectile = Projectile;
				missilePenetrateEvent.ProjectilePart = ProjectilePart;
				missilePenetrateEvent.AimedAt = AimedAt;
				missilePenetrateEvent.ApparentTarget = ApparentTarget;
				missilePenetrateEvent.MissilePath = MissilePath;
				missilePenetrateEvent.Type = Type;
				missilePenetrateEvent.AimLevel = AimLevel;
				missilePenetrateEvent.NaturalHitResult = NaturalHitResult;
				missilePenetrateEvent.PathInvolvesPlayer = PathInvolvesPlayer;
				missilePenetrateEvent.MessageAsFrom = MessageAsFrom;
				missilePenetrateEvent.Penetrations = Penetrations;
				missilePenetrateEvent.OutcomeMessageFragment = OutcomeMessageFragment;
				flag = Launcher.HandleEvent(missilePenetrateEvent);
				Penetrations = missilePenetrateEvent.Penetrations;
				OutcomeMessageFragment = missilePenetrateEvent.OutcomeMessageFragment;
			}
		}
	}
}
