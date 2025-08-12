namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class GetLevelUpDiceEvent : PooledEvent<GetLevelUpDiceEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Actor;

		public int Level;

		public string BaseHPGain;

		public string BaseSPGain;

		public string BaseMPGain;

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
			Level = 0;
			BaseHPGain = null;
			BaseSPGain = null;
			BaseMPGain = null;
		}

		public static void GetFor(GameObject Actor, int Level, ref string BaseHPGain, ref string BaseSPGain, ref string BaseMPGain)
		{
			if (Actor.HasRegisteredEvent("GetLevelUpDice"))
			{
				Event @event = Event.New("GetLevelUpDice");
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Level", Level);
				@event.SetParameter("BaseHPGain", BaseHPGain);
				@event.SetParameter("BaseSPGain", BaseSPGain);
				@event.SetParameter("BaseMPGain", BaseMPGain);
				bool num = Actor.FireEvent(@event);
				BaseHPGain = @event.GetStringParameter("BaseHPGain");
				BaseSPGain = @event.GetStringParameter("BaseSPGain");
				BaseMPGain = @event.GetStringParameter("BaseMPGain");
				if (!num)
				{
					return;
				}
			}
			if (Actor.WantEvent(PooledEvent<GetLevelUpDiceEvent>.ID, CascadeLevel))
			{
				GetLevelUpDiceEvent getLevelUpDiceEvent = PooledEvent<GetLevelUpDiceEvent>.FromPool();
				getLevelUpDiceEvent.Actor = Actor;
				getLevelUpDiceEvent.Level = Level;
				getLevelUpDiceEvent.BaseHPGain = BaseHPGain;
				getLevelUpDiceEvent.BaseSPGain = BaseSPGain;
				getLevelUpDiceEvent.BaseMPGain = BaseMPGain;
				Actor.HandleEvent(getLevelUpDiceEvent);
				BaseHPGain = getLevelUpDiceEvent.BaseHPGain;
				BaseSPGain = getLevelUpDiceEvent.BaseSPGain;
				BaseMPGain = getLevelUpDiceEvent.BaseMPGain;
			}
		}
	}
}
