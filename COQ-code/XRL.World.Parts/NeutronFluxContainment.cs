using System;
using XRL.Liquids;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts
{
	[Serializable]
	public class NeutronFluxContainment : IPoweredPart
	{
		public NeutronFluxContainment()
		{
			WorksOnSelf = true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != AfterObjectCreatedEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != PooledEvent<NeutronFluxPourExplodesEvent>.ID)
			{
				return ID == PowerSwitchFlippedEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(NeutronFluxPourExplodesEvent E)
		{
			if (E.PouredTo == ParentObject)
			{
				if (E.Prospective)
				{
					if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null) && E.PouredBy != null)
					{
						E.PouredBy.Fail(ParentObject.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + GetStatusPhrase() + " and cannot be poured into.");
						E.Interrupt = true;
					}
					return false;
				}
				if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
				{
					return false;
				}
			}
			else if (E.PouredFrom == ParentObject)
			{
				if (E.Prospective)
				{
					if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
					{
						if (E.PouredBy != null)
						{
							E.PouredBy.Fail(ParentObject.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + GetStatusPhrase() + " and cannot be poured from.");
							E.Interrupt = true;
						}
						return false;
					}
					if (!IsSuitableToReceiveNeutronFlux(E.PouredTo, E.PouredBy))
					{
						GameObject pouredBy = E.PouredBy;
						if (pouredBy == null || !pouredBy.IsPlayer())
						{
							return false;
						}
						if (Popup.ShowYesNo("There's no magnetic containment inside " + E.PouredTo.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ". Pour anyway?") != 0)
						{
							E.Interrupt = true;
							return false;
						}
					}
				}
				else if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null) && IsSuitableToReceiveNeutronFlux(E.PouredTo, E.PouredBy))
				{
					return false;
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeginTakeActionEvent E)
		{
			GameObject @object = E.Object;
			if (@object != null && @object.IsPlayer() && ParentObject.GetObjectContext() == E.Object)
			{
				LiquidVolume liquidVolume = ParentObject.LiquidVolume;
				if (liquidVolume != null && liquidVolume.ContainsLiquid("neutronflux") && WasReady() && !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, E.Traveling ? 102 : 2, null, UseChargeIfUnpowered: false, 0L, null))
				{
					if (E.Traveling)
					{
						if (!E.TravelMessagesSuppressed)
						{
							E.TravelMessagesSuppressed = true;
							if (Popup.ShowYesNo(GetWarningMessage() + " Do you want to stop travelling?") == DialogResult.Yes)
							{
								return false;
							}
						}
					}
					else
					{
						Popup.Show(GetWarningMessage());
						AutoAct.Interrupt();
					}
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CellChangedEvent E)
		{
			CheckExplosion();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EffectAppliedEvent E)
		{
			CheckExplosion();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EffectRemovedEvent E)
		{
			CheckExplosion();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BootSequenceInitializedEvent E)
		{
			CheckExplosion();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(PowerSwitchFlippedEvent E)
		{
			CheckExplosion();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AfterObjectCreatedEvent E)
		{
			if (E.Context == "Wares" || E.Context == "Stock" || E.Context == "Restock")
			{
				ParentObject.LiquidVolume?.Empty();
			}
			else
			{
				LiquidVolume liquidVolume = ParentObject.LiquidVolume;
				if (liquidVolume != null && liquidVolume.ContainsLiquid("neutronflux"))
				{
					if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: true, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
					{
						ParentObject.GetPart<BootSequence>()?.ForceBoot();
					}
					else
					{
						ParentObject.LiquidVolume.Empty();
					}
				}
			}
			return base.HandleEvent(E);
		}

		public override bool WantTurnTick()
		{
			return true;
		}

		public override void TurnTick(long TimeTick, int Amount)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount, null, UseChargeIfUnpowered: false, 0, NeedStatusUpdate: false, null);
			CheckExplosion();
		}

		public static bool IsSuitableToReceiveNeutronFlux(GameObject Object, GameObject Actor = null)
		{
			if (!GameObject.Validate(ref Object))
			{
				return false;
			}
			if (Object.LiquidVolume == null)
			{
				return false;
			}
			if (GetPreferredLiquidEvent.GetFor(Object, Actor) == "neutronflux")
			{
				return true;
			}
			if (Object.HasPart<NoDamage>() || Object.HasPart<NoDamageExcept>())
			{
				return true;
			}
			if (Object.TryGetPart<NeutronFluxContainment>(out var Part) && Part.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
			{
				return true;
			}
			return false;
		}

		public void CheckExplosion()
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null && liquidVolume.ContainsLiquid("neutronflux") && !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
			{
				GameObject holder = ParentObject.Holder;
				DidX("explode", null, "!", null, null, null, holder, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: true);
				LiquidNeutronFlux.Explode(ParentObject, holder);
			}
		}

		private string GetWarningMessage()
		{
			return ParentObject.Does("beep", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " loudly and " + ParentObject.GetVerb("flash") + " a warning glyph.";
		}
	}
}
