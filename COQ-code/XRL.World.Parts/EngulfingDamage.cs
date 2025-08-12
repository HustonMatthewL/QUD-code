using System;
using XRL.Rules;

namespace XRL.World.Parts
{
	[Serializable]
	public class EngulfingDamage : IPart
	{
		public string Amount = "1-6";

		public string DamageMessage = "from %t digestive enzymes!";

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("EndTurnEngulfing");
			base.Register(Object, Registrar);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "EndTurnEngulfing")
			{
				GameObject parameter = E.GetParameter<GameObject>("Object");
				if (parameter != null)
				{
					Damage value = new Damage(Stat.Roll(Amount));
					Event @event = Event.New("TakeDamage");
					@event.AddParameter("Damage", value);
					@event.AddParameter("Owner", ParentObject);
					@event.AddParameter("Attacker", ParentObject);
					@event.AddParameter("Message", DamageMessage);
					parameter.FireEvent(@event);
				}
			}
			return true;
		}
	}
}
