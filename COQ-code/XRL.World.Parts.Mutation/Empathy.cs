using System;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class Empathy : BaseMutation
	{
		public Empathy()
		{
			DisplayName = "Empathy";
			base.Type = "Mental";
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			base.Register(Object, Registrar);
		}

		public override bool ChangeLevel(int NewLevel)
		{
			return base.ChangeLevel(NewLevel);
		}

		public override bool Mutate(GameObject GO, int Level)
		{
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			return base.Unmutate(GO);
		}
	}
}
