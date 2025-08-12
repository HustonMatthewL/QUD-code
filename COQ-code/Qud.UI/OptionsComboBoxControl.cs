using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI
{
	[ExecuteInEditMode]
	public class OptionsComboBoxControl : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
	{
		private class Context : NavigationContext
		{
		}

		public NavigationContext editingContext;

		public FrameworkContext frameworkContext;

		public OptionsComboBoxRow data;

		public UITextSkin text;

		public FrameworkScroller optionsScroller;

		private Context context;

		public void SetupContexts(ScrollChildContext scontext)
		{
			if (this.context != null && this.context.IsActive() && this.context.parentContext is ScrollChildContext scrollChildContext && scrollChildContext.index != scontext.index)
			{
				this.context = new Context();
				if (optionsScroller.scrollContext.IsActive())
				{
					optionsScroller.scrollContext.parentContext.Activate();
				}
			}
			else if (NavigationController.instance.activeContext is Context { parentContext: ScrollChildContext parentContext } context && parentContext.index == scontext.index)
			{
				this.context = context;
			}
			if (scontext != null)
			{
				scontext.proxyTo = this.context ?? (this.context = new Context());
				this.context.parentContext = scontext;
				this.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
				{
					InputButtonTypes.AcceptButton,
					XRL.UI.Framework.Event.Helpers.Handle(delegate
					{
						optionsScroller.scrollContext.Activate();
					})
				} };
				optionsScroller.scrollContext.parentContext = this.context;
			}
		}

		public void OnSelectOption(FrameworkDataElement element)
		{
			if (element is MenuOption menuOption)
			{
				Options.SetOption(data.Id, menuOption.Id);
				Options.UpdateFlags();
				data.Value = menuOption.Id;
				Render();
			}
		}

		public NavigationContext GetNavigationContext()
		{
			return context;
		}

		public void setData(FrameworkDataElement data)
		{
			if (data is OptionsComboBoxRow optionsComboBoxRow)
			{
				this.data = optionsComboBoxRow;
				Render();
			}
			else
			{
				this.data = null;
				optionsScroller.BeforeShow(null, new FrameworkDataElement[0]);
			}
		}

		public void Render()
		{
			text.SetText(data.Title);
			int selectedIndex = 0;
			optionsScroller.BeforeShow(null, data.Options.Select(delegate(string option, int index)
			{
				if (option == data.Value)
				{
					selectedIndex = index;
				}
				return new MenuOption
				{
					Description = ((option == data.Value) ? "{{W|" : "{{c|") + option + "}}",
					Id = option
				};
			}));
			optionsScroller.scrollContext.selectedPosition = selectedIndex;
			ScrollContext<FrameworkDataElement, NavigationContext> scrollContext = optionsScroller.scrollContext;
			if (scrollContext.buttonHandlers == null)
			{
				scrollContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
			}
			optionsScroller.scrollContext.buttonHandlers[InputButtonTypes.CancelButton] = delegate
			{
				optionsScroller.scrollContext.selectedPosition = selectedIndex;
			};
			data.ValueChangedSinceLastObserved(this);
		}

		public void Update()
		{
			OptionsComboBoxRow optionsComboBoxRow = data;
			if (optionsComboBoxRow != null && optionsComboBoxRow.ValueChangedSinceLastObserved(this))
			{
				Render();
			}
		}
	}
}
