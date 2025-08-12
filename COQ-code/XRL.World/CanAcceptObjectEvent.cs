namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class CanAcceptObjectEvent : PooledEvent<CanAcceptObjectEvent>
	{
		public GameObject Object;

		public GameObject Holder;

		public GameObject Container;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Holder = null;
			Container = null;
		}

		public static bool Relevant(GameObject Container)
		{
			if (GameObject.Validate(ref Container))
			{
				if (Container.HasRegisteredEvent("CanAcceptObject"))
				{
					return true;
				}
				if (Container.WantEvent(PooledEvent<CanAcceptObjectEvent>.ID, MinEvent.CascadeLevel))
				{
					return true;
				}
			}
			return false;
		}

		public static bool Check(GameObject Object, GameObject Holder, GameObject Container)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Container) && Container.HasRegisteredEvent("CanAcceptObject"))
			{
				Event @event = Event.New("CanAcceptObject");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Holder", Holder);
				@event.SetParameter("Container", Container);
				flag = Container.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Container) && Container.WantEvent(PooledEvent<CanAcceptObjectEvent>.ID, MinEvent.CascadeLevel))
			{
				CanAcceptObjectEvent canAcceptObjectEvent = PooledEvent<CanAcceptObjectEvent>.FromPool();
				canAcceptObjectEvent.Object = Object;
				canAcceptObjectEvent.Holder = Holder;
				canAcceptObjectEvent.Container = Container;
				flag = Container.HandleEvent(canAcceptObjectEvent);
			}
			return flag;
		}
	}
}
