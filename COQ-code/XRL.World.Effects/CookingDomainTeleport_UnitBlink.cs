using System;
using XRL.Messages;
using XRL.Rules;

namespace XRL.World.Effects
{
	[Serializable]
	public class CookingDomainTeleport_UnitBlink : ProceduralCookingEffectUnit
	{
		public int Tier = 1;

		public override void Init(GameObject target)
		{
			Tier = Stat.Random(20, 25);
		}

		public override string GetDescription()
		{
			return "Whenever @thisCreature take@s avoidable damage, there's a " + Tier + "% chance @they teleport to a random space on the map instead.";
		}

		public override string GetTemplatedDescription()
		{
			return "Whenever @thisCreature take@s avoidable damage, there's a 20-25% chance @they teleport to a random space on the map instead.";
		}

		public override void Apply(GameObject Object, Effect parent)
		{
			Object.RegisterEffectEvent(parent, "BeforeApplyDamage");
		}

		public override void Remove(GameObject Object, Effect parent)
		{
			Object.UnregisterEffectEvent(parent, "BeforeApplyDamage");
		}

		public override void FireEvent(Event E)
		{
			if (!(E.ID == "BeforeApplyDamage") || parent == null)
			{
				return;
			}
			GameObject @object = parent.Object;
			if (@object == null)
			{
				return;
			}
			Cell currentCell = @object.CurrentCell;
			if (currentCell == null || currentCell.ParentZone == null || currentCell.ParentZone.IsWorldMap())
			{
				return;
			}
			Damage damage = E.GetParameter("Damage") as Damage;
			if (!damage.HasAttribute("Unavoidable") && IComponent<GameObject>.CheckRealityDistortionUsability(@object, null, @object, null, null, null) && Tier.in100())
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
				if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
				{
					MessageQueue.AddPlayerMessage("Fate intervenes and you deal no damage to " + @object.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".", 'r');
				}
				damage.Amount = 0;
				@object.RandomTeleport(Swirl: true);
			}
		}
	}
}
