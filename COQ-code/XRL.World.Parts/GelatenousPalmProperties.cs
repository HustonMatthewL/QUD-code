using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts
{
	[Serializable]
	public class GelatenousPalmProperties : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("DefendMeleeHit");
			base.Register(Object, Registrar);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "DefendMeleeHit")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
				GameObject parentObject = ParentObject;
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
				gameObjectParameter = Disarming.Disarm(parentObject, gameObjectParameter2, 100);
				if (gameObjectParameter != null)
				{
					ParentObject.ReceiveObject(gameObjectParameter);
					if (gameObjectParameter2.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage(gameObjectParameter.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " lost in the goop!");
					}
				}
			}
			return base.FireEvent(E);
		}
	}
}
