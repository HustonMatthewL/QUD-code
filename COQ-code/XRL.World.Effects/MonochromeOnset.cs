using System;
using XRL.World.Parts;

namespace XRL.World.Effects
{
	[Serializable]
	public class MonochromeOnset : Effect, ITierInitialized
	{
		public int Stage;

		public int Bonus;

		public int Days;

		public int Count;

		public bool SawSore;

		public bool Mature;

		public MonochromeOnset()
		{
			DisplayName = "blurry vision";
		}

		public override bool SameAs(Effect e)
		{
			return false;
		}

		public override string GetDetails()
		{
			return "Vision is blurred.";
		}

		public override bool Apply(GameObject Object)
		{
			if (Object.HasEffect<Monochrome>())
			{
				return false;
			}
			if (Object.HasEffect<MonochromeOnset>())
			{
				return false;
			}
			if (!Object.FireEvent("ApplyDiseaseOnset"))
			{
				return false;
			}
			if (!ApplyEffectEvent.Check(Object, "DiseaseOnset", this))
			{
				return false;
			}
			if (!Object.FireEvent("ApplyMonochrome"))
			{
				return false;
			}
			if (!ApplyEffectEvent.Check(Object, "Monochrome", this))
			{
				return false;
			}
			Duration = 1;
			return true;
		}

		public override int GetEffectType()
		{
			return 100679700;
		}

		public override string GetDescription()
		{
			return "blurry vision";
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDiseaseOnsetEvent>.ID)
			{
				return ID == ModifyDefendingSaveEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetDiseaseOnsetEvent E)
		{
			if (E.Effect == null)
			{
				E.Effect = this;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(ModifyDefendingSaveEvent E)
		{
			if (Bonus != 0 && E.Vs == "Monochrome Disease Onset")
			{
				E.Roll += Bonus;
				if (E.Actual)
				{
					Bonus = 0;
				}
			}
			return base.HandleEvent(E);
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("DrinkingFrom");
			Registrar.Register("Eating");
			Registrar.Register("EndTurn");
			base.Register(Object, Registrar);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "EndTurn")
			{
				Count++;
				if (Count >= 1200)
				{
					Count = 0;
					Days++;
					if (base.Object.MakeSave("Toughness", 13, null, null, "Monochrome Disease Onset"))
					{
						Stage--;
						if (SawSore && Stage > -2 && base.Object.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("You feel a bit better.");
						}
					}
					else
					{
						Stage++;
						if (Stage < 3)
						{
							if (base.Object.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("Your vision blurs.");
							}
							SawSore = true;
						}
					}
					if (Stage <= -2)
					{
						if (SawSore && base.Object.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("Your vision clears up.");
						}
						Duration = 0;
						return true;
					}
					if (Stage >= 3 || Days >= 5)
					{
						Duration = 0;
						base.Object.ApplyEffect(new Monochrome());
					}
				}
			}
			else if (E.ID == "DrinkingFrom")
			{
				LiquidVolume liquidVolume = E.GetGameObjectParameter("Container").LiquidVolume;
				if (liquidVolume != null && liquidVolume.HasPrimaryOrSecondaryLiquid("honey") && Bonus < 2)
				{
					Bonus = 2;
				}
			}
			else if (E.ID == "Eating")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Food");
				if (gameObjectParameter == null)
				{
					return true;
				}
				if (Bonus > 0)
				{
					return true;
				}
				if (gameObjectParameter.Blueprint.Contains("Yuckwheat") && Bonus < 2)
				{
					Bonus = 2;
				}
			}
			return base.FireEvent(E);
		}
	}
}
