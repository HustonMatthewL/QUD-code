using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class RecoilOnDeath : IPoweredPart
	{
		public string DestinationZone = "";

		public int DestinationX = 40;

		public int DestinationY = 13;

		public RecoilOnDeath()
		{
			ChargeUse = 0;
			WorksOnEquipper = true;
			IsEMPSensitive = false;
			NameForStatus = "EmergencyMedEvac";
		}

		public override bool SameAs(IPart p)
		{
			RecoilOnDeath recoilOnDeath = p as RecoilOnDeath;
			if (recoilOnDeath.DestinationZone != DestinationZone)
			{
				return false;
			}
			if (recoilOnDeath.DestinationX != DestinationX)
			{
				return false;
			}
			if (recoilOnDeath.DestinationY != DestinationY)
			{
				return false;
			}
			return base.SameAs(p);
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeDismemberEvent>.ID && ID != EquippedEvent.ID)
			{
				return ID == UnequippedEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(BeforeDismemberEvent E)
		{
			if (E.Part.ObjectEquippedOnThisOrAnyParent(ParentObject) && ParentObject.IsWorn())
			{
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EquippedEvent E)
		{
			if (ParentObject.IsWorn())
			{
				E.Actor.RegisterPartEvent(this, "BeforeDie");
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(UnequippedEvent E)
		{
			E.Actor.UnregisterPartEvent(this, "BeforeDie");
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return false;
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "BeforeDie")
			{
				GameObject equipped = ParentObject.Equipped;
				if (equipped != null)
				{
					if (DestinationZone.IsNullOrEmpty())
					{
						return true;
					}
					if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
					{
						return true;
					}
					Cell cell = equipped.CurrentCell;
					ZoneManager zoneManager = The.ZoneManager;
					Cell cell2 = zoneManager.GetZone(DestinationZone).GetCell(DestinationX, DestinationY);
					equipped.RestorePristineHealth();
					if (DestinationX == -1 || DestinationY == -1)
					{
						try
						{
							List<Cell> emptyReachableCells = zoneManager.ActiveZone.GetEmptyReachableCells();
							cell2 = ((emptyReachableCells.Count <= 0) ? zoneManager.ActiveZone.GetCell(40, 20) : emptyReachableCells.GetRandomElement());
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
							cell2 = zoneManager.ActiveZone.GetCell(40, 20);
						}
					}
					if (equipped.IsPlayer())
					{
						Popup.Show("Just before your demise, you are transported to safety! " + ParentObject.Does("disintegrate", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + ".");
					}
					IComponent<GameObject>.XDidY(equipped, "dematerialize", "out of the local region of spacetime", null, null, null, equipped);
					equipped.DirectMoveTo(cell2);
					if (equipped.IsPlayer())
					{
						zoneManager.SetActiveZone(cell.ParentZone);
					}
					cell.RemoveObject(equipped);
					cell2.AddObject(equipped);
					zoneManager.ProcessGoToPartyLeader();
					equipped.TeleportSwirl(null, "&C", Voluntary: true);
					equipped.UseEnergy(1000, "Item", null, null);
					E.RequestInterfaceExit();
					if (equipped.IsPlayer())
					{
						ParentObject.Destroy();
					}
					return false;
				}
			}
			return base.FireEvent(E);
		}
	}
}
