using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class BeforeApplyDamageEvent : IDamageEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeApplyDamageEvent), null, CountPool, ResetPool);

		public new static readonly int CascadeLevel = 17;

		private static List<BeforeApplyDamageEvent> Pool;

		private static int PoolCounter;

		public BeforeApplyDamageEvent()
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

		public static void ResetTo(ref BeforeApplyDamageEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static BeforeApplyDamageEvent FromPool()
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
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BeforeApplyDamage"))
			{
				Event @event = Event.New("BeforeApplyDamage");
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
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
			{
				BeforeApplyDamageEvent beforeApplyDamageEvent = FromPool();
				beforeApplyDamageEvent.Damage = Damage;
				beforeApplyDamageEvent.Object = Object;
				beforeApplyDamageEvent.Actor = Actor;
				beforeApplyDamageEvent.Source = Source;
				beforeApplyDamageEvent.Weapon = Weapon;
				beforeApplyDamageEvent.Projectile = Projectile;
				beforeApplyDamageEvent.Indirect = Indirect;
				ParentEvent?.PreprocessChildEvent(beforeApplyDamageEvent);
				flag = Object.HandleEvent(beforeApplyDamageEvent);
				ParentEvent?.ProcessChildEvent(beforeApplyDamageEvent);
			}
			return flag;
		}
	}
}
