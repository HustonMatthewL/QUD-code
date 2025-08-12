using System;
using System.Collections.Generic;

namespace XRL.World.Parts
{
	[Serializable]
	public abstract class IAIEnergyCellReload : IPart
	{
		public void CheckEnergyCellReload(IAICommandListEvent E, int ToleranceFactor = 2, int RequirePercent = 110)
		{
			if (ParentObject.Equipped != E.Actor)
			{
				return;
			}
			Inventory inventory = E.Actor.Inventory;
			if (inventory == null || E.Actor.Stat("Intelligence") < 7)
			{
				return;
			}
			EnergyCellSocket part = ParentObject.GetPart<EnergyCellSocket>();
			if (part == null)
			{
				return;
			}
			int @for = QueryDrawEvent.GetFor(ParentObject);
			if (@for <= 0)
			{
				return;
			}
			int num = ParentObject.QueryCharge(LiveOnly: false, 0L);
			if (num >= @for * ToleranceFactor)
			{
				return;
			}
			List<GameObject> list = Event.NewGameObjectList();
			inventory.GetObjects(list, part.CompatibleCell);
			if (list.Count == 0)
			{
				return;
			}
			int num2 = num * RequirePercent / 100;
			if (RequirePercent > 100)
			{
				if (num2 <= num)
				{
					num2 = num + 1;
				}
			}
			else if (RequirePercent < 100 && num2 >= num)
			{
				num2 = num - 1;
			}
			GameObject gameObject = null;
			int num3 = 0;
			foreach (GameObject item in list)
			{
				int num4 = item.QueryCharge(LiveOnly: false, 0L);
				if (num4 >= num2 && (gameObject == null || num4 > num3))
				{
					gameObject = item;
					num3 = num4;
				}
			}
			if (gameObject != null)
			{
				E.Add(EnergyCellSocket.REPLACE_CELL_INTERACTION, 1, ParentObject, Inv: true, Self: false, gameObject);
			}
		}
	}
}
