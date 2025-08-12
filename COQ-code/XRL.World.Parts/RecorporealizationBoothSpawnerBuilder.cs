using System.Linq;
using Genkit;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts
{
	public class RecorporealizationBoothSpawnerBuilder : ZoneBuilderSandbox
	{
		public void BuildZone(Zone zone)
		{
			InfluenceMapRegion influenceMapRegion = ZoneBuilderSandbox.GenerateInfluenceMap(zone, null, InfluenceMapSeedStrategy.LargestRegion, 200).Regions.Where((InfluenceMapRegion r) => r.maxRect.Width >= 12 && r.maxRect.Height >= 10).FirstOrDefault();
			Cell cell = null;
			if (influenceMapRegion != null)
			{
				cell = zone.GetCell(influenceMapRegion.maxRect.x1, influenceMapRegion.maxRect.y1);
			}
			if (cell == null)
			{
				cell = (from c in zone.GetCells()
					where c.X < 69 && c.Y < 16
					select c).GetRandomElement();
			}
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					zone.GetCell(cell.X + j, cell.Y + i)?.Clear(null, Important: false, Combat: true);
				}
			}
			cell.AddObject("RemortingNook_11X19");
		}
	}
}
