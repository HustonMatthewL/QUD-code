using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class WasReplicatedEvent : IReplicationEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(WasReplicatedEvent), null, CountPool, ResetPool);

		private static List<WasReplicatedEvent> Pool;

		private static int PoolCounter;

		public GameObject Replica;

		public WasReplicatedEvent()
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

		public static void ResetTo(ref WasReplicatedEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static WasReplicatedEvent FromPool()
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

		public override void Reset()
		{
			base.Reset();
			Replica = null;
		}

		public static void Send(GameObject Object, GameObject Actor, GameObject Replica, string Context = null, bool Temporary = false)
		{
			if (GameObject.Validate(ref Object) && Object.HasRegisteredEvent("WasReplicated"))
			{
				Event @event = Event.New("WasReplicated");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Context", Context);
				@event.SetParameter("Temporary", Temporary ? 1 : 0);
				@event.SetParameter("Replica", Replica);
				Object.FireEvent(@event);
			}
			if (GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
			{
				WasReplicatedEvent wasReplicatedEvent = FromPool();
				wasReplicatedEvent.Object = Object;
				wasReplicatedEvent.Actor = Actor;
				wasReplicatedEvent.Replica = Replica;
				wasReplicatedEvent.Context = Context;
				wasReplicatedEvent.Temporary = Temporary;
				Object.HandleEvent(wasReplicatedEvent);
			}
		}
	}
}
