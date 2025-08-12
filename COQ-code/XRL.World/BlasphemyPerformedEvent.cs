using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool, Cascade = 15)]
	public class BlasphemyPerformedEvent : MinEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(BlasphemyPerformedEvent), null, CountPool, ResetPool);

		public new static readonly int CascadeLevel = 15;

		private static List<BlasphemyPerformedEvent> Pool;

		private static int PoolCounter;

		public GameObject Actor;

		public GameObject Object;

		public Worshippable Being;

		public BlasphemyPerformedEvent()
		{
			base.ID = ID;
		}

		public override int GetCascadeLevel()
		{
			return CascadeLevel;
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

		public static void ResetTo(ref BlasphemyPerformedEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static BlasphemyPerformedEvent FromPool()
		{
			return MinEvent.FromPool(ref Pool, ref PoolCounter);
		}

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Actor = null;
			Object = null;
			Being = null;
		}

		public static void Send(GameObject Actor, GameObject Object, Worshippable Being = null, Zone Zone = null)
		{
			if (!GameObject.Validate(ref Actor))
			{
				return;
			}
			if (Being == null)
			{
				Being = Factions.FindWorshippable(Object);
			}
			if (Being != null)
			{
				if (Actor.IsPlayer())
				{
					if (GlobalConfig.GetBoolSetting("WorshipReputation"))
					{
						The.Game.PlayerReputation.BlasphemyPerformed(Being);
					}
				}
				else if (Actor.IsCreature)
				{
					Actor.RequirePart<SacralTracking>();
				}
			}
			if (Zone == null)
			{
				Zone = Actor.CurrentZone;
			}
			bool flag = Zone?.WantEvent(ID, CascadeLevel) ?? false;
			bool flag2 = (!flag || Actor.GetCurrentZone() != Zone) && Actor.WantEvent(ID, CascadeLevel);
			bool flag3 = GameObject.Validate(ref Object) && (!flag || Object.GetCurrentZone() != Zone) && Object.WantEvent(ID, CascadeLevel);
			if (flag || flag2 || flag3)
			{
				BlasphemyPerformedEvent blasphemyPerformedEvent = FromPool();
				blasphemyPerformedEvent.Actor = Actor;
				blasphemyPerformedEvent.Object = Object;
				blasphemyPerformedEvent.Being = Being;
				if (flag)
				{
					Zone.HandleEvent(blasphemyPerformedEvent);
				}
				if (flag2)
				{
					Actor.HandleEvent(blasphemyPerformedEvent);
				}
				if (flag3)
				{
					Object.HandleEvent(blasphemyPerformedEvent);
				}
			}
		}
	}
}
