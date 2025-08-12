using System;
using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class EarlyBeforeDeathRemovalEvent : IDeathEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(EarlyBeforeDeathRemovalEvent), null, CountPool, ResetPool);

		private static List<EarlyBeforeDeathRemovalEvent> Pool;

		private static int PoolCounter;

		public EarlyBeforeDeathRemovalEvent()
		{
			base.ID = ID;
		}

		public static int CountPool()
		{
			if (Pool != null)
			{
				return Pool.Count;
			}
			return 0;
		}

		public static void ResetPool()
		{
			while (PoolCounter > 0)
			{
				Pool[--PoolCounter].Reset();
			}
		}

		public static void ResetTo(ref EarlyBeforeDeathRemovalEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static EarlyBeforeDeathRemovalEvent FromPool()
		{
			return MinEvent.FromPool(ref Pool, ref PoolCounter);
		}

		public override bool Dispatch(IEventHandler Handler)
		{
			if (!base.Dispatch(Handler))
			{
				return false;
			}
			return Handler.HandleEvent(this);
		}

		public static void Send(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, bool AlwaysUsePopups = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
		{
			bool flag = true;
			try
			{
				if (flag && GameObject.Validate(ref Dying) && Dying.HasRegisteredEvent("EarlyBeforeDeathRemoval"))
				{
					Event @event = Event.New("EarlyBeforeDeathRemoval");
					@event.SetParameter("Dying", Dying);
					@event.SetParameter("Killer", Killer);
					@event.SetParameter("Weapon", Weapon);
					@event.SetParameter("Projectile", Projectile);
					@event.SetParameter("KillerText", KillerText);
					@event.SetParameter("Reason", Reason);
					@event.SetParameter("ThirdPersonReason", ThirdPersonReason);
					@event.SetFlag("Accidental", Accidental);
					@event.SetFlag("AlwaysUsePopups", AlwaysUsePopups);
					flag = Dying.FireEvent(@event);
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("EarlyBeforeDeathRemoval registered event handling", x);
			}
			try
			{
				if (flag && GameObject.Validate(ref Dying) && Dying.WantEvent(ID, MinEvent.CascadeLevel))
				{
					EarlyBeforeDeathRemovalEvent earlyBeforeDeathRemovalEvent = FromPool();
					earlyBeforeDeathRemovalEvent.Dying = Dying;
					earlyBeforeDeathRemovalEvent.Killer = Killer;
					earlyBeforeDeathRemovalEvent.Weapon = Weapon;
					earlyBeforeDeathRemovalEvent.Projectile = Projectile;
					earlyBeforeDeathRemovalEvent.Accidental = Accidental;
					earlyBeforeDeathRemovalEvent.AlwaysUsePopups = AlwaysUsePopups;
					earlyBeforeDeathRemovalEvent.KillerText = KillerText;
					earlyBeforeDeathRemovalEvent.Reason = Reason;
					earlyBeforeDeathRemovalEvent.ThirdPersonReason = ThirdPersonReason;
					flag = Dying.HandleEvent(earlyBeforeDeathRemovalEvent);
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogError("EarlyBeforeDeathRemoval MinEvent handling", x2);
			}
		}
	}
}
