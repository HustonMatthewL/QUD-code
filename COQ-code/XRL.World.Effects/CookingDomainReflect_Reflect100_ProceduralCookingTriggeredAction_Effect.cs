using System;
using System.Collections.Generic;

namespace XRL.World.Effects
{
	[Serializable]
	public class CookingDomainReflect_Reflect100_ProceduralCookingTriggeredAction_Effect : BasicTriggeredCookingEffect
	{
		public int Times = 1;

		public override string GetDetails()
		{
			if (Times > 1)
			{
				return "Reflect 100% damage the next " + Times + " times @they take damage.";
			}
			return "Reflect 100% damage the next time @they take damage.";
		}

		public CookingDomainReflect_Reflect100_ProceduralCookingTriggeredAction_Effect()
		{
			Duration = 50;
			DisplayName = null;
		}

		public override void ApplyEffect(GameObject Object)
		{
			Object.RegisterEffectEvent(this, "BeforeApplyDamage");
			base.ApplyEffect(Object);
		}

		public override void RemoveEffect(GameObject Object)
		{
			Object.UnregisterEffectEvent(this, "BeforeApplyDamage");
			base.RemoveEffect(Object);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "BeforeApplyDamage")
			{
				if (Duration <= 0)
				{
					return true;
				}
				if (Times <= 0)
				{
					return true;
				}
				Damage damage = E.GetParameter("Damage") as Damage;
				GameObject gameObject = E.GetParameter("Owner") as GameObject;
				if (damage.Amount > 0 && !damage.HasAttribute("reflected"))
				{
					int amount = damage.Amount;
					if (amount > 0 && gameObject != null && gameObject != base.Object)
					{
						Event @event = new Event("TakeDamage");
						Damage damage2 = new Damage(amount);
						damage2.Attributes = new List<string>(damage.Attributes);
						if (!damage2.HasAttribute("reflected"))
						{
							damage2.Attributes.Add("reflected");
						}
						@event.AddParameter("Damage", damage2);
						@event.AddParameter("Owner", base.Object);
						@event.AddParameter("Attacker", base.Object);
						@event.AddParameter("Message", "from %t tiny spines!");
						if (base.Object.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("&GYou reflect " + amount + " damage back at " + gameObject.the + gameObject.ShortDisplayName + "&G.");
						}
						else if (gameObject.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("&R" + base.Object.The + base.Object.ShortDisplayName + "&R" + base.Object.GetVerb("reflect") + " " + amount + " damage back at you.");
						}
						else if (base.Object.IsVisible())
						{
							IComponent<GameObject>.AddPlayerMessage(base.Object.The + base.Object.ShortDisplayName + "&y" + base.Object.GetVerb("reflect") + " " + amount + " damage back at " + gameObject.the + gameObject.ShortDisplayName + "&y.");
						}
						gameObject.FireEvent(@event);
						base.Object.FireEvent("ReflectedDamage");
						Times--;
						if (Times <= 0)
						{
							Duration = 0;
						}
					}
				}
			}
			return base.FireEvent(E);
		}
	}
}
