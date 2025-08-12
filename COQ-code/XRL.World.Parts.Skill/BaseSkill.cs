using System;
using XRL.World.Conversations;
using XRL.World.Conversations.Parts;
using XRL.World.Skills;

namespace XRL.World.Parts.Skill
{
	[Serializable]
	public class BaseSkill : IPart
	{
		public const int ELEMENT_WEIGHT_HIGH = 3;

		public const int ELEMENT_WEIGHT_MEDIUM = 2;

		public const int ELEMENT_WEIGHT_LOW = 1;

		[NonSerialized]
		private IBaseSkillEntry _Entry;

		public string DisplayName
		{
			get
			{
				string text = Entry?.Name;
				if (text != null)
				{
					return text;
				}
				text = base.Name;
				int num = text.LastIndexOf('_');
				if (num != -1)
				{
					text = text.Substring(num + 1);
				}
				return text;
			}
			set
			{
				if (value == DisplayName)
				{
					MetricsManager.LogCallingModError("You do not need to set the display name of the skill " + DisplayName + ", please remove the attempt to set it");
					return;
				}
				MetricsManager.LogCallingModError("You cannot set the display name of the skill " + DisplayName + " to " + value + ", please remove the attempt to set it");
			}
		}

		public IBaseSkillEntry Entry => _Entry ?? (_Entry = SkillFactory.Factory.GetFirstEntry(base.Name));

		public virtual string GetDescription(IBaseSkillEntry Entry)
		{
			return Entry.Description;
		}

		public virtual bool MeetsRequirements(GameObject Object, bool ShowPopup)
		{
			return true;
		}

		public virtual bool IsValidForElement(IConversationElement Element, IConversationPart Part, string Context)
		{
			if (Part is WaterRitualLearnSkill waterRitualLearnSkill)
			{
				waterRitualLearnSkill.Text = GetWaterRitualText(waterRitualLearnSkill.Entry);
				waterRitualLearnSkill.TagName = ShowWaterRitualTagName(waterRitualLearnSkill.Entry);
				waterRitualLearnSkill.Reputation = GetWaterRitualReputationCost(waterRitualLearnSkill.Entry);
				waterRitualLearnSkill.Points = GetWaterRitualSkillPointCost(waterRitualLearnSkill.Entry);
			}
			return true;
		}

		public virtual bool ShowWaterRitualTagName(IBaseSkillEntry Entry)
		{
			return true;
		}

		public virtual string GetWaterRitualText(IBaseSkillEntry Entry)
		{
			return null;
		}

		public virtual int GetWaterRitualReputationCost(IBaseSkillEntry Entry)
		{
			return Entry.Cost;
		}

		public virtual int GetWaterRitualSkillPointCost(IBaseSkillEntry Entry)
		{
			return 0;
		}

		public virtual bool BeforeAddSkill(BeforeAddSkillEvent E)
		{
			if (!E.IsInclusion)
			{
				ShowAddPopup(E);
			}
			return true;
		}

		public virtual void ShowAddPopup(BeforeAddSkillEvent E)
		{
			if (E.IsWaterRitual)
			{
				E.Actor.ShowSuccess(E.Source.Does("teach", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you {{W|" + DisplayName + "}}!");
			}
			else if (E.IsBook)
			{
				E.Actor.ShowSuccess("You learn {{W|" + DisplayName + "}}!");
			}
		}

		public virtual bool AddSkill(GameObject GO)
		{
			return true;
		}

		public virtual bool RemoveSkill(GameObject GO)
		{
			return true;
		}

		public virtual void UseEnergy(int Amount)
		{
			ParentObject.UseEnergy(Amount, "Skill", null, null);
		}

		public virtual void UseEnergy(int Amount, string Type)
		{
			ParentObject.UseEnergy(Amount, Type, null, null);
		}

		public virtual string GetWeaponCriticalDescription()
		{
			return null;
		}

		public virtual int GetWeaponCriticalModifier(GameObject Attacker, GameObject Defender, GameObject Weapon)
		{
			return 0;
		}

		public virtual void WeaponMadeCriticalHit(GameObject Attacker, GameObject Defender, GameObject Weapon, string Properties)
		{
		}
	}
}
