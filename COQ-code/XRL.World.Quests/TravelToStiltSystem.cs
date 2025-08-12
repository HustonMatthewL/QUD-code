using System;

namespace XRL.World.Quests
{
	[Serializable]
	public class TravelToStiltSystem : IQuestSystem
	{
		public override void Register(XRLGame Game, IEventRegistrar Registrar)
		{
			Registrar.Register(ZoneActivatedEvent.ID);
		}

		public override bool HandleEvent(ZoneActivatedEvent E)
		{
			if (E.Zone.ZoneID == "JoppaWorld.5.2.1.1.10" || E.Zone.ZoneID == "JoppaWorld.5.2.1.2.10")
			{
				The.Game.FinishQuestStep("O Glorious Shekhinah!", "Make a Pilgrimage to the Six Day Stilt", -1, CanFinishQuest: true, E.Zone.ZoneID);
			}
			return base.HandleEvent(E);
		}

		public override GameObject GetInfluencer()
		{
			if (50.in100())
			{
				return GameObject.FindByBlueprint("Wardens Esther");
			}
			return GameObject.FindByBlueprint("Tszappur");
		}
	}
}
