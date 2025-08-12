using System;
using XRL.UI;

namespace XRL.World
{
	[Serializable]
	public class SocialSifrahTokenDisplayAFarmersToken : SifrahPrioritizableToken
	{
		public SocialSifrahTokenDisplayAFarmersToken()
		{
			Description = "display a farmer's token";
			Tile = "Items/sw_token.bmp";
			RenderString = "\t";
			ColorString = "&y";
			DetailColor = 'g';
		}

		public override int GetPriority()
		{
			if (!IsAvailable())
			{
				return 0;
			}
			return int.MaxValue;
		}

		public override int GetTiebreakerPriority()
		{
			return int.MaxValue;
		}

		public bool IsAvailable()
		{
			return The.Player.ContainsBlueprint("Farmers Token");
		}

		public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
		{
			if (!IsAvailable())
			{
				return true;
			}
			return base.GetDisabled(Game, Slot, ContextObject);
		}

		public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
		{
			if (!IsAvailable())
			{
				Popup.ShowFail("You do not have a farmer's token.");
				return false;
			}
			return base.CheckTokenUse(Game, Slot, ContextObject);
		}
	}
}
