using System;
using XRL.UI;
using XRL.World.ZoneParts;

namespace XRL.World.Effects
{
	[Serializable]
	public class AmbientRealityStabilized : Effect
	{
		public int Strength;

		public int Visibility = 2;

		public bool Projective;

		public GameObject Owner;

		public AmbientRealityStabilized()
		{
			Duration = 1;
			DisplayName = "{{Y|astral friction}}";
		}

		public override string GetStateDescription()
		{
			return "{{Y|subject to astral friction}}";
		}

		public AmbientRealityStabilized(int Strength)
			: this()
		{
			this.Strength = Strength;
		}

		public AmbientRealityStabilized(int Strength, GameObject Owner)
			: this(Strength)
		{
			this.Owner = Owner;
		}

		public override int GetEffectType()
		{
			return 16777280;
		}

		public override string GetDescription()
		{
			return null;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == SingletonEvent<EndTurnEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(EndTurnEvent E)
		{
			AmbientStabilization ambientStabilization = base.Object?.CurrentZone?.GetPart<AmbientStabilization>();
			if (ambientStabilization == null)
			{
				if (Duration > 0)
				{
					GameObject @object = base.Object;
					if (@object != null && @object.IsPlayer())
					{
						Popup.Show("The astral friction diffuses.");
					}
					RealityStabilized obj = base.Object?.GetEffect<RealityStabilized>();
					Duration = 0;
					base.Object?.RemoveEffect(this);
					obj?.Maintain();
					return false;
				}
			}
			else
			{
				Strength = ambientStabilization.Strength;
				ApplyRealityStabilization();
			}
			return base.HandleEvent(E);
		}

		public override bool Apply(GameObject Object)
		{
			if (Object.HasEffect<AmbientRealityStabilized>())
			{
				return false;
			}
			if (!Object.FireEvent("ApplyAmbientRealityStabilized"))
			{
				return false;
			}
			return ApplyRealityStabilization(Initial: true);
		}

		public bool ApplyRealityStabilization(bool Initial = false)
		{
			RealityStabilized effect = base.Object.GetEffect<RealityStabilized>();
			if (effect == null)
			{
				effect = new RealityStabilized(Strength, Owner);
				if (!base.Object.ForceApplyEffect(effect))
				{
					return false;
				}
			}
			else
			{
				effect.Strength = Strength;
				if (Initial)
				{
					effect.SynchronizeEffect();
				}
			}
			return true;
		}
	}
}
