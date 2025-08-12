using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class TookDamageEvent : IDamageEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(TookDamageEvent), null, CountPool, ResetPool);

		public new static readonly int CascadeLevel = 17;

		private static List<TookDamageEvent> Pool;

		private static int PoolCounter;

		public TookDamageEvent()
		{
			base.ID = ID;
		}

		public override int GetCascadeLevel()
		{
			return CascadeLevel;
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

		public static void ResetTo(ref TookDamageEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static TookDamageEvent FromPool()
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

		public static void Send(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, GameObject Weapon = null, GameObject Projectile = null, bool Indirect = false, Event ParentEvent = null)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("TookDamage"))
			{
				Event @event = Event.New("TookDamage");
				@event.SetParameter("Damage", Damage);
				@event.SetParameter("Defender", Object);
				@event.SetParameter("Owner", Actor);
				@event.SetParameter("Attacker", Actor);
				@event.SetParameter("Source", Source);
				@event.SetParameter("Weapon", Weapon);
				@event.SetParameter("Projectile", Projectile);
				@event.SetFlag("Indirect", Indirect);
				ParentEvent?.PreprocessChildEvent(@event);
				flag = Object.FireEvent(@event, ParentEvent);
				ParentEvent?.ProcessChildEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
			{
				TookDamageEvent tookDamageEvent = FromPool();
				tookDamageEvent.Damage = Damage;
				tookDamageEvent.Object = Object;
				tookDamageEvent.Actor = Actor;
				tookDamageEvent.Source = Source;
				tookDamageEvent.Weapon = Weapon;
				tookDamageEvent.Projectile = Projectile;
				tookDamageEvent.Indirect = Indirect;
				ParentEvent?.PreprocessChildEvent(tookDamageEvent);
				flag = Object.HandleEvent(tookDamageEvent);
				ParentEvent?.ProcessChildEvent(tookDamageEvent);
			}
		}
	}
}
