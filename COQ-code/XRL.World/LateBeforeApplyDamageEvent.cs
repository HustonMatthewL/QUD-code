using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class LateBeforeApplyDamageEvent : IDamageEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(LateBeforeApplyDamageEvent), null, CountPool, ResetPool);

		private static List<LateBeforeApplyDamageEvent> Pool;

		private static int PoolCounter;

		public LateBeforeApplyDamageEvent()
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

		public static void ResetTo(ref LateBeforeApplyDamageEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static LateBeforeApplyDamageEvent FromPool()
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
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("LateBeforeApplyDamage"))
			{
				Event @event = Event.New("LateBeforeApplyDamage");
				@event.SetParameter("Damage", Damage);
				@event.SetParameter("Object", Object);
				@event.SetParameter("Owner", Actor);
				@event.SetParameter("Source", Source);
				@event.SetParameter("Weapon", Weapon);
				@event.SetParameter("Projectile", Projectile);
				@event.SetFlag("Indirect", Indirect);
				ParentEvent?.PreprocessChildEvent(@event);
				flag = Object.FireEvent(@event, ParentEvent);
				ParentEvent?.ProcessChildEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
			{
				LateBeforeApplyDamageEvent lateBeforeApplyDamageEvent = FromPool();
				lateBeforeApplyDamageEvent.Damage = Damage;
				lateBeforeApplyDamageEvent.Object = Object;
				lateBeforeApplyDamageEvent.Actor = Actor;
				lateBeforeApplyDamageEvent.Source = Source;
				lateBeforeApplyDamageEvent.Weapon = Weapon;
				lateBeforeApplyDamageEvent.Projectile = Projectile;
				lateBeforeApplyDamageEvent.Indirect = Indirect;
				ParentEvent?.PreprocessChildEvent(lateBeforeApplyDamageEvent);
				flag = Object.HandleEvent(lateBeforeApplyDamageEvent);
				ParentEvent?.ProcessChildEvent(lateBeforeApplyDamageEvent);
			}
			return flag;
		}
	}
}
