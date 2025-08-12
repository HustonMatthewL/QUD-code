namespace XRL.World
{
	[GameEvent(Cascade = 15, Cache = Cache.Pool)]
	public class GenericDeepQueryEvent : PooledEvent<GenericDeepQueryEvent>
	{
		public new static readonly int CascadeLevel = 15;

		public GameObject Object;

		public GameObject Subject;

		public GameObject Source;

		public string Query;

		public int Level;

		public bool Result;

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
			Object = null;
			Subject = null;
			Source = null;
			Query = null;
			Level = 0;
			Result = false;
		}

		public static bool Check(GameObject Object, string Query, GameObject Subject = null, GameObject Source = null, int Level = 0)
		{
			bool flag = true;
			bool flag2 = false;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GenericDeepQuery"))
			{
				Event @event = Event.New("GenericDeepQuery");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Subject", Subject);
				@event.SetParameter("Source", Source);
				@event.SetParameter("Query", Query);
				@event.SetParameter("Level", Level);
				@event.SetFlag("Result", flag2);
				flag = Object.FireEvent(@event);
				flag2 = @event.HasFlag("Result");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GenericDeepQueryEvent>.ID, CascadeLevel))
			{
				GenericDeepQueryEvent genericDeepQueryEvent = PooledEvent<GenericDeepQueryEvent>.FromPool();
				genericDeepQueryEvent.Object = Object;
				genericDeepQueryEvent.Subject = Subject;
				genericDeepQueryEvent.Source = Source;
				genericDeepQueryEvent.Query = Query;
				genericDeepQueryEvent.Level = Level;
				genericDeepQueryEvent.Result = flag2;
				flag = Object.HandleEvent(genericDeepQueryEvent);
				flag2 = genericDeepQueryEvent.Result;
			}
			return flag2;
		}
	}
}
