namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class CheckLoadAmmoEvent : PooledEvent<CheckLoadAmmoEvent>
	{
		public GameObject Object;

		public GameObject Actor;

		public string Message;

		public bool ActivePartsIgnoreSubject;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Actor = null;
			Message = null;
			ActivePartsIgnoreSubject = false;
		}

		public static bool Check(GameObject Object, GameObject Actor, out string Message, bool ActivePartsIgnoreSubject = false)
		{
			Message = null;
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CheckLoadAmmo"))
			{
				Event @event = Event.New("CheckLoadAmmo");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Loader", Actor);
				@event.SetParameter("Message", Message);
				@event.SetFlag("ActivePartsIgnoreSubject", ActivePartsIgnoreSubject);
				flag = Object.FireEvent(@event);
				Message = @event.GetStringParameter("Message");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckLoadAmmoEvent>.ID, MinEvent.CascadeLevel))
			{
				CheckLoadAmmoEvent checkLoadAmmoEvent = PooledEvent<CheckLoadAmmoEvent>.FromPool();
				checkLoadAmmoEvent.Object = Object;
				checkLoadAmmoEvent.Actor = Actor;
				checkLoadAmmoEvent.Message = Message;
				checkLoadAmmoEvent.ActivePartsIgnoreSubject = ActivePartsIgnoreSubject;
				flag = Object.HandleEvent(checkLoadAmmoEvent);
				Message = checkLoadAmmoEvent.Message;
			}
			return flag;
		}

		public static bool Check(GameObject Object, GameObject Actor, bool ActivePartsIgnoreSubject = false)
		{
			string Message;
			return Check(Object, Actor, out Message, ActivePartsIgnoreSubject);
		}
	}
}
