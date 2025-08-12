using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Qud.UI
{
	public class ControlId : MonoBehaviour
	{
		public string id;

		private static bool CanvasEnabled(GameObject go)
		{
			if (go == null)
			{
				return true;
			}
			Canvas canvas = go?.GetComponent<Canvas>();
			if (canvas != null && !canvas.enabled)
			{
				return false;
			}
			return CanvasEnabled(go?.transform?.parent?.gameObject);
		}

		public static GameObject Get(string id, bool includeInactive = false)
		{
			return (from o in Object.FindObjectsOfType<ControlId>(includeInactive)
				where o.id == id && CanvasEnabled(o.gameObject)
				select o).FirstOrDefault()?.gameObject;
		}

		public static IEnumerable<GameObject> GetAll(string id, bool includeInactive = false)
		{
			return from o in Object.FindObjectsOfType<ControlId>(includeInactive)
				where CanvasEnabled(o.gameObject)
				select o into cid
				select cid.gameObject;
		}

		public static void Assign(GameObject go, string id, bool findNestedComponent = false)
		{
			if (go == null)
			{
				MetricsManager.LogWarning("ControlID::Assign called with null go");
				return;
			}
			ControlId controlId = go.GetComponent<ControlId>();
			if (controlId == null && findNestedComponent)
			{
				controlId = go.GetComponentInChildren<ControlId>();
			}
			if (controlId == null)
			{
				controlId = go.AddComponent<ControlId>();
			}
			controlId.id = id;
		}
	}
}
