namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class GetThrownWeaponFlexPhaseProviderEvent : PooledEvent<GetThrownWeaponFlexPhaseProviderEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Object;

		public GameObject Actor;

		public IThrownWeaponFlexPhaseProvider Provider;

		public int Priority;

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
			Provider = null;
			Priority = 0;
		}

		public static IThrownWeaponFlexPhaseProvider GetFor(GameObject Object, GameObject Actor)
		{
			IThrownWeaponFlexPhaseProvider thrownWeaponFlexPhaseProvider = null;
			int num = 0;
			bool flag = true;
			if (flag)
			{
				bool flag2 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetThrownWeaponFlexPhaseProvider");
				bool flag3 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetThrownWeaponFlexPhaseProvider");
				if (flag2 || flag3)
				{
					Event @event = Event.New("GetThrownWeaponFlexPhaseProvider");
					@event.SetParameter("Object", Object);
					@event.SetParameter("Actor", Actor);
					@event.SetParameter("Provider", thrownWeaponFlexPhaseProvider);
					@event.SetParameter("Priority", num);
					flag = (!flag2 || Object.FireEvent(@event)) && (!flag3 || Actor.FireEvent(@event));
					thrownWeaponFlexPhaseProvider = @event.GetParameter("Provider") as IThrownWeaponFlexPhaseProvider;
					num = @event.GetIntParameter("Priority");
				}
			}
			if (flag)
			{
				bool flag4 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetThrownWeaponFlexPhaseProviderEvent>.ID, CascadeLevel);
				bool flag5 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetThrownWeaponFlexPhaseProviderEvent>.ID, CascadeLevel);
				if (flag4 || flag5)
				{
					GetThrownWeaponFlexPhaseProviderEvent getThrownWeaponFlexPhaseProviderEvent = PooledEvent<GetThrownWeaponFlexPhaseProviderEvent>.FromPool();
					getThrownWeaponFlexPhaseProviderEvent.Object = Object;
					getThrownWeaponFlexPhaseProviderEvent.Actor = Actor;
					getThrownWeaponFlexPhaseProviderEvent.Provider = thrownWeaponFlexPhaseProvider;
					getThrownWeaponFlexPhaseProviderEvent.Priority = num;
					flag = (!flag4 || Object.HandleEvent(getThrownWeaponFlexPhaseProviderEvent)) && (!flag5 || Actor.HandleEvent(getThrownWeaponFlexPhaseProviderEvent));
					thrownWeaponFlexPhaseProvider = getThrownWeaponFlexPhaseProviderEvent.Provider;
					num = getThrownWeaponFlexPhaseProviderEvent.Priority;
				}
			}
			return thrownWeaponFlexPhaseProvider;
		}
	}
}
