namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class CommandEvent : PooledEvent<CommandEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Actor;

		public GameObject Target;

		public Cell TargetCell;

		public string Command;

		public int StandoffDistance;

		public bool Forced;

		public bool Silent;

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
			Target = null;
			TargetCell = null;
			Command = null;
			StandoffDistance = 0;
			Forced = false;
			Silent = false;
		}

		public static bool Send(GameObject Actor, string Command, ref bool InterfaceExitRequested, GameObject Target = null, Cell TargetCell = null, int StandoffDistance = 0, GameObject Handler = null)
		{
			if (Handler == null)
			{
				Handler = Actor;
			}
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent(Command))
			{
				Event @event = Event.New(Command);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("TargetCell", TargetCell);
				@event.SetParameter("Command", Command);
				@event.SetParameter("StandoffDistance", StandoffDistance);
				flag = Handler.FireEvent(@event);
				if (@event.InterfaceExitRequested())
				{
					InterfaceExitRequested = true;
				}
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<CommandEvent>.ID, CascadeLevel))
			{
				CommandEvent commandEvent = PooledEvent<CommandEvent>.FromPool();
				commandEvent.Actor = Actor;
				commandEvent.Target = Target;
				commandEvent.TargetCell = TargetCell;
				commandEvent.Command = Command;
				commandEvent.StandoffDistance = StandoffDistance;
				flag = Handler.HandleEvent(commandEvent);
				if (commandEvent.InterfaceExitRequested())
				{
					InterfaceExitRequested = true;
				}
			}
			return flag;
		}

		public static bool Send(GameObject Actor, string Command, ref bool InterfaceExitRequested, GameObject Target = null, Cell TargetCell = null, int StandoffDistance = 0, bool Forced = false, bool Silent = false, GameObject Handler = null)
		{
			if (Handler == null)
			{
				Handler = Actor;
			}
			bool flag = true;
			if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent(Command))
			{
				Event @event = Event.New(Command);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Target", Target);
				@event.SetParameter("TargetCell", TargetCell);
				@event.SetParameter("Command", Command);
				@event.SetParameter("StandoffDistance", StandoffDistance);
				@event.SetFlag("Forced", Forced);
				@event.SetSilent(Silent);
				flag = Handler.FireEvent(@event);
				if (@event.InterfaceExitRequested())
				{
					InterfaceExitRequested = true;
				}
			}
			if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<CommandEvent>.ID, CascadeLevel))
			{
				CommandEvent commandEvent = PooledEvent<CommandEvent>.FromPool();
				commandEvent.Actor = Actor;
				commandEvent.Target = Target;
				commandEvent.TargetCell = TargetCell;
				commandEvent.Command = Command;
				commandEvent.StandoffDistance = StandoffDistance;
				commandEvent.Forced = Forced;
				commandEvent.Silent = Silent;
				flag = Handler.HandleEvent(commandEvent);
				if (commandEvent.InterfaceExitRequested())
				{
					InterfaceExitRequested = true;
				}
			}
			return flag;
		}

		public static bool Send(GameObject Actor, string Command, GameObject Target = null, Cell TargetCell = null, int StandoffDistance = 0, bool Forced = false, bool Silent = false, GameObject Handler = null)
		{
			bool InterfaceExitRequested = false;
			return Send(Actor, Command, ref InterfaceExitRequested, Target, TargetCell, StandoffDistance, Forced, Silent, Handler);
		}
	}
}
