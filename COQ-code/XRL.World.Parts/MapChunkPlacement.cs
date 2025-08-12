using System;
using XRL.EditorFormats.Map;

namespace XRL.World.Parts
{
	[Serializable]
	public class MapChunkPlacement : IPart
	{
		public string Map;

		public int Width;

		public int Height;

		public int Rotation;

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("EnteredCell");
			base.Register(Object, Registrar);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "EnteredCell")
			{
				PlaceFromFile(ParentObject.GetCurrentCell(), Map, Width, Height, int.TryParse(GetTag("ChunkPadding", "2"), out var result) ? result : 2, Rotation, GetTag("ChunkHint"));
			}
			return true;
		}

		public static void PlaceFromFile(Cell Cell, string File, int Width, int Height, int Padding = 2, int Rotation = 0, string Hint = null, Action<Cell> PreAction = null, Action<Cell> PostAction = null)
		{
			PlaceFromFile(Cell.ParentZone, File, Cell.X, Cell.Y, Width, Height, Padding, Rotation, Hint, PreAction, PostAction);
		}

		public static void PlaceFromFile(Zone Z, string File, int X, int Y, int Width, int Height, int Padding = 2, int Rotation = 0, string Hint = null, Action<Cell> PreAction = null, Action<Cell> PostAction = null)
		{
			MapFile mapFile = MapFile.LoadWithMods(File);
			int num = ((Rotation % 2 == 0) ? Width : Height);
			int num2 = ((Rotation % 2 == 0) ? Height : Width);
			if (X + num + Padding >= Z.Width)
			{
				X -= X + num + Padding - Z.Width;
			}
			if (Y + num2 + Padding >= Z.Height)
			{
				Y -= Y + num2 + Padding - Z.Height;
			}
			if (Hint == "Center")
			{
				X = Z.Width / 2 - num / 2;
				Y = Z.Height / 2 - num2 / 2;
			}
			if (PreAction == null)
			{
				PreAction = delegate(Cell X)
				{
					X.ClearWalls();
				};
			}
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					Cell cell = Z.GetCell(X + j, Y + i);
					if (cell != null)
					{
						Translate(j, i, Rotation, Width, Height, out var TX, out var TY);
						mapFile.Cells[TX, TY].ApplyTo(cell, CheckEmpty: true, PreAction, PostAction);
					}
				}
			}
		}

		public static void Translate(int X, int Y, int Rotation, int Width, int Height, out int TX, out int TY)
		{
			Width--;
			Height--;
			switch (Rotation)
			{
			case 1:
				TX = Y;
				TY = Height - X;
				break;
			case 2:
				TX = Width - X;
				TY = Height - Y;
				break;
			case 3:
				TX = Width - Y;
				TY = X;
				break;
			default:
				TX = X;
				TY = Y;
				break;
			}
		}
	}
}
