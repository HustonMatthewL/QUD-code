using System;
using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AfterDieEvent : IDeathEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterDieEvent), null, CountPool, ResetPool);

		private static List<AfterDieEvent> Pool;

		private static int PoolCounter;

		public AfterDieEvent()
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

		public static void ResetTo(ref AfterDieEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static AfterDieEvent FromPool()
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
				if (flag && GameObject.Validate(ref Dying) && Dying.HasRegisteredEvent("AfterDie"))
				{
					Event @event = Event.New("AfterDie");
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
				MetricsManager.LogError("AfterDie registered event handling", x);
			}
			try
			{
				if (flag && GameObject.Validate(ref Dying) && Dying.WantEvent(ID, MinEvent.CascadeLevel))
				{
					AfterDieEvent afterDieEvent = FromPool();
					afterDieEvent.Dying = Dying;
					afterDieEvent.Killer = Killer;
					afterDieEvent.Weapon = Weapon;
					afterDieEvent.Projectile = Projectile;
					afterDieEvent.Accidental = Accidental;
					afterDieEvent.AlwaysUsePopups = AlwaysUsePopups;
					afterDieEvent.KillerText = KillerText;
					afterDieEvent.Reason = Reason;
					afterDieEvent.ThirdPersonReason = ThirdPersonReason;
					flag = Dying.HandleEvent(afterDieEvent);
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogError("AfterDie MinEvent handling", x2);
			}
		}
	}
}
