using System;
using System.Collections.Generic;
using System.Linq;
using Qud.UI;
using UnityEngine;
using XRL.CharacterBuilds.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds
{
	public abstract class AbstractBuilderModuleWindowBase : WindowBase
	{
		public AbstractEmbarkBuilderModule _module;

		public EmbarkBuilderModuleWindowDescriptor descriptor;

		public RectTransform _safeArea;

		public static readonly string RANDOM = "random";

		public static readonly string RESET = "reset";

		public RectTransform safeArea
		{
			get
			{
				if (_safeArea == null)
				{
					_safeArea = (from g in GetComponentsInChildren<RectTransform>()
						where g.gameObject.name == "SafeArea"
						select g).FirstOrDefault();
				}
				return _safeArea;
			}
		}

		public virtual bool isWindowEnabled => _module.enabled;

		public virtual NavigationContext GetNavigationContext()
		{
			return null;
		}

		public virtual bool HasSelection()
		{
			return _module?.getData() != null;
		}

		public virtual bool UseOverlay()
		{
			return true;
		}

		public virtual string DataErrors()
		{
			return _module.DataErrors();
		}

		public virtual string DataWarnings()
		{
			return _module.DataWarnings();
		}

		public virtual void AfterShow(EmbarkBuilderModuleWindowDescriptor descriptor)
		{
		}

		public virtual GameObject InstantiatePrefab(GameObject prefab)
		{
			return prefab;
		}

		public virtual void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
		{
			this.descriptor = descriptor;
			GetNavigationContext()?.Setup();
			GetNavigationContext()?.ActivateAndEnable();
		}

		public T GetModule<T>() where T : AbstractEmbarkBuilderModule
		{
			return _module.builder.GetModule<T>();
		}

		public virtual UIBreadcrumb GetBreadcrumb()
		{
			return null;
		}

		public EmbarkBuilderOverlayWindow GetOverlayWindow()
		{
			return _module?.builder?.GetOverlayWindow();
		}

		public virtual IEnumerable<MenuOption> GetKeyLegend()
		{
			yield return new MenuOption
			{
				InputCommand = "NavigationXYAxis",
				Description = "navigate"
			};
			yield return new MenuOption
			{
				InputCommand = "Accept",
				Description = "select"
			};
		}

		public virtual IEnumerable<MenuOption> GetKeyMenuBar()
		{
			yield return new MenuOption
			{
				Id = RANDOM,
				InputCommand = "CmdChargenRandom",
				Description = "Randomize Selection"
			};
			if (HasSelection())
			{
				yield return new MenuOption
				{
					Id = RESET,
					InputCommand = "CmdChargenReset",
					Description = "Reset Selection"
				};
			}
		}

		public void HandleMenuOption(FrameworkDataElement menuOption)
		{
			if (menuOption is MenuOption menuOption2)
			{
				HandleMenuOption(menuOption2);
			}
		}

		public virtual void DebugQuickstart(string type)
		{
			RandomSelection();
		}

		public virtual void DailySelection()
		{
			RandomSelection();
		}

		public virtual void RandomSelection()
		{
			_module?.RandomSelection();
			_module.builder.RefreshActiveWindow();
		}

		public virtual void ResetSelection()
		{
			_module?.ResetSelection();
			_module.builder.RefreshActiveWindow();
		}

		public virtual void HandleMenuOption(MenuOption menuOption)
		{
			if (menuOption.Id == RANDOM)
			{
				RandomSelection();
			}
			else if (menuOption.Id == RESET)
			{
				ResetSelection();
			}
		}

		public GameObject GetChild(string Name)
		{
			try
			{
				return base.gameObject.transform.Find(Name).gameObject;
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception getting control name " + Name + " : " + ex);
				return null;
			}
		}
	}
}
