using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class AnimateInZone : IPart
	{
		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == EnteredCellEvent.ID;
			}
			return true;
		}

		private void Animate(GameObject Object)
		{
			AnimateObject.Animate(Object);
			GameObjectFactory.ApplyBuilders(Object, ParentObject.GetBlueprint());
		}

		public override bool HandleEvent(EnteredCellEvent E)
		{
			try
			{
				Cell.SpiralEnumerator enumerator = E.Cell.IterateAdjacent(20, IncludeSelf: false, LocalOnly: true).GetEnumerator();
				while (enumerator.MoveNext())
				{
					foreach (GameObject object2 in enumerator.Current.Objects)
					{
						if (object2.Brain == null && AnimateObject.CanBeAnimated(object2))
						{
							Animate(object2);
							return base.HandleEvent(E);
						}
					}
				}
				GameObject @object = E.Cell.AddObject(PopulationManager.RollOneFrom("DynamicObjectsTable:AnimatableFurniture").Blueprint);
				Animate(@object);
			}
			finally
			{
				ParentObject.Destroy(null, Silent: true);
			}
			return base.HandleEvent(E);
		}
	}
}
