using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class Banner : IActivePart
	{
		public bool Raised = true;

		public string Effect;

		public string Faction;

		public string Description;

		[NonSerialized]
		private Type _effectType;

		private Type effectType
		{
			get
			{
				if (_effectType == null)
				{
					if (!Effect.Contains("."))
					{
						_effectType = ModManager.ResolveType("XRL.World.Effects." + Effect);
					}
					else
					{
						_effectType = ModManager.ResolveType(Effect);
					}
				}
				return _effectType;
			}
		}

		public Banner()
		{
			ChargeUse = 0;
			IsBootSensitive = false;
			IsEMPSensitive = false;
			MustBeUnderstood = false;
			WorksOnWearer = true;
			WorksOnSelf = true;
		}

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != DroppedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != EquippedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != InventoryActionEvent.ID && ID != TakenEvent.ID)
			{
				return ID == UnequippedEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(EndTurnEvent E)
		{
			if (((ParentObject.CurrentCell != null && Raised) || ParentObject.Equipped != null) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
			{
				GameObject gameObject = ParentObject.equippedOrSelf();
				if (gameObject != null)
				{
					Cell cell = gameObject.GetCurrentCell();
					if (cell != null)
					{
						foreach (GameObject item in cell.ParentZone.GetObjectsWithPart("Combat"))
						{
							if (item.IsMemberOfFaction(Faction) && gameObject.HasLOSTo(item))
							{
								ApplyEffectTo(item);
							}
						}
					}
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetDisplayNameEvent E)
		{
			if ((ParentObject.CurrentCell != null && Raised) || ParentObject.Equipped != null)
			{
				E.AddTag("{{y|[{{g|raised}}]}}", -20);
			}
			else
			{
				E.AddTag("{{y|[{{K|furled}}]}}", -20);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetShortDescriptionEvent E)
		{
			if (Description == null)
			{
				Effect effect = Activator.CreateInstance(effectType) as Effect;
				Description = "Bestows the {{|" + effect.DisplayName + "}} effect to " + XRL.World.Faction.GetFormattedName(Faction) + " who can see this item.";
			}
			E.Postfix.AppendRules(Description);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EquippedEvent E)
		{
			Raised = true;
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(UnequippedEvent E)
		{
			Raised = false;
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(DroppedEvent E)
		{
			Raised = false;
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(TakenEvent E)
		{
			Raised = false;
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetInventoryActionsEvent E)
		{
			if (!Raised && ParentObject.CurrentCell != null)
			{
				E.AddAction("Raise", "raise", "RaiseBanner", null, 'r', FireOnActor: false, 10);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "RaiseBanner")
			{
				E.Actor.PlayWorldOrUISound("Sounds/Interact/sfx_interact_banner_raise", null);
				IComponent<GameObject>.XDidYToZ(E.Actor, "raise", ParentObject, null, null, null, null, E.Actor);
				Raised = true;
			}
			return base.HandleEvent(E);
		}

		public void ApplyEffectTo(GameObject target)
		{
			Type type = effectType;
			if (target.HasEffect(type))
			{
				target.GetEffect(type).Duration = 2;
				return;
			}
			Effect effect = Activator.CreateInstance(type) as Effect;
			effect.Duration = 2;
			target.ApplyEffect(effect);
		}
	}
}
