using XRL.Rules;

namespace XRL.World.Effects
{
	public class CookingDomainHP_OnDamaged : ProceduralCookingEffectWithTrigger
	{
		public int Tier;

		public override void Init(GameObject target)
		{
			Tier = ((Tier > 0) ? Tier : Stat.Random(8, 10));
			base.Init(target);
		}

		public override string GetTriggerDescription()
		{
			return "whenever @thisCreature take@s damage, there's a " + Tier + "% chance";
		}

		public override string GetTemplatedTriggerDescription()
		{
			return "whenever @thisCreature take@s damage, there's a 8-10% chance";
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("TookDamage");
			base.Register(Object, Registrar);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "TookDamage" && Tier.in100())
			{
				Trigger();
			}
			return base.FireEvent(E);
		}
	}
}
