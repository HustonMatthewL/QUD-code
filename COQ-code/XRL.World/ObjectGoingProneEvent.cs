using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class ObjectGoingProneEvent : IObjectCellInteractionEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(ObjectGoingProneEvent), null, CountPool, ResetPool);

		private static List<ObjectGoingProneEvent> Pool;

		private static int PoolCounter;

		public bool Voluntary;

		public bool UsePopups;

		public ObjectGoingProneEvent()
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

		public static void ResetTo(ref ObjectGoingProneEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static ObjectGoingProneEvent FromPool()
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
			Voluntary = false;
			UsePopups = false;
		}

		public static void Send(GameObject Object, Cell Cell, bool Voluntary = false, bool UsePopups = false)
		{
			bool flag = true;
			if (flag && Cell != null && Cell.HasObjectWithRegisteredEvent("ObjectGoingProne"))
			{
				Event @event = Event.New("ObjectGoingProne");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Cell", Cell);
				@event.SetFlag("Voluntary", Voluntary);
				@event.SetFlag("UsePopups", UsePopups);
				flag = Cell.FireEvent(@event);
			}
			if (flag && Cell != null && Cell.WantEvent(ID, MinEvent.CascadeLevel))
			{
				ObjectGoingProneEvent objectGoingProneEvent = FromPool();
				objectGoingProneEvent.Object = Object;
				objectGoingProneEvent.Cell = Cell;
				objectGoingProneEvent.Forced = !Voluntary;
				objectGoingProneEvent.System = false;
				objectGoingProneEvent.IgnoreGravity = false;
				objectGoingProneEvent.NoStack = false;
				objectGoingProneEvent.Direction = ".";
				objectGoingProneEvent.Type = null;
				objectGoingProneEvent.Dragging = null;
				objectGoingProneEvent.Actor = null;
				objectGoingProneEvent.ForceSwap = null;
				objectGoingProneEvent.Ignore = null;
				objectGoingProneEvent.Voluntary = Voluntary;
				objectGoingProneEvent.UsePopups = UsePopups;
				flag = Cell.HandleEvent(objectGoingProneEvent);
			}
		}
	}
}
