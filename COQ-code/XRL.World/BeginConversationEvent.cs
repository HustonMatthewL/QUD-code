using System.Collections.Generic;
using XRL.World.Conversations;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class BeginConversationEvent : IConversationMinEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeginConversationEvent), null, CountPool, ResetPool);

		private static List<BeginConversationEvent> Pool;

		private static int PoolCounter;

		public Node StartNode;

		public BeginConversationEvent()
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

		public static void ResetTo(ref BeginConversationEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static BeginConversationEvent FromPool()
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
			StartNode = null;
		}

		public static bool Check(GameObject Actor, GameObject SpeakingWith, GameObject Transmitter, GameObject Receiver, Conversation Conversation, Node StartNode, bool CanTrade = false, bool Physical = false, bool Mental = false)
		{
			bool flag = true;
			if (flag)
			{
				bool flag2 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("BeginConversation");
				bool flag3 = GameObject.Validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("BeginConversation");
				if (flag2 || flag3)
				{
					Event @event = Event.New("BeginConversation");
					@event.SetParameter("Actor", Actor);
					@event.SetParameter("SpeakingWith", SpeakingWith);
					@event.SetParameter("Conversation", Conversation);
					@event.SetFlag("CanTrade", CanTrade);
					@event.SetFlag("Physical", Physical);
					@event.SetFlag("Mental", Mental);
					if (flag && flag2)
					{
						flag = Actor.FireEvent(@event);
					}
					if (flag && flag3)
					{
						flag = SpeakingWith.FireEvent(@event);
					}
				}
			}
			if (flag)
			{
				BeginConversationEvent beginConversationEvent = FromPool();
				beginConversationEvent.Actor = Actor;
				beginConversationEvent.SpeakingWith = SpeakingWith;
				beginConversationEvent.Transmitter = Transmitter;
				beginConversationEvent.Receiver = Receiver;
				beginConversationEvent.Conversation = Conversation;
				beginConversationEvent.StartNode = StartNode;
				beginConversationEvent.CanTrade = CanTrade;
				beginConversationEvent.Physical = Physical;
				beginConversationEvent.Mental = Mental;
				flag = IConversationMinEvent.DispatchAll(beginConversationEvent);
			}
			return flag;
		}
	}
}
