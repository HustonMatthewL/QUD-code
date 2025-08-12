using System;
using System.Collections.Generic;

namespace XRL.World.Parts
{
	[Serializable]
	public class ReflectDamage : IActivePart
	{
		public int ReflectPercentage = 100;

		public ReflectDamage()
		{
			WorksOnSelf = true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == BeforeApplyDamageEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(BeforeApplyDamageEvent E)
		{
			if (GameObject.Validate(E.Actor) && E.Actor != E.Object && E.Damage.Amount > 0 && !E.Damage.HasAttribute("reflected") && IsObjectActivePartSubject(E.Object))
			{
				int num = MyPowerLoadLevel();
				int num2 = ReflectPercentage;
				int num3 = IComponent<GameObject>.PowerLoadBonus(num, 100, 10);
				if (num3 != 0)
				{
					num2 = num2 * (100 + num3) / 100;
				}
				int num4 = (int)((float)E.Damage.Amount * ((float)num2 / 100f));
				if (num2 > 0 && num4 == 0)
				{
					num4 = 1;
				}
				if (num4 > 0)
				{
					int? powerLoadLevel = num;
					if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
					{
						if (ParentObject.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("You reflect " + num4 + " damage back at " + E.Actor.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".", 'G');
						}
						else if (E.Actor.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("reflect", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + num4 + " damage back at you.", 'R');
						}
						else if (Visible())
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("reflect", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + num4 + " damage back at " + E.Actor.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
						}
						List<string> list = new List<string>(E.Damage.Attributes);
						if (!list.Contains("reflected"))
						{
							list.Add("reflected");
						}
						E.Actor.TakeDamage(num4, "from %t damage reflection!", string.Join(" ", list.ToArray()), null, null, null, E.Object);
						E.Object.FireEvent("ReflectedDamage");
					}
				}
			}
			return base.HandleEvent(E);
		}
	}
}
