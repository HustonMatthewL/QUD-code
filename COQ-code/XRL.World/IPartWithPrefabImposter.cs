using ConsoleLib.Console;
using UnityEngine;

namespace XRL.World
{
	public class IPartWithPrefabImposter : IPart
	{
		public string prefabID;

		public bool ImposterActive = true;

		public bool VisibleOnly = true;

		public int X;

		public int Y;

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != LeftCellEvent.ID && ID != OnDestroyObjectEvent.ID && ID != ZoneActivatedEvent.ID)
			{
				return ID == ZoneDeactivatedEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetDebugInternalsEvent E)
		{
			E.AddEntry(this, "VisibleOnly", VisibleOnly);
			E.AddEntry(this, "prefabID", prefabID);
			return base.HandleEvent(E);
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Object.SetIntProperty("HasImposter", 1);
			base.Register(Object, Registrar);
		}

		public override bool FinalRender(RenderEvent E, bool bAlt)
		{
			if (ImposterActive && (!VisibleOnly || ParentObject.IsVisible()))
			{
				E.Imposters.Add(new ImposterExtra.ImposterInfo(prefabID, new Vector2(X, Y)));
			}
			return base.FinalRender(E, bAlt);
		}

		public override bool Render(RenderEvent E)
		{
			return true;
		}
	}
}
