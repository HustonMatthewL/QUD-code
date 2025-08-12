using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class UrbanCamouflage : ICamouflage
	{
		public UrbanCamouflage()
		{
			base.EffectClass = "UrbanCamouflaged";
			Description = "Urban camouflage: This item grants the wearer +=level= DV near trash and furniture.";
		}
	}
}
