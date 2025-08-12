using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Core;

namespace XRL.World
{
	public class RenderEvent : IRenderable
	{
		public int x;

		public int y;

		public bool WantsToPaint;

		public bool DisableFullscreenColorEffects;

		public bool Alt;

		public bool CustomDraw;

		public string WantsBackdrop;

		public bool BackdropBleedthrough;

		public bool AsIfKnown;

		public string _Tile;

		public string Final;

		public string ColorString;

		public string RenderString;

		public string BackgroundString;

		public string DetailColor;

		public int ColorStringPriority;

		public int DetailColorPriority;

		public int BackgroundStringPriority;

		public bool NoWake;

		public bool HFlip;

		public bool VFlip;

		public List<ImposterExtra.ImposterInfo> Imposters = new List<ImposterExtra.ImposterInfo>();

		public int HighestLayer = -1;

		public LightLevel Lit = LightLevel.None;

		public string Tile
		{
			get
			{
				return _Tile;
			}
			set
			{
				if (value == null)
				{
					HFlip = false;
					VFlip = false;
				}
				_Tile = value;
			}
		}

		public bool ColorsVisible => Zone.ColorsVisible(Lit);

		string IRenderable.getTile()
		{
			return Tile;
		}

		public string getColorString()
		{
			return ColorString;
		}

		public string getRenderString()
		{
			return RenderString;
		}

		public char getDetailColor()
		{
			if (string.IsNullOrEmpty(DetailColor))
			{
				return '\0';
			}
			return DetailColor[0];
		}

		public RenderEvent GreyOutForUI()
		{
			DetailColor = "K";
			ColorString = "&K";
			return this;
		}

