using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleLib.Console;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace XRL.CharacterBuilds.Qud.UI
{
	[UIView("CharacterCreation:CustomizeCharacter", false, false, false, null, null, false, 0, false, NavCategory = "Chargen", UICanvas = "Chargen/CustomizeCharacter", UICanvasHost = 1)]
	[HasModSensitiveStaticCache]
	public class QudCustomizeCharacterModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudCustomizeCharacterModule, FrameworkScroller>
	{
		public class PetData : FrameworkDataElement
		{
			public Renderable Icon;
		}

		private PronounSet fromGenderPlaceholder = new PronounSet();

		[ModSensitiveStaticCache(false)]
		private List<PetData> availablePets;

		public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
		{
			if (base.module.data == null)
			{
				base.module.data = new QudCustomizeCharacterModuleData();
			}
			UpdateUI(descriptor);
			base.BeforeShow(descriptor);
		}

		public void UpdateUI(EmbarkBuilderModuleWindowDescriptor descriptor = null)
		{
			base.prefabComponent.BeforeShow(descriptor, GetSelections());
			base.prefabComponent.onSelected.RemoveAllListeners();
			base.prefabComponent.onSelected.AddListener(SelectMenuOption);
		}

		public IEnumerable<PrefixMenuOption> GetSelections()
		{
			yield return new PrefixMenuOption
			{
				Id = "Name",
				Prefix = "Name: ",
				Description = (base.module.data?.name ?? "<random>")
			};
			if (Gender.EnableSelection)
			{
				yield return new PrefixMenuOption
				{
					Id = "Gender",
					Prefix = "Gender: ",
					Description = base.module.genderName
				};
			}
			if (PronounSet.EnableSelection)
			{
				yield return new PrefixMenuOption
				{
					Id = "PronounSet",
					Prefix = "Pronoun Set: ",
					Description = base.module.pronounSetName
				};
			}
			if (GameObjectFactory.Factory.GetBlueprintsWithTag("StartingPet").Count > 0)
			{
				yield return new PrefixMenuOption
				{
					Id = "Pet",
					Prefix = "Pet: ",
					Description = ((from p in GetPets()
						where p.Id == base.module.data?.pet
						select p).FirstOrDefault()?.Description ?? "<none>")
				};
			}
		}

		public override void RandomSelection()
		{
			FrameworkDataElement dataAt = base.prefabComponent.scrollContext.GetDataAt(base.prefabComponent.scrollContext.selectedPosition);
			if (dataAt?.Id == "Name")
			{
				base.module.setName(base.module.builder.info.fireBootEvent(QudGameBootModule.BOOTEVENT_GENERATERANDOMPLAYERNAME, null, The.Core.GenerateRandomPlayerName(base.module.builder.GetModule<QudSubtypeModule>().data.Subtype)));
			}
			if (dataAt?.Id == "Pet")
			{
				base.module.setPet(GetPets().GetRandomElement().Id);
			}
			UpdateUI();
		}

		public async void SelectMenuOption(FrameworkDataElement dataElement)
		{
			if (dataElement.Id == "Pet")
			{
				await OnChoosePet();
			}
			if (dataElement.Id == "Name")
			{
				string text = await Popup.AskStringAsync("Enter name:", base.module.data?.name ?? "", 80, 0, null, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, null);
				base.module.setName(text);
			}
			if (dataElement.Id == "Gender")
			{
				Gender gender = await OnChooseGenderAsync();
				if (gender != null)
				{
					base.module.data.gender = gender;
				}
			}
			if (dataElement.Id == "PronounSet")
			{
				PronounSet pronounSet = await OnChoosePronounSetAsync();
				if (pronounSet == fromGenderPlaceholder)
				{
					base.module.data.pronounSet = null;
				}
				else if (pronounSet != null)
				{
					base.module.data.pronounSet = pronounSet;
				}
			}
			UpdateUI();
		}

		public async Task<Gender> OnChooseGenderAsync()
		{
			List<Gender> availableGenders = Gender.GetAllGenericPersonal();
			List<string> options = availableGenders.Select((Gender gender) => gender.Name).ToList();
			options.Add("<create new>");
			int num = await Popup.PickOptionAsync("Choose Gender", null, "", options.ToArray(), null, null, null, null, 0, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true);
			if (num > -1)
			{
				if (num != options.Count - 1)
				{
					return availableGenders[num];
				}
				int num2 = await Popup.PickOptionAsync("Select Base Gender", null, "", availableGenders.Select((Gender PronounSet) => PronounSet.Name).ToArray(), null, null, null, null, 0, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true);
				if (num2 > -1)
				{
					Gender original = availableGenders[num2];
					Gender newGender = new Gender(original);
					if (await newGender.CustomizeAsync())
					{
						return newGender;
					}
				}
			}
			return null;
		}

		public async Task<PronounSet> OnChoosePronounSetAsync()
		{
			List<PronounSet> availablePronounSets = PronounSet.GetAllGenericPersonal();
			List<string> options = new List<string>();
			options.Add("<from gender>");
			options.AddRange(availablePronounSets.Select((PronounSet pronounSet) => pronounSet.Name));
			options.Add("<create new>");
			int num = await Popup.PickOptionAsync("Choose Pronoun Set", null, "", options.ToArray(), null, null, null, null, 0, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true);
			if (num > -1)
			{
				if (num != options.Count - 1)
				{
					if (num == 0)
					{
						return fromGenderPlaceholder;
					}
					return availablePronounSets[num - 1];
				}
				int num2 = await Popup.PickOptionAsync("Select Base Set", null, "", availablePronounSets.Select((PronounSet PronounSet) => PronounSet.Name).ToArray(), null, null, null, null, 0, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true);
				if (num2 > -1)
				{
					PronounSet original = availablePronounSets[num2];
					PronounSet newPronounSet = new PronounSet(original);
					if (await newPronounSet.CustomizeAsync())
					{
						return newPronounSet;
					}
				}
			}
			return null;
		}

		public IEnumerable<PetData> GetPets()
		{
			foreach (GameObjectBlueprint item in GameObjectFactory.Factory.GetBlueprintsWithTag("StartingPet"))
			{
				yield return new PetData
				{
					Id = item.Name,
					Icon = new Renderable(item),
					Description = item.GetTag("PetName", item.DisplayName())
				};
			}
		}

		public async Task OnChoosePet()
		{
			if (availablePets == null)
			{
				availablePets = GetPets().ToList();
				if (availablePets.Count > 0)
				{
					availablePets.Insert(0, new PetData
					{
						Id = null,
						Description = "<none>",
						Icon = null
					});
				}
			}
			if (availablePets.Count > 0)
			{
				int num = await Popup.PickOptionAsync("Choose Pet", null, "", availablePets.Select((PetData o) => o.Description).ToArray(), null, availablePets.Select((PetData o) => o.Icon).ToArray(), null, null, 0, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true);
				if (base.module.data == null)
				{
					base.module.data = new QudCustomizeCharacterModuleData();
				}
				if (num <= 0)
				{
					base.module.data.pet = null;
				}
				else
				{
					base.module.data.pet = availablePets[num].Id;
				}
				base.module.setData(base.module.data);
			}
		}

		public override UIBreadcrumb GetBreadcrumb()
		{
			return new UIBreadcrumb
			{
				Id = GetType().FullName,
				Title = "Customize",
				IconPath = "Items/sw_book1.bmp",
				IconDetailColor = Color.clear,
				IconForegroundColor = The.Color.Gray
			};
		}
	}
}
