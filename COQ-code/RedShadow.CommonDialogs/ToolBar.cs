using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RedShadow.CommonDialogs
{
	public class ToolBar : MonoBehaviour
	{
		public ToolBarButton ToolBarButtonPrefab;

		public GameObject ToolBarSeparatorPrefab;

		private readonly List<ToolBarButton> _buttons = new List<ToolBarButton>();

		[FormerlySerializedAs("onClick")]
		[SerializeField]
		private MenuItemEvent _onClick = new MenuItemEvent();

		public MenuItemEvent onClick
		{
			get
			{
				return _onClick;
			}
			set
			{
				_onClick = value;
			}
		}

		public ToolBarButton addButton(Sprite icon, string text)
		{
			ToolBarButton component = Object.Instantiate(ToolBarButtonPrefab).GetComponent<ToolBarButton>();
			GameObject gameObject = base.transform.Find("Panel").gameObject;
			component.transform.SetParent(gameObject.transform, worldPositionStays: false);
			component.setText(text);
			component.setIcon(icon);
			component.setCallback(delegate
			{
				onItemClick(text);
			});
			_buttons.Add(component);
			return component;
		}

		public GameObject addSeparator()
		{
			GameObject obj = Object.Instantiate(ToolBarSeparatorPrefab);
			GameObject gameObject = base.transform.Find("Panel").gameObject;
			obj.transform.SetParent(gameObject.transform, worldPositionStays: false);
			return obj;
		}

		public ToolBarButton findItem(string text)
		{
			return _buttons.Find((ToolBarButton i) => i.Text == text);
		}

		public void clear()
		{
			_buttons.Clear();
			foreach (Transform item in base.transform.Find("Panel").gameObject.transform)
			{
				Object.Destroy(item.gameObject);
			}
		}

		private void onItemClick(string text)
		{
			_onClick.Invoke(text);
		}
	}
}
