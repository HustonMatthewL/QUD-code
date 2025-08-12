using System;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class KindrishProperties : LowStatBooster
	{
		public static bool ReturnAward()
		{
			The.Game.PlayerReputation.Modify("Hindren", 400, "Quest");
			GameObject gameObject = GameObject.Create("Force Bracelet", 0, 1);
			gameObject.MakeUnderstood();
			Popup.Show("You receive " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
			IComponent<GameObject>.ThePlayer.ReceiveObject(gameObject);
			return true;
		}

		public KindrishProperties()
		{
			base.AffectedStats = "Strength,Agility,Toughness,Intelligence,Willpower,Ego";
			base.Amount = 3;
			DescribeStatusForProperty = null;
		}
	}
}
