using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts
{
	[Serializable]
	public class HologramMaterialPrimary : IPoweredPart
	{
		public static readonly int ICON_COLOR_PRIORITY;

		public string Tile;

		public string RenderString = "@";

		public int FlickerFrame;

		public int FrameOffset;

		public HologramMaterialPrimary()
		{
			ChargeUse = 1;
			IsBootSensitive = false;
			IsEMPSensitive = true;
			MustBeUnderstood = false;
			WorksOnWearer = true;
			WorksOnSelf = true;
		}

		public override void AddedAfterCreation()
		{
			base.AddedAfterCreation();
			ParentObject.MakeNonflammable();
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanBeDismemberedEvent>.ID && ID != PooledEvent<CanBeInvoluntarilyMovedEvent>.ID && ID != PooledEvent<GetElectricalConductivityEvent>.ID && ID != PooledEvent<GetMatterPhaseEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID && ID != PooledEvent<GetScanTypeEvent>.ID && ID != ObjectCreatedEvent.ID)
			{
				return ID == PooledEvent<RespiresEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(ObjectCreatedEvent E)
		{
			ParentObject.MakeImperviousToHeat();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetElectricalConductivityEvent E)
		{
			if (E.Pass == 1 && E.Object == ParentObject)
			{
				E.Value = 0;
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CanBeDismemberedEvent E)
		{
			if (E.Object == ParentObject)
			{
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
		{
			if (E.Object == ParentObject)
			{
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetMatterPhaseEvent E)
		{
			E.MinMatterPhase(4);
			return false;
		}

		public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
		{
			E.PercentageReduction = 100;
			return false;
		}

		public override bool HandleEvent(GetScanTypeEvent E)
		{
			if (E.Object == ParentObject)
			{
				E.ScanType = Scanning.Scan.Tech;
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(RespiresEvent E)
		{
			return false;
		}

		public override void Initialize()
		{
			base.Initialize();
			Tile = ParentObject.Render.Tile;
			RenderString = ParentObject.Render.RenderString;
		}

		public override bool Render(RenderEvent E)
		{
			string text = null;
			if (WasReady())
			{
				int num = (XRLCore.CurrentFrame + FrameOffset) % 200;
				if (FlickerFrame > 0 || Stat.RandomCosmetic(1, 200) == 1)
				{
					E.Tile = null;
					if (FlickerFrame == 0)
					{
						E.RenderString = "_";
					}
					else if (FlickerFrame == 1)
					{
						E.RenderString = "-";
					}
					else if (FlickerFrame == 2)
					{
						E.RenderString = "|";
					}
					if (num < 8)
					{
						text = "&C";
					}
					else
					{
						text = "&Y";
					}
					if (FlickerFrame == 0)
					{
						FlickerFrame = 3;
					}
					FlickerFrame--;
				}
				text = ((num < 4) ? "&C" : ((num < 8) ? "&b" : ((num >= 12) ? "&B" : "&c")));
				if (!Options.DisableTextAnimationEffects)
				{
					FrameOffset += Stat.Random(0, 20);
				}
				if (FlickerFrame == 0 && Stat.RandomCosmetic(1, 400) == 1)
				{
					text = "&Y";
				}
			}
			else
			{
				text = "&K";
			}
			if (!text.IsNullOrEmpty())
			{
				E.ApplyColors(text, ICON_COLOR_PRIORITY);
			}
			return base.Render(E);
		}
	}
}
