using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class TookEnvironmentalDamageEvent : IDamageEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(TookEnvironmentalDamageEvent), null, CountPool, ResetPool);

		private static List<TookEnvironmentalDamageEvent> Pool;

		private static int PoolCounter;

		public TookEnvironmentalDamageEvent()
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

		public static void ResetTo(ref TookEnvironmentalDamageEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static TookEnvironmentalDamageEvent FromPool()
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

		public static void Send(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, bool Indirect = false, Event ParentEvent = null)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("TookEnvironmentalDamage"))
			{
				Event @event = Event.New("TookEnvironmentalDamage");
				@event.SetParameter("Damage", Damage);
				@event.SetParameter("Defender", Object);
				@event.SetParameter("Owner", Actor);
				@event.SetParameter("Source", Source);
				@event.SetFlag("Indirect", Indirect);
				ParentEvent?.PreprocessChildEvent(@event);
				flag = Object.FireEvent(@event, ParentEvent);
				ParentEvent?.ProcessChildEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
			{
				TookEnvironmentalDamageEvent tookEnvironmentalDamageEvent = FromPool();
				tookEnvironmentalDamageEvent.Damage = Damage;
				tookEnvironmentalDamageEvent.Object = Object;
				tookEnvironmentalDamageEvent.Actor = Actor;
				tookEnvironmentalDamageEvent.Source = Source;
				tookEnvironmentalDamageEvent.Indirect = Indirect;
				ParentEvent?.PreprocessChildEvent(tookEnvironmentalDamageEvent);
				flag = Object.HandleEvent(tookEnvironmentalDamageEvent);
				ParentEvent?.ProcessChildEvent(tookEnvironmentalDamageEvent);
			}
		}
	}
}
