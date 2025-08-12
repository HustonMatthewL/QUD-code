using XRL.EditorFormats.Map;

namespace XRL.World.ZoneBuilders
{
	public class MapBuilder
	{
		public string FileName;

		public int Width = 80;

		public int Height = 25;

		public int X = -1;

		public int Y = -1;

		public bool ClearBeforePlace;

		public bool ClearChasms;

		public bool CheckEmpty = true;

		public bool BuildReachability = true;

		private MapFile _Map;

		public MapFile Map => _Map ?? (_Map = MapFile.LoadWithMods(FileName));

		public MapBuilder()
		{
		}

		public MapBuilder(string mapFileName)
			: this()
		{
			FileName = mapFileName;
		}

		public void Clear(Cell C)
		{
			if (ClearBeforePlace)
			{
				C.Clear(null, Important: false, Combat: true);
			}
			if (ClearChasms)
			{
				C.ClearObjectsWithPart("ChasmMaterial", Important: true, Combat: true);
			}
		}

		public static bool BuildFromFile(Zone Z, string FileName, bool Clear = false)
		{
			return new MapBuilder
			{
				FileName = FileName,
				ClearBeforePlace = Clear
			}.BuildZone(Z);
		}

		public bool BuildZone(Zone Z)
		{
			MapFile map = Map;
			if (map.width == 0)
			{
				MetricsManager.LogError("Couldn't find the map: " + FileName);
				return false;
			}
			if (X == -1)
			{
				X = Z.Width / 2 - Width / 2;
			}
			if (Y == -1)
			{
				Y = Z.Height / 2 - Height / 2;
			}
			for (int i = 0; i < Height; i++)
			{
				for (int j = 0; j < Width; j++)
				{
					Cell cell = Z.GetCell(X + j, Y + i);
					if (cell != null)
					{
						map.Cells[j, i].ApplyTo(cell, CheckEmpty, Clear);
					}
				}
			}
			if (BuildReachability)
			{
				Cell cell2 = Z.GetCell(X + Width / 2, Y + Height / 2);
				if (cell2.IsEmpty())
				{
					Z.BuildReachableMap(cell2.X, cell2.Y);
				}
				else
				{
					Cell.SpiralEnumerator enumerator = cell2.IterateAdjacent(5, IncludeSelf: true, LocalOnly: true).GetEnumerator();
					while (enumerator.MoveNext())
					{
						Cell current = enumerator.Current;
						if (current.IsEmpty())
						{
							Z.BuildReachableMap(current.X, current.Y);
							break;
						}
					}
				}
			}
			return true;
		}
	}
}
