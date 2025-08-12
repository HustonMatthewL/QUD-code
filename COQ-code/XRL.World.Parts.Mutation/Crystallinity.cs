using System;
using System.Collections.Generic;
using System.Linq;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class Crystallinity : BaseDefaultEquipmentMutation
	{
		public string BodyPartType = "Quincunx";

		public bool CreateObject = true;

		public bool RefractAdded;

		public Crystallinity()
		{
			DisplayName = "Crystallinity";
			base.Type = "Physical";
		}

		public override bool CanLevel()
		{
			return false;
		}

		public override bool GeneratesEquipment()
		{
			return false;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == PooledEvent<GetItemElementsEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetItemElementsEvent E)
		{
			if (E.IsRelevantCreature(ParentObject))
			{
				E.Add("jewels", 1);
			}
			return base.HandleEvent(E);
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			base.Register(Object, Registrar);
		}

		public override string GetDescription()
		{
			return "You are a crystalline being.";
		}

		public override string GetLevelText(int Level)
		{
			return string.Concat(string.Concat(string.Concat("" + "+4 AV\n", "-50 Electrical Resistance\n"), "25% chance to refract light-based attacks\n"), "Effects that make non-biological clones of you produce twice as many.");
		}

		public override bool FireEvent(Event E)
		{
			return base.FireEvent(E);
		}

		public override bool ChangeLevel(int NewLevel)
		{
			return base.ChangeLevel(NewLevel);
		}

		public override bool Mutate(GameObject GO, int Level)
		{
			base.StatShifter.SetStatShift(GO, "ElectricResistance", -50, baseValue: true);
			base.StatShifter.SetStatShift(GO, "AV", 4, baseValue: true);
			GO.ModIntProperty("MentalCloneMultiplier", 2);
			if (!GO.HasPart<RefractLight>())
			{
				GO.AddPart<RefractLight>().Chance = 25;
				RefractAdded = true;
			}
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			base.StatShifter.RemoveStatShifts(GO);
			if (RefractAdded)
			{
				GO.RemovePart<RefractLight>();
			}
			GO.ModIntProperty("MentalCloneMultiplier", -2);
			return base.Unmutate(GO);
		}

		public override void OnRegenerateDefaultEquipment(Body body)
		{
			if (CreateObject)
			{
				List<BodyPart> list = (from p in body.GetParts()
					where p.VariantType == BodyPartType
					select p).ToList();
				for (int i = 0; i < list.Count; i++)
				{
					list[i].DefaultBehavior = GameObject.Create("Crystalline Point");
					list[i].DefaultBehavior.SetStringProperty("TemporaryDefaultBehavior", "Crystallinity");
					list[i].DefaultBehavior.GetPart<MeleeWeapon>().BaseDamage = GetPointDamage(base.Level);
				}
			}
		}

		public string GetPointDamage(int Level)
		{
			if (Level <= 3)
			{
				return "1d2";
			}
			if (Level <= 6)
			{
				return "1d3";
			}
			if (Level <= 9)
			{
				return "1d4";
			}
			if (Level <= 12)
			{
				return "1d6";
			}
			if (Level <= 15)
			{
				return "1d8";
			}
			if (Level <= 18)
			{
				return "1d10";
			}
			return "1d12";
		}
	}
}
