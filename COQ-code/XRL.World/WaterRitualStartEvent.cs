using XRL.World.Parts;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class WaterRitualStartEvent : PooledEvent<WaterRitualStartEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Actor;

		public GameObject SpeakingWith;

		public WaterRitualRecord Record;

		public bool Initial;

		public override int GetCascadeLevel()
		{
			return CascadeLevel;
		}

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Actor = null;
			SpeakingWith = null;
			Record = null;
			Initial = false;
		}

		public static void Send(GameObject Actor, GameObject SpeakingWith, WaterRitualRecord Record, bool Initial)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("WaterRitualStart"))
			{
				Event @event = Event.New("WaterRitualStart");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("SpeakingWith", SpeakingWith);
				@event.SetParameter("Record", Record);
				@event.SetFlag("Initial", Initial);
				flag = Actor.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<WaterRitualStartEvent>.ID, CascadeLevel))
			{
				WaterRitualStartEvent waterRitualStartEvent = PooledEvent<WaterRitualStartEvent>.FromPool();
				waterRitualStartEvent.Actor = Actor;
				waterRitualStartEvent.SpeakingWith = SpeakingWith;
				waterRitualStartEvent.Record = Record;
				waterRitualStartEvent.Initial = Initial;
				flag = Actor.HandleEvent(waterRitualStartEvent);
			}
		}
	}
}
