using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AfterRelicGeneratedEvent : PooledEvent<AfterRelicGeneratedEvent>
	{
		public GameObject Object;

		public string NameElement;

		public List<string> Elements;

		public string Type;

		public string Subtype;

		public int Tier;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			NameElement = null;
			Elements = null;
			Type = null;
			Subtype = null;
			Tier = 0;
		}

		public static void Send(GameObject Object, List<string> Elements, string NameElement, string Type, string Subtype, int Tier)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AfterRelicGenerated"))
			{
				Event @event = Event.New("AfterRelicGenerated");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Elements", Elements);
				@event.SetParameter("NameElement", NameElement);
				@event.SetParameter("Type", Type);
				@event.SetParameter("Subtype", Subtype);
				@event.SetParameter("Tier", Tier);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<AfterRelicGeneratedEvent>.ID, MinEvent.CascadeLevel))
			{
				AfterRelicGeneratedEvent afterRelicGeneratedEvent = PooledEvent<AfterRelicGeneratedEvent>.FromPool();
				afterRelicGeneratedEvent.Object = Object;
				afterRelicGeneratedEvent.Elements = Elements;
				afterRelicGeneratedEvent.NameElement = NameElement;
				afterRelicGeneratedEvent.Type = Type;
				afterRelicGeneratedEvent.Subtype = Subtype;
				afterRelicGeneratedEvent.Tier = Tier;
				flag = Object.HandleEvent(afterRelicGeneratedEvent);
			}
		}
	}
}
