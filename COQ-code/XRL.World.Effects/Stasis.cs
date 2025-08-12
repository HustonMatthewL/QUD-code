using System;
using XRL.World.Parts;

namespace XRL.World.Effects
{
	[Serializable]
	public class Stasis : Effect
	{
		public Stasis()
		{
			DisplayName = "{{M|stasis}}";
			Duration = 1;
		}

		public override string GetDescription()
		{
			return "{{M|in stasis}}";
		}

		public override string GetDetails()
		{
			return "Suspended in spacetime. Cannot act or be interacted with.";
		}

		public override int GetEffectType()
		{
			return 33558528;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeAITakingActionEvent>.ID && ID != BeforeApplyDamageEvent.ID && ID != CanApplyEffectEvent.ID && ID != PooledEvent<CanBeDismemberedEvent>.ID && ID != PooledEvent<CanBeInvoluntarilyMovedEvent>.ID && ID != CanBeReplicatedEvent.ID && ID != PooledEvent<CanTemperatureReturnToAmbientEvent>.ID && ID != SingletonEvent<EarlyBeforeBeginTakeActionEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetCompanionStatusEvent>.ID && ID != PooledEvent<GetKineticResistanceEvent>.ID)
			{
				return ID == PooledEvent<IsConversationallyResponsiveEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(BeforeAITakingActionEvent E)
		{
			if (E.Actor == base.Object)
			{
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CanBeDismemberedEvent E)
		{
			return false;
		}

		public override bool HandleEvent(GetCompanionStatusEvent E)
		{
			if (E.Object == base.Object)
			{
				E.AddStatus("in stasis", 200);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CanTemperatureReturnToAmbientEvent E)
		{
			if (E.Object == base.Object)
			{
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
		{
			if (E.Object == base.Object)
			{
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EarlyBeforeBeginTakeActionEvent E)
		{
			if (CheckMaintenance())
			{
				E.PreventAction = true;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(BeforeApplyDamageEvent E)
		{
			if (E.Actor != null)
			{
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your attack bounces harmlessly off of " + base.Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".", IComponent<GameObject>.ConsequentialColor(base.Object));
				}
				else if (IComponent<GameObject>.Visible(E.Actor) || IComponent<GameObject>.Visible(base.Object))
				{
					IComponent<GameObject>.AddPlayerMessage(E.Actor.Poss("attack") + " bounces harmlessly off of " + base.Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".", IComponent<GameObject>.ConsequentialColor(base.Object));
				}
			}
			return false;
		}

		public override bool HandleEvent(IsConversationallyResponsiveEvent E)
		{
			if (E.Speaker == base.Object)
			{
				if (E.Mental && !E.Physical)
				{
					E.Message = "You can dimly sense " + base.Object.poss("mind") + ", but it is unresponsive.";
				}
				else
				{
					E.Message = base.Object.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + base.Object.Is + " utterly unresponsive.";
				}
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CanApplyEffectEvent E)
		{
			return false;
		}

		public override bool HandleEvent(CanBeReplicatedEvent E)
		{
			return false;
		}

		public override bool HandleEvent(EndTurnEvent E)
		{
			CheckMaintenance();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetKineticResistanceEvent E)
		{
			E.LinearIncrease = 99999999;
			E.PercentageIncrease = 0;
			E.LinearReduction = 0;
			E.PercentageReduction = 0;
			return false;
		}

		public override bool Apply(GameObject Object)
		{
			if (!MaintenanceSustained(Object))
			{
				return false;
			}
			if (Object.IsInStasis())
			{
				return false;
			}
			Object.ForfeitTurn();
			return true;
		}

		public override void Remove(GameObject Object)
		{
			base.Remove(Object);
		}

		private void ApplyStats()
		{
			base.StatShifter.SetStatShift(base.Object, "AcidResistance", 1000);
			base.StatShifter.SetStatShift(base.Object, "ColdResistance", 1000);
			base.StatShifter.SetStatShift(base.Object, "ElectricResistance", 1000);
			base.StatShifter.SetStatShift(base.Object, "HeatResistance", 1000);
		}

		private void UnapplyStats()
		{
			base.StatShifter.RemoveStatShifts(base.Object);
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("AfterDeepCopyWithoutEffects");
			Registrar.Register("BeforeDeepCopyWithoutEffects");
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "BeforeDeepCopyWithoutEffects")
			{
				UnapplyStats();
			}
			else if (E.ID == "AfterDeepCopyWithoutEffects")
			{
				ApplyStats();
			}
			return base.FireEvent(E);
		}

		public bool CheckMaintenance(GameObject Skip = null)
		{
			if (!MaintenanceSustained(Skip))
			{
				base.Object?.RemoveEffect(this);
				return false;
			}
			return true;
		}

		public bool MaintenanceSustained(GameObject Skip = null)
		{
			if (!EligibleForStasis(base.Object))
			{
				return false;
			}
			Cell cell = base.Object.CurrentCell;
			if (cell == null)
			{
				return false;
			}
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				if (cell.Objects[i] != Skip && cell.Objects[i].GetPart<Stasisfield>() != null && cell.Objects[i].PhaseMatches(base.Object))
				{
					return true;
				}
			}
			return false;
		}

		public static bool EligibleForStasis(GameObject obj)
		{
			if (!GameObject.Validate(ref obj))
			{
				return false;
			}
			if (!obj.IsReal)
			{
				return false;
			}
			if (obj.HasTagOrProperty("ForcefieldNullifier"))
			{
				return false;
			}
			return true;
		}
	}
}
