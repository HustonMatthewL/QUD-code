namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class JoinPartyLeaderPossibleEvent : PooledEvent<JoinPartyLeaderPossibleEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Companion;

		public GameObject Leader;

		public Cell CurrentCell;

		public Cell TargetCell;

		public bool IsMobile;

		public bool Result;

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
			IsMobile = false;
			Result = false;
		}

		public static bool Check(GameObject Companion, GameObject Leader, Cell CurrentCell, ref Cell TargetCell, bool IsMobile)
		{
			if (!GameObject.Validate(ref Companion))
			{
				return false;
			}
			bool flag = IsMobile;
			if (Companion.HasRegisteredEvent("JoinPartyLeaderPossible"))
			{
				Event @event = Event.New("JoinPartyLeaderPossible");
				@event.SetParameter("Companion", Companion);
				@event.SetParameter("Leader", Leader);
				@event.SetParameter("CurrentCell", CurrentCell);
				@event.SetParameter("TargetCell", TargetCell);
				@event.SetFlag("IsMobile", IsMobile);
				@event.SetFlag("Result", flag);
				bool num = Companion.FireEvent(@event);
				TargetCell = @event.GetParameter("TargetCell") as Cell;
				flag = @event.HasFlag("Result");
				if (!num)
				{
					return flag;
				}
			}
			if (Companion.WantEvent(PooledEvent<JoinPartyLeaderPossibleEvent>.ID, CascadeLevel))
			{
				JoinPartyLeaderPossibleEvent joinPartyLeaderPossibleEvent = PooledEvent<JoinPartyLeaderPossibleEvent>.FromPool();
				joinPartyLeaderPossibleEvent.Companion = Companion;
				joinPartyLeaderPossibleEvent.Leader = Leader;
				joinPartyLeaderPossibleEvent.CurrentCell = CurrentCell;
				joinPartyLeaderPossibleEvent.TargetCell = TargetCell;
				joinPartyLeaderPossibleEvent.IsMobile = IsMobile;
				joinPartyLeaderPossibleEvent.Result = flag;
				Companion.HandleEvent(joinPartyLeaderPossibleEvent);
				TargetCell = joinPartyLeaderPossibleEvent.TargetCell;
				flag = joinPartyLeaderPossibleEvent.Result;
			}
			return flag;
		}
	}
}
