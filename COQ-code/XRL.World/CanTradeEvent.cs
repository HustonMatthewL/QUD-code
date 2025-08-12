using System.Collections.Generic;
using XRL.World.Conversations;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class CanTradeEvent : IConversationMinEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(CanTradeEvent), null, CountPool, ResetPool);

		private static List<CanTradeEvent> Pool;

		private static int PoolCounter;

		public GameObject Trader;

		public bool Base;

		public CanTradeEvent()
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

		public static void ResetTo(ref CanTradeEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static CanTradeEvent FromPool()
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

		public override void Reset()
		{
			base.Reset();
			Trader = null;
			Base = false;
		}

		public static bool Check(GameObject Actor, GameObject SpeakingWith, GameObject Transmitter, GameObject Receiver, ref GameObject Trader, Conversation Conversation, bool Base, bool Physical = false, bool Mental = false)
		{
			bool flag = true;
			bool flag2 = Base;
			if (flag)
			{
				bool flag3 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("CanTrade");
				bool flag4 = GameObject.Validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("CanTrade");
				if (flag3 || flag4)
				{
					Event @event = Event.New("CanTrade");
					@event.SetParameter("Actor", Actor);
					@event.SetParameter("SpeakingWith", SpeakingWith);
					@event.SetParameter("Conversation", Conversation);
					@event.SetFlag("Base", Base);
					@event.SetFlag("Physical", Physical);
					@event.SetFlag("Mental", Mental);
					@event.SetFlag("CanTrade", flag2);
					if (flag && flag3)
					{
						flag = Actor.FireEvent(@event);
					}
					if (flag && flag4)
					{
						flag = SpeakingWith.FireEvent(@event);
					}
					flag2 = @event.HasFlag("CanTrade");
				}
			}
			if (flag)
			{
				CanTradeEvent canTradeEvent = FromPool();
				canTradeEvent.Actor = Actor;
				canTradeEvent.SpeakingWith = SpeakingWith;
				canTradeEvent.Transmitter = Transmitter;
				canTradeEvent.Receiver = Receiver;
				canTradeEvent.Trader = Trader;
				canTradeEvent.Conversation = Conversation;
				canTradeEvent.Base = Base;
				canTradeEvent.Physical = Physical;
				canTradeEvent.Mental = Mental;
				canTradeEvent.CanTrade = flag2;
				flag = IConversationMinEvent.DispatchAll(canTradeEvent);
				Trader = canTradeEvent.Trader;
				flag2 = canTradeEvent.CanTrade;
			}
			return flag2;
		}
	}
}
