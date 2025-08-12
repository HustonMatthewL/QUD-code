using System;
using System.Collections.Generic;
using Qud.API;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Skills;

namespace XRL.World.Effects
{
	[Serializable]
	public class BrainBrineCurse : Effect, ITierInitialized
	{
		public BrainBrineCurse()
		{
			Duration = 1;
		}

		public override string GetDescription()
		{
			return null;
		}

		public override int GetEffectType()
		{
			return 67117056;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("EndTurn");
			base.Register(Object, Registrar);
		}

		public void GainChoice(string Choice)
		{
			switch (Choice)
			{
			case "skills":
			{
				List<PowerEntry> powerPool = SkillFactory.GetPowerPool(base.Object, null, 0, int.MaxValue, CheckRequirements: false, IncludeInitiatory: true);
				if (powerPool.Count <= 0)
				{
					break;
				}
				powerPool.ShuffleInPlace();
				int j = 0;
				for (int num = base.Object.GetSeededRange("brainbrine", 2, 3); j < num && j < powerPool.Count; j++)
				{
					PowerEntry powerEntry = powerPool[j];
					string @class = powerEntry.Class;
					string name = powerEntry.Name;
					if (powerEntry.Cost == 0 && powerEntry.ParentSkill != null)
					{
						if (base.Object.HasSkill(powerEntry.ParentSkill.Class))
						{
							num++;
							continue;
						}
						@class = powerEntry.ParentSkill.Class;
						name = powerEntry.ParentSkill.Name;
					}
					if (base.Object.IsPlayer())
					{
						Popup.Show("You learn the skill {{C|" + name + "}}!");
					}
					base.Object.AddSkill(@class);
				}
				break;
			}
			case "sp":
				base.Object.GainSP(base.Object.GetSeededRange("brainbrine", 400, 500));
				break;
			case "ego":
				base.Object.GainEgo(base.Object.GetSeededRange("brainbrine", 1, 2));
				break;
			case "int":
				base.Object.GainIntelligence(base.Object.GetSeededRange("brainbrine", 2, 3));
				break;
			case "wis":
				base.Object.GainWillpower(base.Object.GetSeededRange("brainbrine", 2, 3));
				break;
			case "secrets":
			{
				int i = 0;
				for (int seededRange = base.Object.GetSeededRange("brainbrine", 6, 8); i < seededRange; i++)
				{
					JournalAPI.RevealRandomSecret();
				}
				break;
			}
			case "+mutation":
			{
				MutationEntry mutationEntry2 = MutationsAPI.RandomlyMutate(base.Object, (MutationEntry e) => e.IsMental() && !e.IsDefect(), base.Object.GetSeededRandom("brainbrine"));
				if (mutationEntry2 != null)
				{
					Popup.Show("You gained the mutation {{G|" + mutationEntry2.DisplayName + "}}!");
				}
				else
				{
					Popup.Show("Your mind begins to morph but the physiology of your brain restricts it.");
				}
				break;
			}
			case "-mutation":
			{
				MutationEntry mutationEntry = MutationsAPI.RandomlyMutate(base.Object, (MutationEntry e) => e.IsMental() && e.IsDefect(), base.Object.GetSeededRandom("brainbrine"), allowMultipleDefects: true);
				if (mutationEntry != null)
				{
					Popup.Show("You gained the defect {{R|" + mutationEntry.DisplayName + "}}!");
				}
				else
				{
					Popup.Show("Your mind begins to morph but the physiology of your brain restricts it.");
				}
				break;
			}
			}
		}

		public bool IsValidTarget(GameObject Object)
		{
			if (Duration > 0 && Object != null && Object.IsPlayer())
			{
				return !Object.HasEffect<Confused>();
			}
			return false;
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "EndTurn" && IsValidTarget(base.Object))
			{
				Duration = 0;
				Popup.Show("You shake the water from your addled brain, but someone else's thoughts have already taken root.");
				BallBag<string> ballBag = new BallBag<string>(base.Object.GetSeededRandom("brainbrine"));
				if (SkillFactory.GetPowerPool(base.Object, null, 0, int.MaxValue, CheckRequirements: false, IncludeInitiatory: true).Count > 0)
				{
					ballBag.Add("skills", 20);
					ballBag.Add("sp", 20);
				}
				ballBag.Add("int", 10);
				ballBag.Add("wis", 10);
				ballBag.Add("secrets", 30);
				ballBag.Add("+mutation", 30);
				ballBag.Add("-mutation", 15);
				int num = DrinkMagnifier.Magnify(base.Object, 1);
				for (int i = 0; i < num; i++)
				{
					GainChoice(ballBag.PeekOne());
				}
				if (Options.AnySifrah)
				{
					if (50.in100())
					{
						TinkeringSifrah.AwardInsight();
					}
					if (50.in100())
					{
						SocialSifrah.AwardInsight();
					}
					if (50.in100())
					{
						RitualSifrah.AwardInsight();
					}
					if (50.in100())
					{
						PsionicSifrah.AwardInsight();
					}
				}
				base.Object.LoseEgo(base.Object.GetSeededRange("brainbrine", num, num));
			}
			return base.FireEvent(E);
		}
	}
}
