namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class GetTonicDurationEvent : PooledEvent<GetTonicDurationEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public static readonly int PASSES = 3;

		public GameObject Object;

		public GameObject Actor;

		public GameObject Subject;

		public string Type;

		public string Checking;

		public int BaseDuration;

		public int Duration;

		public int Dosage;

		public int Pass;

		public bool Healing;

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
			Actor = null;
			Subject = null;
			Type = null;
			Checking = null;
			BaseDuration = 0;
			Duration = 0;
			Dosage = 0;
			Pass = 0;
			Healing = false;
		}

		public static int GetFor(GameObject Object, GameObject Actor, GameObject Subject, string Type, int BaseDuration, int Dosage = 1, bool Healing = false)
		{
			int num = BaseDuration;
			bool flag = true;
			Event @event = null;
			GetTonicDurationEvent getTonicDurationEvent = null;
			bool flag2 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetTonicDuration");
			bool flag3 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetTonicDuration");
			bool flag4 = GameObject.Validate(ref Subject) && Subject.HasRegisteredEvent("GetTonicDuration");
			bool flag5 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetTonicDurationEvent>.ID, CascadeLevel);
			bool flag6 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetTonicDurationEvent>.ID, CascadeLevel);
			bool flag7 = GameObject.Validate(ref Subject) && Subject.WantEvent(PooledEvent<GetTonicDurationEvent>.ID, CascadeLevel);
			if (flag2 || flag3 || flag4 || flag5 || flag6 || flag7)
			{
				bool flag8 = true;
				int num2 = 1;
				while (flag && flag8 && num2 <= PASSES)
				{
					flag8 = false;
					if (flag && (flag2 || flag3 || flag4))
					{
						if (@event == null)
						{
							@event = Event.New("GetTonicDuration");
							@event.SetParameter("Object", Object);
							@event.SetParameter("Actor", Actor);
							@event.SetParameter("Subject", Subject);
							@event.SetParameter("Type", Type);
							@event.SetParameter("BaseDuration", BaseDuration);
							@event.SetParameter("Dosage", Dosage);
							@event.SetParameter("Pass", num2);
							@event.SetFlag("Healing", Healing);
						}
						@event.SetParameter("Duration", num);
						if (flag && flag2 && GameObject.Validate(ref Object))
						{
							@event.SetParameter("Checking", "Object");
							flag = Object.FireEvent(@event);
							flag8 = true;
						}
						if (flag && flag3 && GameObject.Validate(ref Actor))
						{
							@event.SetParameter("Checking", "Actor");
							flag = Actor.FireEvent(@event);
							flag8 = true;
						}
						if (flag && flag4 && GameObject.Validate(ref Subject))
						{
							@event.SetParameter("Checking", "Subject");
							flag = Subject.FireEvent(@event);
							flag8 = true;
						}
						num = @event.GetIntParameter("Duration");
					}
					if (flag && (flag5 || flag6 || flag7))
					{
						if (getTonicDurationEvent == null)
						{
							getTonicDurationEvent = PooledEvent<GetTonicDurationEvent>.FromPool();
						}
						getTonicDurationEvent.Object = Object;
						getTonicDurationEvent.Actor = Actor;
						getTonicDurationEvent.Subject = Subject;
						getTonicDurationEvent.Type = Type;
						getTonicDurationEvent.BaseDuration = BaseDuration;
						getTonicDurationEvent.Duration = num;
						getTonicDurationEvent.Dosage = Dosage;
						getTonicDurationEvent.Pass = num2;
						getTonicDurationEvent.Healing = Healing;
						if (flag && flag5 && GameObject.Validate(ref Object))
						{
							getTonicDurationEvent.Checking = "Object";
							flag = Object.HandleEvent(getTonicDurationEvent);
							flag8 = true;
						}
						if (flag && flag6 && GameObject.Validate(ref Actor))
						{
							getTonicDurationEvent.Checking = "Actor";
							flag = Actor.HandleEvent(getTonicDurationEvent);
							flag8 = true;
						}
						if (flag && flag7 && GameObject.Validate(ref Subject))
						{
							getTonicDurationEvent.Checking = "Subject";
							flag = Subject.HandleEvent(getTonicDurationEvent);
							flag8 = true;
						}
						num = getTonicDurationEvent.Duration;
					}
					num2++;
				}
			}
			return num;
		}
	}
}
