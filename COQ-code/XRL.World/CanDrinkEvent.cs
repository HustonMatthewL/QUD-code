using XRL.World.Parts;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class CanDrinkEvent : PooledEvent<CanDrinkEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public GameObject Object;

		public LiquidVolume Liquid;

		public bool CanDrinkThis;

		public bool CouldDrinkOther;

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
			Liquid = null;
			CanDrinkThis = false;
			CouldDrinkOther = false;
		}

		public static void GetFor(GameObject Object, LiquidVolume Liquid, out bool CanDrinkThis, out bool CouldDrinkOther)
		{
			bool flag = true;
			CanDrinkThis = false;
			CouldDrinkOther = false;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CanDrink"))
			{
				Event @event = Event.New("CanDrink");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Liquid", Liquid);
				@event.SetFlag("CanDrinkThis", CanDrinkThis);
				@event.SetFlag("CouldDrinkOther", CouldDrinkOther);
				flag = Object.FireEvent(@event);
				CanDrinkThis = @event.HasFlag("CanDrinkThis");
				CouldDrinkOther = @event.HasFlag("CouldDrinkOther");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CanDrinkEvent>.ID, CascadeLevel))
			{
				CanDrinkEvent canDrinkEvent = PooledEvent<CanDrinkEvent>.FromPool();
				canDrinkEvent.Object = Object;
				canDrinkEvent.Liquid = Liquid;
				canDrinkEvent.CanDrinkThis = CanDrinkThis;
				canDrinkEvent.CouldDrinkOther = CouldDrinkOther;
				flag = Object.HandleEvent(canDrinkEvent);
				CanDrinkThis = canDrinkEvent.CanDrinkThis;
				CouldDrinkOther = canDrinkEvent.CouldDrinkOther;
			}
		}
	}
}
