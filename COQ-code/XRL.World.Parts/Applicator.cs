using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class Applicator : IPart
	{
		public override bool SameAs(IPart p)
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != AfterInventoryActionEvent.ID)
			{
				return ID == GetInventoryActionsAlwaysEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
		{
			int @default = 0;
			if (E.Object.Equipped == E.Actor || E.Object.InInventory == E.Actor)
			{
				@default = ((!E.Object.IsImportant()) ? 100 : (-1));
			}
			E.AddAction("Apply", "apply", "Apply", null, 'a', FireOnActor: false, @default);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AfterInventoryActionEvent E)
		{
			if (E.Command == "Apply")
			{
				E.Actor?.UseEnergy(1000, "Item Applicator", null, null);
				E.RequestInterfaceExit();
			}
			return base.HandleEvent(E);
		}
	}
}
