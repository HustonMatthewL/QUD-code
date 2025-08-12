using XRL.Wish;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Biomes
{
	[HasWishCommand]
	public static class FirethumbedTemplate
	{
		public static void Apply(GameObject GO)
		{
			GO.Slimewalking = true;
			if (GO.HasBodyPart("Hand"))
			{
				GO.RequirePart<DisplayNameAdjectives>().AddAdjective("firethumbed");
				GO.RequirePart<DisplayNameColor>().SetColorByPriority("R", 10);
				GO.RequirePart<FirethumbedIconColor>();
				if (!GO.HasPart(typeof(FlamingRay)))
				{
					GO.RequirePart<Mutations>().AddMutation(new FlamingRay(), 6);
				}
			}
		}

		[WishCommand("firethumbed", null)]
		public static void Wish(string Blueprint)
		{
			WishResult wishResult = WishSearcher.SearchForBlueprint(Blueprint);
			GameObject gameObject = GameObjectFactory.Factory.CreateObject(wishResult.Result, 0, 0, null, null, null, "Wish");
			Apply(gameObject);
			The.PlayerCell.getClosestEmptyCell().AddObject(gameObject);
		}
	}
}
