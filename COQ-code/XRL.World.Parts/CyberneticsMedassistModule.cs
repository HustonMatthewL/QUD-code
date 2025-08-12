using System;
using System.Collections.Generic;
using System.Text;
using XRL.Collections;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts
{
	[Serializable]
	public class CyberneticsMedassistModule : IPoweredPart
	{
		public static readonly string COMMAND_NAME = "CommandToggleMedassistModule";

		public Guid ActivatedAbilityID = Guid.Empty;

		public int TonicCapacity = 8;

		public CyberneticsMedassistModule()
		{
			ChargeUse = 0;
			WorksOnImplantee = true;
			NameForStatus = "DeploymentExpertSystem";
		}

		public void CollectStats(Templates.StatCollector stats)
		{
			stats.Set("Loadout", GetCurrentLoadout().ToString());
		}

		public StringBuilder GetCurrentLoadout()
		{
			Inventory inventory = ParentObject.Inventory;
			if (inventory != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\n{{c|Current loadout:}}");
				if (inventory.Objects.Count > 0)
				{
					foreach (GameObject @object in inventory.Objects)
					{
						stringBuilder.Append("\n ").Append('ú').Append(' ')
							.Append(@object.DisplayName);
					}
				}
				else
				{
					stringBuilder.Append(" {{y|no injectors}}");
				}
				return stringBuilder;
			}
			return null;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != InventoryActionEvent.ID && ID != OwnerGetInventoryActionsEvent.ID && ID != UnimplantedEvent.ID)
			{
				return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
		{
			DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject?.Implantee);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(ImplantedEvent E)
		{
			E.Implantee.RegisterPartEvent(this, "BeforeApplyDamage");
			E.Implantee.RegisterPartEvent(this, "CommandToggleMedassistModule");
			E.Implantee.RegisterPartEvent(this, "OwnerGetInventoryActions");
			ActivatedAbilityID = E.Implantee.AddActivatedAbility("Medassist Module", COMMAND_NAME, "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(UnimplantedEvent E)
		{
			E.Implantee.UnregisterPartEvent(this, "BeforeApplyDamage");
			E.Implantee.UnregisterPartEvent(this, "CommandToggleMedassistModule");
			E.Implantee.UnregisterPartEvent(this, "OwnerGetInventoryActions");
			E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CommandEvent E)
		{
			if (E.Command == COMMAND_NAME && E.Actor == ParentObject.Implantee)
			{
				ParentObject.Implantee.ToggleActivatedAbility(ActivatedAbilityID, Silent: false, null);
				ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetShortDescriptionEvent E)
		{
			E.Infix = GetCurrentLoadout();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetInventoryActionsEvent E)
		{
			E.AddAction("Load Tonics", "load tonics", "LoadTonics", null, 'o');
			E.AddAction("Eject Tonics", "eject tonics", "EjectTonics", null, 'j');
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
		{
			if (CanBeLoaded(E.Object))
			{
				E.AddAction("Load Into Medassist Module", "load into medassist module", "LoadTonic", null, 'm', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: true, ParentObject);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "LoadTonic")
			{
				Inventory inventory = ParentObject.Inventory;
				if (inventory == null)
				{
					throw new Exception("inventory missing from " + ParentObject.DebugName);
				}
				if (!CanBeLoaded(E.Item))
				{
					throw new Exception("received invalid item " + E.Item.DebugName);
				}
				if (GetLoadedCount() >= TonicCapacity)
				{
					E.Actor.Fail(E.Actor.Poss(ParentObject, Definite: true, null) + ParentObject.Is + " full.");
				}
				else
				{
					E.Item.PlayWorldSound("Sounds/Interact/sfx_interact_medassistModule_tonic_load");
					GameObject gameObject = E.Item.RemoveOne();
					gameObject.RemoveFromContext();
					inventory.AddObject(gameObject);
					if (E.Actor.IsPlayer())
					{
						Popup.Show("You slot " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " into " + E.Actor.poss(ParentObject, Definite: true, null) + ".");
					}
					E.Actor.UseEnergy(1000);
					E.RequestInterfaceExit();
				}
			}
			else if (E.Command == "LoadTonics")
			{
				Inventory inventory2 = ParentObject.Inventory;
				if (inventory2 == null)
				{
					throw new Exception("inventory missing from " + ParentObject.DebugName);
				}
				if (GetLoadedCount() >= TonicCapacity)
				{
					E.Actor.Fail(E.Actor.Poss(ParentObject, Definite: true, null) + ParentObject.Is + " full.");
				}
				else
				{
					using ScopeDisposedList<GameObject> scopeDisposedList2 = ScopeDisposedList<GameObject>.GetFromPool();
					using ScopeDisposedList<GameObject> scopeDisposedList = ScopeDisposedList<GameObject>.GetFromPool();
					E.Actor.GetContents(scopeDisposedList);
					foreach (GameObject item in scopeDisposedList)
					{
						if (CanBeLoaded(item))
						{
							BodyPart BodyPartContext;
							int Relation;
							IContextRelationManager RelationManager;
							GameObject objectContext = item.GetObjectContext(out BodyPartContext, out Relation, out RelationManager);
							if (objectContext != null && objectContext != ParentObject && Relation != 4 && Relation != 6)
							{
								scopeDisposedList2.Add(item);
							}
						}
					}
					if (scopeDisposedList2.Count <= 0)
					{
						E.Actor.Fail("You have no tonics to load.");
					}
					else
					{
						GameObject gameObject2 = PickItem.ShowPicker(scopeDisposedList2, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor, null, null, null, PreserveOrder: false, null, ShowContext: true);
						if (gameObject2 != null)
						{
							E.Item.PlayWorldSound("Sounds/Interact/sfx_interact_medassistModule_tonic_load");
							gameObject2.SplitFromStack();
							gameObject2.RemoveFromContext();
							inventory2.AddObject(gameObject2);
							if (E.Actor.IsPlayer())
							{
								Popup.Show("You slot " + gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " into " + E.Actor.poss(ParentObject, Definite: true, null) + ".");
							}
							E.Actor.UseEnergy(1000);
							E.RequestInterfaceExit();
						}
					}
				}
			}
			else if (E.Command == "EjectTonics")
			{
				Inventory inventory3 = ParentObject.Inventory;
				if (inventory3 == null)
				{
					throw new Exception("inventory missing from " + ParentObject.DebugName);
				}
				int num = 0;
				foreach (GameObject @object in inventory3.Objects)
				{
					num += @object.Count;
				}
				if (num <= 0)
				{
					E.Actor.Fail(E.Actor.Poss(ParentObject, Definite: true, null) + ParentObject.Is + " empty.");
				}
				else
				{
					E.Item.PlayWorldSound("Sounds/Interact/sfx_interact_medassistModule_tonic_eject");
					int num2 = TonicCapacity * 3;
					GameObject gameObject3 = null;
					while (inventory3.Objects.Count > 0 && --num2 > 0)
					{
						gameObject3 = inventory3.Objects[0];
						E.Actor.TakeObject(gameObject3, NoStack: false, Silent: false, 0);
					}
					if (E.Actor.IsPlayer())
					{
						Popup.Show("You eject " + ((num == 1) ? gameObject3.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) : "the injectors") + " from " + E.Actor.poss(ParentObject, Definite: true, null) + ".");
						E.Actor.UseEnergy(1000);
						E.RequestInterfaceExit();
					}
				}
			}
			return base.HandleEvent(E);
		}

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool WantTurnTick()
		{
			return true;
		}

		public override void TurnTick(long TimeTick, int Amount)
		{
			AttemptMedicalAssistance();
		}

		public override bool GetActivePartLocallyDefinedFailure()
		{
			return !ParentObject.Implantee.IsActivatedAbilityToggledOn(ActivatedAbilityID);
		}

		public override string GetActivePartLocallyDefinedFailureDescription()
		{
			return "Deactivated";
		}

		public void AttemptMedicalAssistance(Damage damage = null)
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
			{
				return;
			}
			Inventory inventory = ParentObject.Inventory;
			if (inventory == null || inventory.Objects.Count <= 0)
			{
				return;
			}
			GameObject implantee = ParentObject.Implantee;
			if (implantee.GetTonicEffectCount() >= implantee.GetTonicCapacity())
			{
				return;
			}
			int num = 0;
			GameObject gameObject = null;
			int i = 0;
			for (int count = inventory.Objects.Count; i < count; i++)
			{
				GameObject gameObject2 = inventory.Objects[i];
				if (!gameObject2.IsBroken() && !gameObject2.IsRusted())
				{
					int utilityScore = GetUtilityScore(implantee, gameObject2, damage);
					if (utilityScore > num)
					{
						gameObject = gameObject2;
						num = utilityScore;
					}
				}
			}
			if (gameObject == null || IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
			{
				return;
			}
			int @for = GetTonicDosageEvent.GetFor(gameObject, implantee, ParentObject);
			if (@for <= 0)
			{
				return;
			}
			List<Effect> tonicEffects = implantee.GetTonicEffects();
			if (implantee.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your " + ParentObject.does("inject", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you with " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
			}
			Event @event = Event.New("ApplyingTonic");
			@event.SetParameter("Subject", implantee);
			@event.SetParameter("Actor", ParentObject);
			@event.SetParameter("Tonic", gameObject);
			@event.SetParameter("Dosage", @for);
			@event.SetFlag("External", State: false);
			@event.SetFlag("Involuntary", State: true);
			if (!implantee.FireEvent(@event))
			{
				if (implantee.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("The injection fails.");
				}
				return;
			}
			Event event2 = Event.New("ApplyTonic");
			event2.SetParameter("Owner", ParentObject);
			event2.SetParameter("Target", implantee);
			event2.SetParameter("Actor", ParentObject);
			event2.SetParameter("Subject", implantee);
			event2.SetParameter("Attacker", (object)null);
			event2.SetParameter("Overdose", "No");
			event2.SetParameter("Dosage", @for);
			event2.SetFlag("External", State: false);
			event2.SetFlag("Involuntary", State: true);
			if (!gameObject.FireEvent(event2))
			{
				if (implantee.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("The injection fails.");
				}
				return;
			}
			gameObject.Destroy();
			Event event3 = Event.New("TonicAutoApplied");
			event3.SetParameter("Subject", implantee);
			event3.SetParameter("By", ParentObject);
			event3.SetParameter("Damage", damage);
			foreach (Effect tonicEffect in implantee.GetTonicEffects())
			{
				if (!tonicEffects.Contains(tonicEffect))
				{
					tonicEffect.FireEvent(event3);
				}
			}
		}

		public static int GetUtilityScore(GameObject who, GameObject tonic, Damage damage = null)
		{
			return GetUtilityScoreEvent.GetFor(who, tonic, damage);
		}

		public override bool AllowStaticRegistration()
		{
			return false;
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "BeforeApplyDamage" && E.GetParameter("Damage") is Damage damage)
			{
				GameObject implantee = ParentObject.Implantee;
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				if (E.HasParameter("Phase"))
				{
					int intParameter = E.GetIntParameter("Phase");
					bool num = implantee.PhaseMatches(intParameter);
					AttemptMedicalAssistance(damage);
					if (num && !implantee.PhaseMatches(intParameter))
					{
						return false;
					}
				}
				else if (gameObjectParameter != null)
				{
					bool num2 = implantee.PhaseMatches(gameObjectParameter);
					AttemptMedicalAssistance(damage);
					if (num2 && !implantee.PhaseMatches(gameObjectParameter))
					{
						return false;
					}
				}
				else
				{
					int phase = implantee.GetPhase();
					AttemptMedicalAssistance(damage);
					if (!implantee.PhaseMatches(phase))
					{
						return false;
					}
				}
			}
			return base.FireEvent(E);
		}

		public static bool CanBeLoaded(GameObject obj)
		{
			Tonic part = obj.GetPart<Tonic>();
			if (part == null)
			{
				return false;
			}
			if (part.Eat)
			{
				return false;
			}
			return true;
		}

		public int GetLoadedCount()
		{
			int num = 0;
			Inventory inventory = ParentObject.Inventory;
			if (inventory != null)
			{
				foreach (GameObject @object in inventory.Objects)
				{
					num += @object.Count;
				}
			}
			return num;
		}
	}
}
