using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
	[Serializable]
	public class Tonic : IPart
	{
		public bool CausesOverdose = true;

		public bool Eat;

		public string BehaviorDescription = "This item is a tonic. Applying one tonic while under the effects of another may produce undesired results.";

		public override bool SameAs(IPart p)
		{
			Tonic tonic = p as Tonic;
			if (tonic.CausesOverdose != CausesOverdose)
			{
				return false;
			}
			if (tonic.Eat != Eat)
			{
				return false;
			}
			if (tonic.BehaviorDescription != BehaviorDescription)
			{
				return false;
			}
			return base.SameAs(p);
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID && ID != GetInventoryActionsAlwaysEvent.ID && ID != GetShortDescriptionEvent.ID)
			{
				return ID == InventoryActionEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetShortDescriptionEvent E)
		{
			E.Postfix.AppendRules(BehaviorDescription);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
		{
			if (!E.Object.IsBroken() && !E.Object.IsRusted())
			{
				int @default = 0;
				if (E.Object.HasPart<Empty_Tonic_Applicator>())
				{
					@default = -100;
				}
				else if (E.Object.Equipped == E.Actor || E.Object.InInventory == E.Actor)
				{
					if (E.Object.IsImportant())
					{
						@default = -1;
					}
					else if (E.Actor.FireEvent("CanApplyTonic") && (!Eat || E.Actor.HasPart<Stomach>()))
					{
						@default = 100;
					}
				}
				if (Eat)
				{
					E.AddAction("Eat", "eat", "Apply", null, 'e', FireOnActor: false, @default, 0, Override: true);
					if (!E.Actor.OnWorldMap())
					{
						E.AddAction("Feed To", "feed to", "ApplyTo", null, 'f', FireOnActor: false, -2, 0, Override: true);
					}
				}
				else
				{
					E.AddAction("Apply", "apply", "Apply", null, 'a', FireOnActor: false, @default);
					if (!E.Actor.OnWorldMap())
					{
						E.AddAction("Apply To", "apply to", "ApplyTo", null, 'a', FireOnActor: false, -2);
					}
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "Apply" || E.Command == "ApplyInvoluntarily" || E.Command == "ApplyExternally")
			{
				bool flag = E.Command == "ApplyInvoluntarily" || E.Command == "ApplyExternally";
				bool flag2 = E.Command == "ApplyInvoluntarily";
				GameObject gameObject = E.ObjectTarget ?? E.Actor;
				if (Eat && !gameObject.HasPart<Stomach>())
				{
					E.Actor.Fail(gameObject.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " unable to consume tonics.");
					return false;
				}
				if (E.Item.IsBroken())
				{
					E.Actor.Fail(E.Item.Itis + " broken...");
					return false;
				}
				if (E.Item.IsRusted())
				{
					E.Actor.Fail(E.Item.Itis + " rusted...");
					return false;
				}
				if (!flag2 && !flag && !E.Item.ConfirmUseImportant(E.Actor, "use", "up", 1))
				{
					return false;
				}
				E.Actor.PlayWorldOrUISound("Sounds/Interact/sfx_interact_applicator_apply", null);
				int @for = GetTonicDosageEvent.GetFor(ParentObject, gameObject, E.Actor);
				Event @event = Event.New("ApplyingTonic");
				@event.SetParameter("Subject", gameObject);
				@event.SetParameter("Actor", E.Actor);
				@event.SetParameter("Tonic", ParentObject);
				@event.SetParameter("Dosage", @for);
				@event.SetFlag("External", flag);
				@event.SetFlag("Involuntary", flag2);
				@event.SetFlag("ShowMessage", State: true);
				if (!gameObject.FireEvent(@event))
				{
					return false;
				}
				if (!BeforeConsumeEvent.Check(E.Actor, gameObject, ParentObject, Eat, Drink: false, !Eat, Inhale: false, Absorb: false, !flag2))
				{
					return false;
				}
				string Message = null;
				if (E.Actor.IsPlayer() || IComponent<GameObject>.Visible(E.Actor))
				{
					ParentObject.MakeUnderstood(out Message);
				}
				List<Effect> tonicEffects = gameObject.GetTonicEffects();
				int tonicCapacity = gameObject.GetTonicCapacity();
				if (tonicEffects.Count >= tonicCapacity && CausesOverdose)
				{
					foreach (Effect item in tonicEffects)
					{
						if (!gameObject.MakeSave("Toughness", 16 + 3 * (tonicEffects.Count - tonicCapacity), null, null, "Overdose", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
						{
							Event event2 = Event.New("Overdose");
							event2.SetParameter("Subject", gameObject);
							event2.SetParameter("Actor", E.Actor);
							event2.SetParameter("Tonic", ParentObject);
							event2.SetParameter("Dosage", @for);
							event2.SetFlag("External", flag);
							event2.SetFlag("Involuntary", flag2);
							item.FireEvent(event2);
						}
					}
				}
				bool flag3 = false;
				string value = "No";
				if (gameObject.IsMutant())
				{
					int chance = 5;
					if (gameObject.HasPart<TonicAllergy>())
					{
						chance = 33;
					}
					if (gameObject.IsMutant() && chance.in100())
					{
						gameObject.SetLongProperty("Overdosing", 1L);
						flag3 = true;
					}
				}
				try
				{
					Event event3 = Event.New("ApplyTonic");
					event3.SetParameter("Subject", gameObject);
					event3.SetParameter("Target", gameObject);
					event3.SetParameter("Actor", E.Actor);
					event3.SetParameter("Owner", E.Actor);
					event3.SetParameter("Attacker", E.Actor);
					event3.SetParameter("Overdose", value);
					event3.SetParameter("Dosage", @for);
					event3.SetFlag("External", flag);
					event3.SetFlag("Involuntary", flag2);
					if (ParentObject.FireEvent(event3))
					{
						AfterConsumeEvent.Send(E.Actor, gameObject, ParentObject, Eat, Drink: false, !Eat, Inhale: false, Absorb: false, !flag2);
						if (Eat)
						{
							Event e = Event.New("Eating", "Food", ParentObject, "Subject", gameObject);
							gameObject.FireEvent(e);
							if (!flag)
							{
								gameObject.UseEnergy(1000, "Item Eat", null, null);
							}
						}
						if (!flag && !gameObject.IsPlayer() && IComponent<GameObject>.Visible(gameObject))
						{
							IComponent<GameObject>.AddPlayerMessage(gameObject.Does(Eat ? "eat" : "apply", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
						}
						ParentObject.Destroy(null, Silent: true);
					}
				}
				finally
				{
					if (flag3)
					{
						gameObject.SetLongProperty("Overdosing", 0L);
					}
					if (!Message.IsNullOrEmpty())
					{
						Popup.Show(Message);
					}
				}
			}
			else if (E.Command == "ApplyTo")
			{
				if (E.Item.IsBroken())
				{
					return E.Actor.Fail(E.Item.Itis + " broken...");
				}
				if (E.Item.IsRusted())
				{
					return E.Actor.Fail(E.Item.Itis + " rusted...");
				}
				if (E.Actor.OnWorldMap())
				{
					return E.Actor.Fail("You cannot do that on the world map.");
				}
				if (!E.Item.ConfirmUseImportant(E.Actor, "use", "up", 1))
				{
					return false;
				}
				Cell cell = PickDirection(ForAttack: false, POV: E.Actor, Label: "Apply " + E.Item.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, null));
				if (cell == null)
				{
					return false;
				}
				GameObject combatTarget = cell.GetCombatTarget(E.Actor, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: false);
				if (combatTarget == null)
				{
					combatTarget = cell.GetCombatTarget(E.Actor, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, null, null, null, null, null, AllowInanimate: false);
					if (combatTarget != null)
					{
						if (cell.GetCombatTarget(E.Actor, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: false) == null)
						{
							return E.Actor.Fail("You are out of phase with " + combatTarget.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
						}
						return E.Actor.Fail("You cannot reach " + combatTarget.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
					}
					return E.Actor.Fail("There is no one there you can " + (Eat ? "feed" : "apply") + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " to.");
				}
				Event event4 = Event.New("CanApplyTonic");
				event4.SetParameter("Subject", combatTarget);
				event4.SetParameter("Actor", E.Actor);
				event4.SetParameter("Tonic", ParentObject);
				event4.SetFlag("External", State: true);
				event4.SetFlag("ShowMessage", State: true);
				if (!combatTarget.FireEvent(event4))
				{
					return false;
				}
				if (combatTarget == E.Actor)
				{
					if (Eat)
					{
						return E.Actor.Fail("If you want to eat " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " " + E.Actor.itself + ", you can do so through the eat action.");
					}
					return E.Actor.Fail("If you want to apply " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " to " + E.Actor.itself + ", you can do so through the apply action.");
				}
				if (combatTarget.IsHostileTowards(E.Actor) || (!combatTarget.IsLedBy(E.Actor) && GetUtilityScoreEvent.GetFor(combatTarget, ParentObject, null, ForPermission: true) <= 0))
				{
					if (Eat)
					{
						return E.Actor.Fail(combatTarget.Does("do", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " not want to consume " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
					}
					return E.Actor.Fail(combatTarget.Does("do", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " not want " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " applied to " + combatTarget.them + ". You'll need to equip " + ParentObject.them + " as a weapon and attack with " + ParentObject.them + ".");
				}
				ParentObject.SplitFromStack();
				IComponent<GameObject>.WDidXToYWithZ(E.Actor, Eat ? "feed" : "apply", ParentObject, "to", combatTarget, null, null, null, null, combatTarget);
				GameObject parentObject = ParentObject;
				GameObject actor2 = E.Actor;
				GameObject parentObject2 = ParentObject;
				GameObject objectTarget = combatTarget;
				InventoryActionEvent.Check(parentObject, actor2, parentObject2, "ApplyExternally", Auto: false, OwnershipHandled: false, !Eat, Forced: false, Silent: false, 0, 0, 0, objectTarget);
				E.Actor.UseEnergy(1000, "Item ApplyTo", null, null);
				E.RequestInterfaceExit();
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(ExamineCriticalFailureEvent E)
		{
			if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && 50.in100() && !Eat && !ParentObject.IsBroken() && !ParentObject.IsRusted())
			{
				IComponent<GameObject>.XDidY(E.Actor, "accidentally prick", E.Actor.itself + " with " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null), null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, E.Actor.IsPlayer());
				if (InventoryActionEvent.Check(ParentObject, E.Actor, ParentObject, "Apply"))
				{
					return false;
				}
			}
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("ProjectileHit");
			Registrar.Register("ThrownProjectileHit");
			Registrar.Register("WeaponAfterDamage");
			base.Register(Object, Registrar);
		}

		public override bool FireEvent(Event E)
		{
			if ((E.ID == "ProjectileHit" || E.ID == "ThrownProjectileHit" || E.ID == "WeaponAfterDamage") && !Eat)
			{
				GameObject Object = E.GetGameObjectParameter("Defender");
				if (GameObject.Validate(ref Object))
				{
					if (E.GetIntParameter("Penetrations") > 0 && !IsBroken() && !IsRusted())
					{
						if (Object.IsCreature)
						{
							GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
							InventoryActionEvent.Check(ParentObject, gameObjectParameter, ParentObject, "ApplyInvoluntarily", Auto: false, OwnershipHandled: false, OverrideEnergyCost: true, Forced: false, Silent: false, 0, 0, 0, Object);
						}
					}
					else
					{
						if (IComponent<GameObject>.Visible(Object))
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("fail", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " to penetrate " + Object.poss("armor") + " and" + ParentObject.Is + " destroyed.");
						}
						ParentObject.Destroy(null, Silent: true);
					}
				}
			}
			return base.FireEvent(E);
		}
	}
}
