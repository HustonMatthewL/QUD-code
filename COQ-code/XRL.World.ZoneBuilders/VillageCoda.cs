using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Genkit;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Annals;
using XRL.Collections;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.World.AI;
using XRL.World.Capabilities;
using XRL.World.Conversations.Parts;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Quests;
using XRL.World.Quests.GolemQuest;
using XRL.World.WorldBuilders;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders
{
	public class VillageCoda : VillageCodaBase
	{
		public string dynamicCreatureTableName;

		public bool SurfaceRevealer = true;

		private CodaSystem _System;

		private string[] staticPerVillage = new string[4] { "*Storage,*Furniture", "*LiquidStorage,*Furniture", "*Seating,*Furniture", "*Sleep*,*Furniture" };

		private string[] staticPerBuilding = new string[1] { "*LightSource,*Furniture" };

		private Dictionary<string, string> staticVillageResults = new Dictionary<string, string>();

		private Dictionary<string, PopulationInfo> ChiliadTables = new Dictionary<string, PopulationInfo>();

		public const string SULTAN_ID = "CodaSultan";

		public const string VILLAGE_ID = "CodaVillage";

		public const string END_EVENT_ID = "CodaSultanEndEvent";

		public const string EVENT_END_COVENANT = "In =event.year=, =subject.name= cleansed the =village.region= of the plagues of the Gyre and, through the tutelage of the tinker monks at Grit Gate, taught =mayor.name= to sow =village.plant= along their fertile tracks.";

		public const string EVENT_END_RETURN = "In =event.year=, a triad of plagues afflicted the land. Tongues rotted away in the mouths of kith and kin, their legs annealed to iron, and darkness bloomed from the earth. =subject.name= and their warden physickers walked beneath the chrome arches and healed the sick.";

		public const string EVENT_END_MAROON = "In =event.year=, a triad of plagues afflicted the land. Tongues rotted away in the mouths of kith and kin, their legs annealed to iron, and darkness bloomed from the earth. The warden physickers voiced a prayer to =subject.name= the Above and walked beneath the chrome arches to heal the sick.";

		public const string EVENT_END_ACCEDE = "In =event.year=, =subject.name=, the Above, forsook the people of Qud in favor of its sludges and microorganisms, and then disappeared. =pronouns.Subjective= =verb:were:afterpronoun= 216 years old.";

		public const string EVENT_END_LAUNCH = "At twilight in the shadow of the Spindle, the people of =village.name= saw an image on the horizon that looked like a =subject.personTerm= bathed in starfire. It was =subject.name=, and after =pronouns.subjective= came and left =village.name=, the people built a monument to =pronouns.objective=, and thenceforth called =pronouns.objective= =subject.PersonTerm=-in-Starfire.";

		public const int STATUE_CHUNK_WIDTH = 13;

		public const int STATUE_CHUNK_HEIGHT = 8;

		public const int STATUE_CHUNK_PADDING = 1;

		public const string STATUE_PREFIX = "This shrine depicts a significant event from the life of the ancient =sultan.term= =subject.name=:\n\n";

		public CodaSystem System => _System ?? (_System = The.Game.GetSystem<CodaSystem>());

		public static bool IsRuined => The.Game.HasGameState("CodaRuined");

		public static bool IsPlagued => The.Game.HasGameState("CodaPlagued");

		public static bool IsDespised => The.Game.HasGameState("CodaDespised");

		public static bool HasAmaranthinePrism => The.Game.HasGameState("CodaAmaranthinePrism");

		public static bool HasAmaranthineDust => The.Game.HasGameState("CodaAmaranthineDust");

		public static bool HasTauNoLonger => The.Game.HasGameState("CodaTauNoLonger");

		public static bool HasWanderingTau => The.Game.HasGameState("CodaWanderingTau");

		public static bool HasDeadTau => The.Game.HasGameState("CodaDeadTau");

		public static bool HasStarshiibLocket => The.Game.HasGameState("CodaStarshiibLocket");

		public static bool HasFoundSonnet => The.Game.HasGameState("CodaFoundSonnet");

		public static bool HasReturnedSonnet => The.Game.HasGameState("CodaReturnedSonnet");

		public static bool ChoseNacham => The.Game.HasGameState("ChoseNacham");

		public static bool ChoseVaam => The.Game.HasGameState("ChoseVaam");

		public static bool ChoseDagasha => The.Game.HasGameState("ChoseDagasha");

		public static bool ChoseKah => The.Game.HasGameState("ChoseKah");

		public string mayorTemplate
		{
			get
			{
				if (villageSnapshot == null)
				{
					return null;
				}
				if (villageSnapshot.GetProperty("mayorTemplate") != "unknown")
				{
					return villageSnapshot.GetProperty("mayorTemplate");
				}
				return "Mayor";
			}
		}

		public GameObject generateWarden(GameObject baseObject, bool GivesRep = false)
		{
			GameObject gameObject;
			if (baseObject != null)
			{
				gameObject = baseObject;
			}
			else
			{
				Func<GameObject> func = FuzzyFunctions.DoThisButRarelyDoThat(delegate
				{
					GameObject aNonLegendaryCreature = EncountersAPI.GetANonLegendaryCreature((GameObjectBlueprint ob) => ob.HasTag(dynamicCreatureTableName) && (ob.HasPart("Body") || ob.HasTagOrProperty("BodySubstitute")) && (ob.HasPart("Combat") || ob.HasTagOrProperty("BodySubstitute")) && !ob.HasTag("Merchant") && !ob.HasTag("ExcludeFromVillagePopulations"));
					if (aNonLegendaryCreature == null)
					{
						MetricsManager.LogEditorError("village.cs::getBaseVillager()", "We didn't get a " + dynamicCreatureTableName + " member (3), should we or is the default ok?");
						return (GameObject)null;
					}
					return aNonLegendaryCreature;
				}, () => EncountersAPI.GetANonLegendaryCreature((GameObjectBlueprint ob) => ob.HasPart("Body") && ob.HasPart("Combat") && !ob.HasTag("Merchant") && !ob.HasTag("ExcludeFromVillagePopulations")), "33");
				try
				{
					gameObject = func();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					gameObject = null;
				}
				if (gameObject == null)
				{
					gameObject = getBaseVillager(NoRep: true);
					preprocessVillager(gameObject);
				}
				else
				{
					preprocessVillager(gameObject, foreign: true);
				}
			}
			gameObject.Brain.Mobile = true;
			gameObject.Brain.Factions = "";
			gameObject.Brain.Allegiance.Clear();
			gameObject.Brain.Allegiance.Add("Wardens", 100);
			gameObject.Brain.Allegiance.Hostile = false;
			gameObject.Brain.Allegiance.Calm = true;
			gameObject = HeroMaker.MakeHero(gameObject, "SpecialVillagerHeroTemplate_Warden", -1, "Warden");
			gameObject.RequirePart<Interesting>();
			gameObject.SetIntProperty("VillageWarden", 1);
			gameObject.SetIntProperty("NamedVillager", 1);
			if (isVillageZero)
			{
				gameObject.SetIntProperty("WaterRitualNoSellSkill", 1);
			}
			GivesRep givesRep = gameObject.GetPart<GivesRep>();
			givesRep?.ResetRelatedFactions();
			if (GivesRep)
			{
				gameObject.SetStringProperty("staticFaction1", villageFaction + ",friend,defending their village");
				string propertyOrTag = gameObject.GetPropertyOrTag("NoHateFactions");
				propertyOrTag = ((!propertyOrTag.IsNullOrEmpty()) ? (propertyOrTag + ",Wardens") : "Wardens");
				gameObject.SetStringProperty("NoHateFactions", propertyOrTag);
				if (givesRep == null)
				{
					givesRep = gameObject.AddPart<GivesRep>();
				}
				givesRep.FillInRelatedFactions(Initial: true);
			}
			else if (givesRep != null)
			{
				gameObject.RemovePart(givesRep);
			}
			string text = HistoricStringExpander.ExpandString("<spice.villages.warden.introDialog.!random>");
			gameObject.SetIntProperty("SuppressSimpleConversation", 1);
			ConversationsAPI.addSimpleConversationToObject(gameObject, text, "Live and drink.", null, null, null, ClearLost: true);
			TakeOnRoleEvent.Send(gameObject, "Warden");
			return gameObject;
		}

		public GameObject generateMayor(GameObject baseObject, string specialTemplate = "SpecialVillagerHeroTemplate_Mayor", bool GivesRep = true)
		{
			GameObject gameObject = null;
			if (baseObject != null)
			{
				gameObject = baseObject;
			}
			else
			{
				gameObject = getBaseVillager();
				preprocessVillager(gameObject);
				setVillagerProperties(gameObject);
			}
			if (gameObject.Brain != null)
			{
				gameObject.Brain.SetFactionFeeling(villageFaction, RuleSettings.REPUTATION_LOVED);
				gameObject.Brain.SetFactionFeeling(villagerBaseFaction, RuleSettings.REPUTATION_LOVED);
				gameObject.Brain.SetFactionMembership(villageFaction, 100);
				if (!isVillageZero)
				{
					gameObject.Brain.SetFactionMembership(villagerBaseFaction, 25);
				}
			}
			GivesRep givesRep = gameObject.GetPart<GivesRep>();
			givesRep?.ResetRelatedFactions();
			if (GivesRep)
			{
				string propertyOrTag = gameObject.GetPropertyOrTag("NoHateFactions");
				propertyOrTag = ((!propertyOrTag.IsNullOrEmpty()) ? (propertyOrTag + ",Wardens") : "Wardens");
				gameObject.SetStringProperty("NoHateFactions", propertyOrTag);
				if (givesRep == null)
				{
					givesRep = gameObject.AddPart<GivesRep>();
				}
				givesRep.FillInRelatedFactions(Initial: true);
			}
			else if (givesRep != null)
			{
				gameObject.RemovePart(givesRep);
			}
			gameObject = HeroMaker.MakeHero(gameObject, specialTemplate, -1, mayorTemplate);
			gameObject.RequirePart<Interesting>();
			gameObject.SetStringProperty("Mayor", villageFaction);
			gameObject.SetIntProperty("VillageMayor", 1);
			gameObject.SetIntProperty("NamedVillager", 1);
			gameObject.SetIntProperty("ParticipantVillager", 1);
			gameObject.SetStringProperty("WaterRitual_Skill", signatureSkill ?? RollOneFrom("Village_RandomTaughtSkill"));
			if (signatureDish != null)
			{
				gameObject.AddPart(new TeachesDish(signatureDish, "What a savory smell! Teach me to cook the favorite dish of " + villageName + ".\n"));
			}
			string newValue = ((villageSnapshot.GetList("sacredThings").Count > 0) ? villageSnapshot.GetList("sacredThings").GetRandomElement() : villageSnapshot.GetProperty("defaultSacredThing"));
			string newValue2 = ((villageSnapshot.GetList("profaneThings").Count > 0) ? villageSnapshot.GetList("profaneThings").GetRandomElement() : villageSnapshot.GetProperty("defaultProfaneThing"));
			string message = HistoricStringExpander.ExpandString("<spice.villages.mayor.introDialog.!random>").Replace("*villageName*", villageName).Replace("*sacredThing*", newValue)
				.Replace("*profaneThing*", newValue2);
			gameObject.SetIntProperty("SuppressSimpleConversation", 1);
			AddVillagerConversation(gameObject, message, "Live and drink, =pronouns.formalAddressTerm=.");
			LandingPadsSystem.AddDynamicVillageConversation(gameObject);
			TakeOnRoleEvent.Send(gameObject, "Mayor");
			return gameObject;
		}

		public GameObject generateMerchant(GameObject baseObject)
		{
			GameObject baseVillager;
			if (!isVillageZero && baseObject == null && If.d100(20))
			{
				baseVillager = getBaseVillager(NoRep: true);
				preprocessVillager(baseVillager);
				setVillagerProperties(baseVillager);
				string additionalSpecializationTemplate = (baseVillager.GetBlueprint().DescendsFrom("Dromad") ? "SpecialVillagerHeroTemplate_DromadMerchant" : "SpecialVillagerHeroTemplate_Merchant");
				baseVillager = HeroMaker.MakeHero(baseVillager, additionalSpecializationTemplate, -1, "Merchant");
			}
			else
			{
				if (baseObject != null)
				{
					baseVillager = baseObject;
				}
				else
				{
					baseVillager = GameObjectFactory.Factory.Blueprints["Chiliad Creature Dromads"].createOne();
					preprocessVillager(baseVillager, foreign: true);
				}
				baseVillager.RemovePart<DromadCaravan>();
				baseVillager.RemovePart<ConversationScript>();
				string text = NameMaker.MakeTitle(baseVillager, null, null, null, null, null, null, null, null, null, "Merchant", null, SpecialFaildown: false, null, null);
				baseVillager.GiveProperName(null, Force: false, null, SpecialFaildown: false, null, null);
				if (!text.IsNullOrEmpty())
				{
					baseVillager.RequirePart<Titles>().AddTitle(text, -5);
				}
				if (baseVillager.GetSpecies() != "dromad")
				{
					baseVillager.RequirePart<DisplayNameColor>().SetColorByPriority("Y", 30);
					baseVillager.RequirePart<MerchantIconColor>();
				}
			}
			baseVillager.RequirePart<Interesting>();
			baseVillager.SetIntProperty("SuppressSimpleConversation", 1);
			if (baseVillager.GetBlueprint().DescendsFrom("Dromad"))
			{
				AddVillagerConversation(baseVillager, "Welcome, =player.species=. What do you desire?");
			}
			else
			{
				AddVillagerConversation(baseVillager, "Come. Browse my wares, =player.formalAddressTerm=.", "Live and drink, =pronouns.formalAddressTerm=.");
			}
			if (baseVillager.Brain.Allegiance.IsNullOrEmpty())
			{
				baseVillager.Brain.Factions = villageFaction + "-100";
			}
			else if (!baseVillager.Brain.Allegiance.ContainsKey(villageFaction))
			{
				baseVillager.Brain.Allegiance[villageFaction] = 25;
			}
			GenericInventoryRestocker genericInventoryRestocker = baseVillager.RequirePart<GenericInventoryRestocker>();
			genericInventoryRestocker.Clear();
			for (int i = 0; i <= 2 && villageTier > i; i++)
			{
				genericInventoryRestocker.AddTable("Tier" + (villageTier - i).ToStringCached() + "Wares");
			}
			genericInventoryRestocker.PerformRestock(Silent: true);
			baseVillager.SetIntProperty("VillageMerchant", 1);
			baseVillager.SetIntProperty("NamedVillager", 1);
			if (villageSnapshot.hasProperty("worships_faction") && GameObjectFactory.Factory.GetFactionMembers(villageSnapshot.GetProperty("worships_faction")).Count > 0)
			{
				GameObject @object = GameObjectFactory.Factory.CreateObject(PopulationManager.RollOneFrom("Figurines " + villageTier).Blueprint, 0, 0, null, null, delegate(GameObject o)
				{
					if (o.TryGetPart<RandomFigurine>(out var Part))
					{
						Part.Creature = GameObjectFactory.Factory.GetFactionMembers(villageSnapshot.GetProperty("worships_faction")).GetRandomElement().Name;
					}
				});
				baseVillager.ReceiveObject(@object);
			}
			TakeOnRoleEvent.Send(baseVillager, "Merchant");
			return baseVillager;
		}

		public void PopulateCodaMerchant(PopulationLayout Building)
		{
			GameObject gameObject = zone.FindObject((GameObject x) => x.IsCombatObject() && x.HasIntProperty("VillageMerchant"));
			if (HasAmaranthinePrism)
			{
				GameObject gameObject2 = GameObject.Create("Cyclopean Prism");
				if (gameObject2.TryGetPart<CyclopeanPrism>(out var Part))
				{
					Part.ResetPrism();
				}
				gameObject2.SetIntProperty("NoAIEquip", 1);
				if (GameObject.Validate(gameObject))
				{
					gameObject.ReceiveObject(gameObject2);
				}
				else
				{
					PlaceObjectInBuilding(gameObject2, Building);
				}
			}
			if (HasDeadTau)
			{
				GameObject gameObject3 = GameObject.Create("DeadTauFigurine");
				if (GameObject.Validate(gameObject))
				{
					gameObject.ReceiveObject(gameObject3);
				}
				else
				{
					PlaceObjectInBuilding(gameObject3, Building);
				}
			}
			if (HasWanderingTau)
			{
				GameObject gameObject4 = GameObject.Create("WanderingTauFigurine");
				if (GameObject.Validate(gameObject))
				{
					gameObject.ReceiveObject(gameObject4);
				}
				else
				{
					PlaceObjectInBuilding(gameObject4, Building);
				}
			}
			if (HasTauNoLonger)
			{
				GameObject gameObject5 = GameObject.Create("TauNoLongerFigurine");
				if (GameObject.Validate(gameObject))
				{
					gameObject.ReceiveObject(gameObject5);
				}
				else
				{
					PlaceObjectInBuilding(gameObject5, Building);
				}
			}
		}

		public GameObject generateApothecary(GameObject immigrant = null)
		{
			int num = Math.Min(Math.Max(villageTier, 1), 8);
			string additionalSpecializationTemplate = "SpecialVillagerHeroTemplate_Apothecary";
			GameObject gameObject;
			if (immigrant == null && !If.d100(50))
			{
				gameObject = ((!isVillageZero) ? GameObjectFactory.Factory.Blueprints["HumanApothecary" + num].createOne() : GameObjectFactory.Factory.Blueprints["HumanApothecary_Village0"].createOne());
			}
			else
			{
				if (immigrant != null)
				{
					gameObject = immigrant;
				}
				else
				{
					gameObject = getBaseVillager(NoRep: true);
					preprocessVillager(gameObject);
					setVillagerProperties(gameObject);
				}
				GameObject gameObject2 = ((!isVillageZero) ? GameObjectFactory.Factory.Blueprints["HumanApothecary" + num].createOne() : GameObjectFactory.Factory.Blueprints["HumanApothecary_Village0"].createOne());
				foreach (BaseSkill skill in gameObject2.GetPart<XRL.World.Parts.Skills>().SkillList)
				{
					gameObject.AddSkill(skill.Name);
				}
				GenericInventoryRestocker genericInventoryRestocker = gameObject.RequirePart<GenericInventoryRestocker>();
				genericInventoryRestocker.Table = gameObject2.GetPart<GenericInventoryRestocker>()?.Table ?? "Village Apothecary 1";
				genericInventoryRestocker.Chance = 100;
				gameObject.Statistics["XP"].BaseValue = Math.Max(gameObject.Stat("XP"), gameObject2.Stat("XP"));
				gameObject.Statistics["Hitpoints"].BaseValue = Math.Max(gameObject.Stat("Hitpoints"), gameObject2.Stat("Hitpoints"));
				gameObject.Statistics["Intelligence"].BaseValue = Math.Max(gameObject.Stat("Intelligence"), 15);
				gameObject.Statistics["Intelligence"].BaseValue = Math.Max(gameObject.Stat("Toughness"), 15);
			}
			gameObject = HeroMaker.MakeHero(gameObject, additionalSpecializationTemplate, -1, "Apothecary");
			gameObject.RequirePart<Interesting>();
			gameObject.RemovePart<ConversationScript>();
			gameObject.SetIntProperty("SuppressSimpleConversation", 1);
			if (villageTier <= 3)
			{
				ConversationsAPI.addSimpleConversationToObject(gameObject, "I've the cure for what ails you.~You don't look so good. You need more yuckwheat and honey in your diet.~Cook your meals with yuckwheat if you feel sick. Catch a disease early enough and you can kill it.~\"Ease the pain, addle the brain.\" Be careful when you chew witchwood bark.", "Live and drink.", null, null, null, ClearLost: true);
			}
			else
			{
				ConversationsAPI.addSimpleConversationToObject(gameObject, "I've the cure for what ails you.~You don't look so good. You need more yuckwheat and honey in your diet.~Cook your meals with yuckwheat if you feel sick. Catch a disease early enough and you can kill it.~\"Ease the pain, addle the brain.\" Be careful when you chew witchwood bark.~In the market for a tonic, =player.formalAddressTerm=? Spend water now or blood later, your choice.~Prickly-boons and yuckwheat for trade.~If you came for the humble pie, you had best not have led any mind-hunters here.~Have you got enough tonics?", "Live and drink.", null, null, null, ClearLost: true);
			}
			if (gameObject.Brain.Allegiance.IsNullOrEmpty())
			{
				gameObject.Brain.Factions = villageFaction + "-100";
			}
			else if (!gameObject.Brain.Allegiance.ContainsKey(villageFaction))
			{
				gameObject.Brain.Allegiance[villageFaction] = 50;
			}
			gameObject.SetIntProperty("VillageApothecary", 1);
			gameObject.SetIntProperty("NamedVillager", 1);
			TakeOnRoleEvent.Send(gameObject, "Apothecary");
			return gameObject;
		}

		public void PopulateCodaApothecary(PopulationLayout Building)
		{
			GameObject gameObject = zone.FindObject((GameObject x) => x.IsCombatObject() && x.HasIntProperty("VillageApothecary"));
			if (HasAmaranthineDust)
			{
				GameObject gameObject2 = GameObject.Create("Amaranthine Dust");
				gameObject2.Count = Stat.Random(3, 4);
				if (GameObject.Validate(gameObject))
				{
					gameObject.ReceiveObject(gameObject2);
				}
				else
				{
					PlaceObjectInBuilding(gameObject2, Building);
				}
			}
		}

		public GameObject generateTinker(GameObject immigrant = null)
		{
			string additionalSpecializationTemplate = "SpecialVillagerHeroTemplate_Tinker";
			GameObject gameObject;
			if (immigrant == null && !If.d100(50))
			{
				gameObject = ((!isVillageZero) ? GameObjectFactory.Factory.Blueprints["HumanTinker" + villageTier].createOne() : GameObjectFactory.Factory.Blueprints["HumanTinker_Village0"].createOne());
			}
			else
			{
				if (immigrant != null)
				{
					gameObject = immigrant;
				}
				else
				{
					gameObject = getBaseVillager(NoRep: true);
					preprocessVillager(gameObject);
					setVillagerProperties(gameObject);
				}
				GameObject gameObject2 = ((!isVillageZero) ? GameObjectFactory.Factory.Blueprints["HumanTinker" + villageTier].createOne() : GameObjectFactory.Factory.Blueprints["HumanTinker_Village0"].createOne());
				foreach (BaseSkill skill in gameObject2.GetPart<XRL.World.Parts.Skills>().SkillList)
				{
					gameObject.AddSkill(skill.Name);
				}
				GenericInventoryRestocker genericInventoryRestocker = gameObject.RequirePart<GenericInventoryRestocker>();
				genericInventoryRestocker.Table = gameObject2.GetPart<GenericInventoryRestocker>()?.Table ?? "Village Tinker 1";
				genericInventoryRestocker.Chance = 100;
				gameObject.GetStat("XP").BaseValue = Math.Max(gameObject.GetStatValue("XP"), gameObject2.GetStatValue("XP"));
				gameObject.GetStat("Hitpoints").BaseValue = Math.Max(gameObject.GetStatValue("Hitpoints"), gameObject2.GetStatValue("Hitpoints"));
				gameObject.GetStat("Intelligence").BaseValue = Math.Max(gameObject.GetStatValue("Intelligence"), 16);
			}
			gameObject = HeroMaker.MakeHero(gameObject, additionalSpecializationTemplate, -1, "Tinker");
			gameObject.RequirePart<Interesting>();
			gameObject.RemovePart<ConversationScript>();
			gameObject.SetIntProperty("SuppressSimpleConversation", 1);
			ConversationsAPI.addSimpleConversationToObject(gameObject, "Need a gadget repaired or identified, =player.formalAddressTerm=? Or if you're a tinker =player.reflexive=, perhaps you'd like to peruse my schematics?", "Live and drink, tinker.", null, null, null, ClearLost: true);
			if (gameObject.Brain.Allegiance.IsNullOrEmpty())
			{
				gameObject.Brain.Factions = villageFaction + "-100";
			}
			else if (!gameObject.Brain.Allegiance.ContainsKey(villageFaction))
			{
				gameObject.Brain.Allegiance[villageFaction] = 50;
			}
			gameObject.SetIntProperty("VillageTinker", 1);
			gameObject.SetIntProperty("NamedVillager", 1);
			TakeOnRoleEvent.Send(gameObject, "Tinker");
			return gameObject;
		}

		public void PlaceTinker(GameObject Object, PopulationLayout Building)
		{
			PlaceObjectInBuilding(Object, Building, "AlongInsideWall");
			int i = 0;
			for (int num = Stat.Random(2, 3); i < num; i++)
			{
				PlaceObjectInBuilding(GameObject.Create("Workbench"), Building, "AlongInsideWall");
			}
			int j = 0;
			for (int num2 = Stat.Random(0, 2); j < num2; j++)
			{
				PlaceObjectInBuilding(GameObject.Create("Table"), Building, "AlongInsideWall");
			}
		}

		public void PopulateCodaTinker(PopulationLayout Tinker)
		{
			PlaceObjectInBuilding(GenerateMechanicalGolem(), Tinker, "InsideCorner");
			PlaceObjectInBuilding(GameObject.Create("Electric Generator"), Tinker, "AlongInsideWall");
			if (ChoseNacham)
			{
				PlaceObjectInBuilding(GameObject.Create("Nacham's Loom"), Tinker, "AlongInsideWall");
			}
			if (ChoseVaam)
			{
				GameObject gameObject = GameObject.Create("Va'am's Blower");
				PlaceObjectInBuilding(gameObject, Tinker, "InsideCorner");
				if (gameObject.TryGetPart<Fan>(out var Part))
				{
					string directionFrom = gameObject.CurrentCell.GetDirectionFrom(Tinker.position);
					Part.Direction = directionFrom.GetRandomElement().ToString();
				}
			}
			if (ChoseDagasha)
			{
				PlaceObjectInBuilding(GameObject.Create("Dagasha's Crucifix"), Tinker, "AlongInsideWall");
			}
			if (ChoseKah)
			{
				GameObject gameObject2 = GameObject.Create("Kah's Conveyor");
				PlaceObjectInBuilding(gameObject2, Tinker, "InsideCorner");
				if (gameObject2.CurrentCell.Y > Tinker.position.Y)
				{
					gameObject2.CurrentCell.GetCellFromDirection("N").AddObject("ConveyorPadN");
				}
				else
				{
					gameObject2.CurrentCell.GetCellFromDirection("S").AddObject("ConveyorPadS");
				}
			}
		}

		public GameObject GenerateMechanicalGolem()
		{
			GameObject gameObject = GameObject.Create("Mechanical Golem");
			GameObject gameObject2 = VehicleRecord.ResolveRecordsFor(null, null, "Golem")?.GetRandomElement();
			bool flag = false;
			if (gameObject2 == null)
			{
				gameObject2 = GameObject.CreateSample(GolemBodySelection.GetBodyBySpecies().GetRandomElement().Value);
				flag = true;
			}
			GolemQuestSelection.ProcessDescription(gameObject, gameObject2);
			gameObject.Render.DisplayName = "mechanical " + gameObject2.GetBlueprint().DisplayName();
			gameObject.Render.Tile = gameObject2.Render.Tile;
			gameObject.ForceApplyEffect(new Broken());
			if (flag)
			{
				gameObject2.Pool();
			}
			return gameObject;
		}

		public void PlaceEtaEarthling(PopulationLayout Building)
		{
			GameObject @object = GameObject.Create(HasReturnedSonnet ? "LoveAndFearCompleted" : "LoveAndFearUncompleted");
			int num = int.MaxValue;
			Location2D position = buildings[0].position;
			GameObject gameObject = null;
			Zone.ObjectEnumerator enumerator = zone.IterateObjects().GetEnumerator();
			while (enumerator.MoveNext())
			{
				GameObject current = enumerator.Current;
				if (current.HasPart<Container>() && current.GetBlueprint().DescendsFrom("BaseBookshelf"))
				{
					Location2D location = current.CurrentCell.Location;
					int num2 = position.Distance(location);
					if (num2 < num)
					{
						gameObject = current;
						num = num2;
					}
				}
			}
			if (gameObject == null)
			{
				gameObject = GameObject.Create("Bookshelf");
				PlaceObjectInBuilding(gameObject, Building, "AlongInsideWall");
			}
			gameObject.ReceiveObject(@object);
		}

		public GameObject generateImmigrant(string type, string name, string gender, string role, string whyQ, string whyA)
		{
			GameObject gameObject = ((type != null) ? GameObject.Create(type) : GameObject.Create(PopulationManager.RollOneFrom("DynamicInheritsTable:Creature:Tier" + villageTier).Blueprint));
			preprocessVillager(gameObject, foreign: true);
			gameObject.SetStringProperty("HeroNameColor", "&Y");
			setVillagerProperties(gameObject);
			gameObject = role switch
			{
				"mayor" => generateMayor(gameObject, "SpecialVillagerHeroTemplate_" + mayorTemplate), 
				"warden" => generateWarden(gameObject, isVillageZero), 
				"merchant" => generateMerchant(gameObject), 
				"tinker" => generateTinker(gameObject), 
				"apothecary" => generateApothecary(gameObject), 
				_ => HeroMaker.MakeHero(gameObject), 
			};
			gameObject.RequirePart<Interesting>();
			gameObject.SetIntProperty("NamedVillager", 1);
			gameObject.SetIntProperty("ParticipantVillager", 1);
			gameObject.Render.DisplayName = name;
			if (!gender.IsNullOrEmpty())
			{
				gameObject.SetGender(gender);
			}
			if (role != "mayor" && role != "warden")
			{
				gameObject.RemovePart<GivesRep>();
			}
			if (role == "villager")
			{
				gameObject.SetIntProperty("SuppressSimpleConversation", 1);
			}
			AddVillagerConversation(gameObject, gameObject.GetTag("SimpleConversation", "Moon and Sun. Wisdom and will.~May the earth yield for us this season.~Peace, =player.formalAddressTerm=."), "Live and drink.", whyQ, whyA, role != "villager");
			return gameObject;
		}

		public GameObject generatePet(string species, out string name)
		{
			GameObject gameObject = ((species != null) ? GameObject.Create(species) : GameObject.Create(PopulationManager.RollOneFrom("DynamicInheritsTable:BaseAnimal:Tier" + villageTier).Blueprint));
			string text = NameMaker.MakeTitle(gameObject, null, null, null, null, null, null, null, null, null, null, null, SpecialFaildown: false, null, null);
			name = gameObject.GiveProperName(null, Force: false, null, SpecialFaildown: false, null, null);
			if (!text.IsNullOrEmpty())
			{
				gameObject.RequirePart<Titles>().AddTitle(text, -5);
			}
			setVillagerProperties(gameObject);
			gameObject.RequirePart<SmartuseForceTwiddles>();
			gameObject.RemovePart<Pettable>();
			Pettable pettable = new Pettable();
			gameObject.AddPart(pettable);
			pettable.PettableIfPositiveFeeling = true;
			pettable.UseFactionForFeelingFloor = villageFaction;
			gameObject.SetIntProperty("VillagePet", 1);
			gameObject.RequirePart<Interesting>().Key = "VillagePet";
			ConversationsAPI.addSimpleConversationToObject(gameObject, gameObject.GetTag("SimpleConversation", "*does not react*"), "Live and drink.");
			return gameObject;
		}

		public List<PopulationResult> ResolveBuildingContents(List<PopulationResult> templateResults)
		{
			List<PopulationResult> list = new List<PopulationResult>(templateResults.Count);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			using ScopeDisposedList<string> scopeDisposedList = ScopeDisposedList<string>.GetFromPool();
			foreach (PopulationResult templateResult in templateResults)
			{
				for (int i = 0; i < templateResult.Number; i++)
				{
					if (!templateResult.Blueprint.StartsWith("*"))
					{
						list.Add(new PopulationResult(templateResult.Blueprint));
						continue;
					}
					if (staticVillageResults.ContainsKey(templateResult.Blueprint) && !Stat.Chance(20))
					{
						list.Add(new PopulationResult(staticVillageResults[templateResult.Blueprint]));
						continue;
					}
					if (dictionary.ContainsKey(templateResult.Blueprint) && !Stat.Chance(20))
					{
						list.Add(new PopulationResult(dictionary[templateResult.Blueprint]));
						continue;
					}
					string text = templateResult.Blueprint.Replace("*", "");
					if (!ChiliadTables.TryGetValue(templateResult.Blueprint, out var value))
					{
						scopeDisposedList.Clear();
						scopeDisposedList.Add("SemanticChiliad");
						DelimitedStringEnumerator enumerator2 = text.DelimitedBy(',').GetEnumerator();
						while (enumerator2.MoveNext())
						{
							scopeDisposedList.Add("Semantic" + enumerator2.Current);
						}
						value = (ChiliadTables[templateResult.Blueprint] = GameObjectFactory.Factory.FabricateTagTable(templateResult.Blueprint, scopeDisposedList, -1, villageTechTier, Dynamic: false, Strict: true));
					}
					PopulationResult populationResult = value.GenerateOne();
					if (populationResult == null)
					{
						populationResult = PopulationManager.RollOneFrom("DynamicSemanticTable:" + text + "::" + villageTechTier);
					}
					if (populationResult == null || populationResult.Blueprint.IsNullOrEmpty())
					{
						Debug.LogError("Couldn't resolve object for " + templateResult.Blueprint);
						continue;
					}
					populationResult.Hint = templateResult.Hint;
					list.Add(populationResult);
					if (staticPerBuilding.Contains(templateResult.Blueprint) && !dictionary.ContainsKey(templateResult.Blueprint))
					{
						dictionary.Add(templateResult.Blueprint, populationResult.Blueprint);
					}
					if (staticPerVillage.Contains(templateResult.Blueprint) && !staticVillageResults.ContainsKey(templateResult.Blueprint))
					{
						staticVillageResults.Add(templateResult.Blueprint, populationResult.Blueprint);
					}
				}
			}
			return list;
		}

		public override void addInitialStructures()
		{
			List<ISultanDungeonSegment> list = new List<ISultanDungeonSegment>();
			int num = 7;
			int num2 = 72;
			int num3 = 7;
			int num4 = 17;
			string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_InitialStructureSegmentation"), null, "Full").Blueprint;
			if (blueprint == "None")
			{
				return;
			}
			string[] array = blueprint.Split(';');
			foreach (string text in array)
			{
				switch (text)
				{
				case "FullHMirror":
				{
					SultanRectDungeonSegment sultanRectDungeonSegment3 = new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22));
					sultanRectDungeonSegment3.mutator = "HMirror";
					list.Add(sultanRectDungeonSegment3);
					continue;
				}
				case "FullVMirror":
				{
					SultanRectDungeonSegment sultanRectDungeonSegment2 = new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22));
					sultanRectDungeonSegment2.mutator = "VMirror";
					list.Add(sultanRectDungeonSegment2);
					continue;
				}
				case "FullHVMirror":
				{
					SultanRectDungeonSegment sultanRectDungeonSegment = new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22));
					sultanRectDungeonSegment.mutator = "HVMirror";
					list.Add(sultanRectDungeonSegment);
					continue;
				}
				case "Full":
					list.Add(new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22)));
					continue;
				}
				if (text.StartsWith("BSP:"))
				{
					int nSegments = Convert.ToInt32(text.Split(':')[1]);
					partition(new Rect2D(2, 2, 78, 24), ref nSegments, list);
				}
				else if (text.StartsWith("Ring:"))
				{
					int num5 = Convert.ToInt32(text.Split(':')[1]);
					list.Add(new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22)));
					if (num5 == 2)
					{
						list.Add(new SultanRectDungeonSegment(new Rect2D(20, 8, 60, 16)));
					}
					if (num5 == 3)
					{
						list.Add(new SultanRectDungeonSegment(new Rect2D(15, 8, 65, 16)));
						list.Add(new SultanRectDungeonSegment(new Rect2D(25, 10, 55, 14)));
					}
				}
				else if (text.StartsWith("Blocks"))
				{
					string[] array2 = text.Split(':')[1].Split(',');
					int num6 = array2[0].RollCached();
					for (int j = 0; j < num6; j++)
					{
						int num7 = array2[1].RollCached();
						int num8 = array2[2].RollCached();
						int num9 = Stat.Random(2, 78 - num7);
						int num10 = Stat.Random(2, 23 - num8);
						int num11 = num9 + num7;
						int num12 = num10 + num8;
						if (num < num9)
						{
							num = num9;
						}
						if (num2 > num11)
						{
							num2 = num11;
						}
						if (num3 < num10)
						{
							num3 = num10;
						}
						if (num4 > num12)
						{
							num4 = num12;
						}
						SultanRectDungeonSegment sultanRectDungeonSegment4 = new SultanRectDungeonSegment(new Rect2D(num9, num10, num9 + num7, num10 + num8));
						if (text.Contains("[HMirror]"))
						{
							sultanRectDungeonSegment4.mutator = "HMirror";
						}
						if (text.Contains("[VMirror]"))
						{
							sultanRectDungeonSegment4.mutator = "VMirror";
						}
						if (text.Contains("[HVMirror]"))
						{
							sultanRectDungeonSegment4.mutator = "HVMirror";
						}
						list.Add(sultanRectDungeonSegment4);
					}
				}
				else if (text.StartsWith("Circle"))
				{
					string[] array3 = text.Split(':')[1].Split(',');
					list.Add(new SultanCircleDungeonSegment(Location2D.Get(array3[0].RollCached(), array3[1].RollCached()), array3[2].RollCached()));
				}
				else if (text.StartsWith("Tower"))
				{
					string[] array4 = text.Split(':')[1].Split(',');
					list.Add(new SultanTowerDungeonSegment(Location2D.Get(array4[0].RollCached(), array4[1].RollCached()), array4[2].RollCached(), array4[3].RollCached()));
				}
			}
			ColorOutputMap colorOutputMap = new ColorOutputMap(80, 25);
			for (int k = 0; k < list.Count; k++)
			{
				string text2 = "";
				text2 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_StructureTemplate")).Blueprint;
				int n = 3;
				string text3 = "";
				string text4 = "";
				text4 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_StructureTemplate")).Blueprint;
				int n2 = 3;
				if (text2.Contains(","))
				{
					string[] array5 = text2.Split(',');
					text2 = array5[0];
					text3 = array5[1];
				}
				WaveCollapseFastModel waveCollapseFastModel = new WaveCollapseFastModel(text2, n, list[k].width(), list[k].height(), periodicInput: true, periodicOutput: false, 8, 0);
				waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
				if (!text3.IsNullOrEmpty())
				{
					waveCollapseFastModel.ClearColors(text3);
				}
				waveCollapseFastModel.UpdateSample(text4.Split(',')[0], n2, periodicInput: true, periodicOutput: false, 8, 0);
				waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
				ColorOutputMap colorOutputMap2 = new ColorOutputMap(waveCollapseFastModel);
				colorOutputMap2.ReplaceBorders(new Color32(byte.MaxValue, 0, 0, byte.MaxValue), new Color32(0, 0, 0, byte.MaxValue));
				if (list[k].mutator == "HMirror")
				{
					colorOutputMap2.HMirror();
				}
				if (list[k].mutator == "VMirror")
				{
					colorOutputMap2.VMirror();
				}
				if (list[k].mutator == "HVMirror")
				{
					colorOutputMap2.HMirror();
					colorOutputMap2.VMirror();
				}
				colorOutputMap.Paste(colorOutputMap2, list[k].x1, list[k].y1);
				waveCollapseFastModel = null;
				MemoryHelper.GCCollect();
			}
			string text5 = RollOneFrom("Village_InitialStructureSegmentationFullscreenMutation");
			int num13 = 0;
			int num14 = 0;
			for (int l = 0; l < list.Count; l++)
			{
				string text6 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Village_StructureWall")).Blueprint;
				if (text6 == "*auto")
				{
					text6 = GetDefaultWall(zone);
				}
				for (int m = list[l].y1; m < list[l].y2; m++)
				{
					for (int num15 = list[l].x1; num15 < list[l].x2; num15++)
					{
						if (!list[l].contains(num15, m))
						{
							continue;
						}
						int num16 = l + 1;
						while (true)
						{
							if (num16 < list.Count)
							{
								if (list[num16].contains(num15, m))
								{
									break;
								}
								num16++;
								continue;
							}
							Color32 a = colorOutputMap.getPixel(num15, m);
							if (list[l].HasCustomColor(num15, m))
							{
								a = list[l].GetCustomColor(num15, m);
							}
							if (WaveCollapseTools.equals(a, ColorOutputMap.BLACK))
							{
								zone.GetCell(num15 + num13, m + num14).ClearWalls();
								zone.GetCell(num15 + num13, m + num14).AddObject(text6);
								if (text5 == "VMirror" || text5 == "HVMirror")
								{
									zone.GetCell(num15 + num13, zone.Height - (m + num14) - 1).ClearWalls();
									zone.GetCell(num15 + num13, zone.Height - (m + num14) - 1).AddObject(text6);
								}
								if (text5 == "HMirror" || text5 == "HVMirror")
								{
									zone.GetCell(zone.Width - (num15 + num13) - 1, m + num14).ClearWalls();
									zone.GetCell(zone.Width - (num15 + num13) - 1, m + num14).AddObject(text6);
								}
								if (text5 == "HVMirror")
								{
									zone.GetCell(zone.Width - (num15 + num13) - 1, zone.Height - (m + num14) - 1).ClearWalls();
									zone.GetCell(zone.Width - (num15 + num13) - 1, zone.Height - (m + num14) - 1).AddObject(text6);
								}
							}
							break;
						}
					}
				}
			}
		}

		public static void villageClear(Zone Z)
		{
			string tag = Z.GetTerrainObject().GetTag("VillageClearBehavior");
			if (tag.IsNullOrEmpty())
			{
				return;
			}
			string[] array = tag.Split(':');
			if (!(array[0] == "circles"))
			{
				return;
			}
			int num = int.Parse(array[1]);
			for (int i = 0; i < num; i++)
			{
				foreach (Cell item in Z.GetRandomCell().GetCellsInACosmeticCircle(Stat.Random(6, 10)))
				{
					Debug.Log("clearing " + item.X + "," + item.Y);
					item.Clear(null, Important: false, Combat: false, (GameObject o) => o.GetBlueprint().DescendsFrom("Widget"));
				}
			}
		}

		public bool BuildZone(Zone Z)
		{
			bool bDraw = false;
			zone = Z;
			zone.SetZoneProperty("relaxedbiomes", "true");
			zone.SetZoneProperty("faction", villageFaction);
			villageSnapshot = base.villageEntity.GetCurrentSnapshot();
			region = villageSnapshot.GetProperty("region");
			villagerBaseFaction = villageSnapshot.GetProperty("baseFaction");
			villageName = villageSnapshot.GetProperty("name");
			dynamicCreatureTableName = "DynamicObjectsTable:" + region + "_Creatures";
			Z.SetZoneProperty("villageEntityId", base.villageEntity.id);
			isVillageZero = villageSnapshot.GetProperty("isVillageZero", "false").EqualsNoCase("true");
			Tier.Constrain(ref villageTier);
			Tier.Constrain(ref villageTechTier);
			generateVillageTheme();
			generateSignatureItems();
			generateSignatureDish();
			generateSignatureLiquid();
			generateSignatureSkill();
			generateStoryType();
			getVillageDoorStyle();
			makeSureThereIsEnoughSpace();
			foreach (Cell cell3 in Z.GetCells())
			{
				for (int num = cell3.Objects.Count - 1; num >= 0; num--)
				{
					GameObject gameObject = cell3.Objects[num];
					if (!gameObject.IsPlayer() && !gameObject.HasTagOrProperty("NoVillageStrip"))
					{
						if (gameObject.HasTagOrProperty("RequireVillagePlacement"))
						{
							gameObject.Physics.CurrentCell = null;
							requiredPlacementObjects.Add(gameObject);
						}
						else if (gameObject.HasPart<Combat>() || gameObject.HasTagOrProperty("BodySubstitute"))
						{
							gameObject.Physics.CurrentCell = null;
							originalCreatures.Add(gameObject);
						}
						else if (gameObject.IsWall() && gameObject.HasTag("Category_Settlement"))
						{
							gameObject.Physics.CurrentCell = null;
							originalWalls.Add(gameObject);
						}
						else if (gameObject.GetBlueprint().InheritsFrom("Plant") || gameObject.GetBlueprint().InheritsFrom("BasePlant") || gameObject.GetBlueprint().HasTag("PlantLike"))
						{
							gameObject.Physics.CurrentCell = null;
							if (gameObject != null)
							{
								originalPlants.Add(gameObject);
							}
						}
						else if (gameObject.HasPart<LiquidVolume>())
						{
							gameObject.Physics.CurrentCell = null;
							if (gameObject.IsOpenLiquidVolume())
							{
								originalLiquids.Add(gameObject);
							}
						}
						else if (gameObject.GetBlueprint().InheritsFrom("Furniture"))
						{
							gameObject.Physics.CurrentCell = null;
							originalFurniture.Add(gameObject);
						}
						else if (gameObject.GetBlueprint().InheritsFrom("Item"))
						{
							gameObject.Physics.CurrentCell = null;
							originalItems.Add(gameObject);
						}
					}
				}
			}
			villageClear(Z);
			addInitialStructures();
			FabricatePlayerStatue();
			Rect2D plot = Rect2D.zero;
			if (EndGame.IsUltimate)
			{
				plot = ReserveUltraPlot(IsRuined);
			}
			if (IsPlagued)
			{
				FabricatePlagueyard();
				for (int num2 = Stat.Random(3, 5); num2 >= 0; num2--)
				{
					FabricateVantabudCluster();
				}
			}
			InfluenceMap regionMap = new InfluenceMap(Z.Width, Z.Height);
			for (int i = 0; i < Z.Width; i++)
			{
				for (int j = 0; j < Z.Height; j++)
				{
					regionMap.Walls[i, j] = (Z.GetCell(i, j).HasObjectWithTagOrProperty("Wall") ? 1 : 0);
				}
			}
			AddAvoidedToMap(regionMap);
			try
			{
				regionMap.SeedAllUnseeded();
			}
			catch (Exception)
			{
				makeSureThereIsEnoughSpace();
				regionMap.SeedAllUnseeded();
			}
			while (regionMap.LargestSize() > 150)
			{
				regionMap.AddSeedAtMaximaInLargestSeed();
			}
			regionMap.SeedGrowthProbability = new List<int>();
			for (int k = 0; k < regionMap.Seeds.Count; k++)
			{
				regionMap.SeedGrowthProbability.Add(Stat.Random(10, 1000));
			}
			regionMap.Recalculate(bDraw);
			int num3 = Stat.Random(4, 9);
			int num4 = 0;
			int num5 = regionMap.FindClosestSeedTo(Location2D.Get(40, 13), (InfluenceMapRegion r) => r.maxRect.ReduceBy(1, 1).Width >= 6 && r.maxRect.ReduceBy(1, 1).Height >= 6 && r.AdjacentRegions.Count > 1);
			Location2D location2D = regionMap.Seeds[num5];
			townSquare = regionMap.Regions[num5];
			townSquareLayout = null;
			foreach (InfluenceMapRegion region in regionMap.Regions)
			{
				Rect2D Rect = GridTools.MaxRectByArea(region.GetGrid()).Translate(region.BoundingBox.UpperLeft).ReduceBy(1, 1);
				PopulationLayout populationLayout = new PopulationLayout(Z, region, Rect);
				if (region.AdjacentRegions.Count <= 1 && region.Size >= 9 && !region.IsEdgeRegion() && region != townSquare)
				{
					buildings.Add(populationLayout);
				}
				else if ((Rect.Width >= 5 && Rect.Height >= 5 && num3 > 0) || region == townSquare)
				{
					string liquidBlueprint = originalLiquids?.GetRandomElement()?.Blueprint ?? getZoneDefaultLiquid(zone);
					if (region == townSquare)
					{
						townSquareLayout = populationLayout;
						if (fabricateStoryBuilding())
						{
							buildings.Add(populationLayout);
						}
						continue;
					}
					string text = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_BuildingStyle")).Blueprint;
					if (text.StartsWith("wfc,") && !getWfcBuildingTemplate(text.Split(',')[1]).Any((ColorOutputMap t) => t.extrawidth <= Rect.Width && t.extraheight <= Rect.Height))
					{
						text = "squarehut";
					}
					buildings.Add(populationLayout);
					if (text == "burrow")
					{
						FabricateBurrow(populationLayout);
						populationLayout.hasStructure = true;
					}
					if (text == "aerie")
					{
						FabricateAerie(populationLayout);
					}
					if (text == "pond")
					{
						FabricatePond(populationLayout, liquidBlueprint);
					}
					if (text == "islandpond")
					{
						FabricateIslandPond(populationLayout, liquidBlueprint);
						populationLayout.hasStructure = true;
					}
					if (text == "walledpond")
					{
						FabricateWalledPond(populationLayout, liquidBlueprint);
						populationLayout.hasStructure = true;
					}
					if (text == "walledislandpond")
					{
						FabricateWalledIslandPond(populationLayout, liquidBlueprint);
						populationLayout.hasStructure = true;
					}
					if (text == "tent")
					{
						FabricateTent(populationLayout);
						populationLayout.hasStructure = true;
					}
					if (text == "roundhut")
					{
						FabricateHut(populationLayout, isRound: true);
						populationLayout.hasStructure = true;
					}
					if (text == "squarehut")
					{
						FabricateHut(populationLayout, isRound: false);
						populationLayout.hasStructure = true;
					}
					if (text.StartsWith("wfc,"))
					{
						getWfcBuildingTemplate(text.Split(',')[1]).ShuffleInPlace();
						bool flag = false;
						foreach (ColorOutputMap item in getWfcBuildingTemplate(text.Split(',')[1]))
						{
							int num6 = item.width / 2;
							int num7 = item.height / 2;
							if (item.extrawidth > populationLayout.innerRect.Width || item.extraheight > populationLayout.innerRect.Height)
							{
								continue;
							}
							for (int m = 0; m < item.width; m++)
							{
								for (int n = 0; n < item.height; n++)
								{
									Cell cell = Z.GetCell(populationLayout.position.X - num6 + m, populationLayout.position.Y - num7 + n);
									if (cell != null)
									{
										if (ColorExtensionMethods.Equals(item.getPixel(m, n), ColorOutputMap.BLACK))
										{
											cell.AddObject(getAVillageWall());
										}
										else
										{
											ColorExtensionMethods.Equals(item.getPixel(m, n), ColorOutputMap.RED);
										}
									}
								}
							}
							populationLayout.hasStructure = true;
							flag = true;
							break;
						}
						if (!flag)
						{
							FabricateHut(populationLayout, isRound: false);
							populationLayout.hasStructure = true;
						}
					}
					num3--;
					num4++;
				}
				else if (region.AdjacentRegions.Count == 1 && !region.IsEdgeRegion() && townSquare != region)
				{
					VillageCodaBase.MakeCaveBuilding(Z, regionMap, region);
					buildings.Add(populationLayout);
					populationLayout.hasStructure = true;
				}
			}
			placeStatues();
			regionMap.SeedAllUnseeded(bDraw);
			CarvePathwaysFromLocations(Z, bCarveDoors: true, regionMap, location2D);
			zone.ClearReachableMap(bValue: false);
			zone.BuildReachableMap(location2D.X, location2D.Y);
			SnakeToConnections(Location2D.Get(location2D.X, location2D.Y));
			clearDegenerateDoors();
			applyDoorFilters();
			for (int num8 = 0; num8 < Z.Width; num8++)
			{
				for (int num9 = 0; num9 < Z.Height; num9++)
				{
					regionMap.Walls[num8, num9] = (Z.GetCell(num8, num9).HasObjectWithTag("Wall") ? 1 : 0);
				}
			}
			AddAvoidedToMap(regionMap);
			List<Location2D> list = new List<Location2D>();
			foreach (PopulationLayout building5 in buildings)
			{
				Location2D position = building5.position;
				if (position != null)
				{
					list.Add(position);
				}
			}
			regionMap.Recalculate(bDraw);
			InfluenceMap influenceMap = regionMap.copy();
			using (Pathfinder pathfinder = zone.getPathfinder())
			{
				NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 4, 80, 80, 6, 3, -3, 1, new List<NoiseMapNode>());
				for (int num10 = 0; num10 < zone.Width; num10++)
				{
					for (int num11 = 0; num11 < zone.Height; num11++)
					{
						if (zone.GetCell(num10, num11).HasWall() || IsPointAvoided(num10, num11))
						{
							pathfinder.CurrentNavigationMap[num10, num11] = 4999;
						}
						else
						{
							pathfinder.CurrentNavigationMap[num10, num11] = noiseMap.Noise[num10, num11];
						}
					}
				}
				foreach (PopulationLayout building6 in buildings)
				{
					foreach (Location2D cell4 in building6.originalRegion.Cells)
					{
						int x = cell4.X;
						int y = cell4.Y;
						if (x != 0 && x != 79 && y != 0 && y != 24 && Z.GetCell(x, y).IsEmpty())
						{
							int num12 = 0;
							int num13 = 0;
							if (Z.GetCell(x - 1, y).HasWall() || Z.GetCell(x - 1, y).HasObjectWithTag("Door"))
							{
								num13++;
							}
							if (Z.GetCell(x + 1, y).HasWall() || Z.GetCell(x + 1, y).HasObjectWithTag("Door"))
							{
								num13++;
							}
							if (Z.GetCell(x, y - 1).HasWall() || Z.GetCell(x, y - 1).HasObjectWithTag("Door"))
							{
								num12++;
							}
							if (Z.GetCell(x, y + 1).HasWall() || Z.GetCell(x, y + 1).HasObjectWithTag("Door"))
							{
								num12++;
							}
							if ((num12 == 2 && num13 == 0) || (num12 == 0 && num13 == 2))
							{
								influenceMap.Walls[x, y] = 1;
							}
						}
					}
				}
				for (int num14 = 0; num14 < 80; num14++)
				{
					for (int num15 = 0; num15 < 25; num15++)
					{
						if (burrowedDoors.Contains(Location2D.Get(num14, num15)))
						{
							influenceMap.Walls[num14, num15] = 1;
						}
					}
				}
				influenceMap.Recalculate(bDraw);
				string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_BuildingFloor")).Blueprint;
				string text2 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_BuildingPath")).Blueprint;
				if (text2 == "Pond")
				{
					text2 = getZoneDefaultLiquid(zone);
				}
				foreach (GameObject item2 in Z.FindObjects("PathConnection"))
				{
					if (!pathfinder.FindPath(item2.CurrentCell.Location, location2D, Display: false, CardinalDirectionsOnly: true))
					{
						continue;
					}
					foreach (PathfinderNode step in pathfinder.Steps)
					{
						if (!text2.IsNullOrEmpty())
						{
							zone.GetCell(step.pos).RequireObject(text2);
						}
						if (!buildingPaths.Contains(step.pos))
						{
							buildingPaths.Add(step.pos);
						}
					}
				}
				foreach (PopulationLayout building7 in buildings)
				{
					if (pathfinder.FindPath(building7.position, location2D, Display: false, CardinalDirectionsOnly: true))
					{
						foreach (PathfinderNode step2 in pathfinder.Steps)
						{
							if (!text2.IsNullOrEmpty())
							{
								zone.GetCell(step2.pos).AddObject(text2);
							}
							if (!buildingPaths.Contains(step2.pos))
							{
								buildingPaths.Add(step2.pos);
							}
						}
					}
					foreach (Location2D cell5 in building7.originalRegion.Cells)
					{
						if (Z.GetCell(cell5).HasWall() || buildingPaths.Contains(cell5))
						{
							continue;
						}
						if (influenceMap.Regions.Count() <= building7.region.Seed)
						{
							MetricsManager.LogEditorError("village insideOutMap", "insideOutMap didn't have seed");
							building7.outside.Add(cell5);
							int num16 = Z.GetCell(cell5).CountObjectWithTagCardinalAdjacent("Wall");
							if (num16 > 0)
							{
								building7.outsideWall.Add(cell5);
							}
							if (num16 >= 2)
							{
								building7.outsideCorner.Add(cell5);
							}
							continue;
						}
						if (!influenceMap.Regions[building7.region.Seed].Cells.Contains(cell5))
						{
							building7.outside.Add(cell5);
							int num17 = Z.GetCell(cell5).CountObjectWithTagCardinalAdjacent("Wall");
							if (num17 > 0)
							{
								building7.outsideWall.Add(cell5);
							}
							if (num17 >= 2)
							{
								building7.outsideCorner.Add(cell5);
							}
							continue;
						}
						building7.inside.Add(cell5);
						if (!blueprint.IsNullOrEmpty())
						{
							Z.GetCell(cell5).AddObject(blueprint);
						}
						int num18 = Z.GetCell(cell5).CountObjectWithTagCardinalAdjacent("Wall");
						if (num18 > 0)
						{
							building7.insideWall.Add(cell5);
						}
						if (num18 >= 2)
						{
							building7.insideCorner.Add(cell5);
						}
					}
				}
			}
			Dictionary<InfluenceMapRegion, Rect2D> dictionary = new Dictionary<InfluenceMapRegion, Rect2D>();
			Dictionary<InfluenceMapRegion, string> dictionary2 = new Dictionary<InfluenceMapRegion, string>();
			InfluenceMap influenceMap2 = new InfluenceMap(Z.Width, Z.Height);
			influenceMap2.Seeds = new List<Location2D>(regionMap.Seeds);
			Z.SetInfluenceMapWalls(influenceMap2.Walls);
			AddAvoidedToMap(influenceMap2);
			influenceMap2.Recalculate(bDraw);
			int num19 = 0;
			for (int num20 = 0; num20 < influenceMap2.Regions.Count; num20++)
			{
				InfluenceMapRegion R = influenceMap2.Regions[num20];
				Rect2D value;
				if (!dictionary.ContainsKey(R))
				{
					value = GridTools.MaxRectByArea(R.GetGrid()).Translate(R.BoundingBox.UpperLeft);
					dictionary.Add(R, value);
				}
				else
				{
					value = dictionary[R];
				}
				if (num20 == num5)
				{
					continue;
				}
				if (list.Contains(regionMap.Seeds[R.Seed]))
				{
					dictionary2.Add(R, "building");
					PopulationLayout building = buildings.First((PopulationLayout b) => b.position == regionMap.Seeds[R.Seed]);
					string text3 = RollOneFrom("Villages_BuildingTheme_" + villageTheme);
					foreach (PopulationResult item3 in ResolveBuildingContents(PopulationManager.Generate(ResolvePopulationTableName("Villages_BuildingContents_Dwelling_" + text3))))
					{
						PlaceObjectInBuilding(item3, building);
					}
				}
				else if (value.Area >= 4)
				{
					dictionary2.Add(R, "greenspace");
					if (num19 == 0 && signatureHistoricObjectInstance != null)
					{
						string wallObject = "IronFence";
						string blueprint2 = "Iron Gate";
						Z.GetCell(value.Center).AddObject(signatureHistoricObjectInstance);
						ZoneBuilderSandbox.encloseRectWithWall(zone, new Rect2D(value.Center.x - 1, value.Center.y - 1, value.Center.x + 1, value.Center.y + 1), wallObject);
						Z.GetCell(value.Center).GetCellFromDirection(Directions.GetRandomCardinalDirection()).Clear()
							.AddObject(blueprint2);
					}
					else
					{
						string blueprint3 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_GreenspaceContents")).Blueprint;
						int num21 = 20;
						if (blueprint3 == "aquaculture")
						{
							string blueprint4 = originalLiquids?.GetRandomElement()?.Blueprint ?? getZoneDefaultLiquid(zone);
							GameObject aFarmablePlant = getAFarmablePlant();
							Maze maze = RecursiveBacktrackerMaze.Generate(Math.Max(1, R.BoundingBox.Width / 3 + 1), Math.Max(1, R.BoundingBox.Height / 3 + 1), bShow: false, ZoneBuilderSandbox.GetOracleIntFromString("aquaculture" + num19, 0, 2147483646));
							for (int num22 = R.BoundingBox.x1; num22 <= R.BoundingBox.x2; num22++)
							{
								for (int num23 = R.BoundingBox.y1; num23 <= R.BoundingBox.y2; num23++)
								{
									int num24 = (num22 - R.BoundingBox.x1) / 3;
									int num25 = (num23 - R.BoundingBox.y1) / 3;
									int num26 = (num22 - R.BoundingBox.x1) % 3;
									int num27 = (num23 - R.BoundingBox.y1) % 3;
									bool flag2 = false;
									if (num26 == 1 && num27 == 1)
									{
										flag2 = maze.Cell[num24, num25].AnyOpen();
									}
									if (num26 == 1 && num27 == 0)
									{
										flag2 = maze.Cell[num24, num25].N;
									}
									if (num26 == 1 && num27 == 2)
									{
										flag2 = maze.Cell[num24, num25].S;
									}
									if (num26 == 2 && num27 == 1)
									{
										flag2 = maze.Cell[num24, num25].E;
									}
									if (num26 == 0 && num27 == 1)
									{
										flag2 = maze.Cell[num24, num25].W;
									}
									if (flag2)
									{
										if (R.Cells.Contains(Location2D.Get(num22, num23)) && !buildingPaths.Contains(Location2D.Get(num22, num23)))
										{
											Z.GetCell(num22, num23)?.AddObject(aFarmablePlant.Blueprint, base.setVillageDomesticatedProperties);
										}
									}
									else if (R.Cells.Contains(Location2D.Get(num22, num23)) && Z.GetCell(num22, num23) != null)
									{
										Z.GetCell(num22, num23).AddObject(blueprint4);
									}
								}
							}
						}
						else if (blueprint3 == "farm" && value.Area >= num21 && value.Width >= 7 && value.Height <= 7)
						{
							value = value.ReduceBy(1, 1).Clamp(1, 1, 78, 23);
							if (value.Width <= 6 || value.Height <= 6)
							{
								continue;
							}
							Location2D location = value.GetRandomDoorCell().location;
							ZoneBuilderSandbox.PlaceObjectOnRect(Z, "BrinestalkFence", value);
							GetCell(Z, location).Clear();
							GetCell(Z, location).AddObject("Brinestalk Gate");
							string cellSide = value.GetCellSide(location.Point);
							Rect2D r2 = value.ReduceBy(0, 0);
							int num28 = 0;
							if (cellSide == "N")
							{
								num28 = ((Stat.Random(0, 1) == 0) ? 2 : 3);
							}
							if (cellSide == "S")
							{
								num28 = ((Stat.Random(0, 1) != 0) ? 1 : 0);
							}
							if (cellSide == "E")
							{
								num28 = ((Stat.Random(0, 1) != 0) ? 3 : 0);
							}
							if (cellSide == "W")
							{
								num28 = ((Stat.Random(0, 1) == 0) ? 1 : 2);
							}
							if (num28 == 0 || num28 == 1)
							{
								r2.y2 = r2.y1 + 3;
							}
							else
							{
								r2.y1 = r2.y2 - 3;
							}
							if (num28 == 0 || num28 == 3)
							{
								r2.x2 = r2.x1 + 3;
							}
							else
							{
								r2.x1 = r2.x2 - 3;
							}
							ClearRect(Z, r2);
							ZoneBuilderSandbox.PlaceObjectOnRect(Z, getAVillageWall(), r2);
							Location2D location2 = r2.GetRandomDoorCell(cellSide, 1).location;
							Z.GetCell(location2).Clear();
							Z.GetCell(location2).AddObject(getAVillageDoor());
							burrowedDoors.Add(Location2D.Get(location2.X, location2.Y));
							ZoneBuilderSandbox.PlacePopulationInRect(Z, value.ReduceBy(1, 1), ResolvePopulationTableName("Villages_FarmAnimals"), base.setVillageDomesticatedProperties);
							ZoneBuilderSandbox.PlacePopulationInRect(Z, r2.ReduceBy(1, 1), ResolvePopulationTableName("Villages_FarmHutContents"));
						}
						else if (blueprint3 == "garden" || blueprint3 == "farm")
						{
							int num29 = Stat.Random(1, 4);
							GameObject aFarmablePlant2 = getAFarmablePlant();
							string blueprint5 = originalLiquids?.GetRandomElement()?.Blueprint ?? getZoneDefaultLiquid(zone);
							if (num29 == 1)
							{
								bool flag3 = Stat.Random(1, 100) <= 33;
								for (int num30 = R.BoundingBox.x1; num30 <= R.BoundingBox.x2; num30++)
								{
									for (int num31 = R.BoundingBox.y1; num31 <= R.BoundingBox.y2; num31++)
									{
										if (num30 % 2 == 0)
										{
											if (R.Cells.Contains(Location2D.Get(num30, num31)) && !buildingPaths.Contains(Location2D.Get(num30, num31)))
											{
												Z.GetCell(num30, num31)?.AddObject(aFarmablePlant2.Blueprint, base.setVillageDomesticatedProperties);
											}
										}
										else if (flag3 && R.Cells.Contains(Location2D.Get(num30, num31)) && Z.GetCell(num30, num31) != null)
										{
											Z.GetCell(num30, num31).AddObject(blueprint5);
										}
									}
								}
							}
							if (num29 == 2)
							{
								string blueprint6 = originalLiquids?.GetRandomElement()?.Blueprint ?? getZoneDefaultLiquid(zone);
								bool flag4 = Stat.Random(1, 100) <= 33;
								for (int num32 = R.BoundingBox.x1; num32 <= R.BoundingBox.x2; num32++)
								{
									for (int num33 = R.BoundingBox.y1; num33 <= R.BoundingBox.y2; num33++)
									{
										if (num33 % 2 == 0)
										{
											if (R.Cells.Contains(Location2D.Get(num32, num33)) && !buildingPaths.Contains(Location2D.Get(num32, num33)))
											{
												Z.GetCell(num32, num33)?.AddObject(aFarmablePlant2.Blueprint, base.setVillageDomesticatedProperties);
											}
										}
										else if (flag4 && R.Cells.Contains(Location2D.Get(num32, num33)) && Z.GetCell(num32, num33) != null)
										{
											Z.GetCell(num32, num33).AddObject(blueprint6);
										}
									}
								}
							}
							if (num29 == 3)
							{
								int num34 = Stat.Random(20, 98);
								for (int num35 = R.BoundingBox.x1; num35 <= R.BoundingBox.x2; num35++)
								{
									for (int num36 = R.BoundingBox.y1; num36 <= R.BoundingBox.y2; num36++)
									{
										if (R.Cells.Contains(Location2D.Get(num35, num36)) && !buildingPaths.Contains(Location2D.Get(num35, num36)) && Stat.Random(1, 100) <= num34)
										{
											Z.GetCell(num35, num36)?.AddObject(aFarmablePlant2.Blueprint, base.setVillageDomesticatedProperties);
										}
									}
								}
							}
							if (num29 == 4)
							{
								int num37 = Stat.Random(20, 98);
								for (int num38 = R.BoundingBox.x1; num38 <= R.BoundingBox.x2; num38++)
								{
									for (int num39 = R.BoundingBox.y1; num39 <= R.BoundingBox.y2; num39++)
									{
										if (R.Cells.Contains(Location2D.Get(num38, num39)) && !buildingPaths.Contains(Location2D.Get(num38, num39)) && Stat.Random(1, 100) <= num37)
										{
											Z.GetCell(num38, num39)?.AddObject(getAFarmablePlant());
										}
									}
								}
							}
						}
					}
					num19++;
				}
				else if (influenceMap2.SeedToRegionMap[R.Seed].AdjacentRegions.Count == 1)
				{
					dictionary2.Add(R, "cubby");
				}
				else
				{
					dictionary2.Add(R, "hall");
				}
			}
			placeNonTakeableSignatureItems();
			buildings.RemoveAll((PopulationLayout b) => b.inside.Count == 0 && b.outside.Count == 0);
			GameObject gameObject2 = generateVillageOven();
			PlaceObjectInBuilding(gameObject2, buildings.GetRandomElement(), If.OneIn(10) ? "Outside" : "Inside", (Location2D l) => !zone.GetCell(l).HasOpenLiquidVolume() && !zone.GetCell(l).MightBlockPaths());
			if (gameObject2 != null && gameObject2.CurrentCell != null)
			{
				gameObject2.CurrentCell.RemoveObjects((GameObject o) => o.IsOpenLiquidVolume());
			}
			PopulationLayout building2 = PickBuilding();
			PopulationLayout populationLayout2 = PickBuilding();
			PopulationLayout building3 = PickBuilding();
			PopulationLayout building4 = PickBuilding();
			if (villageSnapshot.GetProperty("abandoned") != "true")
			{
				GameObject gameObject3 = null;
				GameObject gameObject4 = null;
				GameObject gameObject5 = null;
				GameObject gameObject6 = null;
				GameObject gameObject7 = null;
				if (villageSnapshot.listProperties.ContainsKey("immigrant_type"))
				{
					List<string> list2 = villageSnapshot.listProperties["immigrant_type"];
					List<string> list3 = villageSnapshot.listProperties["immigrant_name"];
					List<string> list4 = villageSnapshot.listProperties["immigrant_gender"];
					List<string> list5 = villageSnapshot.listProperties["immigrant_role"];
					List<string> list6 = villageSnapshot.listProperties["immigrant_dialogWhy_Q"];
					List<string> list7 = villageSnapshot.listProperties["immigrant_dialogWhy_A"];
					for (int num40 = 0; num40 < list2.Count; num40++)
					{
						string text4 = list2[num40];
						string name;
						if (num40 >= list3.Count)
						{
							Debug.LogWarning("missing immigrant name for " + text4 + " in position " + num40);
							name = "MISSING_NAME";
						}
						else
						{
							name = list3[num40];
						}
						string gender;
						if (num40 >= list4.Count)
						{
							Debug.LogWarning("missing immigrant gender for " + text4 + " in position " + num40);
							gender = null;
						}
						else
						{
							gender = list4[num40];
						}
						string text5;
						if (num40 >= list5.Count)
						{
							Debug.LogWarning("missing immigrant role for " + text4 + " in position " + num40);
							text5 = "villager";
						}
						else
						{
							text5 = list5[num40];
						}
						string whyQ;
						if (num40 >= list6.Count)
						{
							Debug.LogWarning("missing immigrant dialog why Q for " + text4 + " in position " + num40);
							whyQ = "MISSING_QUESTION";
						}
						else
						{
							whyQ = list6[num40];
						}
						string whyA;
						if (num40 >= list7.Count)
						{
							Debug.LogWarning("missing immigrant dialog why A for " + text4 + " in position " + num40);
							whyA = "MISSING_ANSWER";
						}
						else
						{
							whyA = list7[num40];
						}
						try
						{
							GameObject gameObject8 = generateImmigrant(text4, name, gender, text5, whyQ, whyA);
							switch (text5)
							{
							case "mayor":
								gameObject3 = gameObject8;
								break;
							case "merchant":
								gameObject4 = gameObject8;
								break;
							case "tinker":
								gameObject5 = gameObject8;
								break;
							case "apothecary":
								gameObject6 = gameObject8;
								break;
							case "warden":
								gameObject7 = gameObject8;
								break;
							default:
								ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), gameObject8);
								break;
							}
						}
						catch (Exception x2)
						{
							MetricsManager.LogException("Failed to generate immigrant.", x2);
						}
					}
				}
				if (villageSnapshot.GetProperty("government") != "anarchism")
				{
					GameObject baseObject = null;
					if (villageSnapshot.GetProperty("government") == "colonialism")
					{
						baseObject = GameObject.Create(villageSnapshot.GetProperty("colonistType"));
					}
					if (gameObject7 != null)
					{
						ZoneBuilderSandbox.PlaceObject(Z, townSquare, gameObject7);
					}
					else
					{
						ZoneBuilderSandbox.PlaceObject(Z, townSquare, generateWarden(baseObject, isVillageZero));
					}
				}
				GameObject gameObject9 = null;
				if (villageSnapshot.GetProperty("government") == "colonialism")
				{
					gameObject9 = GameObject.Create(villageSnapshot.GetProperty("colonistType"));
					setVillagerProperties(gameObject9);
				}
				if (gameObject3 != null)
				{
					PlaceObjectInBuilding(gameObject3, building2, If.OneIn(100) ? "Outside" : "Inside");
				}
				else
				{
					PlaceObjectInBuilding(generateMayor(gameObject9, "SpecialVillagerHeroTemplate_" + mayorTemplate), building2, If.OneIn(100) ? "Outside" : "Inside");
				}
				if (gameObject4 != null)
				{
					PlaceObjectInBuilding(gameObject4, building4, If.OneIn(100) ? "Outside" : "Inside");
				}
				else if (isVillageZero || If.Chance(100))
				{
					PlaceObjectInBuilding(generateMerchant(null), building4, If.OneIn(100) ? "Outside" : "Inside");
				}
				if (isVillageZero)
				{
					if (base.region == "Saltmarsh")
					{
						ZoneBuilderSandbox.PlaceObject(Z, townSquare, GameObject.Create("WatervineFarmerConvert"));
					}
					else if (base.region == "DesertCanyon")
					{
						ZoneBuilderSandbox.PlaceObject(Z, townSquare, GameObject.Create("PigFarmerConvert"));
					}
					else if (base.region == "Hills")
					{
						ZoneBuilderSandbox.PlaceObject(Z, townSquare, GameObject.Create("CannibalConvert"));
					}
					else if (base.region == "Saltdunes")
					{
						ZoneBuilderSandbox.PlaceObject(Z, townSquare, GameObject.Create("IssachariConvert"));
					}
					else
					{
						ZoneBuilderSandbox.PlaceObject(Z, townSquare, GameObject.Create("WatervineFarmerConvert"));
					}
				}
				GameObject gameObject10 = null;
				bool flag5 = false;
				if (gameObject5 != null)
				{
					gameObject10 = gameObject5;
				}
				else if (isVillageZero || If.Chance(100))
				{
					gameObject10 = generateTinker();
					flag5 = true;
				}
				if (gameObject10 != null)
				{
					PlaceTinker(gameObject10, populationLayout2);
				}
				GameObject gameObject11 = null;
				bool flag6 = false;
				if (gameObject6 != null)
				{
					gameObject11 = gameObject6;
				}
				else if (isVillageZero || If.Chance(100))
				{
					gameObject11 = generateApothecary();
					flag6 = true;
				}
				if (gameObject11 != null)
				{
					string hint = (If.OneIn(50) ? "Outside" : "AlongInsideWall");
					PlaceObjectInBuilding(gameObject11, building3, hint);
					int num41 = 0;
					for (int num42 = Stat.Random(1, 2); num41 < num42; num41++)
					{
						PlaceObjectInBuilding(GameObject.Create("Table"), building3, hint);
					}
					int num43 = 0;
					for (int num44 = Stat.Random(0, 1); num43 < num44; num43++)
					{
						PlaceObjectInBuilding(GameObject.Create("Alchemist Table"), building3, hint);
					}
					int num45 = 0;
					for (int num46 = Stat.Random(2, 3); num45 < num46; num45++)
					{
						PlaceObjectInBuilding(GameObject.Create("Woven Basket"), building3, hint);
					}
				}
				int num47 = Stat.Random(4, 10);
				if (!isVillageZero)
				{
					if (!flag5)
					{
						num47++;
					}
					if (!flag6)
					{
						num47++;
					}
				}
				if (villageSnapshot.listProperties.ContainsKey("populationMultiplier"))
				{
					foreach (string item4 in villageSnapshot.listProperties["populationMultiplier"])
					{
						num47 *= int.Parse(item4);
					}
				}
				if (IsPlagued)
				{
					num47 -= 2;
				}
				for (int num48 = 0; num48 < num47; num48++)
				{
					ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), generateVillager());
				}
				if (villageSnapshot.GetProperty("government") == "colonialism")
				{
					int num49 = 0;
					for (int num50 = Stat.Random(2, 3); num49 < num50; num49++)
					{
						GameObject gameObject12 = GameObject.Create(villageSnapshot.GetProperty("colonistType"));
						setVillagerProperties(gameObject12);
						gameObject12.SetIntProperty("SuppressSimpleConversation", 1);
						AddVillagerConversation(gameObject12, gameObject12.GetTag("SimpleConversation", "Moon and Sun. Wisdom and will.~May the earth yield for us this season.~Peace, =player.formalAddressTerm=."));
						ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), gameObject12);
					}
				}
				if (villageSnapshot.GetProperty("government") == "representative democracy")
				{
					int num51 = 0;
					for (int num52 = Stat.Random(2, 4); num51 < num52; num51++)
					{
						ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), generateMayor(null, "SpecialVillagerHeroTemplate_" + mayorTemplate, GivesRep: false));
					}
				}
				ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), generateVillager(bUnique: true));
			}
			if (villageSnapshot.listProperties.ContainsKey("pet_petSpecies"))
			{
				for (int num53 = 0; num53 < villageSnapshot.listProperties["pet_petSpecies"].Count; num53++)
				{
					try
					{
						List<string> petNames = new List<string>();
						int num54 = int.Parse(villageSnapshot.listProperties["pet_number"][num53]);
						for (int num55 = 0; num55 < num54; num55++)
						{
							string name2;
							GameObject obj2 = generatePet(villageSnapshot.listProperties["pet_petSpecies"][num53], out name2);
							ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), obj2);
							petNames.Add(name2);
						}
						GameObject petSample = GameObject.Create(villageSnapshot.listProperties["pet_petSpecies"][num53]);
						zone.ForeachObjectWithTagOrProperty("Villager", delegate(GameObject o)
						{
							string text6 = HistoricStringExpander.ExpandString("<spice.villages.pet.originStory.!random>", null, null, QudHistoryHelpers.BuildContextFromObjectTextFragments(villageSnapshot.listProperties["pet_petSpecies"][num53]));
							text6 = ((int.Parse(villageSnapshot.listProperties["pet_number"][num53]) != 1) ? text6.Replace("@them@", "them").Replace("@they@", "they").Replace("@they're@", "they're")
								.Replace("@they've@", "they've")
								.Replace("@their@", "their")
								.Replace("@Them@", "Them")
								.Replace("@They@", "They")
								.Replace("@They're@", "They're")
								.Replace("@They've@", "They've")
								.Replace("@Their@", "Their")
								.Replace("@has@", "have")
								.Replace("@Name@", Grammar.MakeAndList(petNames)) : text6.Replace("@them@", petSample.them).Replace("@they@", petSample.it).Replace("@they're@", petSample.itis)
								.Replace("@they've@", petSample.ithas)
								.Replace("@their@", petSample.its)
								.Replace("@Them@", petSample.Them)
								.Replace("@They@", petSample.It)
								.Replace("@They're@", petSample.Itis)
								.Replace("@They've@", petSample.Ithas)
								.Replace("@Their@", petSample.Its)
								.Replace("@has@", "has")
								.Replace("@Name@", petNames[0]));
							if (!o.HasTagOrProperty("VillagePet"))
							{
								AddVillagerConversation(o, o.GetTag("SimpleConversation", "Moon and Sun. Wisdom and will.~May the earth yield for us this season.~Peace, =player.formalAddressTerm=."), "Live and drink.", villageSnapshot.listProperties["pet_dialogWhy_Q"][num53], text6, AppendConversation: true);
							}
						});
					}
					catch (Exception x3)
					{
						MetricsManager.LogException("Failed to generate pet.", x3);
					}
				}
			}
			Z.ForeachObjectWithPart("Brain", delegate(GameObject obj)
			{
				AllegianceSet allegianceSet = obj.PartyLeader?.Brain.Allegiance;
				if (allegianceSet != null && allegianceSet.TryGetValue(villageFaction, out var Value))
				{
					obj.Brain.Allegiance.TryAdd(villageFaction, Value);
				}
			});
			Z.ForeachObjectWithPart("SecretObject", delegate(GameObject obj)
			{
				obj.RemovePart<SecretObject>();
			});
			placeStories();
			Z.ForeachObject(delegate(GameObject o)
			{
				if ((o.GetBlueprint().HasTag("Furniture") || o.GetBlueprint().HasTag("Vessel")) && o.Physics != null)
				{
					o.Physics.Owner = villageFaction;
				}
				if (villageSnapshot.listProperties.ContainsKey("signatureLiquids") && o.GetBlueprint().HasTag("Vessel") && If.Chance(80))
				{
					LiquidVolume liquidVolume = o.LiquidVolume;
					if (liquidVolume != null)
					{
						liquidVolume.InitialLiquid = villageSnapshot.GetList("signatureLiquids").GetRandomElement();
					}
				}
				if (o.HasStringProperty("GivesDynamicQuest") && o.Brain != null)
				{
					o.Brain.Wanders = false;
					o.Brain.WandersRandomly = false;
				}
			});
			if (villageSnapshot.GetProperty("abandoned") == "true")
			{
				int num56 = 1;
				try
				{
					num56 = Convert.ToInt32(villageSnapshot.GetProperty("ruinScale"));
					if (num56 < 1)
					{
						num56 = 1;
					}
					if (num56 > 4)
					{
						num56 = 4;
					}
				}
				catch (Exception ex2)
				{
					Logger.Exception(ex2);
				}
				if (num56 > 1)
				{
					int num57 = 10;
					if (num56 == 3)
					{
						num57 = 50;
					}
					if (num56 == 4)
					{
						num57 = 100;
					}
					new Ruiner().RuinZone(Z, num57, bUnderground: false);
					for (int num58 = 0; num58 < num57 * 2; num58++)
					{
						ZoneBuilderSandbox.PlaceObject(getARegionalPlant(), Z);
					}
				}
				if (If.Chance(0))
				{
					foreach (GameObject originalCreature in originalCreatures)
					{
						ZoneBuilderSandbox.PlaceObject(originalCreature, Z);
					}
				}
				Z.ReplaceAll("Torchpost", "Chiliad Unlit Torchpost");
				Z.ReplaceAll("Chiliad Torchpost", "Chiliad Unlit Torchpost");
				Z.ReplaceAll("Sconce", "Chiliad Unlit Torchpost");
			}
			PopulateCodaApothecary(building3);
			PopulateCodaMerchant(building4);
			PopulateCodaTinker(populationLayout2);
			if (HasFoundSonnet || HasReturnedSonnet)
			{
				PlaceEtaEarthling(building2);
			}
			if (HasStarshiibLocket)
			{
				PopulationLayout randomElement = buildings.GetRandomElement();
				PlaceObjectInBuilding(GameObject.Create("Chiliad Hologram Locket"), randomElement, "InsideCorner");
			}
			if (EndGame.IsUltimate)
			{
				FabricateUltraHut(plot, IsRuined);
			}
			ZoneBuilderSandbox.EnsureAllVoidsConnected(Z);
			if (Z.HasBuilder("RiverBuilder"))
			{
				new RiverBuilder(hardClear: false, originalLiquids?.GetRandomElement()?.Blueprint ?? getZoneDefaultLiquid(Z), VillageMode: true).BuildZone(Z);
			}
			if (Z.HasBuilder("RoadBuilder"))
			{
				new RoadBuilder(HardClear: false).BuildZone(Z);
			}
			foreach (GameObject requiredPlacementObject in requiredPlacementObjects)
			{
				if (requiredPlacementObject.HasPart<Combat>())
				{
					setVillagerProperties(requiredPlacementObject);
				}
				ZoneBuilderSandbox.PlaceObject(requiredPlacementObject, zone);
			}
			string damageChance = ((villageSnapshot.GetProperty("abandoned") == "true") ? Stat.Random(5, 25).ToString() : (10 - villageTechTier).ToString());
			PowerGrid powerGrid = new PowerGrid();
			powerGrid.PreferWalls = false;
			powerGrid.DamageChance = damageChance;
			if ((10 + villageTechTier * 3).in100())
			{
				powerGrid.MissingConsumers = "1d6";
				powerGrid.MissingProducers = "1d3";
			}
			powerGrid.BuildZone(Z);
			Hydraulics hydraulics = new Hydraulics();
			hydraulics.DamageChance = damageChance;
			if ((10 + villageTechTier * 3).in100())
			{
				hydraulics.MissingConsumers = "1d6";
				hydraulics.MissingProducers = "1d3";
			}
			hydraulics.BuildZone(Z);
			MechanicalPower mechanicalPower = new MechanicalPower();
			mechanicalPower.DamageChance = damageChance;
			if ((20 - villageTechTier).in100())
			{
				mechanicalPower.MissingConsumers = "1d6";
				mechanicalPower.MissingProducers = "1d3";
			}
			mechanicalPower.BuildZone(Z);
			PlaceSultanStories();
			Z.SetMusic("Music/Mehmets Book on Strings");
			Z.FireEvent("VillageInit");
			cleanup();
			new IsCheckpoint().BuildZoneWithKey(Z, villageName);
			Cell cell2 = Z.GetCell(0, 0);
			if (SurfaceRevealer && !cell2.HasObject("VillageSurface") && base.villageEntity != null)
			{
				GameObject gameObject13 = GameObject.Create("VillageSurface");
				if (gameObject13.TryGetPart<VillageSurface>(out var Part))
				{
					Cell worldCell = Z.GetWorldCell();
					Part.VillageName = villageName;
					Part.RevealKey = "villageReveal_" + villageName;
					Part.RevealLocation = new Vector2i(worldCell.X, worldCell.Y);
					Part.RevealSecret = base.villageEntity.id;
					Part.IsVillageZero = base.villageEntity.GetEntityProperty("isVillageZero", -1L).EqualsNoCase("true");
					if (base.villageEntity.GetEntityProperty("abandoned", -1L).EqualsNoCase("true"))
					{
						Part.RevealString = "You discover the abandoned village of " + villageName + ".";
					}
					else
					{
						Part.RevealString = "You discover the village of " + villageName + ".";
					}
					cell2.AddObject(gameObject13);
				}
			}
			return true;
		}

		public static HistoricEntity GenerateSultanEntity(GameObject Object)
		{
			int flipYear = HistoryAPI.GetFlipYear();
			HistoricEntity historicEntity = The.Game.sultanHistory.CreateEntity("CodaSultan", flipYear + 1001);
			HistoricEvent historicEvent = historicEntity.events[0];
			historicEvent.SetEntityProperty("name", Object.Render.DisplayName.Strip());
			historicEvent.SetEntityProperty("statueTile", Object.Render.Tile);
			historicEvent.SetEntityProperty("statueBase", "true");
			historicEvent.SetEntityProperty("isCandidate", "true");
			historicEvent.SetEntityProperty("type", "sultan");
			historicEvent.SetEntityProperty("period", 7.ToStringCached());
			if (Object.TryGetPart<Tattoos>(out var Part))
			{
				List<string> list = new List<string>(3);
				list.Add(ConsoleLib.Console.ColorUtility.FindLastForeground(Part.ColorString).ToString());
				list.Add(Part.DetailColor);
				list.Add("K");
				historicEvent.AddListProperty("palette", list);
			}
			else
			{
				historicEvent.AddListProperty("palette", Crayons.GetRandomDistinctColorsAll(3));
			}
			Faction faction = Factions.Get("PlayerCult");
			faction.DisplayName = "Cult of " + Object.Render.DisplayName.Strip();
			faction.Emblem.Copy(Object.Render);
			int num = 1;
			HashSet<string> hashSet = new HashSet<string>();
			foreach (JournalAccomplishment item in (from x in PlayerMuralGameState.Instance.GetAccomplishments()
				where x.MuralCategory != MuralCategory.Dies
				select x).Take(10))
			{
				if (item.GospelText.IsNullOrEmpty())
				{
					MetricsManager.LogError("Mural accomplishment (" + item.ID.Coalesce("NO ID") + ") does not have gospel text: \"" + item.Text.Coalesce(item.MuralText) + "\"");
				}
				else if (hashSet.Add(item.GospelText))
				{
					GospelEvent gospelEvent = new GospelEvent(item.GospelText);
					string text = $"CodaSultanGospel{num++}";
					historicEntity.ApplyEvent(gospelEvent, flipYear + Calendar.GetYear(item.Time));
					gospelEvent.SetEventProperty("Mural", "1");
					gospelEvent.SetEventProperty("NoteID", text);
					JournalAPI.AddSultanNote(text, item.GospelText ?? "[NO GOSPEL]", historicEntity.id, gospelEvent.id);
				}
			}
			return historicEntity;
		}

		public void PlaceSultanStories()
		{
			bool flag = Stat.Chance(50);
			BallBag<GameObject> ballBag = new BallBag<GameObject>();
			for (int i = 0; i < zone.Height; i++)
			{
				for (int j = 0; j < zone.Width; j++)
				{
					GameObject highestRenderLayerObject = zone.GetCell(j, i).GetHighestRenderLayerObject();
					if (GameObject.Validate(highestRenderLayerObject) && !highestRenderLayerObject.IsCombatObject() && !highestRenderLayerObject.HasPart<ModPainted>() && !highestRenderLayerObject.HasPart<ModEngraved>() && !highestRenderLayerObject.HasPart<RevealNoteOnLook>() && !highestRenderLayerObject.HasPart<VillageStoryReveal>() && !highestRenderLayerObject.HasPart<AnimatedMaterialFire>())
					{
						int weight = 0;
						if (highestRenderLayerObject.HasPropertyOrTag("Vessel"))
						{
							weight = 1000;
						}
						else if (highestRenderLayerObject.HasPropertyOrTag("Furniture"))
						{
							weight = 750;
						}
						else if (highestRenderLayerObject.HasPropertyOrTag("Door"))
						{
							weight = 250;
						}
						ballBag.Add(highestRenderLayerObject, weight);
					}
				}
			}
			foreach (PopulationLayout building in buildings)
			{
				foreach (Location2D cell2 in building.originalRegion.Cells)
				{
					Cell cell = zone.GetCell(cell2);
					GameObject firstWall = cell.GetFirstWall();
					if (firstWall == null)
					{
						continue;
					}
					int num = 0;
					Cell.SpiralEnumerator enumerator3 = cell.IterateAdjacent().GetEnumerator();
					while (enumerator3.MoveNext())
					{
						if (enumerator3.Current.IsPassable())
						{
							num++;
						}
					}
					if (num > 2)
					{
						ballBag.Add(firstWall, num * 2);
					}
				}
			}
			HistoricEntity entity = The.Game.sultanHistory.GetEntity("CodaSultan");
			HistoricEntitySnapshot currentSnapshot = entity.GetCurrentSnapshot();
			string entityProperty = entity.GetEntityProperty("name", -1L);
			foreach (HistoricEvent @event in entity.events)
			{
				if (@event.HasEventProperty("Mural"))
				{
					string eventProperty = @event.GetEventProperty("NoteID");
					IBaseJournalEntry baseJournalEntry = JournalAPI.NotesByID[eventProperty];
					GameObject gameObject = ballBag.PickOne();
					if (gameObject == null)
					{
						MetricsManager.LogError("Insufficient objects to place coda sultan stories.");
						break;
					}
					if (flag)
					{
						gameObject.AddPart(new ModPainted
						{
							PaintedEvent = @event,
							Sultan = entityProperty,
							Painting = baseJournalEntry.Text
						});
					}
					else
					{
						gameObject.AddPart(new ModEngraved
						{
							EngravedEvent = @event,
							Sultan = entityProperty,
							Engraving = baseJournalEntry.Text
						});
					}
					string text = "Y";
					string text2 = "C";
					if (flag)
					{
						HistoricPerspective historicPerspective = @event.RequirePerspective(currentSnapshot);
						text = historicPerspective.mainColor.Coalesce(text);
						text2 = historicPerspective.supportColor.Coalesce(text2);
					}
					gameObject.Render.ColorString = "&" + text;
					gameObject.Render.DetailColor = text2;
					if (!gameObject.Render.TileColor.IsNullOrEmpty())
					{
						gameObject.Render.TileColor = gameObject.Render.ColorString;
					}
				}
			}
		}

		public HistoricEvent GenerateEndEvent()
		{
			HistoricEntity entity = The.Game.sultanHistory.GetEntity("CodaSultan");
			string sultanTerm = HistoryAPI.GetSultanTerm();
			string text = "";
			text = (EndGame.IsMarooned ? "In =event.year=, a triad of plagues afflicted the land. Tongues rotted away in the mouths of kith and kin, their legs annealed to iron, and darkness bloomed from the earth. The warden physickers voiced a prayer to =subject.name= the Above and walked beneath the chrome arches to heal the sick." : (EndGame.IsCovenant ? "In =event.year=, =subject.name= cleansed the =village.region= of the plagues of the Gyre and, through the tutelage of the tinker monks at Grit Gate, taught =mayor.name= to sow =village.plant= along their fertile tracks." : (EndGame.IsAccede ? "In =event.year=, =subject.name=, the Above, forsook the people of Qud in favor of its sludges and microorganisms, and then disappeared. =pronouns.Subjective= =verb:were:afterpronoun= 216 years old." : ((!EndGame.IsLaunch) ? "In =event.year=, a triad of plagues afflicted the land. Tongues rotted away in the mouths of kith and kin, their legs annealed to iron, and darkness bloomed from the earth. =subject.name= and their warden physickers walked beneath the chrome arches and healed the sick." : "At twilight in the shadow of the Spindle, the people of =village.name= saw an image on the horizon that looked like a =subject.personTerm= bathed in starfire. It was =subject.name=, and after =pronouns.subjective= came and left =village.name=, the people built a monument to =pronouns.objective=, and thenceforth called =pronouns.objective= =subject.PersonTerm=-in-Starfire."))));
			string text2 = text.StartReplace().AddObject(System.Sultan).AddReplacer("sultan.term", sultanTerm)
				.AddReplacer("mayor.name", NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, null, null, null, null, FailureOkay: false, SpecialFaildown: false, null, null))
				.AddReplacer("village.name", villageName)
				.AddReplacer("village.region", HistoricStringExpander.ExpandString("<spice.history.regions.terrain." + region + ".pluralRegionName.!random>"))
				.AddReplacer("village.plant", HistoricStringExpander.ExpandString("<spice.history.regions.terrain." + region + ".regionCrop.!random>"))
				.AddReplacer("event.year", System.EndYear + " AR")
				.ToString();
			GospelEvent gospelEvent = new GospelEvent(text2);
			entity.ApplyEvent(gospelEvent, HistoryAPI.GetFlipYear() + System.EndYear);
			gospelEvent.SetEventProperty("PlayerStatue", "1");
			gospelEvent.SetEventProperty("NoteID", "CodaSultanEndEvent");
			JournalAPI.AddSultanNote("CodaSultanEndEvent", text2, "CodaSultan", gospelEvent.id);
			return gospelEvent;
		}

		public static HistoricEntity GenerateVillageEntity(string Region, string BaseFaction = null, int YearOffsetLow = 400, int YearOffsetHigh = 900)
		{
			History history = The.Game.sultanHistory;
			long currentYear = history.currentYear;
			history.currentYear = HistoryAPI.GetFlipYear() + Calendar.GetYear() + Stat.Random(YearOffsetLow, YearOffsetHigh);
			HistoricEntity historicEntity = history.CreateEntity("CodaVillage", history.currentYear);
			HistoricEntity entity = history.GetEntity("CodaSultan");
			historicEntity.ApplyEvent(new InitializeVillage(Region, BaseFaction, VillageCodaBase.GetEligibleVillagers(BaseFaction).GetRandomElement().Name, "Chiliad Qudish", entity.GetListProperties("palette", -1L, ReadOnly: false)));
			if (!IsRuined)
			{
				historicEntity.ApplyEvent(QudHistoryFactory.GenerateVillageEvent(VillageZero: false, AllowAbandoned: false), historicEntity.lastYear + Stat.Random(10, 20));
			}
			if (IsPlagued)
			{
				historicEntity.events[0].AddEntityListItem("sharedDiseases", 50.in100() ? "Glotrot" : "Ironshank");
			}
			HistoricEvent historicEvent;
			if (IsDespised)
			{
				historicEntity.ApplyEvent(historicEvent = new CodaDespises(), historicEntity.lastYear + Stat.Random(10, 20));
			}
			else
			{
				historicEntity.ApplyEvent(historicEvent = new CodaWorships(), historicEntity.lastYear + Stat.Random(10, 20));
			}
			if (IsRuined)
			{
				Abandoned abandoned = new Abandoned();
				historicEntity.ApplyEvent(abandoned, historicEntity.lastYear + Stat.Random(10, 20));
				abandoned.SetEntityProperty("ruinScale", "4");
			}
			if (The.Game.FinishedQuests.TryGetValue("Landing Pads", out var Value))
			{
				SlynthPopulationInflux(historicEntity, Value.GetProperty("Faction"));
			}
			historicEntity.ApplyEvent(new VillageProverb("proverbsCoda", historicEvent.GetListProperties("sacredThings")?.GetRandomElement(), historicEvent.GetListProperties("profaneThings")?.GetRandomElement(), 100), historicEntity.lastYear);
			historicEntity.MutateListPropertyAtCurrentYear("Gospels", (string s) => QudHistoryHelpers.ConvertGospelToSultanateCalendarEra(s, history.currentYear));
			if (historicEntity.HasEntityProperty("worships_creature", -1L))
			{
				GameObject gameObject = JoppaWorldBuilder.GenerateLairOwner("Terrain" + Region, Stat.Random(1, 8));
				The.ZoneManager.CacheObject(gameObject);
				Worships.PostProcessEvent(historicEntity, gameObject.GetReferenceDisplayName(int.MaxValue, null, "Worship", NoColor: false, Stripped: true), gameObject.ID);
			}
			if (historicEntity.HasEntityProperty("despises_creature", -1L))
			{
				GameObject gameObject2 = JoppaWorldBuilder.GenerateLairOwner("Terrain" + Region, Stat.Random(1, 8));
				The.ZoneManager.CacheObject(gameObject2);
				Despises.PostProcessEvent(historicEntity, gameObject2.GetReferenceDisplayName(int.MaxValue, null, "Despise", NoColor: false, Stripped: true), gameObject2.ID);
			}
			history.currentYear = currentYear;
			return historicEntity;
		}

		private static void SlynthPopulationInflux(HistoricEntity Village, string FactionName)
		{
			HistoricEvent historicEvent = Village.events[0];
			string slynthType = GetSlynthType(FactionName);
			string slynthRole = GetSlynthRole(Village, FactionName);
			if (!slynthType.IsNullOrEmpty() && !slynthRole.IsNullOrEmpty())
			{
				GameObject gameObject = GameObject.CreateSample(slynthType);
				historicEvent.AddEntityListItem("immigrant_name", NameMaker.MakeName(gameObject, null, null, null, null, null, null, null, null, null, null, null, null, FailureOkay: false, SpecialFaildown: false, null, null), Force: true);
				historicEvent.AddEntityListItem("immigrant_gender", gameObject.GetGender().Name);
				historicEvent.AddEntityListItem("immigrant_role", slynthRole, Force: true);
				historicEvent.AddEntityListItem("immigrant_dialogWhy_Q", "", Force: true);
				historicEvent.AddEntityListItem("immigrant_dialogWhy_A", "", Force: true);
				historicEvent.AddEntityListItem("immigrant_type", slynthType, Force: true);
				gameObject.Pool();
			}
		}

		private static string GetSlynthType(string FactionName)
		{
			Faction ifExists = Factions.GetIfExists(FactionName);
			if (ifExists == null)
			{
				return null;
			}
			switch (FactionName)
			{
			case "Joppa":
				return "Chiliad Slynth Joppa";
			case "Barathrumites":
				return "Chiliad Slynth Grit Gate";
			case "Hindren":
				return "Chiliad Slynth Bey Lah";
			case "Kyakukya":
				return "Chiliad Slynth Kyakukya";
			case "Ezra":
				return "Chiliad Slynth Ezra";
			case "YdFreehold":
				return "Chiliad Slynth Yd Freehold";
			case "Mechanimists":
				return "Chiliad Slynth Mechanimists";
			case "Mopango":
				return "Chiliad Slynth Mopango";
			case "Chavvah":
				return "Chiliad Slynth Chavvah";
			default:
				if (ifExists.GetIntProperty("Village") >= 1)
				{
					return "Chiliad Slynth Village";
				}
				return null;
			}
		}

		private static string GetSlynthRole(HistoricEntity Village, string FactionName)
		{
			string text = "";
			BallBag<string> roleBag = PopulationInflux.GetRoleBag();
			List<string> listProperties = Village.GetListProperties("immigrant_role", -1L);
			switch (FactionName)
			{
			case "Barathrumites":
				text = "tinker";
				break;
			case "Joppa":
			case "Kyakukya":
				text = "apothecary";
				break;
			default:
				text = roleBag.PickOne();
				break;
			}
			while (!text.IsNullOrEmpty())
			{
				if (text == "villager" || !listProperties.Contains(text))
				{
					return text;
				}
				text = roleBag.PickOne();
			}
			return "villager";
		}

		public void SetStatueVisuals(GameObject Object)
		{
			string stringGameState = The.Game.GetStringGameState("CodaStatueMaterial", "Stone");
			Render render = Object.Render;
			render.DisplayName = "shrine to " + System.Sultan.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: true, Short: false, BaseOnly: true);
			render.Tile = System.Sultan.Render.Tile;
			if (EndGame.IsAccede)
			{
				Object.AddPart(new ModDesecrated());
			}
			if (stringGameState == "Gold")
			{
				render.ColorString = "&W";
				render.DetailColor = "w";
				return;
			}
			render.ColorString = "&y";
			render.DetailColor = "Y";
			if (!Object.HasPart<ModDesecrated>())
			{
				Object.AddPart(new MossyUnityMaterial());
			}
		}

		public void SetStatueDescription(GameObject Object)
		{
			GenerateEndEvent();
			GameObject sultan = System.Sultan;
			string sultanTerm = HistoryAPI.GetSultanTerm();
			Description part = Object.GetPart<Description>();
			string text = "This shrine depicts a significant event from the life of the ancient =sultan.term= =subject.name=:\n\n".StartReplace().AddObject(sultan).AddReplacer("sultan.term", sultanTerm)
				.ToString();
			IBaseJournalEntry value = JournalAPI.NotesByID.GetValue("CodaSultanEndEvent");
			Object.AddPart(new RevealNoteOnLook(value.ID));
			part.Short = text + value.Text;
		}

		public void FabricatePlayerStatue()
		{
			int x = 1;
			int y = 1;
			int num = Stat.Random(1, 4);
			int rotation = 0;
			switch (num)
			{
			case 2:
				x = 65;
				break;
			case 3:
				x = 65;
				y = 15;
				rotation = 2;
				break;
			case 4:
				y = 15;
				rotation = 2;
				break;
			}
			MapChunkPlacement.PlaceFromFile(zone.GetCell(x, y), "preset_tile_chunks/CodaPlayerStatue.rpm", 13, 8, 0, rotation, null, null, PlacePostAction);
		}

		private void PlacePostAction(Cell Cell)
		{
			AvoidPoint(Cell.Location);
			GameObject gameObject = Cell.FindObject("Coda Player Statue");
			if (gameObject != null)
			{
				SetStatueVisuals(gameObject);
				SetStatueDescription(gameObject);
			}
		}
	}
}
