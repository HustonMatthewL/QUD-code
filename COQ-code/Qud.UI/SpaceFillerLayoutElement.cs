using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Qud.UI
{
	[ExecuteAlways]
	public class SpaceFillerLayoutElement : MonoBehaviour, ILayoutElement
	{
		private float _preferredWidth;

		public RectTransform leftSideMirrorTarget;

		public float minWidth => -1f;

		public float preferredWidth => _preferredWidth;

		public float flexibleWidth => -1f;

		public float minHeight => -1f;

		public float preferredHeight => -1f;

		public float flexibleHeight => -1f;

		public int layoutPriority => 10;

		private IEnumerable<RectTransform> OtherSiblings()
		{
			int count = base.gameObject.transform.parent.childCount;
			for (int index = 0; index < count; index++)
			{
				GameObject gameObject = base.gameObject.transform.parent.GetChild(index).gameObject;
				if (!(gameObject == leftSideMirrorTarget.gameObject) && !(gameObject == base.gameObject))
				{
					yield return gameObject.transform as RectTransform;
				}
			}
		}

		public void CalculateLayoutInputHorizontal()
		{
			float num = LayoutUtility.GetPreferredWidth(leftSideMirrorTarget);
			RectTransform rectTransform = base.gameObject.transform.parent as RectTransform;
			float num3 = OtherSiblings().Sum(delegate(RectTransform s)
			{
				float num2 = LayoutUtility.GetMinWidth(s);
				return (num2 > 0f) ? num2 : LayoutUtility.GetPreferredWidth(s);
			});
			_preferredWidth = Math.Max(0f, Math.Min(num, rectTransform.rect.width - num3 - num));
		}

		public void CalculateLayoutInputVertical()
		{
		}
	}
}
