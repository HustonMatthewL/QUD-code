using System;
using XRL.UI;

namespace XRL.World
{
	[Serializable]
	public class DynamicQuestRewardElement_GameObject : DynamicQuestRewardElement
	{
		public string cacheID;

		public DynamicQuestRewardElement_GameObject()
		{
		}

		public DynamicQuestRewardElement_GameObject(GameObject go)
			: this()
		{
			cacheID = The.ZoneManager.CacheObject(go);
		}

		public override string getRewardConversationType()
		{
			return null;
		}

		public override string getRewardAcceptQuestText()
		{
			return null;
		}

		public override void award()
		{
			GameObject gameObject = The.ZoneManager.PullCachedObject(cacheID);
			gameObject.MakeUnderstood();
			Popup.Show("You receive " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
			The.Player.ReceiveObject(gameObject);
		}
	}
}
