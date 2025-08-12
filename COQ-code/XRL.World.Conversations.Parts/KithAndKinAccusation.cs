namespace XRL.World.Conversations.Parts
{
	public class KithAndKinAccusation : IKithAndKinPart
	{
		public override bool WantEvent(int ID, int Propagation)
		{
			if (!base.WantEvent(ID, Propagation))
			{
				return ID == PrepareTextEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(PrepareTextEvent E)
		{
			string circumstanceInfluence = base.CircumstanceInfluence;
			string motiveInfluence = base.MotiveInfluence;
			string value = ((circumstanceInfluence == motiveInfluence) ? circumstanceInfluence : (circumstanceInfluence + " and " + motiveInfluence));
			E.Text.StartReplace().AddReplacer("all.influence", value).AddReplacer("motive.influence", motiveInfluence)
				.AddReplacer("thief.name", base.ThiefName)
				.Execute();
			return base.HandleEvent(E);
		}
	}
}
