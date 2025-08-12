using System.Collections.Generic;

namespace XRL.World.ZoneBuilders
{
	public class FindASpecificItemDynamicQuestTemplate_FabricateQuestItem : ZoneBuilderSandbox
	{
		public string deliveryItemID;

		public FindASpecificItemDynamicQuestTemplate_FabricateQuestItem()
		{
		}

		public FindASpecificItemDynamicQuestTemplate_FabricateQuestItem(string deliveryItemID)
			: this()
		{
			this.deliveryItemID = deliveryItemID;
		}

		public bool BuildZone(Zone zone)
		{
			GameObject cachedObjects = The.ZoneManager.GetCachedObjects(deliveryItemID);
			cachedObjects.SetIntProperty("norestock", 1);
			List<GameObject> objectsWithTagOrProperty = zone.GetObjectsWithTagOrProperty("LairOwner");
			if (objectsWithTagOrProperty.Count > 0)
			{
				GameObject randomElement = objectsWithTagOrProperty.GetRandomElement();
				randomElement.Inventory.AddObject(cachedObjects.DeepCopy(CopyEffects: false, CopyID: true));
				randomElement.Brain.PerformEquip();
				return true;
			}
			List<GameObject> objectsWithTagOrProperty2 = zone.GetObjectsWithTagOrProperty("NamedVillager");
			if (objectsWithTagOrProperty2.Count > 0)
			{
				GameObject randomElement2 = objectsWithTagOrProperty2.GetRandomElement();
				randomElement2.Inventory.AddObject(cachedObjects.DeepCopy(CopyEffects: false, CopyID: true));
				randomElement2.Brain.PerformEquip();
				return true;
			}
			GameObject gameObject = zone.GetObjectWithTag("RelicContainer") ?? GameObject.Create("RelicChest");
			gameObject.Inventory.AddObject(cachedObjects.DeepCopy(CopyEffects: false, CopyID: true));
			gameObject.SetImportant(flag: true);
			if (gameObject.CurrentCell == null || gameObject.CurrentCell.IsSolid())
			{
				List<Cell> list = zone.GetEmptyCellsWithNoFurniture();
				if (list.Count == 0)
				{
					list = zone.GetEmptyCells();
				}
				if (list.Count == 0)
				{
					list = zone.GetEmptyReachableCells();
				}
				if (list.Count == 0)
				{
					list = zone.GetReachableCells();
				}
				if (list.Count == 0)
				{
					list = zone.GetCells();
				}
				list.GetRandomElement()?.AddObject(gameObject);
			}
			return true;
		}
	}
}
