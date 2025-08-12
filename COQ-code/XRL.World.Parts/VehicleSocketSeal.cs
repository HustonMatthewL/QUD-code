using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class VehicleSocketSeal : IPart
	{
		[NonSerialized]
		private Vehicle _Vehicle;

		public Vehicle Vehicle => _Vehicle ?? (_Vehicle = ParentObject.GetPart<Vehicle>());

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("BeforeReplaceCell");
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "BeforeReplaceCell")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
				if (!Vehicle.IsOwnedBy(gameObjectParameter) && (!Vehicle.PilotID.IsNullOrEmpty() || Vehicle.Autonomous) && !ParentObject.InSamePartyAs(gameObjectParameter))
				{
					if (gameObjectParameter.IsPlayer())
					{
						return gameObjectParameter.ShowFailure(ParentObject.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + "'s socket is sealed.");
					}
					return false;
				}
			}
			return base.FireEvent(E);
		}
	}
}
