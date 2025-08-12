using XRL.World.Parts.Mutation;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetMutationTermEvent : PooledEvent<GetMutationTermEvent>
	{
		public const string DEFAULT_TERM = "mutation";

		public const string DEFAULT_COLOR = "M";

		public GameObject Creature;

		public BaseMutation Mutation;

		public string Term;

		public string Color;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Creature = null;
			Mutation = null;
			Term = null;
			Color = null;
		}

		public static GetMutationTermEvent FromPool(GameObject Creature, BaseMutation Mutation, string Term, string Color)
		{
			GetMutationTermEvent getMutationTermEvent = PooledEvent<GetMutationTermEvent>.FromPool();
			getMutationTermEvent.Creature = Creature;
			getMutationTermEvent.Mutation = Mutation;
			getMutationTermEvent.Term = Term;
			getMutationTermEvent.Color = Color;
			return getMutationTermEvent;
		}

		public static void GetFor(GameObject Creature, out string Term, out string Color, BaseMutation Mutation = null)
		{
			Term = "mutation";
			Color = "M";
			bool flag = true;
			if (flag && GameObject.Validate(ref Creature) && Creature.HasRegisteredEvent("GetMutationTerm"))
			{
				Event @event = Event.New("GetMutationTerm");
				@event.SetParameter("Creature", Creature);
				@event.SetParameter("Mutation", Mutation);
				@event.SetParameter("Term", Term);
				@event.SetParameter("Color", Color);
				flag = Creature.FireEvent(@event);
				Term = @event.GetStringParameter("Term");
				Color = @event.GetStringParameter("Color");
			}
			if (flag && GameObject.Validate(ref Creature) && Creature.WantEvent(PooledEvent<GetMutationTermEvent>.ID, MinEvent.CascadeLevel))
			{
				GetMutationTermEvent getMutationTermEvent = FromPool(Creature, Mutation, Term, Color);
				flag = Creature.HandleEvent(getMutationTermEvent);
				Term = getMutationTermEvent.Term;
				Color = getMutationTermEvent.Color;
			}
		}

		public static string GetFor(GameObject Creature, BaseMutation Mutation = null)
		{
			string text = "mutation";
			string text2 = "M";
			bool flag = true;
			if (flag && GameObject.Validate(ref Creature) && Creature.HasRegisteredEvent("GetMutationTerm"))
			{
				Event @event = Event.New("GetMutationTerm");
				@event.SetParameter("Creature", Creature);
				@event.SetParameter("Mutation", Mutation);
				@event.SetParameter("Term", text);
				@event.SetParameter("Color", text2);
				flag = Creature.FireEvent(@event);
				text = @event.GetStringParameter("Term");
				text2 = @event.GetStringParameter("Color");
			}
			if (flag && GameObject.Validate(ref Creature) && Creature.WantEvent(PooledEvent<GetMutationTermEvent>.ID, MinEvent.CascadeLevel))
			{
				GetMutationTermEvent getMutationTermEvent = FromPool(Creature, Mutation, text, text2);
				flag = Creature.HandleEvent(getMutationTermEvent);
				text = getMutationTermEvent.Term;
				text2 = getMutationTermEvent.Color;
			}
			return text;
		}
	}
}
