using System.Collections.Generic;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.World
{
	[GameEvent(Cascade = 17, Cache = Cache.Pool)]
	public class BeforeAddSkillEvent : IAddSkillEvent
	{
		public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeAddSkillEvent), null, CountPool, ResetPool);

		public new static readonly int CascadeLevel = 17;

		private static List<BeforeAddSkillEvent> Pool;

		private static int PoolCounter;

		public List<IBaseSkillEntry> Include = new List<IBaseSkillEntry>();

		public BeforeAddSkillEvent()
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

		public static void ResetTo(ref BeforeAddSkillEvent E)
		{
			MinEvent.ResetTo(E, Pool, ref PoolCounter);
			E = null;
		}

		public static BeforeAddSkillEvent FromPool()
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
			Include.Clear();
		}

		public static BeforeAddSkillEvent FromPool(GameObject Actor, BaseSkill Skill)
		{
			BeforeAddSkillEvent beforeAddSkillEvent = FromPool();
			beforeAddSkillEvent.Actor = Actor;
			beforeAddSkillEvent.Skill = Skill;
			return beforeAddSkillEvent;
		}

		public static BeforeAddSkillEvent FromSkill(BaseSkill Skill, GameObject Source = null, string Context = null)
		{
			BeforeAddSkillEvent beforeAddSkillEvent = FromPool();
			beforeAddSkillEvent.Actor = Skill.ParentObject;
			beforeAddSkillEvent.Source = Source;
			beforeAddSkillEvent.Skill = Skill;
			beforeAddSkillEvent.Context = Context;
			if (SkillFactory.Factory.TryGetFirstEntry(Skill.Name, out beforeAddSkillEvent.Entry))
			{
				if (beforeAddSkillEvent.Entry is SkillEntry skillEntry)
				{
					foreach (PowerEntry value in skillEntry.Powers.Values)
					{
						if (value.Cost == 0 && !beforeAddSkillEvent.Actor.HasSkill(value.Class))
						{
							beforeAddSkillEvent.Include.Add(value);
						}
					}
				}
				else if (beforeAddSkillEvent.Entry is PowerEntry { ParentSkill: { Cost: 0 } parentSkill } && !beforeAddSkillEvent.Actor.HasSkill(parentSkill.Class))
				{
					beforeAddSkillEvent.Include.Add(parentSkill);
				}
			}
			return beforeAddSkillEvent;
		}

		public static void Send(BeforeAddSkillEvent E)
		{
			if (!E.Skill.BeforeAddSkill(E))
			{
				return;
			}
			if (E.Actor.HasRegisteredEvent("BeforeAddSkill"))
			{
				Event @event = Event.New("BeforeAddSkill");
				@event.SetParameter("Actor", E.Actor);
				@event.SetParameter("Skill", E.Skill);
				@event.SetParameter("Entry", E.Entry);
				@event.SetParameter("Source", E.Source);
				@event.SetParameter("Context", E.Context);
				@event.SetParameter("Include", E.Include);
				if (!E.Actor.FireEvent(@event))
				{
					return;
				}
			}
			E.Actor.HandleEvent(E);
		}
	}
}
