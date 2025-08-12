using System.Collections.Generic;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class GetAttackerMeleePenetrationEvent : IMeleePenetrationEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetAttackerMeleePenetrationEvent), null, CountPool, ResetPool);

		private static List<GetAttackerMeleePenetrationEvent> Pool;

		private static int PoolCounter;

		public GetAttackerMeleePenetrationEvent()
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

		public static void ResetTo(ref GetAttackerMeleePenetrationEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static GetAttackerMeleePenetrationEvent FromPool()
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

		public static bool Process(ref int Penetrations, ref int StatBonus, ref int MaxStatBonus, ref int PenetrationBonus, ref int MaxPenetrationBonus, int AV, bool Critical, string Properties, string Hand, GameObject Attacker, GameObject Defender, GameObject Weapon)
		{
			return IMeleePenetrationEvent.Process(ref Penetrations, ref StatBonus, ref MaxStatBonus, ref PenetrationBonus, ref MaxPenetrationBonus, AV, Critical, Properties, Hand, Attacker, Defender, Weapon, "AttackerGetWeaponPenModifier", Attacker, ID, IMeleePenetrationEvent.CascadeLevel, FromPool);
		}
	}
}
