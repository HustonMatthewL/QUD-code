namespace XRL.World.ObjectBuilders
{
	public class Tier2HumanoidEquipment : IObjectBuilder
	{
		public override void Apply(GameObject GO, string Context)
		{
			GO.ReceiveObjectFromPopulation("Melee Weapons 2", null, NoStack: false, 0, 0, null, null, null, Context);
			if (75.in100())
			{
				GO.ReceiveObjectFromPopulation("Armor 2", null, NoStack: false, 0, 0, null, null, null, Context);
			}
			if (5.in100())
			{
				GO.ReceiveObjectFromPopulation("Junk 2", null, NoStack: false, 0, 0, null, null, null, Context);
			}
		}
	}
}
