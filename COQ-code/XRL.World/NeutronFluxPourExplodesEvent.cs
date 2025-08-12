using XRL.World.Parts;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class NeutronFluxPourExplodesEvent : PooledEvent<NeutronFluxPourExplodesEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject PouredFrom;

		public GameObject PouredTo;

		public GameObject PouredBy;

		public LiquidVolume PouredLiquid;

		public bool Prospective;

		public bool Interrupt;

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
			PouredFrom = null;
			PouredTo = null;
			PouredBy = null;
			PouredLiquid = null;
			Prospective = false;
			Interrupt = false;
		}

		public static bool Check(out bool Interrupt, GameObject PouredFrom, GameObject PouredTo, GameObject PouredBy = null, LiquidVolume PouredLiquid = null, bool Prospective = false)
		{
			Interrupt = false;
			bool flag = true;
			if (flag && GameObject.Validate(ref PouredFrom) && PouredFrom.HasRegisteredEvent("NeutronFluxPourExplodes"))
			{
				Event @event = Event.New("NeutronFluxPourExplodes");
				@event.SetParameter("PouredFrom", PouredFrom);
				@event.SetParameter("PouredTo", PouredTo);
				@event.SetParameter("PouredBy", PouredBy);
				@event.SetParameter("PouredLiquid", PouredLiquid);
				@event.SetFlag("Interrupt", Interrupt);
				@event.SetFlag("Prospective", Prospective);
				flag = PouredFrom.FireEvent(@event);
				Interrupt = @event.HasFlag("Interrupt");
			}
			if (flag && GameObject.Validate(ref PouredTo) && PouredTo.HasRegisteredEvent("NeutronFluxPourExplodes"))
			{
				Event event2 = Event.New("NeutronFluxPourExplodes");
				event2.SetParameter("PouredFrom", PouredFrom);
				event2.SetParameter("PouredTo", PouredTo);
				event2.SetParameter("PouredBy", PouredBy);
				event2.SetParameter("PouredLiquid", PouredLiquid);
				event2.SetFlag("Interrupt", Interrupt);
				event2.SetFlag("Prospective", Prospective);
				flag = PouredTo.FireEvent(event2);
				Interrupt = event2.HasFlag("Interrupt");
			}
			if (flag && GameObject.Validate(ref PouredBy) && PouredBy.HasRegisteredEvent("NeutronFluxPourExplodes"))
			{
				Event event3 = Event.New("NeutronFluxPourExplodes");
				event3.SetParameter("PouredFrom", PouredFrom);
				event3.SetParameter("PouredTo", PouredTo);
				event3.SetParameter("PouredBy", PouredBy);
				event3.SetParameter("PouredLiquid", PouredLiquid);
				event3.SetFlag("Interrupt", Interrupt);
				event3.SetFlag("Prospective", Prospective);
				flag = PouredBy.FireEvent(event3);
				Interrupt = event3.HasFlag("Interrupt");
			}
			NeutronFluxPourExplodesEvent neutronFluxPourExplodesEvent = null;
			if (flag && GameObject.Validate(ref PouredFrom) && PouredFrom.WantEvent(PooledEvent<NeutronFluxPourExplodesEvent>.ID, CascadeLevel))
			{
				if (neutronFluxPourExplodesEvent == null)
				{
					neutronFluxPourExplodesEvent = PooledEvent<NeutronFluxPourExplodesEvent>.FromPool();
				}
				neutronFluxPourExplodesEvent.PouredFrom = PouredFrom;
				neutronFluxPourExplodesEvent.PouredTo = PouredTo;
				neutronFluxPourExplodesEvent.PouredBy = PouredBy;
				neutronFluxPourExplodesEvent.PouredLiquid = PouredLiquid;
				neutronFluxPourExplodesEvent.Interrupt = Interrupt;
				neutronFluxPourExplodesEvent.Prospective = Prospective;
				flag = PouredFrom.HandleEvent(neutronFluxPourExplodesEvent);
				Interrupt = neutronFluxPourExplodesEvent.Interrupt;
			}
			if (flag && GameObject.Validate(ref PouredTo) && PouredTo.WantEvent(PooledEvent<NeutronFluxPourExplodesEvent>.ID, CascadeLevel))
			{
				if (neutronFluxPourExplodesEvent == null)
				{
					neutronFluxPourExplodesEvent = PooledEvent<NeutronFluxPourExplodesEvent>.FromPool();
				}
				neutronFluxPourExplodesEvent.PouredFrom = PouredFrom;
				neutronFluxPourExplodesEvent.PouredTo = PouredTo;
				neutronFluxPourExplodesEvent.PouredBy = PouredBy;
				neutronFluxPourExplodesEvent.PouredLiquid = PouredLiquid;
				neutronFluxPourExplodesEvent.Interrupt = Interrupt;
				neutronFluxPourExplodesEvent.Prospective = Prospective;
				flag = PouredTo.HandleEvent(neutronFluxPourExplodesEvent);
				Interrupt = neutronFluxPourExplodesEvent.Interrupt;
			}
			if (flag && GameObject.Validate(ref PouredBy) && PouredBy.WantEvent(PooledEvent<NeutronFluxPourExplodesEvent>.ID, CascadeLevel))
			{
				if (neutronFluxPourExplodesEvent == null)
				{
					neutronFluxPourExplodesEvent = PooledEvent<NeutronFluxPourExplodesEvent>.FromPool();
				}
				neutronFluxPourExplodesEvent.PouredFrom = PouredFrom;
				neutronFluxPourExplodesEvent.PouredTo = PouredTo;
				neutronFluxPourExplodesEvent.PouredBy = PouredBy;
				neutronFluxPourExplodesEvent.PouredLiquid = PouredLiquid;
				neutronFluxPourExplodesEvent.Interrupt = Interrupt;
				neutronFluxPourExplodesEvent.Prospective = Prospective;
				flag = PouredBy.HandleEvent(neutronFluxPourExplodesEvent);
				Interrupt = neutronFluxPourExplodesEvent.Interrupt;
			}
			return flag;
		}

		public static bool Check(GameObject PouredFrom, GameObject PouredTo, GameObject PouredBy = null, LiquidVolume PouredLiquid = null, bool Prospective = false)
		{
			bool Interrupt;
			return Check(out Interrupt, PouredFrom, PouredTo, PouredBy, PouredLiquid, Prospective);
		}
	}
}
