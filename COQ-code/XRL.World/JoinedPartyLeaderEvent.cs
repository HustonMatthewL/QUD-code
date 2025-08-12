namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class JoinedPartyLeaderEvent : PooledEvent<JoinedPartyLeaderEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Companion;

		public GameObject Leader;

		public Cell PreviousCell;

		public Cell TargetCell;

		public int DistanceFromPreviousCell;

		public int DistanceFromLeader;

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
			Companion = null;
			Leader = null;
			PreviousCell = null;
			TargetCell = null;
			DistanceFromPreviousCell = 0;
			DistanceFromLeader = 0;
		}

		public static void Send(GameObject Companion, GameObject Leader, Cell PreviousCell, Cell TargetCell, int DistanceFromPreviousCell, int DistanceFromLeader)
		{
			bool flag = Companion.HasRegisteredEvent("JoinedPartyLeader");
			bool flag2 = Leader.HasRegisteredEvent("JoinedPartyLeader");
			if (flag || flag2)
			{
				Event @event = Event.New("JoinedPartyLeader");
				@event.SetParameter("Companion", Companion);
				@event.SetParameter("Leader", Leader);
				@event.SetParameter("PreviousCell", PreviousCell);
				@event.SetParameter("TargetCell", TargetCell);
				@event.SetParameter("DistanceFromPreviousCell", DistanceFromPreviousCell);
				@event.SetParameter("DistanceFromLeader", DistanceFromLeader);
				if (flag)
				{
					Companion.FireEvent(@event);
				}
				if (flag2)
				{
					Leader.FireEvent(@event);
				}
			}
			bool num = Companion.WantEvent(PooledEvent<JoinedPartyLeaderEvent>.ID, CascadeLevel);
			bool flag3 = Leader.WantEvent(PooledEvent<JoinedPartyLeaderEvent>.ID, CascadeLevel);
			if (num || flag3)
			{
				JoinedPartyLeaderEvent joinedPartyLeaderEvent = PooledEvent<JoinedPartyLeaderEvent>.FromPool();
				joinedPartyLeaderEvent.Companion = Companion;
				joinedPartyLeaderEvent.Leader = Leader;
				joinedPartyLeaderEvent.PreviousCell = PreviousCell;
				joinedPartyLeaderEvent.TargetCell = TargetCell;
				joinedPartyLeaderEvent.DistanceFromPreviousCell = DistanceFromPreviousCell;
				joinedPartyLeaderEvent.DistanceFromLeader = DistanceFromLeader;
				if (flag)
				{
					Companion.HandleEvent(joinedPartyLeaderEvent);
				}
				if (flag2)
				{
					Leader.HandleEvent(joinedPartyLeaderEvent);
				}
			}
		}
	}
}
