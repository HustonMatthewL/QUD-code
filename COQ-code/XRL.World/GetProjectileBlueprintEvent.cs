namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetProjectileBlueprintEvent : PooledEvent<GetProjectileBlueprintEvent>
	{
		public GameObject Object;

		public string Blueprint;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Object = null;
			Blueprint = null;
		}

		public static string GetFor(GameObject Object)
		{
			bool flag = true;
			string text = null;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetProjectileBlueprint"))
			{
				Event @event = Event.New("GetProjectileBlueprint");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Blueprint", text);
				flag = Object.FireEvent(@event);
				text = @event.GetStringParameter("Blueprint");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetProjectileBlueprintEvent>.ID, MinEvent.CascadeLevel))
			{
				GetProjectileBlueprintEvent getProjectileBlueprintEvent = PooledEvent<GetProjectileBlueprintEvent>.FromPool();
				getProjectileBlueprintEvent.Object = Object;
				getProjectileBlueprintEvent.Blueprint = text;
				flag = Object.HandleEvent(getProjectileBlueprintEvent);
				text = getProjectileBlueprintEvent.Blueprint;
			}
			return text;
		}
	}
}
