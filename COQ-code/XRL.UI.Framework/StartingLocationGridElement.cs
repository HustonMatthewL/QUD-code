using System;
using ConsoleLib.Console;

namespace XRL.UI.Framework
{
	[Serializable]
	public class StartingLocationGridElement : IRenderable
	{
		public string Tile;

		public string Foreground;

		public string Detail;

		public string Background;

		public ColorChars getColorChars()
		{
			ColorChars result = default(ColorChars);
			result.foreground = Foreground[0];
			result.background = Background[0];
			result.detail = Detail[0];
			return result;
		}

		public string getColorString()
		{
			throw new NotImplementedException();
		}

		public char getDetailColor()
		{
			return Detail[0];
		}

		public bool getHFlip()
		{
			return false;
		}

		public string getRenderString()
		{
			throw new NotImplementedException();
		}

		public string getTile()
		{
			return Tile;
		}

		public string getTileColor()
		{
			throw new NotImplementedException();
		}

		public bool getVFlip()
		{
			return false;
		}
	}
}