		public void RenderEffectIndicator(string renderString, string tile, string colorString, string detailColor, int frameHint, int durationHint = 10)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num >= frameHint && num < frameHint + durationHint)
			{
				Tile = tile;
				RenderString = renderString;
				ColorString = colorString;
				DetailColor = detailColor;
			}
		}

		public bool ImpostersMatch(RenderEvent ev)
		{
			if (ev.Imposters.Count != Imposters.Count)
			{
				return false;
			}
			for (int i = 0; i < ev.Imposters.Count; i++)
			{
				if (Imposters[i].SameAs(ev.Imposters[i]))
				{
					return false;
				}
			}
			return true;
		}

		public void Reset()
		{
			Final = null;
			ColorString = "&y";
			BackgroundString = "";
			HighestLayer = -1;
			WantsToPaint = false;
			Alt = false;
			CustomDraw = false;
			AsIfKnown = false;
			_Tile = null;
			RenderString = " ";
			DetailColor = null;
			Lit = LightLevel.Light;
			NoWake = false;
			HFlip = false;
			VFlip = false;
			Imposters.Clear();
			ColorStringPriority = 0;
			DetailColorPriority = 0;
			WantsBackdrop = null;
			BackdropBleedthrough = false;
			BackgroundStringPriority = 0;
		}

		public bool getHFlip()
		{
			return HFlip;
		}

		public bool getVFlip()
		{
			return VFlip;
		}

		public ColorChars getColorChars()
		{
			char foreground = 'y';
			char background = 'k';
			string colorString = getColorString();
			if (!string.IsNullOrEmpty(colorString))
			{
				int num = colorString.LastIndexOf(ColorChars.FOREGROUND_INDICATOR);
				int num2 = colorString.LastIndexOf(ColorChars.BACKGROUND_INDICATOR);
				if (num >= 0 && num < colorString.Length - 1)
				{
					foreground = colorString[num + 1];
				}
				if (num2 >= 0 && num2 < colorString.Length - 1)
				{
					background = colorString[num2 + 1];
				}
			}
			ColorChars result = default(ColorChars);
			result.detail = getDetailColor();
			result.foreground = foreground;
			result.background = background;
			return result;
		}

		public Color GetForegroundColor()
		{
			if (ColorString != null)
			{
				int num = ColorString.LastIndexOf('&');
				if (num != -1)
				{
					return ConsoleLib.Console.ColorUtility.ColorMap[ColorString[num + 1]];
				}
			}
			return The.Color.Gray;
		}

		public char GetForegroundColorChar()
		{
			if (ColorString != null && ColorString.Length > 1)
			{
				return ColorString[1];
			}
			return 'y';
		}

		public Color GetDetailColor()
		{
			if (DetailColor != null && DetailColor.Length > 0)
			{
				if (ConsoleLib.Console.ColorUtility.ColorMap.TryGetValue(DetailColor[0], out var value))
				{
					return value;
				}
				return GetForegroundColor();
			}
			return GetForegroundColor();
		}

		public char GetDetailColorChar()
		{
			if (DetailColor != null && DetailColor.Length > 0)
			{
				return DetailColor[0];
			}
			return 'w';
		}

		public Color GetBackgroundColor()
		{
			if (BackgroundString != null)
			{
				int num = BackgroundString.LastIndexOf('^');
				if (num != -1)
				{
					return ConsoleLib.Console.ColorUtility.ColorMap[BackgroundString[num + 1]];
				}
			}
			if (ColorString != null)
			{
				int num2 = ColorString.LastIndexOf('^');
				if (num2 != -1)
				{
					return ConsoleLib.Console.ColorUtility.ColorMap[ColorString[num2 + 1]];
				}
			}
			return The.Color.DarkBlack;
		}

		string IRenderable.getTileColor()
		{
			return null;
		}

		public void TileVariantColors(string SingleColor, string TileForeground, string TileDetail)
		{
			if (!ColorsVisible)
			{
				return;
			}
			if (!Tile.IsNullOrEmpty())
			{
				if (!TileForeground.IsNullOrEmpty())
				{
					ColorString = TileForeground;
				}
				if (!TileForeground.IsNullOrEmpty())
				{
					DetailColor = TileDetail;
				}
			}
			else if (!SingleColor.IsNullOrEmpty())
			{
				ColorString = SingleColor;
			}
		}

		public void ApplyColors(string TextForeground, string TileForeground, string TileDetail, string Background, int TextForegroundPriority, int TileForegroundPriority, int TileDetailPriority, int BackgroundPriority)
		{
			if (!ColorsVisible)
			{
				return;
			}
			if (!Tile.IsNullOrEmpty())
			{
				if (TileForegroundPriority > ColorStringPriority && !TileForeground.IsNullOrEmpty())
				{
					ColorString = TileForeground;
					ColorStringPriority = TileForegroundPriority;
				}
				if (TileDetailPriority > DetailColorPriority && !TileDetail.IsNullOrEmpty())
				{
					DetailColor = TileDetail;
					DetailColorPriority = TileDetailPriority;
				}
			}
			else if (TextForegroundPriority > ColorStringPriority && !TextForeground.IsNullOrEmpty())
			{
				ColorString = TextForeground;
				ColorStringPriority = TextForegroundPriority;
			}
			if (BackgroundPriority > BackgroundStringPriority && !Background.IsNullOrEmpty())
			{
				BackgroundString = Background;
				BackgroundStringPriority = BackgroundPriority;
			}
		}

		public void ApplyColors(string Foreground, string Background, string Detail, int ForegroundPriority, int BackgroundPriority, int DetailPriority)
		{
			if (ColorsVisible)
			{
				if (ForegroundPriority > ColorStringPriority && !Foreground.IsNullOrEmpty())
				{
					ColorString = Foreground;
					ColorStringPriority = ForegroundPriority;
				}
				if (BackgroundPriority > BackgroundStringPriority && !Background.IsNullOrEmpty())
				{
					BackgroundString = Background;
					BackgroundStringPriority = BackgroundPriority;
				}
				if (!Tile.IsNullOrEmpty() && DetailPriority > DetailColorPriority && !Detail.IsNullOrEmpty())
				{
					DetailColor = Detail;
					DetailColorPriority = DetailPriority;
				}
			}
		}

		public void ApplyColors(string Foreground, string Detail, int ForegroundPriority, int DetailPriority)
		{
			if (ColorsVisible)
			{
				if (ForegroundPriority > ColorStringPriority && !Foreground.IsNullOrEmpty())
				{
					ColorString = Foreground;
					ColorStringPriority = ForegroundPriority;
				}
				if (!Tile.IsNullOrEmpty() && DetailPriority > DetailColorPriority && !Detail.IsNullOrEmpty())
				{
					DetailColor = Detail;
					DetailColorPriority = DetailPriority;
				}
			}
		}

		public void ApplyBackgroundColor(string BackgroundString, int BackgroundStringPriority)
		{
			if (ColorsVisible && BackgroundStringPriority > this.BackgroundStringPriority && !BackgroundString.IsNullOrEmpty())
			{
				this.BackgroundString = BackgroundString;
				this.BackgroundStringPriority = BackgroundStringPriority;
			}
		}

		public void ApplyDetailColor(string Detail, int DetailPriority)
		{
			if (ColorsVisible && DetailPriority > ColorStringPriority && !Detail.IsNullOrEmpty())
			{
				DetailColor = Detail;
				DetailColorPriority = DetailPriority;
			}
		}

		public void ApplyColors(string Foreground, int ForegroundPriority)
		{
			if (ColorsVisible && ForegroundPriority > ColorStringPriority && !Foreground.IsNullOrEmpty())
			{
				ColorString = Foreground;
				ColorStringPriority = ForegroundPriority;
			}
		}

		public string GetSpriteName()
		{
			if (Tile != null)
			{
				return Tile;
			}
			if (RenderString.Length == 0)
			{
				return "Text/" + 32 + ".bmp";
			}
			return "Text/" + (int)RenderString[0] + ".bmp";
		}
	}
}
