namespace XRL.World.ZoneBuilders
{
	public class CaveNorthMouth : IConnectionBuilder
	{
		public bool BuildZone(Zone Z)
		{
			Range = 3;
			return ConnectionMouth(Z, "Cave", "North");
		}
	}
}
