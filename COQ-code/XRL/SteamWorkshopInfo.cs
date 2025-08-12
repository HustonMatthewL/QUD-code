using Steamworks;
using UnityEngine;

namespace XRL
{
	public class SteamWorkshopInfo
	{
		public ulong WorkshopId;

		public string Title;

		public string Description;

		public string Tags;

		public string Visibility;

		public string ImagePath;

		public void OpenWorkshopPage()
		{
			if (WorkshopId == 0L)
			{
				return;
			}
			if (PlatformManager.SteamInitialized)
			{
				string text = "steam://url/CommunityFilePage/" + WorkshopId;
				if (SteamUtils.IsOverlayEnabled())
				{
					SteamFriends.ActivateGameOverlayToWebPage(text);
				}
				else
				{
					Application.OpenURL(text);
				}
			}
			else
			{
				Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=" + WorkshopId);
			}
		}
	}
}
