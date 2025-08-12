using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts
{
	[Serializable]
	public class NightVision : IPoweredPart
	{
		public int Radius = 40;

		public NightVision()
		{
			WorksOnEquipper = true;
			IsPowerLoadSensitive = true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != BeforeRenderEvent.ID && ID != SingletonEvent<EarlyBeforeBeginTakeActionEvent>.ID && ID != EquippedEvent.ID)
			{
				return ID == SingletonEvent<EndTurnEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(EarlyBeforeBeginTakeActionEvent E)
		{
			if (!base.OnWorldMap && ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, 1, null, UseChargeIfUnpowered: false, 0, NeedStatusUpdate: false, null))
			{
				GameObject equipped = ParentObject.Equipped;
				if (equipped != null && equipped.IsPlayer() && AutoAct.IsInterruptable() && !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
				{
					AutoAct.Interrupt(equipped.poss(ParentObject, Definite: true, null) + ParentObject.GetVerb("have") + " stopped working");
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeforeApplyDamageEvent E)
		{
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null) && E.Damage.IsLightDamage())
			{
				E.Damage.Amount = E.Damage.Amount * 5 / 4;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EquippedEvent E)
		{
			if (!base.OnWorldMap)
			{
				ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, 1, null, UseChargeIfUnpowered: false, 0, NeedStatusUpdate: false, null);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeforeRenderEvent E)
		{
			GameObject equipped = ParentObject.Equipped;
			if (equipped != null && equipped.IsPlayer() && !equipped.OnWorldMap())
			{
				Cell cell = equipped.CurrentCell;
				if (cell != null)
				{
					int lastPowerLoadLevel = GetLastPowerLoadLevel();
					int? powerLoadLevel = lastPowerLoadLevel;
					if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
					{
						int num = Radius;
						int num2 = MyPowerLoadBonus(lastPowerLoadLevel, 100, 10);
						if (num2 != 0)
						{
							num = num * (100 + num2) / 100;
						}
						cell.ParentZone.AddLight(cell.X, cell.Y, num, LightLevel.Darkvision);
					}
				}
			}
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}
	}
}
