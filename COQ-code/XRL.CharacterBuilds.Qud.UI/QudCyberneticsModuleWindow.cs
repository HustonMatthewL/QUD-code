using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI
{
	[UIView("CharacterCreation:PickCybernetics", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/PickCybernetics", UICanvasHost = 1)]
	public class QudCyberneticsModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudCyberneticsModule, CategoryMenusScroller>
	{
		public EmbarkBuilderModuleWindowDescriptor windowDescriptor;

		protected const string ID_SEP = "::";

		protected const string EMPTY_CHECK = "[ ]";

		protected const string CHECKED = "[■]";

		private List<CategoryMenuData> cyberneticsMenuState = new List<CategoryMenuData>();

		public void pickCybernetic(int n)
		{
			QudCyberneticsModuleData qudCyberneticsModuleData = new QudCyberneticsModuleData();
			QudCyberneticsModuleDataRow qudCyberneticsModuleDataRow = new QudCyberneticsModuleDataRow();
			qudCyberneticsModuleDataRow.Count = 1;
			qudCyberneticsModuleDataRow.Cybernetic = base.module.cybernetics[n].blueprint?.Name;
			qudCyberneticsModuleDataRow.Variant = base.module.cybernetics[n].slot;
			qudCyberneticsModuleData.selections.Add(qudCyberneticsModuleDataRow);
			base.module.setData(qudCyberneticsModuleData);
			UpdateControls();
		}

		public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
		{
			if (descriptor != null)
			{
				windowDescriptor = descriptor;
			}
			if (base.module.data == null)
			{
				base.module.setData(new QudCyberneticsModuleData());
			}
			base.prefabComponent.onSelected.RemoveAllListeners();
			base.prefabComponent.onSelected.AddListener(SelectCyber);
			UpdateControls();
			base.BeforeShow(descriptor);
		}

		public void SelectCyber(FrameworkDataElement dataElement)
		{
			int n = cyberneticsMenuState[0].menuOptions.FindIndex((PrefixMenuOption d) => d == dataElement);
			pickCybernetic(n);
		}

		public void UpdateControls()
		{
			cyberneticsMenuState = new List<CategoryMenuData>();
			CategoryMenuData categoryMenuData = new CategoryMenuData();
			cyberneticsMenuState.Add(categoryMenuData);
			categoryMenuData.Title = "Cybernetics";
			categoryMenuData.menuOptions = new List<PrefixMenuOption>();
			for (int i = 0; i < base.module.cybernetics.Count; i++)
			{
				PrefixMenuOption item = new PrefixMenuOption
				{
					Prefix = (base.module.IsSelected(base.module.cybernetics[i]) ? "[■]" : "[ ]"),
					Description = base.module.cybernetics[i].GetDescription(),
					LongDescription = base.module.cybernetics[i].GetLongDescription(),
					Renderable = base.module.cybernetics[i].GetRenderable()
				};
				categoryMenuData.menuOptions.Add(item);
			}
			base.prefabComponent.BeforeShow(windowDescriptor, cyberneticsMenuState);
		}

		public override void ResetSelection()
		{
			base.module.setData(new QudCyberneticsModuleData());
			UpdateControls();
		}

		public override void RandomSelection()
		{
			int num = Stat.Roll(0, base.module.cybernetics.Count - 1);
			base.prefabComponent.ContextFor(0, num).ActivateAndEnable();
			pickCybernetic(num);
			UpdateControls();
		}

		public override GameObject InstantiatePrefab(GameObject prefab)
		{
			prefab.GetComponentInChildren<CategoryMenusScroller>().allowVerticalLayout = false;
			return base.InstantiatePrefab(prefab);
		}

		public override UIBreadcrumb GetBreadcrumb()
		{
			Renderable renderable = (base.module?.SelectedChoice())?.GetRenderable();
			return new UIBreadcrumb
			{
				Id = GetType().FullName,
				Title = "Cybernetics",
				IconPath = (renderable?.getTile() ?? "Terrain/sw_cyberterminal.bmp"),
				IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap[renderable?.getColorChars().detail ?? 'W'],
				IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap[renderable?.getColorChars().foreground ?? 'w']
			};
		}
	}
}
