using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AttackerDealingDamageEvent : IDamageEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(AttackerDealingDamageEvent), null, CountPool, ResetPool);

		private static List<AttackerDealingDamageEvent> Pool;

		private static int PoolCounter;

		public AttackerDealingDamageEvent()
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

		public static void ResetTo(ref AttackerDealingDamageEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static AttackerDealingDamageEvent FromPool()
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

		public static bool Check(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, GameObject Weapon = null, GameObject Projectile = null, bool Indirect = false, Event ParentEvent = null)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AttackerDealingDamage"))
			{
				Event @event = Event.New("AttackerDealingDamage");
				@event.SetParameter("Damage", Damage);
				@event.SetParameter("Object", Object);
				@event.SetParameter("Owner", Actor);
				@event.SetParameter("Source", Source);
				@event.SetParameter("Weapon", Weapon);
				@event.SetParameter("Projectile", Projectile);
				@event.SetFlag("Indirect", Indirect);
				ParentEvent?.PreprocessChildEvent(@event);
				flag = Actor.FireEvent(@event, ParentEvent);
				ParentEvent?.ProcessChildEvent(@event);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
			{
				AttackerDealingDamageEvent attackerDealingDamageEvent = FromPool();
				attackerDealingDamageEvent.Damage = Damage;
				attackerDealingDamageEvent.Object = Object;
				attackerDealingDamageEvent.Actor = Actor;
				attackerDealingDamageEvent.Source = Source;
				attackerDealingDamageEvent.Weapon = Weapon;
				attackerDealingDamageEvent.Projectile = Projectile;
				attackerDealingDamageEvent.Indirect = Indirect;
				ParentEvent?.PreprocessChildEvent(attackerDealingDamageEvent);
				flag = Actor.HandleEvent(attackerDealingDamageEvent);
				ParentEvent?.ProcessChildEvent(attackerDealingDamageEvent);
			}
			return flag;
		}
	}
}
