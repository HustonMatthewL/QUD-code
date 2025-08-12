namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetShieldBlockPreferenceEvent : PooledEvent<GetShieldBlockPreferenceEvent>
	{
		public GameObject Shield;

		public GameObject Attacker;

		public GameObject Defender;

		public int Preference;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Shield = null;
			Attacker = null;
			Defender = null;
			Preference = 0;
		}

		public static GetShieldBlockPreferenceEvent FromPool(GameObject Shield, GameObject Attacker, GameObject Defender, int Preference)
		{
			GetShieldBlockPreferenceEvent getShieldBlockPreferenceEvent = PooledEvent<GetShieldBlockPreferenceEvent>.FromPool();
			getShieldBlockPreferenceEvent.Shield = Shield;
			getShieldBlockPreferenceEvent.Attacker = Attacker;
			getShieldBlockPreferenceEvent.Defender = Defender;
			getShieldBlockPreferenceEvent.Preference = Preference;
			return getShieldBlockPreferenceEvent;
		}

		public static int GetFor(GameObject Shield, GameObject Attacker = null, GameObject Defender = null, int Preference = 0)
		{
			if (GameObject.Validate(ref Shield))
			{
				if (Shield.HasRegisteredEvent("GetShieldBlockPreference"))
				{
					Event @event = Event.New("GetShieldBlockPreference");
					@event.SetParameter("Shield", Shield);
					@event.SetParameter("Attacker", Attacker);
					@event.SetParameter("Defender", Defender);
					@event.SetParameter("Preference", Preference);
					bool num = Shield.FireEvent(@event);
					Preference = @event.GetIntParameter("Preference");
					if (!num)
					{
						return Preference;
					}
				}
				if (Shield.WantEvent(PooledEvent<GetShieldBlockPreferenceEvent>.ID, MinEvent.CascadeLevel))
				{
					GetShieldBlockPreferenceEvent getShieldBlockPreferenceEvent = FromPool(Shield, Attacker, Defender, Preference);
					Shield.HandleEvent(getShieldBlockPreferenceEvent);
					Preference = getShieldBlockPreferenceEvent.Preference;
					return Preference;
				}
			}
			return Preference;
		}
	}
}
