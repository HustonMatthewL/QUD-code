using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts
{
	[Serializable]
	public class LoveTonicApplicator : IPart
	{
		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("ApplyTonic");
			base.Register(Object, Registrar);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "ApplyTonic")
			{
				int intParameter = E.GetIntParameter("Dosage");
				GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Subject");
				if (gameObjectParameter2.IsPlayer())
				{
					if (intParameter <= 0)
					{
						return false;
					}
					LoveTonic e = new LoveTonic(GetTonicDurationEvent.GetFor(ParentObject, gameObjectParameter, gameObjectParameter2, "Love", Stat.Random(500 * intParameter, 700 * intParameter), intParameter));
					if (!gameObjectParameter2.ApplyEffect(e))
					{
						return false;
					}
				}
				else
				{
					GameObject gameObject = E.GetGameObjectParameter("Attacker") ?? gameObjectParameter2.CurrentCell?.FastFloodVisibility("Combat", 20)?.GetRandomElement() ?? The.Player;
					if (gameObject != null && intParameter > 0)
					{
						GameObject by = gameObject;
						if (gameObjectParameter2.CheckInfluence(base.Name, by) && GetChance(gameObject, gameObjectParameter2).in100())
						{
							Lovesick e2 = new Lovesick(Stat.Random(3000 * intParameter, 3600 * intParameter), gameObject);
							if (!gameObjectParameter2.ApplyEffect(e2))
							{
								return false;
							}
							goto IL_016d;
						}
					}
					IComponent<GameObject>.AddPlayerMessage(gameObjectParameter2.Does("look", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you over and" + gameObjectParameter2.GetVerb("metabolize") + " the love tonic with no effect.");
				}
			}
			goto IL_016d;
			IL_016d:
			return base.FireEvent(E);
		}

		public int GetChance(GameObject Attacker, GameObject Target)
		{
			return 95 + Attacker.Stat("Level") - Target.Stat("Level") - Target.GetIntProperty("LoveTonicResistance");
		}
	}
}
