using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class JumpedEvent : PooledEvent<JumpedEvent>
	{
		public new static readonly int CascadeLevel = 17;

		public static readonly int PASSES = 3;

		public GameObject Actor;

		public Cell OriginCell;

		public Cell TargetCell;

		public List<Point> Path;

		public int Range;

		public int Pass;

		public string AbilityName;

		public string ProviderKey;

		public string SourceKey;

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
			OriginCell = null;
			TargetCell = null;
			Path = null;
			Range = 0;
			Pass = 0;
			AbilityName = null;
			ProviderKey = null;
			SourceKey = null;
		}

		public static void Send(GameObject Actor, Cell OriginCell, Cell TargetCell, List<Point> Path, int Range, string AbilityName = null, string ProviderKey = null, string SourceKey = null)
		{
			bool flag = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("Jumped");
			bool flag2 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<JumpedEvent>.ID, CascadeLevel);
			if (!(flag || flag2))
			{
				return;
			}
			bool flag3 = true;
			bool flag4 = true;
			int num = 1;
			while (flag4 && flag3 && num <= PASSES)
			{
				flag4 = false;
				if (flag3 && flag && GameObject.Validate(ref Actor))
				{
					Event @event = Event.New("Jumped");
					@event.SetParameter("Actor", Actor);
					@event.SetParameter("OriginCell", OriginCell);
					@event.SetParameter("TargetCell", TargetCell);
					@event.SetParameter("Path", Path);
					@event.SetParameter("Range", Range);
					@event.SetParameter("Pass", num);
					@event.SetParameter("AbilityName", AbilityName);
					@event.SetParameter("ProviderKey", ProviderKey);
					@event.SetParameter("SourceKey", SourceKey);
					flag3 = Actor.FireEvent(@event);
					flag4 = true;
				}
				if (flag3 && flag2 && GameObject.Validate(ref Actor))
				{
					JumpedEvent jumpedEvent = PooledEvent<JumpedEvent>.FromPool();
					jumpedEvent.Actor = Actor;
					jumpedEvent.OriginCell = OriginCell;
					jumpedEvent.TargetCell = TargetCell;
					jumpedEvent.Path = Path;
					jumpedEvent.Range = Range;
					jumpedEvent.Pass = num;
					jumpedEvent.AbilityName = AbilityName;
					jumpedEvent.ProviderKey = ProviderKey;
					jumpedEvent.SourceKey = SourceKey;
					flag3 = Actor.HandleEvent(jumpedEvent);
					flag4 = true;
				}
				num++;
			}
		}
	}
}
