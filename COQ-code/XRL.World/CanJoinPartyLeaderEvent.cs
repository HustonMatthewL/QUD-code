namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class CanJoinPartyLeaderEvent : PooledEvent<CanJoinPartyLeaderEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Companion;

		public GameObject Leader;

		public Cell CurrentCell;

		public Cell TargetCell;

		public int DistanceFromCurrentCell;

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
			CurrentCell = null;
			TargetCell = null;
			DistanceFromCurrentCell = 0;
			DistanceFromLeader = 0;
		}

		public static bool Check(GameObject Companion, GameObject Leader, Cell CurrentCell, Cell TargetCell, int DistanceFromCurrentCell, int DistanceFromLeader)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Companion) && Companion.HasRegisteredEvent("CanJoinPartyLeader"))
			{
				Event @event = Event.New("CanJoinPartyLeader");
				@event.SetParameter("Companion", Companion);
				@event.SetParameter("Leader", Leader);
				@event.SetParameter("CurrentCell", CurrentCell);
				@event.SetParameter("TargetCell", TargetCell);
				@event.SetParameter("DistanceFromCurrentCell", DistanceFromCurrentCell);
				@event.SetParameter("DistanceFromLeader", DistanceFromLeader);
				flag = Companion.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Companion) && Companion.WantEvent(PooledEvent<CanJoinPartyLeaderEvent>.ID, CascadeLevel))
			{
				CanJoinPartyLeaderEvent canJoinPartyLeaderEvent = PooledEvent<CanJoinPartyLeaderEvent>.FromPool();
				canJoinPartyLeaderEvent.Companion = Companion;
				canJoinPartyLeaderEvent.Leader = Leader;
				canJoinPartyLeaderEvent.CurrentCell = CurrentCell;
				canJoinPartyLeaderEvent.TargetCell = TargetCell;
				canJoinPartyLeaderEvent.DistanceFromCurrentCell = DistanceFromCurrentCell;
				canJoinPartyLeaderEvent.DistanceFromLeader = DistanceFromLeader;
				flag = Companion.HandleEvent(canJoinPartyLeaderEvent);
			}
			return flag;
		}
	}
}
