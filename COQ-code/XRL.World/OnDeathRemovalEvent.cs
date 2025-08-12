using System;
using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class OnDeathRemovalEvent : IDeathEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(OnDeathRemovalEvent), null, CountPool, ResetPool);

		private static List<OnDeathRemovalEvent> Pool;

		private static int PoolCounter;

		public OnDeathRemovalEvent()
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

		public static void ResetTo(ref OnDeathRemovalEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static OnDeathRemovalEvent FromPool()
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
				if (flag && GameObject.Validate(ref Dying) && Dying.HasRegisteredEvent("OnDeathRemoval"))
				{
					Event @event = Event.New("OnDeathRemoval");
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
				MetricsManager.LogError("OnDeathRemoval registered event handling", x);
			}
			try
			{
				if (flag && GameObject.Validate(ref Dying) && Dying.WantEvent(ID, MinEvent.CascadeLevel))
				{
					OnDeathRemovalEvent onDeathRemovalEvent = FromPool();
					onDeathRemovalEvent.Dying = Dying;
					onDeathRemovalEvent.Killer = Killer;
					onDeathRemovalEvent.Weapon = Weapon;
					onDeathRemovalEvent.Projectile = Projectile;
					onDeathRemovalEvent.Accidental = Accidental;
					onDeathRemovalEvent.AlwaysUsePopups = AlwaysUsePopups;
					onDeathRemovalEvent.KillerText = KillerText;
					onDeathRemovalEvent.Reason = Reason;
					onDeathRemovalEvent.ThirdPersonReason = ThirdPersonReason;
					flag = Dying.HandleEvent(onDeathRemovalEvent);
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogError("OnDeathRemoval MinEvent handling", x2);
			}
		}
	}
}
