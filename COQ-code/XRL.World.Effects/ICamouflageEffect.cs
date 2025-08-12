using System;
using System.Collections.Generic;
using XRL.Core;

namespace XRL.World.Effects
{
	[Serializable]
	public abstract class ICamouflageEffect : Effect
	{
		[NonSerialized]
		public Dictionary<GameObject, int> Contributions;

		[NonSerialized]
		public Dictionary<GameObject, long> ContributionTicks;

		[NonSerialized]
		public static int MaxContributionsSize = 1;

		public int _Level;

		public int Level
		{
			get
			{
				return _Level;
			}
			set
			{
				if (base.Object != null && base.Object.HasStat("DV"))
				{
					_Level = Math.Max(value, 0);
					base.StatShifter.SetStatShift(base.Object, "DV", _Level);
				}
				else
				{
					_Level = Math.Max(value, 0);
				}
			}
		}

		public ICamouflageEffect()
		{
			Duration = 1;
			DisplayName = "camouflaged";
		}

		public override int GetEffectType()
		{
			return 131072;
		}

		public void SetContribution(GameObject obj, int amount)
		{
			if (amount < 0)
			{
				throw new Exception("amount cannot be negative");
			}
			if (Contributions == null)
			{
				if (amount == 0)
				{
					return;
				}
				Contributions = new Dictionary<GameObject, int>(MaxContributionsSize);
				ContributionTicks = new Dictionary<GameObject, long>(MaxContributionsSize);
			}
			if (Contributions.ContainsKey(obj))
			{
				int num = Contributions[obj];
				if (amount > 0)
				{
					Contributions[obj] = amount;
					ContributionTicks[obj] = XRLCore.CurrentTurn;
					Level += amount - num;
					return;
				}
				Contributions.Remove(obj);
				ContributionTicks.Remove(obj);
				Level -= num;
				if (Level <= 0)
				{
					base.Object.RemoveEffect(this);
				}
			}
			else if (amount > 0)
			{
				Contributions.Add(obj, amount);
				ContributionTicks.Add(obj, XRLCore.CurrentTurn);
				Level += amount;
				if (Contributions.Count > MaxContributionsSize)
				{
					MaxContributionsSize = Contributions.Count;
				}
			}
		}

		public void RemoveContribution(GameObject obj)
		{
			SetContribution(obj, 0);
		}

		public override string GetDescription()
		{
			return DisplayName;
		}

		public override string GetDetails()
		{
			return Level.Signed() + " DV.";
		}

		public override bool Apply(GameObject Object)
		{
			if (Object.HasEffect(base.ClassName))
			{
				return false;
			}
			ApplyChanges();
			return true;
		}

		public override void Remove(GameObject Object)
		{
			UnapplyChanges();
		}

		private void ApplyChanges()
		{
			base.StatShifter.SetStatShift(base.Object, "DV", Level);
		}

		private void UnapplyChanges()
		{
			base.StatShifter.RemoveStatShifts(base.Object);
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("AfterDeepCopyWithoutEffects");
			Registrar.Register("BeforeDeepCopyWithoutEffects");
			Registrar.Register("EndTurn");
			base.Register(Object, Registrar);
		}

		public abstract bool EnablesCamouflage(GameObject GO);

		public override bool FireEvent(Event E)
		{
			if (E.ID == "EndTurn")
			{
				if (Contributions == null || base.Object.CurrentCell == null || !base.Object.CurrentCell.HasObjectOtherThan(EnablesCamouflage, base.Object))
				{
					base.Object.RemoveEffect(this);
				}
				else
				{
					GameObject gameObject = null;
					while (true)
					{
						IL_0064:
						if (gameObject != null)
						{
							RemoveContribution(gameObject);
							gameObject = null;
						}
						long num = XRLCore.CurrentTurn - 1;
						foreach (KeyValuePair<GameObject, long> contributionTick in ContributionTicks)
						{
							if (contributionTick.Value < num)
							{
								gameObject = contributionTick.Key;
								goto IL_0064;
							}
						}
						break;
					}
				}
			}
			else if (E.ID == "BeforeDeepCopyWithoutEffects")
			{
				UnapplyChanges();
			}
			else if (E.ID == "AfterDeepCopyWithoutEffects")
			{
				ApplyChanges();
			}
			return base.FireEvent(E);
		}
	}
}
