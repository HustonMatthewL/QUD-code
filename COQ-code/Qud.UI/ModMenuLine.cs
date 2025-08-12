using System;
using System.Collections.Generic;
using UnityEngine;
using XRL;
using XRL.UI;

namespace Qud.UI
{
	public class ModMenuLine : ControlledSelectable
	{
		public ModInfo modInfo;

		public UITextSkin titleText;

		public GameObject authorSpacer;

		public UITextSkin authorText;

		public InfoChip version;

		public InfoChip size;

		public InfoChip tags;

		public InfoChip location;

		public ImageTinyFrame imageFrame;

		public RectTransform taggedArea;

		public GameObject taggedPrefab;

		private string _lastAuthor = "\0";

		private string _lastTitle = "\0";

		private string _lastPath = "\0";

		private long _lastSize;

		private XRL.Version _lastVersion;

		private List<GameObject> _tagged = new List<GameObject>();

		private ModState? _lastState;

		private Sprite _sprite;

		private string _imgPath;

		public override void Update()
		{
			if (modInfo != data)
			{
				modInfo = data as ModInfo;
			}
			base.Update();
			if (modInfo == null)
			{
				return;
			}
			if (modInfo.DisplayTitle != _lastTitle || _lastState != modInfo.State)
			{
				_lastTitle = modInfo.DisplayTitle;
				if (modInfo.State == ModState.NeedsApproval)
				{
					titleText.SetText("{{W|" + modInfo.DisplayTitle + "}}");
				}
				else if (modInfo.State == ModState.Failed)
				{
					titleText.SetText("{{R|" + modInfo.DisplayTitle + "}}");
				}
				else if (modInfo.State == ModState.Disabled)
				{
					titleText.SetText("{{K|" + modInfo.DisplayTitle + "}}");
				}
				else if (modInfo.State == ModState.Enabled)
				{
					titleText.SetText("{{Y|" + modInfo.DisplayTitle + "}}");
				}
			}
			if (modInfo.Manifest.Author != _lastAuthor)
			{
				_lastAuthor = modInfo.Manifest.Author;
				if (string.IsNullOrEmpty(modInfo.Manifest.Author))
				{
					authorSpacer.SetActive(value: false);
					authorText.gameObject.SetActive(value: false);
				}
				else
				{
					authorSpacer.SetActive(value: true);
					authorText.gameObject.SetActive(value: true);
					authorText.SetText("{{y|by " + _lastAuthor + "}}");
				}
			}
			if (version != null && modInfo.Manifest.Version != _lastVersion)
			{
				_lastVersion = modInfo.Manifest.Version;
				version.value = modInfo.Manifest.Version.ToString();
				version.gameObject.SetActive(!_lastVersion.IsZero());
			}
			if (tags != null && modInfo.Manifest.Tags != tags.value)
			{
				tags.value = modInfo.Manifest.Tags;
				tags.gameObject.SetActive(!string.IsNullOrEmpty(tags.value));
			}
			if (modInfo.Path != _lastPath)
			{
				_lastPath = modInfo.Path;
				location.value = DataManager.SanitizePathForDisplay(modInfo.Path);
			}
			if (modInfo.Size != _lastSize)
			{
				_lastSize = modInfo.Size;
				double num = _lastSize;
				if (num >= 1048576.0)
				{
					num /= 1048576.0;
					size.value = $"{num:0.00} MB";
				}
				else if (_lastSize >= 1024)
				{
					num /= 1024.0;
					size.value = $"{num:0} KB";
				}
				else
				{
					size.value = _lastSize + " bytes";
				}
			}
			if (_lastState != modInfo.State)
			{
				if (_tagged.Count < 1)
				{
					List<GameObject> list = new List<GameObject>();
					foreach (Transform item in taggedArea)
					{
						list.Add(item.gameObject);
					}
					try
					{
						list.ForEach(delegate(GameObject o)
						{
							o.DestroyImmediate();
						});
					}
					catch (Exception)
					{
					}
					GameObject gameObject = taggedPrefab.Instantiate();
					gameObject.transform.SetParent(taggedArea, worldPositionStays: false);
					_tagged.Add(gameObject);
				}
				_lastState = modInfo.State;
				UITextSkin componentInChildren = _tagged[0].GetComponentInChildren<UITextSkin>();
				switch (modInfo.State)
				{
				case ModState.NeedsApproval:
					componentInChildren.SetText("{{W|NEEDS APPROVAL}}");
					imageFrame.borderColor = The.Color.Yellow;
					break;
				case ModState.Enabled:
					componentInChildren.SetText("{{green|ENABLED}}");
					imageFrame.borderColor = The.Color.DarkGreen;
					break;
				case ModState.Disabled:
					componentInChildren.SetText("{{black|DISABLED}}");
					imageFrame.borderColor = The.Color.Black;
					break;
				case ModState.Failed:
					componentInChildren.SetText("{{red|FAILED}}");
					imageFrame.borderColor = The.Color.DarkRed;
					break;
				}
				if (modInfo.GetSprite() != null && imageFrame.sprite != modInfo.GetSprite())
				{
					imageFrame.sprite = modInfo.GetSprite();
				}
			}
			if (modInfo.IsScripting && _tagged.Count < 2)
			{
				GameObject gameObject2 = taggedPrefab.Instantiate();
				gameObject2.transform.SetParent(taggedArea, worldPositionStays: false);
				_tagged.Add(gameObject2);
				gameObject2.GetComponentInChildren<UITextSkin>()?.SetText("{{w|# SCRIPTING}}");
			}
			if (modInfo.Harmony != null && _tagged.Count < 3)
			{
				GameObject gameObject3 = taggedPrefab.Instantiate();
				gameObject3.transform.SetParent(taggedArea, worldPositionStays: false);
				_tagged.Add(gameObject3);
				gameObject3.GetComponentInChildren<UITextSkin>()?.SetText("{{W|# HARMONY PATCHES}}");
			}
		}
	}
}
