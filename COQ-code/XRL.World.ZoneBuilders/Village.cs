using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Annals;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.World.AI;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Quests;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders
{
	public class Village : VillageBase
	{
		public string dynamicCreatureTableName;

		public bool SurfaceRevealer = true;

		private string[] staticPerVillage = new string[4] { "*Storage,*Furniture", "*LiquidStorage,*Furniture", "*Seating,*Furniture", "*Sleep*,*Furniture" };

		private string[] staticPerBuilding = new string[1] { "*LightSource,*Furniture" };

		private Dictionary<string, string> staticVillageResults = new Dictionary<string, string>();

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
				else if (isVillageZero)
				{
					baseVillager = GameObjectFactory.Factory.Blueprints["DromadTrader_Village0"].createOne();
					preprocessVillager(baseVillager, foreign: true);
				}
				else
				{
					baseVillager = GameObjectFactory.Factory.Blueprints["DromadTrader" + villageTier].createOne();
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
				gameObject.Statistics["Toughness"].BaseValue = Math.Max(gameObject.Stat("Toughness"), 15);
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
					PopulationResult populationResult = new PopulationResult(null);
					string populationName = "DynamicSemanticTable:" + templateResult.Blueprint.Replace("*", "") + "::" + villageTechTier;
					populationResult.Blueprint = PopulationManager.RollOneFrom(populationName).Blueprint;
					populationResult.Hint = templateResult.Hint;
					if (populationResult.Blueprint.IsNullOrEmpty())
					{
						Debug.LogError("Couldn't resolve object for " + templateResult.Blueprint);
						continue;
					}
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
			InfluenceMap regionMap = new InfluenceMap(Z.Width, Z.Height);
			for (int i = 0; i < Z.Width; i++)
			{
				for (int j = 0; j < Z.Height; j++)
				{
					regionMap.Walls[i, j] = (Z.GetCell(i, j).HasObjectWithTagOrProperty("Wall") ? 1 : 0);
				}
			}
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
			int num2 = Stat.Random(4, 9);
			int num3 = 0;
			int num4 = regionMap.FindClosestSeedTo(Location2D.Get(40, 13), (InfluenceMapRegion r) => r.maxRect.ReduceBy(1, 1).Width >= 6 && r.maxRect.ReduceBy(1, 1).Height >= 6 && r.AdjacentRegions.Count > 1);
			Location2D location2D = regionMap.Seeds[num4];
			townSquare = regionMap.Regions[num4];
			townSquareLayout = null;
			foreach (InfluenceMapRegion region in regionMap.Regions)
			{
				Rect2D Rect = GridTools.MaxRectByArea(region.GetGrid()).Translate(region.BoundingBox.UpperLeft).ReduceBy(1, 1);
				PopulationLayout populationLayout = new PopulationLayout(Z, region, Rect);
				if (region.AdjacentRegions.Count <= 1 && region.Size >= 9 && !region.IsEdgeRegion() && region != townSquare)
				{
					buildings.Add(populationLayout);
				}
				else if ((Rect.Width >= 5 && Rect.Height >= 5 && num2 > 0) || region == townSquare)
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
							int num5 = item.width / 2;
							int num6 = item.height / 2;
							if (item.extrawidth > populationLayout.innerRect.Width || item.extraheight > populationLayout.innerRect.Height)
							{
								continue;
							}
							for (int m = 0; m < item.width; m++)
							{
								for (int n = 0; n < item.height; n++)
								{
									Cell cell = Z.GetCell(populationLayout.position.X - num5 + m, populationLayout.position.Y - num6 + n);
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
					num2--;
					num3++;
				}
				else if (region.AdjacentRegions.Count == 1 && !region.IsEdgeRegion() && townSquare != region)
				{
					VillageBase.MakeCaveBuilding(Z, regionMap, region);
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
			for (int num7 = 0; num7 < Z.Width; num7++)
			{
				for (int num8 = 0; num8 < Z.Height; num8++)
				{
					regionMap.Walls[num7, num8] = (Z.GetCell(num7, num8).HasObjectWithTag("Wall") ? 1 : 0);
				}
			}
			List<Location2D> list = new List<Location2D>();
			foreach (PopulationLayout building2 in buildings)
			{
				Location2D position = building2.position;
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
				for (int num9 = 0; num9 < zone.Width; num9++)
				{
					for (int num10 = 0; num10 < zone.Height; num10++)
					{
						if (zone.GetCell(num9, num10).HasWall())
						{
							pathfinder.CurrentNavigationMap[num9, num10] = 4999;
						}
						else
						{
							pathfinder.CurrentNavigationMap[num9, num10] = noiseMap.Noise[num9, num10];
						}
					}
				}
				foreach (PopulationLayout building3 in buildings)
				{
					foreach (Location2D cell4 in building3.originalRegion.Cells)
					{
						int x = cell4.X;
						int y = cell4.Y;
						if (x != 0 && x != 79 && y != 0 && y != 24 && Z.GetCell(x, y).IsEmpty())
						{
							int num11 = 0;
							int num12 = 0;
							if (Z.GetCell(x - 1, y).HasWall() || Z.GetCell(x - 1, y).HasObjectWithTag("Door"))
							{
								num12++;
							}
							if (Z.GetCell(x + 1, y).HasWall() || Z.GetCell(x + 1, y).HasObjectWithTag("Door"))
							{
								num12++;
							}
							if (Z.GetCell(x, y - 1).HasWall() || Z.GetCell(x, y - 1).HasObjectWithTag("Door"))
							{
								num11++;
							}
							if (Z.GetCell(x, y + 1).HasWall() || Z.GetCell(x, y + 1).HasObjectWithTag("Door"))
							{
								num11++;
							}
							if ((num11 == 2 && num12 == 0) || (num11 == 0 && num12 == 2))
							{
								influenceMap.Walls[x, y] = 1;
							}
						}
					}
				}
				for (int num13 = 0; num13 < 80; num13++)
				{
					for (int num14 = 0; num14 < 25; num14++)
					{
						if (burrowedDoors.Contains(Location2D.Get(num13, num14)))
						{
							influenceMap.Walls[num13, num14] = 1;
						}
					}
				}
				influenceMap.Recalculate(bDraw);
				foreach (PopulationLayout building4 in buildings)
				{
					string blueprint = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_BuildingFloor")).Blueprint;
					string text2 = PopulationManager.RollOneFrom(ResolvePopulationTableName("Villages_BuildingPath")).Blueprint;
					if (text2 == "Pond")
					{
						text2 = getZoneDefaultLiquid(zone);
					}
					if (pathfinder.FindPath(building4.position, location2D, Display: false, CardinalDirectionsOnly: true))
					{
						foreach (PathfinderNode step in pathfinder.Steps)
						{
							if (!text2.IsNullOrEmpty())
							{
								zone.GetCell(step.pos).AddObject(text2);
							}
							if (!buildingPaths.Contains(step.pos))
							{
								buildingPaths.Add(step.pos);
							}
						}
					}
					foreach (Location2D cell5 in building4.originalRegion.Cells)
					{
						if (Z.GetCell(cell5).HasWall() || buildingPaths.Contains(cell5))
						{
							continue;
						}
						if (influenceMap.Regions.Count() <= building4.region.Seed)
						{
							MetricsManager.LogEditorError("village insideOutMap", "insideOutMap didn't have seed");
							building4.outside.Add(cell5);
							int num15 = Z.GetCell(cell5).CountObjectWithTagCardinalAdjacent("Wall");
							if (num15 > 0)
							{
								building4.outsideWall.Add(cell5);
							}
							if (num15 >= 2)
							{
								building4.outsideCorner.Add(cell5);
							}
							continue;
						}
						if (!influenceMap.Regions[building4.region.Seed].Cells.Contains(cell5))
						{
							building4.outside.Add(cell5);
							int num16 = Z.GetCell(cell5).CountObjectWithTagCardinalAdjacent("Wall");
							if (num16 > 0)
							{
								building4.outsideWall.Add(cell5);
							}
							if (num16 >= 2)
							{
								building4.outsideCorner.Add(cell5);
							}
							continue;
						}
						building4.inside.Add(cell5);
						if (!blueprint.IsNullOrEmpty())
						{
							Z.GetCell(cell5).AddObject(blueprint);
						}
						int num17 = Z.GetCell(cell5).CountObjectWithTagCardinalAdjacent("Wall");
						if (num17 > 0)
						{
							building4.insideWall.Add(cell5);
						}
						if (num17 >= 2)
						{
							building4.insideCorner.Add(cell5);
						}
					}
				}
			}
			Dictionary<InfluenceMapRegion, Rect2D> dictionary = new Dictionary<InfluenceMapRegion, Rect2D>();
			Dictionary<InfluenceMapRegion, string> dictionary2 = new Dictionary<InfluenceMapRegion, string>();
			InfluenceMap influenceMap2 = new InfluenceMap(Z.Width, Z.Height);
			influenceMap2.Seeds = new List<Location2D>(regionMap.Seeds);
			Z.SetInfluenceMapWalls(influenceMap2.Walls);
			influenceMap2.Recalculate();
			int num18 = 0;
			for (int num19 = 0; num19 < influenceMap2.Regions.Count; num19++)
			{
				InfluenceMapRegion R = influenceMap2.Regions[num19];
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
				if (num19 == num4)
				{
					continue;
				}
				if (list.Contains(regionMap.Seeds[R.Seed]))
				{
					dictionary2.Add(R, "building");
					PopulationLayout building = buildings.First((PopulationLayout b) => b.position == regionMap.Seeds[R.Seed]);
					string text3 = RollOneFrom("Villages_BuildingTheme_" + villageTheme);
					foreach (PopulationResult item2 in ResolveBuildingContents(PopulationManager.Generate(ResolvePopulationTableName("Villages_BuildingContents_Dwelling_" + text3))))
					{
						PlaceObjectInBuilding(item2, building);
					}
				}
				else if (value.Area >= 4)
				{
					dictionary2.Add(R, "greenspace");
					if (num18 == 0 && signatureHistoricObjectInstance != null)
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
						int num20 = 20;
						if (blueprint3 == "aquaculture")
						{
							string blueprint4 = originalLiquids?.GetRandomElement()?.Blueprint ?? getZoneDefaultLiquid(zone);
							GameObject aFarmablePlant = getAFarmablePlant();
							Maze maze = RecursiveBacktrackerMaze.Generate(Math.Max(1, R.BoundingBox.Width / 3 + 1), Math.Max(1, R.BoundingBox.Height / 3 + 1), bShow: false, ZoneBuilderSandbox.GetOracleIntFromString("aquaculture" + num18, 0, 2147483646));
							for (int num21 = R.BoundingBox.x1; num21 <= R.BoundingBox.x2; num21++)
							{
								for (int num22 = R.BoundingBox.y1; num22 <= R.BoundingBox.y2; num22++)
								{
									int num23 = (num21 - R.BoundingBox.x1) / 3;
									int num24 = (num22 - R.BoundingBox.y1) / 3;
									int num25 = (num21 - R.BoundingBox.x1) % 3;
									int num26 = (num22 - R.BoundingBox.y1) % 3;
									bool flag2 = false;
									if (num25 == 1 && num26 == 1)
									{
										flag2 = maze.Cell[num23, num24].AnyOpen();
									}
									if (num25 == 1 && num26 == 0)
									{
										flag2 = maze.Cell[num23, num24].N;
									}
									if (num25 == 1 && num26 == 2)
									{
										flag2 = maze.Cell[num23, num24].S;
									}
									if (num25 == 2 && num26 == 1)
									{
										flag2 = maze.Cell[num23, num24].E;
									}
									if (num25 == 0 && num26 == 1)
									{
										flag2 = maze.Cell[num23, num24].W;
									}
									if (flag2)
									{
										if (R.Cells.Contains(Location2D.Get(num21, num22)) && !buildingPaths.Contains(Location2D.Get(num21, num22)))
										{
											Z.GetCell(num21, num22)?.AddObject(aFarmablePlant.Blueprint, base.setVillageDomesticatedProperties);
										}
									}
									else if (R.Cells.Contains(Location2D.Get(num21, num22)) && Z.GetCell(num21, num22) != null)
									{
										Z.GetCell(num21, num22).AddObject(blueprint4);
									}
								}
							}
						}
						else if (blueprint3 == "farm" && value.Area >= num20 && value.Width >= 7 && value.Height <= 7)
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
							int num27 = 0;
							if (cellSide == "N")
							{
								num27 = ((Stat.Random(0, 1) == 0) ? 2 : 3);
							}
							if (cellSide == "S")
							{
								num27 = ((Stat.Random(0, 1) != 0) ? 1 : 0);
							}
							if (cellSide == "E")
							{
								num27 = ((Stat.Random(0, 1) != 0) ? 3 : 0);
							}
							if (cellSide == "W")
							{
								num27 = ((Stat.Random(0, 1) == 0) ? 1 : 2);
							}
							if (num27 == 0 || num27 == 1)
							{
								r2.y2 = r2.y1 + 3;
							}
							else
							{
								r2.y1 = r2.y2 - 3;
							}
							if (num27 == 0 || num27 == 3)
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
							int num28 = Stat.Random(1, 4);
							GameObject aFarmablePlant2 = getAFarmablePlant();
							string blueprint5 = originalLiquids?.GetRandomElement()?.Blueprint ?? getZoneDefaultLiquid(zone);
							if (num28 == 1)
							{
								bool flag3 = Stat.Random(1, 100) <= 33;
								for (int num29 = R.BoundingBox.x1; num29 <= R.BoundingBox.x2; num29++)
								{
									for (int num30 = R.BoundingBox.y1; num30 <= R.BoundingBox.y2; num30++)
									{
										if (num29 % 2 == 0)
										{
											if (R.Cells.Contains(Location2D.Get(num29, num30)) && !buildingPaths.Contains(Location2D.Get(num29, num30)))
											{
												Z.GetCell(num29, num30)?.AddObject(aFarmablePlant2.Blueprint, base.setVillageDomesticatedProperties);
											}
										}
										else if (flag3 && R.Cells.Contains(Location2D.Get(num29, num30)) && Z.GetCell(num29, num30) != null)
										{
											Z.GetCell(num29, num30).AddObject(blueprint5);
										}
									}
								}
							}
							if (num28 == 2)
							{
								string blueprint6 = originalLiquids?.GetRandomElement()?.Blueprint ?? getZoneDefaultLiquid(zone);
								bool flag4 = Stat.Random(1, 100) <= 33;
								for (int num31 = R.BoundingBox.x1; num31 <= R.BoundingBox.x2; num31++)
								{
									for (int num32 = R.BoundingBox.y1; num32 <= R.BoundingBox.y2; num32++)
									{
										if (num32 % 2 == 0)
										{
											if (R.Cells.Contains(Location2D.Get(num31, num32)) && !buildingPaths.Contains(Location2D.Get(num31, num32)))
											{
												Z.GetCell(num31, num32)?.AddObject(aFarmablePlant2.Blueprint, base.setVillageDomesticatedProperties);
											}
										}
										else if (flag4 && R.Cells.Contains(Location2D.Get(num31, num32)) && Z.GetCell(num31, num32) != null)
										{
											Z.GetCell(num31, num32).AddObject(blueprint6);
										}
									}
								}
							}
							if (num28 == 3)
							{
								int num33 = Stat.Random(20, 98);
								for (int num34 = R.BoundingBox.x1; num34 <= R.BoundingBox.x2; num34++)
								{
									for (int num35 = R.BoundingBox.y1; num35 <= R.BoundingBox.y2; num35++)
									{
										if (R.Cells.Contains(Location2D.Get(num34, num35)) && !buildingPaths.Contains(Location2D.Get(num34, num35)) && Stat.Random(1, 100) <= num33)
										{
											Z.GetCell(num34, num35)?.AddObject(aFarmablePlant2.Blueprint, base.setVillageDomesticatedProperties);
										}
									}
								}
							}
							if (num28 == 4)
							{
								int num36 = Stat.Random(20, 98);
								for (int num37 = R.BoundingBox.x1; num37 <= R.BoundingBox.x2; num37++)
								{
									for (int num38 = R.BoundingBox.y1; num38 <= R.BoundingBox.y2; num38++)
									{
										if (R.Cells.Contains(Location2D.Get(num37, num38)) && !buildingPaths.Contains(Location2D.Get(num37, num38)) && Stat.Random(1, 100) <= num36)
										{
											Z.GetCell(num37, num38)?.AddObject(getAFarmablePlant());
										}
									}
								}
							}
						}
					}
					num18++;
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
					for (int num39 = 0; num39 < list2.Count; num39++)
					{
						string text4 = list2[num39];
						string name;
						if (num39 >= list3.Count)
						{
							Debug.LogWarning("missing immigrant name for " + text4 + " in position " + num39);
							name = "MISSING_NAME";
						}
						else
						{
							name = list3[num39];
						}
						string gender;
						if (num39 >= list4.Count)
						{
							Debug.LogWarning("missing immigrant gender for " + text4 + " in position " + num39);
							gender = null;
						}
						else
						{
							gender = list4[num39];
						}
						string text5;
						if (num39 >= list5.Count)
						{
							Debug.LogWarning("missing immigrant role for " + text4 + " in position " + num39);
							text5 = "villager";
						}
						else
						{
							text5 = list5[num39];
						}
						string whyQ;
						if (num39 >= list6.Count)
						{
							Debug.LogWarning("missing immigrant dialog why Q for " + text4 + " in position " + num39);
							whyQ = "MISSING_QUESTION";
						}
						else
						{
							whyQ = list6[num39];
						}
						string whyA;
						if (num39 >= list7.Count)
						{
							Debug.LogWarning("missing immigrant dialog why A for " + text4 + " in position " + num39);
							whyA = "MISSING_ANSWER";
						}
						else
						{
							whyA = list7[num39];
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
					PlaceObjectInBuilding(gameObject3, buildings[0], If.OneIn(100) ? "Outside" : "Inside");
				}
				else
				{
					PlaceObjectInBuilding(generateMayor(gameObject9, "SpecialVillagerHeroTemplate_" + mayorTemplate), buildings[0], If.OneIn(100) ? "Outside" : "Inside");
				}
				if (gameObject4 != null)
				{
					PlaceObjectInBuilding(gameObject4, buildings[(buildings.Count >= 2) ? 1 : 0], If.OneIn(100) ? "Outside" : "Inside");
				}
				else if (isVillageZero || If.Chance(30))
				{
					PlaceObjectInBuilding(generateMerchant(null), buildings[(buildings.Count >= 2) ? 1 : 0], If.OneIn(100) ? "Outside" : "Inside");
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
				else if (isVillageZero || If.Chance(25))
				{
					gameObject10 = generateTinker();
					flag5 = true;
				}
				if (gameObject10 != null)
				{
					int index;
					string hint;
					if (buildings.Count >= 3)
					{
						index = 2;
						hint = (If.OneIn(50) ? "Outside" : "AlongInsideWall");
					}
					else if (buildings.Count >= 2)
					{
						index = 1;
						hint = (If.OneIn(50) ? "Outside" : "AlongInsideWall");
					}
					else
					{
						index = 0;
						hint = (If.OneIn(50) ? "Outside" : "AlongInsideWall");
					}
					PlaceObjectInBuilding(gameObject10, buildings[index], hint);
					int num40 = 0;
					for (int num41 = Stat.Random(2, 3); num40 < num41; num40++)
					{
						PlaceObjectInBuilding(GameObject.Create("Workbench"), buildings[index], hint);
					}
					int num42 = 0;
					for (int num43 = Stat.Random(0, 2); num42 < num43; num42++)
					{
						PlaceObjectInBuilding(GameObject.Create("Table"), buildings[index], hint);
					}
				}
				GameObject gameObject11 = null;
				bool flag6 = false;
				if (gameObject6 != null)
				{
					gameObject11 = gameObject6;
				}
				else if (isVillageZero || If.Chance(25))
				{
					gameObject11 = generateApothecary();
					flag6 = true;
				}
				if (gameObject11 != null)
				{
					int index2 = ((buildings.Count >= 4) ? 3 : ((buildings.Count >= 3) ? 2 : ((buildings.Count >= 2) ? 1 : 0)));
					string hint2 = (If.OneIn(50) ? "Outside" : "AlongInsideWall");
					PlaceObjectInBuilding(gameObject11, buildings[index2], hint2);
					int num44 = 0;
					for (int num45 = Stat.Random(1, 2); num44 < num45; num44++)
					{
						PlaceObjectInBuilding(GameObject.Create("Table"), buildings[index2], hint2);
					}
					int num46 = 0;
					for (int num47 = Stat.Random(0, 1); num46 < num47; num46++)
					{
						PlaceObjectInBuilding(GameObject.Create("Alchemist Table"), buildings[index2], hint2);
					}
					int num48 = 0;
					for (int num49 = Stat.Random(2, 3); num48 < num49; num48++)
					{
						PlaceObjectInBuilding(GameObject.Create("Woven Basket"), buildings[index2], hint2);
					}
				}
				int num50 = Stat.Random(4, 10);
				if (!isVillageZero)
				{
					if (!flag5)
					{
						num50++;
					}
					if (!flag6)
					{
						num50++;
					}
				}
				if (villageSnapshot.listProperties.ContainsKey("populationMultiplier"))
				{
					foreach (string item3 in villageSnapshot.listProperties["populationMultiplier"])
					{
						num50 *= int.Parse(item3);
					}
				}
				for (int num51 = 0; num51 < num50; num51++)
				{
					ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), generateVillager());
				}
				if (villageSnapshot.GetProperty("government") == "colonialism")
				{
					int num52 = 0;
					for (int num53 = Stat.Random(2, 3); num52 < num53; num52++)
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
					int num54 = 0;
					for (int num55 = Stat.Random(2, 4); num54 < num55; num54++)
					{
						ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), generateMayor(null, "SpecialVillagerHeroTemplate_" + mayorTemplate, GivesRep: false));
					}
				}
				ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), generateVillager(bUnique: true));
			}
			if (villageSnapshot.listProperties.ContainsKey("pet_petSpecies"))
			{
				for (int num56 = 0; num56 < villageSnapshot.listProperties["pet_petSpecies"].Count; num56++)
				{
					try
					{
						List<string> petNames = new List<string>();
						int num57 = int.Parse(villageSnapshot.listProperties["pet_number"][num56]);
						for (int num58 = 0; num58 < num57; num58++)
						{
							string name2;
							GameObject obj2 = generatePet(villageSnapshot.listProperties["pet_petSpecies"][num56], out name2);
							ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), obj2);
							petNames.Add(name2);
						}
						GameObject petSample = GameObject.Create(villageSnapshot.listProperties["pet_petSpecies"][num56]);
						zone.ForeachObjectWithTagOrProperty("Villager", delegate(GameObject o)
						{
							string text6 = HistoricStringExpander.ExpandString("<spice.villages.pet.originStory.!random>", null, null, QudHistoryHelpers.BuildContextFromObjectTextFragments(villageSnapshot.listProperties["pet_petSpecies"][num56]));
							text6 = ((int.Parse(villageSnapshot.listProperties["pet_number"][num56]) != 1) ? text6.Replace("@them@", "them").Replace("@they@", "they").Replace("@they're@", "they're")
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
								AddVillagerConversation(o, o.GetTag("SimpleConversation", "Moon and Sun. Wisdom and will.~May the earth yield for us this season.~Peace, =player.formalAddressTerm=."), "Live and drink.", villageSnapshot.listProperties["pet_dialogWhy_Q"][num56], text6, AppendConversation: true);
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
				int num59 = 1;
				try
				{
					num59 = Convert.ToInt32(villageSnapshot.GetProperty("ruinScale"));
					if (num59 < 1)
					{
						num59 = 1;
					}
					if (num59 > 4)
					{
						num59 = 4;
					}
				}
				catch (Exception ex2)
				{
					Logger.Exception(ex2);
				}
				if (num59 > 1)
				{
					int ruinLevel = 10;
					if (num59 == 3)
					{
						ruinLevel = 50;
					}
					if (num59 == 4)
					{
						ruinLevel = 100;
					}
					new Ruiner().RuinZone(Z, ruinLevel, bUnderground: false);
					foreach (GameObject originalPlant in originalPlants)
					{
						ZoneBuilderSandbox.PlaceObject(originalPlant, Z);
					}
				}
				if (If.Chance(70))
				{
					foreach (GameObject originalCreature in originalCreatures)
					{
						ZoneBuilderSandbox.PlaceObject(originalCreature, Z);
					}
				}
				Z.ReplaceAll("Torchpost", "Unlit Torchpost");
				Z.ReplaceAll("Sconce", "Unlit Torchpost");
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
	}
}
