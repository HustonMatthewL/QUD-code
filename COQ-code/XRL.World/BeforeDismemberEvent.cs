using XRL.World.Anatomy;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class BeforeDismemberEvent : PooledEvent<BeforeDismemberEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Object;

		public BodyPart Part;

		public IInventory Where;

		public bool Silent;

		public bool Obliterate;

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
			Part = null;
			Where = null;
			Silent = false;
			Obliterate = false;
		}

		public static bool Check(GameObject Object, BodyPart Part, IInventory Where = null, bool Silent = false, bool Obliterate = false)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BeforeDismember"))
			{
				Event @event = Event.New("BeforeDismember");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Part", Part);
				@event.SetParameter("Where", Where);
				@event.SetSilent(Silent);
				@event.SetFlag("Obliterate", Obliterate);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("Dismember"))
			{
				Event event2 = Event.New("Dismember");
				event2.SetParameter("Object", Object);
				event2.SetParameter("Part", Part);
				event2.SetParameter("Where", Where);
				event2.SetSilent(Silent);
				event2.SetFlag("Obliterate", Obliterate);
				flag = Object.FireEvent(event2);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<BeforeDismemberEvent>.ID, CascadeLevel))
			{
				BeforeDismemberEvent beforeDismemberEvent = PooledEvent<BeforeDismemberEvent>.FromPool();
				beforeDismemberEvent.Object = Object;
				beforeDismemberEvent.Part = Part;
				beforeDismemberEvent.Where = Where;
				beforeDismemberEvent.Silent = Silent;
				beforeDismemberEvent.Obliterate = Obliterate;
				flag = Object.HandleEvent(beforeDismemberEvent);
			}
			return flag;
		}
	}
}
