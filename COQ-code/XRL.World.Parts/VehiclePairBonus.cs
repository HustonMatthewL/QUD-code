using System;
using System.Text;

namespace XRL.World.Parts
{
	[Serializable]
	public class VehiclePairBonus : IPart
	{
		public string Stat;

		public int Paired;

		public int Unpaired;

		public override bool WantTurnTick()
		{
			return true;
		}

		public override void TurnTick(long TimeTick, int Amount)
		{
			if (ParentObject.IsPlayer())
			{
				if (Paired == 0)
				{
					base.StatShifter.RemoveStatShifts();
				}
				else if (Paired != 0)
				{
					base.StatShifter.SetStatShift(Stat, Paired);
				}
			}
			else if (Unpaired == 0)
			{
				base.StatShifter.RemoveStatShifts();
			}
			else if (Unpaired != 0)
			{
				base.StatShifter.SetStatShift(Stat, Unpaired);
			}
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == GetShortDescriptionEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetShortDescriptionEvent E)
		{
			E.Postfix.AppendRules(AppendEffect);
			return base.HandleEvent(E);
		}

		public void AppendEffect(StringBuilder SB)
		{
			if (Paired != 0)
			{
				Statistic.AppendStatAdjustDescription(SB, Stat, Paired);
				SB.Append(" when you are paired with ").Append(ParentObject?.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) ?? "the golem");
				if (Unpaired != 0)
				{
					SB.Append(" and ");
				}
			}
			if (Unpaired != 0)
			{
				Statistic.AppendStatAdjustDescription(SB, Stat, Unpaired);
				SB.Append(" when you are unpaired with ").Append(ParentObject?.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) ?? "the golem");
			}
		}
	}
}
