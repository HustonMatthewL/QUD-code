using XRL.World.Parts.Skill;

namespace XRL.World
{
	[GameEvent(Cache = Cache.Pool)]
	public class IsRepairableEvent : PooledEvent<IsRepairableEvent>
	{
		public GameObject Actor;

		public GameObject Subject;

		public GameObject Tool;

		public BaseSkill Skill;

		public int? MaxRepairTier;

		public override bool Dispatch(IEventHandler Handler)
		{
			return Handler.HandleEvent(this);
		}

		public override void Reset()
		{
			base.Reset();
			Actor = null;
			Subject = null;
			Tool = null;
			Skill = null;
			MaxRepairTier = null;
		}

		public static bool Check(GameObject Actor = null, GameObject Subject = null, GameObject Tool = null, BaseSkill Skill = null, int? MaxRepairTier = null)
		{
			bool result = false;
			if (GameObject.Validate(ref Subject))
			{
				IsRepairableEvent E = PooledEvent<IsRepairableEvent>.FromPool();
				E.Actor = Actor;
				E.Subject = Subject;
				E.Tool = Tool;
				E.Skill = Skill;
				E.MaxRepairTier = MaxRepairTier;
				result = !Subject.HandleEvent(E);
				PooledEvent<IsRepairableEvent>.ResetTo(ref E);
			}
			return result;
		}
	}
}
