using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class QuerySlotListEvent : PooledEvent<QuerySlotListEvent>
	{
		public GameObject Subject;

		public GameObject Object;

		public List<BodyPart> SlotList = new List<BodyPart>();

		public string FailureMessage;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Subject = null;
			Object = null;
			SlotList.Clear();
			FailureMessage = null;
		}

		public static List<BodyPart> GetFor(GameObject Subject, GameObject Object, ref string FailureMessage)
		{
			bool flag = true;
			QuerySlotListEvent querySlotListEvent = PooledEvent<QuerySlotListEvent>.FromPool();
			querySlotListEvent.Subject = Subject;
			querySlotListEvent.Object = Object;
			querySlotListEvent.SlotList.Clear();
			querySlotListEvent.FailureMessage = FailureMessage;
			if (flag && GameObject.Validate(ref Subject) && Subject.HasRegisteredEvent("QuerySlotList"))
			{
				Event @event = Event.New("QuerySlotList");
				@event.SetParameter("Subject", Subject);
				@event.SetParameter("Object", Object);
				@event.SetParameter("SlotList", querySlotListEvent.SlotList);
				@event.SetParameter("FailureMessage", FailureMessage);
				flag = Subject.FireEvent(@event);
				FailureMessage = @event.GetStringParameter("FailureMessage");
				querySlotListEvent.FailureMessage = FailureMessage;
			}
			if (flag && GameObject.Validate(ref Subject) && Subject.WantEvent(PooledEvent<QuerySlotListEvent>.ID, MinEvent.CascadeLevel))
			{
				flag = Subject.HandleEvent(querySlotListEvent);
				FailureMessage = querySlotListEvent.FailureMessage;
			}
			return querySlotListEvent.SlotList;
		}

		public static List<BodyPart> GetFor(GameObject Subject, GameObject Object)
		{
			string FailureMessage = null;
			return GetFor(Subject, Object, ref FailureMessage);
		}
	}
}
