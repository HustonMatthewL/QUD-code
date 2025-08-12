using System;
using System.Collections.Generic;
using XRL.Language;

namespace XRL.World.Parts
{
	[Serializable]
	public class InventoryItemConvertor : IPoweredPart
	{
		public static readonly string DERIVATION_CONTEXT = "InventoryItemConvertor";

		public string ConversionTag;

		public string Verb;

		public string Preposition;

		public int Chance = 100;

		public bool AllowRandomMods;

		public float GiganticFactor = 1f;

		public bool UseChargeEveryTurn = true;

		public bool UseChargeOnProcessing;

		public InventoryItemConvertor()
		{
			ChargeUse = 500;
			WorksOnEquipper = true;
			NameForStatus = "ConversionSystems";
		}

		public override bool SameAs(IPart Part)
		{
			InventoryItemConvertor inventoryItemConvertor = Part as InventoryItemConvertor;
			if (inventoryItemConvertor.ConversionTag != ConversionTag)
			{
				return false;
			}
			if (inventoryItemConvertor.Verb != Verb)
			{
				return false;
			}
			if (inventoryItemConvertor.Preposition != Preposition)
			{
				return false;
			}
			if (inventoryItemConvertor.Chance != Chance)
			{
				return false;
			}
			if (inventoryItemConvertor.AllowRandomMods != AllowRandomMods)
			{
				return false;
			}
			if (inventoryItemConvertor.GiganticFactor != GiganticFactor)
			{
				return false;
			}
			if (inventoryItemConvertor.UseChargeEveryTurn != UseChargeEveryTurn)
			{
				return false;
			}
			if (inventoryItemConvertor.UseChargeOnProcessing != UseChargeOnProcessing)
			{
				return false;
			}
			return base.SameAs(Part);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantTurnTick()
		{
			return true;
		}

		public override void TurnTick(long TimeTick, int Amount)
		{
			ProcessItems(Amount);
		}

		private bool ProcessItem(GameObject Object)
		{
			string text = Object?.GetPropertyOrTag(ConversionTag);
			if (text.IsNullOrEmpty())
			{
				return true;
			}
			if (!Chance.in100())
			{
				return true;
			}
			int num = 1;
			if (text.Contains(":"))
			{
				string[] array = text.Split(new string[1] { ":" }, 2, StringSplitOptions.None);
				text = array[0];
				num = array[1].RollCached();
				if (num <= 0)
				{
					return false;
				}
			}
			if (GiganticFactor != 1f && Object.HasPart<ModGigantic>())
			{
				num = (int)((float)num * GiganticFactor);
			}
			bool flag = Object.GetIntProperty("StoredByPlayer") > 0;
			GameObject gameObject = null;
			try
			{
				gameObject = ((!AllowRandomMods) ? GameObject.CreateUnmodified(text) : GameObject.Create(text));
			}
			catch (Exception x)
			{
				MetricsManager.LogException("ItemConvertor", x);
			}
			if (gameObject == null)
			{
				return true;
			}
			List<GameObject> list = null;
			List<GameObject> list2 = null;
			if (num > 1)
			{
				if (gameObject.CanGenerateStacked())
				{
					gameObject.Count = num;
				}
				else
				{
					list = Event.NewGameObjectList();
					list.Add(gameObject);
					list2 = Event.NewGameObjectList();
					for (int i = 1; i < num; i++)
					{
						try
						{
							GameObject item = ((!AllowRandomMods) ? GameObject.CreateUnmodified(text) : GameObject.Create(text));
							list.Add(item);
							list2.Add(item);
						}
						catch (Exception x2)
						{
							MetricsManager.LogException("ItemConvertor", x2);
						}
					}
				}
			}
			if (flag)
			{
				Object.SetIntProperty("FromStoredByPlayer", 1);
			}
			Object.SplitFromStack();
			try
			{
				WasDerivedFromEvent.Send(Object, ParentObject, gameObject, list2, list, DERIVATION_CONTEXT);
			}
			catch (Exception message)
			{
				MetricsManager.LogError(message);
			}
			try
			{
				DerivationCreatedEvent.Send(gameObject, ParentObject, Object, DERIVATION_CONTEXT);
			}
			catch (Exception message2)
			{
				MetricsManager.LogError(message2);
			}
			if (!Verb.IsNullOrEmpty())
			{
				if (list2 != null && list != null)
				{
					DidXToY(Verb, Object, Preposition, Grammar.MakeAndList(list), null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
				}
				else
				{
					DidXToYWithZ(Verb, Object, Preposition, gameObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: true, IndefiniteIndirectObject: true);
				}
			}
			Object.ReplaceWith(gameObject);
			if (list2 != null)
			{
				Object.GetContext(out var ObjectContext, out var CellContext);
				if (ObjectContext != null)
				{
					foreach (GameObject item2 in list2)
					{
						ObjectContext.ReceiveObject(item2);
					}
				}
				else if (CellContext != null)
				{
					foreach (GameObject item3 in list2)
					{
						CellContext.AddObject(item3);
					}
				}
				else
				{
					MetricsManager.LogError("no context to deliver additional objects from " + ParentObject.DebugName);
				}
			}
			Object.CheckStack();
			return false;
		}

		private bool ProcessInventory(GameObject Object)
		{
			bool result = true;
			Inventory inventory = Object.Inventory;
			if (inventory != null)
			{
				List<GameObject> list = Event.NewGameObjectList();
				inventory.GetObjects(list);
				foreach (GameObject item in list)
				{
					if (item.InInventory == Object && !ProcessItem(item))
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		public bool ProcessItems(int Turns = 1)
		{
			bool result = false;
			if (IsReady(UseChargeEveryTurn, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L, null))
			{
				for (int i = 0; i < Turns; i++)
				{
					if (ForeachActivePartSubjectWhile(ProcessInventory, MayMoveAddOrDestroy: true))
					{
						break;
					}
					result = true;
					if (UseChargeOnProcessing)
					{
						ConsumeCharge(null, null);
					}
				}
			}
			return result;
		}
	}
}
