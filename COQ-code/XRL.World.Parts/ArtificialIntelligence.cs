using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class ArtificialIntelligence : IPoweredPart
	{
		public bool ShowAsDeactivated;

		public ArtificialIntelligence()
		{
			WorksOnSelf = true;
			IsRustSensitive = false;
		}

		public override bool SameAs(IPart p)
		{
			return base.SameAs(p);
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != GetHostileWalkRadiusEvent.ID && (ID != PooledEvent<GetDisplayNameEvent>.ID || !ShowAsDeactivated) && (ID != GetShortDescriptionEvent.ID || !ShowAsDeactivated))
			{
				return ID == PooledEvent<IsConversationallyResponsiveEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(BeginTakeActionEvent E)
		{
			if (!ParentObject.IsPlayer() && IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
			{
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(IsConversationallyResponsiveEvent E)
		{
			if (E.Speaker == ParentObject && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
			{
				E.Message = ParentObject.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ParentObject.Is + " utterly unresponsive.";
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetHostileWalkRadiusEvent E)
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
			{
				E.Radius = 0;
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetDisplayNameEvent E)
		{
			if (ShowAsDeactivated && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
			{
				E.AddTag("{{y|[{{K|deactivated}}]}}", 20);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetShortDescriptionEvent E)
		{
			if (ShowAsDeactivated)
			{
				ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null);
				if (activePartStatus != 0)
				{
					E.Postfix.AppendRules(activePartStatus switch
					{
						ActivePartStatus.Unpowered => "Deactivated: Currently without power.", 
						ActivePartStatus.SwitchedOff => "Deactivated: Currently switched off.", 
						_ => "Deactivated: Currently non-functional.", 
					});
				}
			}
			return base.HandleEvent(E);
		}
	}
}
