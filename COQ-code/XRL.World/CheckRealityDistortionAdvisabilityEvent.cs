namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class CheckRealityDistortionAdvisabilityEvent : PooledEvent<CheckRealityDistortionAdvisabilityEvent>
	{
		public GameObject Object;

		public Cell Cell;

		public GameObject Actor;

		public GameObject Device;

		public IPart Mutation;

		public int Threshold;

		public int Penetration;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Cell = null;
			Actor = null;
			Device = null;
			Mutation = null;
			Threshold = 0;
			Penetration = 0;
		}

		public static bool Check(GameObject Object = null, Cell Cell = null, GameObject Actor = null, GameObject Device = null, IPart Mutation = null, int? Threshold = null, int Penetration = 0)
		{
			bool flag = true;
			int num = Threshold ?? ((Mutation != null) ? 80 : 30);
			if (flag)
			{
				bool flag2 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CheckRealityDistortionAdvisability");
				bool flag3 = Cell?.HasObjectWithRegisteredEvent("CheckRealityDistortionAdvisability") ?? false;
				if (flag2 || flag3)
				{
					Event @event = Event.New("CheckRealityDistortionAdvisability");
					@event.SetParameter("Object", Object);
					@event.SetParameter("Cell", Cell);
					@event.SetParameter("Actor", Actor);
					@event.SetParameter("Mutation", Mutation);
					@event.SetParameter("Device", Device);
					@event.SetParameter("Threshold", num);
					@event.SetParameter("Penetration", Penetration);
					flag = (!flag2 || Object.FireEvent(@event)) && (!flag3 || Cell.FireEvent(@event));
				}
			}
			if (flag)
			{
				bool flag4 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckRealityDistortionAdvisabilityEvent>.ID, MinEvent.CascadeLevel);
				bool flag5 = Cell?.WantEvent(PooledEvent<CheckRealityDistortionAdvisabilityEvent>.ID, MinEvent.CascadeLevel) ?? false;
				if (flag4 || flag5)
				{
					CheckRealityDistortionAdvisabilityEvent checkRealityDistortionAdvisabilityEvent = PooledEvent<CheckRealityDistortionAdvisabilityEvent>.FromPool();
					checkRealityDistortionAdvisabilityEvent.Object = Object;
					checkRealityDistortionAdvisabilityEvent.Cell = Cell;
					checkRealityDistortionAdvisabilityEvent.Actor = Actor;
					checkRealityDistortionAdvisabilityEvent.Mutation = Mutation;
					checkRealityDistortionAdvisabilityEvent.Device = Device;
					checkRealityDistortionAdvisabilityEvent.Threshold = num;
					checkRealityDistortionAdvisabilityEvent.Penetration = Penetration;
					flag = (!flag4 || Object.HandleEvent(checkRealityDistortionAdvisabilityEvent)) && (!flag5 || Cell.HandleEvent(checkRealityDistortionAdvisabilityEvent));
				}
			}
			return flag;
		}
	}
}
