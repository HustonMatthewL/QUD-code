using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class AnyRegenerableLimbsEvent : ILimbRegenerationEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(AnyRegenerableLimbsEvent), null, CountPool, ResetPool);

		private static List<AnyRegenerableLimbsEvent> Pool;

		private static int PoolCounter;

		public AnyRegenerableLimbsEvent()
		{
			base.ID = ID;
		}

		public static int CountPool()
		{
			if (Pool != null)
			{
				return Pool.Count;
			}
			return 0;
		}

		public static void ResetPool()
		{
			while (PoolCounter > 0)
			{
				Pool[--PoolCounter].Reset();
			}
		}

		public static void ResetTo(ref AnyRegenerableLimbsEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static AnyRegenerableLimbsEvent FromPool()
		{
			return MinEvent.FromPool(ref Pool, ref PoolCounter);
		}

		public override bool Dispatch(IEventHandler Handler)
		{
			if (!base.Dispatch(Handler))
			{
				return false;
			}
			return Handler.HandleEvent(this);
		}

		public static bool Send(GameObject Object, GameObject Actor = null, GameObject Source = null, bool Whole = false, bool All = false, bool IncludeMinor = true, bool Voluntary = true, int? ParentID = null, int? Category = null, int[] Categories = null, int? ExceptCategory = null, int[] ExceptCategories = null)
		{
			bool flag = true;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AnyRegenerableLimbs"))
			{
				Event @event = Event.New("AnyRegenerableLimbs");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Source", Source);
				@event.SetParameter("ParentID", ParentID);
				@event.SetParameter("Category", Category);
				@event.SetParameter("Categories", Categories);
				@event.SetParameter("ExceptCategory", ExceptCategory);
				@event.SetParameter("ExceptCategories", ExceptCategories);
				@event.SetFlag("Whole", Whole);
				@event.SetFlag("All", All);
				@event.SetFlag("IncludeMinor", IncludeMinor);
				@event.SetFlag("Voluntary", Voluntary);
				flag = Object.FireEvent(@event);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, ILimbRegenerationEvent.CascadeLevel))
			{
				AnyRegenerableLimbsEvent anyRegenerableLimbsEvent = FromPool();
				anyRegenerableLimbsEvent.Object = Object;
				anyRegenerableLimbsEvent.Actor = Actor;
				anyRegenerableLimbsEvent.Source = Source;
				anyRegenerableLimbsEvent.ParentID = ParentID;
				anyRegenerableLimbsEvent.Category = Category;
				anyRegenerableLimbsEvent.Categories = Categories;
				anyRegenerableLimbsEvent.ExceptCategory = ExceptCategory;
				anyRegenerableLimbsEvent.ExceptCategories = ExceptCategories;
				anyRegenerableLimbsEvent.Whole = Whole;
				anyRegenerableLimbsEvent.All = All;
				anyRegenerableLimbsEvent.IncludeMinor = IncludeMinor;
				anyRegenerableLimbsEvent.Voluntary = Voluntary;
				flag = Object.HandleEvent(anyRegenerableLimbsEvent);
			}
			return !flag;
		}
	}
}
