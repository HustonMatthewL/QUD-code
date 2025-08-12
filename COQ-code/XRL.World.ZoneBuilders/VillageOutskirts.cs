using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using UnityEngine;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders
{
	public class VillageOutskirts : VillageBase
	{
		private string[] staticPerVillage = new string[1] { "*WhateverTagsYouWant" };

		private string[] staticPerBuilding = new string[1] { "*LightSource" };

		private Dictionary<string, string> staticVillageResults = new Dictionary<string, string>();

		public List<PopulationResult> ResolveBuildingContents(List<PopulationResult> templateResults)
		{
			int chance = 0;
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
					if (staticVillageResults.ContainsKey(templateResult.Blueprint) && !Stat.Chance(chance))
					{
						list.Add(new PopulationResult(staticVillageResults[templateResult.Blueprint]));
						continue;
					}
					if (dictionary.ContainsKey(templateResult.Blueprint) && !Stat.Chance(chance))
					{
						list.Add(new PopulationResult(dictionary[templateResult.Blueprint]));
						continue;
					}
					PopulationResult populationResult = new PopulationResult(null);
					string populationName = "DynamicSemanticTable:" + templateResult.Blueprint.Replace("*", "") + "::" + villageTechTier;
					populationResult.Blueprint = PopulationManager.RollOneFrom(populationName).Blueprint;
					populationResult.Hint = templateResult.Hint;
					if (string.IsNullOrEmpty(populationResult.Blueprint))
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
				if (!string.IsNullOrEmpty(text3))
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

		public bool BuildZone(Zone Z)
		{
			zone = Z;
			zone.SetZoneProperty("relaxedbiomes", "true");
			villageSnapshot = base.villageEntity.GetCurrentSnapshot();
			region = villageSnapshot.GetProperty("region");
			villagerBaseFaction = villageSnapshot.GetProperty("baseFaction");
			villageName = villageSnapshot.GetProperty("name");
			Z.SetZoneProperty("villageEntityId", base.villageEntity.id);
			isVillageZero = villageSnapshot.GetProperty("isVillageZero", "false") == "true";
			Tier.Constrain(ref villageTier);
			generateVillageTheme();
			generateSignatureItems();
			generateSignatureDish();
			generateSignatureLiquid();
			generateSignatureSkill();
			generateStoryType();
			getVillageDoorStyle();
			makeSureThereIsEnoughSpace();
			foreach (Cell cell2 in Z.GetCells())
			{
				for (int num = cell2.Objects.Count - 1; num >= 0; num--)
				{
					GameObject gameObject = cell2.Objects[num];
					if (!gameObject.IsPlayer() && !gameObject.HasTagOrProperty("NoVillageStrip"))
					{
						if (gameObject.HasPart<Combat>())
						{
							gameObject.Physics.CurrentCell = null;
							originalCreatures.Add(gameObject);
						}
						else if (gameObject.IsWall() && gameObject.HasTag("Category_Settlement"))
						{
							gameObject.Physics.CurrentCell = null;
							originalWalls.Add(gameObject);
						}
						else if (gameObject.GetBlueprint().InheritsFrom("Plant") || gameObject.GetBlueprint().InheritsFrom("BasePlant") || gameObject.HasTagOrProperty("PlantLike"))
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
			if (1.in100())
			{
				addInitialStructures();
			}
			InfluenceMap regionMap = new InfluenceMap(Z.Width, Z.Height);
			for (int i = 0; i < Z.Width; i++)
			{
				for (int j = 0; j < Z.Height; j++)
				{
					regionMap.Walls[i, j] = (Z.GetCell(i, j).HasObjectWithTagOrProperty("Wall") ? 1 : 0);
				}
			}
			regionMap.SeedAllUnseeded();
			while (regionMap.LargestSize() > 150)
			{
				regionMap.AddSeedAtMaximaInLargestSeed();
			}
			regionMap.SeedGrowthProbability = new List<int>();
			for (int k = 0; k < regionMap.Seeds.Count; k++)
			{
				regionMap.SeedGrowthProbability.Add(Stat.Random(10, 1000));
			}
			regionMap.Recalculate();
			int num2 = Stat.Random(4, 9);
			int num3 = 0;
			int num4 = regionMap.FindClosestSeedTo(Location2D.Get(40, 13), (InfluenceMapRegion r) => r.maxRect.ReduceBy(1, 1).Width >= 6 && r.maxRect.ReduceBy(1, 1).Height >= 6 && r.AdjacentRegions.Count > 1);
			Location2D location2D = regionMap.Seeds[num4];
			townSquare = regionMap.Regions[num4];
			townSquareLayout = null;
			int num5 = Stat.Random(0, 3);
			foreach (InfluenceMapRegion region in regionMap.Regions)
			{
				if (num5 <= 0)
				{
					break;
				}
				num5--;
				Rect2D Rect = GridTools.MaxRectByArea(region.GetGrid()).Translate(region.BoundingBox.UpperLeft).ReduceBy(1, 1);
				PopulationLayout populationLayout = new PopulationLayout(Z, region, Rect);
				if (region.AdjacentRegions.Count <= 1 && region.Size >= 9 && !region.IsEdgeRegion() && region != townSquare)
				{
					buildings.Add(populationLayout);
				}
				else if ((Rect.Width >= 6 && Rect.Height >= 6 && num2 > 0) || region == townSquare)
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
					}
					if (text == "walledpond")
					{
						FabricateWalledPond(populationLayout, liquidBlueprint);
					}
					if (text == "walledislandpond")
					{
						FabricateWalledIslandPond(populationLayout, liquidBlueprint);
					}
					if (text == "tent")
					{
						FabricateTent(populationLayout);
					}
					if (text == "roundhut")
					{
						FabricateHut(populationLayout, isRound: true);
					}
					if (text == "squarehut")
					{
						FabricateHut(populationLayout, isRound: false);
					}
					if (text.StartsWith("wfc,"))
					{
						getWfcBuildingTemplate(text.Split(',')[1]).ShuffleInPlace();
						bool flag = false;
						foreach (ColorOutputMap item in getWfcBuildingTemplate("huts"))
						{
							int num6 = item.width / 2;
							int num7 = item.height / 2;
							if (item.extrawidth > populationLayout.innerRect.Width || item.extraheight > populationLayout.innerRect.Height)
							{
								continue;
							}
							for (int l = 0; l < item.width; l++)
							{
								for (int m = 0; m < item.height; m++)
								{
									Cell cell = Z.GetCell(populationLayout.position.X - num6 + l, populationLayout.position.Y - num7 + m);
									if (cell != null && ColorExtensionMethods.Equals(item.getPixel(l, m), ColorOutputMap.BLACK))
									{
										cell.AddObject(getAVillageWall());
									}
								}
							}
							flag = true;
							break;
						}
						if (!flag)
						{
							FabricateHut(populationLayout, isRound: false);
						}
					}
					num2--;
					num3++;
				}
				else if (region.AdjacentRegions.Count == 1 && !region.IsEdgeRegion() && townSquare != region)
				{
					VillageBase.MakeCaveBuilding(Z, regionMap, region);
					buildings.Add(populationLayout);
				}
			}
			regionMap.SeedAllUnseeded();
			CarvePathwaysFromLocations(Z, bCarveDoors: true, regionMap, location2D);
			zone.ClearReachableMap(bValue: false);
			zone.BuildReachableMap(location2D.X, location2D.Y);
			SnakeToConnections(Location2D.Get(location2D.X, location2D.Y));
			clearDegenerateDoors();
			applyDoorFilters();
			for (int n = 0; n < Z.Width; n++)
			{
				for (int num8 = 0; num8 < Z.Height; num8++)
				{
					regionMap.Walls[n, num8] = (Z.GetCell(n, num8).HasObjectWithTag("Wall") ? 1 : 0);
				}
			}
			List<Location2D> list = new List<Location2D>();
			foreach (PopulationLayout building2 in buildings)
			{
				regionMap.Seeds[building2.region.Seed] = building2.position;
				list.Add(building2.position);
			}
			regionMap.Recalculate();
			InfluenceMap influenceMap = regionMap.copy();
			using (Pathfinder pathfinder = zone.getPathfinder())
			{
				NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 4, 80, 80, 6, 3, -3, 1, new List<NoiseMapNode>());
				for (int num9 = 0; num9 < zone.Height; num9++)
				{
					for (int num10 = 0; num10 < zone.Width; num10++)
					{
						if (zone.GetCell(num10, num9).HasWall())
						{
							pathfinder.CurrentNavigationMap[num10, num9] = 999;
						}
						else
						{
							pathfinder.CurrentNavigationMap[num10, num9] = noiseMap.Noise[num10, num9];
						}
					}
				}
				foreach (PopulationLayout building3 in buildings)
				{
					foreach (Location2D cell3 in building3.region.Cells)
					{
						int x = cell3.X;
						int y = cell3.Y;
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
							if (burrowedDoors.Contains(Location2D.Get(x, y)))
							{
								influenceMap.Walls[x, y] = 1;
							}
						}
					}
				}
				influenceMap.Recalculate();
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
							if (!string.IsNullOrEmpty(text2))
							{
								zone.GetCell(step.pos).AddObject(text2);
							}
							if (!buildingPaths.Contains(step.pos))
							{
								buildingPaths.Add(step.pos);
							}
						}
					}
					foreach (Location2D cell4 in building4.region.Cells)
					{
						if (Z.GetCell(cell4).HasWall() || buildingPaths.Contains(cell4))
						{
							continue;
						}
						if (influenceMap.Regions.Count() <= building4.region.Seed)
						{
							MetricsManager.LogEditorError("village insideOutMap", "insideOutMap didn't have seed");
							building4.outside.Add(cell4);
							int num13 = Z.GetCell(cell4).CountObjectWithTagCardinalAdjacent("Wall");
							if (num13 > 0)
							{
								building4.outsideWall.Add(cell4);
							}
							if (num13 >= 2)
							{
								building4.outsideCorner.Add(cell4);
							}
							continue;
						}
						if (!influenceMap.Regions[building4.region.Seed].Cells.Contains(cell4))
						{
							building4.outside.Add(cell4);
							int num14 = Z.GetCell(cell4).CountObjectWithTagCardinalAdjacent("Wall");
							if (num14 > 0)
							{
								building4.outsideWall.Add(cell4);
							}
							if (num14 >= 2)
							{
								building4.outsideCorner.Add(cell4);
							}
							continue;
						}
						building4.inside.Add(cell4);
						if (!string.IsNullOrEmpty(blueprint))
						{
							Z.GetCell(cell4).AddObject(blueprint);
						}
						int num15 = Z.GetCell(cell4).CountObjectWithTagCardinalAdjacent("Wall");
						if (num15 > 0)
						{
							building4.insideWall.Add(cell4);
						}
						if (num15 >= 2)
						{
							building4.insideCorner.Add(cell4);
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
			int num16 = 0;
			for (int num17 = 0; num17 < influenceMap2.Regions.Count; num17++)
			{
				InfluenceMapRegion R = influenceMap2.Regions[num17];
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
				if (num17 == num4)
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
					if (num16 == 0 && signatureHistoricObjectInstance != null)
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
						int num18 = 20;
						if (blueprint3 == "farm" && value.Area >= num18 && value.Width >= 6 && value.Height <= 6)
						{
							value = value.ReduceBy(1, 1).Clamp(1, 1, 78, 23);
							if (value.Width <= 5 || value.Height <= 5)
							{
								continue;
							}
							Location2D location = value.GetRandomDoorCell().location;
							ZoneBuilderSandbox.PlaceObjectOnRect(Z, "BrinestalkFence", value);
							GetCell(Z, location).Clear();
							GetCell(Z, location).AddObject("Brinestalk Gate");
							string cellSide = value.GetCellSide(location.Point);
							Rect2D r2 = value.ReduceBy(0, 0);
							int num19 = 0;
							if (cellSide == "N")
							{
								num19 = ((Stat.Random(0, 1) == 0) ? 2 : 3);
							}
							if (cellSide == "S")
							{
								num19 = ((Stat.Random(0, 1) != 0) ? 1 : 0);
							}
							if (cellSide == "E")
							{
								num19 = ((Stat.Random(0, 1) != 0) ? 3 : 0);
							}
							if (cellSide == "W")
							{
								num19 = ((Stat.Random(0, 1) == 0) ? 1 : 2);
							}
							if (num19 == 0 || num19 == 1)
							{
								r2.y2 = r2.y1 + 3;
							}
							else
							{
								r2.y1 = r2.y2 - 3;
							}
							if (num19 == 0 || num19 == 3)
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
							ZoneBuilderSandbox.PlacePopulationInRect(Z, value.ReduceBy(1, 1), ResolvePopulationTableName("Villages_FarmAnimals"));
							ZoneBuilderSandbox.PlacePopulationInRect(Z, r2.ReduceBy(1, 1), ResolvePopulationTableName("Villages_FarmHutContents"));
						}
						else if (blueprint3 == "garden" || blueprint3 == "farm")
						{
							int num20 = Stat.Random(1, 4);
							GameObject aFarmablePlant = getAFarmablePlant();
							if (num20 == 1)
							{
								string blueprint4 = originalLiquids?.GetRandomElement()?.Blueprint ?? "SaltyWaterPuddle";
								bool flag2 = Stat.Random(1, 100) <= 33;
								for (int num21 = R.BoundingBox.x1; num21 <= R.BoundingBox.x2; num21++)
								{
									for (int num22 = R.BoundingBox.x1; num22 <= R.BoundingBox.x2; num22++)
									{
										if (num21 % 2 == 0)
										{
											if (R.Cells.Contains(Location2D.Get(num21, num22)) && !buildingPaths.Contains(Location2D.Get(num21, num22)))
											{
												Z.GetCell(num21, num22)?.AddObject(aFarmablePlant.Blueprint, base.setVillageDomesticatedProperties);
											}
										}
										else if (flag2 && R.Cells.Contains(Location2D.Get(num21, num22)) && Z.GetCell(num21, num22) != null)
										{
											Z.GetCell(num21, num22).AddObject(blueprint4);
										}
									}
								}
							}
							if (num20 == 2)
							{
								string blueprint5 = originalLiquids?.GetRandomElement()?.Blueprint ?? "SaltyWaterPuddle";
								bool flag3 = Stat.Random(1, 100) <= 33;
								for (int num23 = R.BoundingBox.x1; num23 <= R.BoundingBox.x2; num23++)
								{
									for (int num24 = R.BoundingBox.x1; num24 <= R.BoundingBox.x2; num24++)
									{
										if (num24 % 2 == 0)
										{
											if (R.Cells.Contains(Location2D.Get(num23, num24)) && !buildingPaths.Contains(Location2D.Get(num23, num24)))
											{
												Z.GetCell(num23, num24)?.AddObject(aFarmablePlant.Blueprint, base.setVillageDomesticatedProperties);
											}
										}
										else if (flag3 && R.Cells.Contains(Location2D.Get(num23, num24)) && Z.GetCell(num23, num24) != null)
										{
											Z.GetCell(num23, num24).AddObject(blueprint5);
										}
									}
								}
							}
							if (num20 == 3)
							{
								int num25 = Stat.Random(20, 98);
								for (int num26 = R.BoundingBox.x1; num26 <= R.BoundingBox.x2; num26++)
								{
									for (int num27 = R.BoundingBox.x1; num27 <= R.BoundingBox.x2; num27++)
									{
										if (R.Cells.Contains(Location2D.Get(num26, num27)) && !buildingPaths.Contains(Location2D.Get(num26, num27)) && Stat.Random(1, 100) <= num25)
										{
											Z.GetCell(num26, num27)?.AddObject(aFarmablePlant.Blueprint, base.setVillageDomesticatedProperties);
										}
									}
								}
							}
							if (num20 == 4)
							{
								int num28 = Stat.Random(20, 98);
								for (int num29 = R.BoundingBox.x1; num29 <= R.BoundingBox.x2; num29++)
								{
									for (int num30 = R.BoundingBox.x1; num30 <= R.BoundingBox.x2; num30++)
									{
										if (R.Cells.Contains(Location2D.Get(num29, num30)) && !buildingPaths.Contains(Location2D.Get(num29, num30)) && Stat.Random(1, 100) <= num28)
										{
											Z.GetCell(num29, num30)?.AddObject(getAFarmablePlant());
										}
									}
								}
							}
						}
					}
					num16++;
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
			buildings.RemoveAll((PopulationLayout b) => b.inside.Count == 0 && b.outside.Count == 0);
			if (villageSnapshot.GetProperty("abandoned") == "true")
			{
				int num31 = 1;
				try
				{
					num31 = Convert.ToInt32(villageSnapshot.GetProperty("ruinScale"));
					if (num31 < 1)
					{
						num31 = 1;
					}
					if (num31 > 4)
					{
						num31 = 4;
					}
				}
				catch (Exception ex)
				{
					Logger.Exception(ex);
				}
				if (num31 > 1)
				{
					int ruinLevel = 10;
					if (num31 == 3)
					{
						ruinLevel = 50;
					}
					if (num31 == 4)
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
			else
			{
				int num32 = Stat.Random(0, 4);
				for (int num33 = 0; num33 < num32; num33++)
				{
					ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), generateVillager());
				}
				Z.ForeachObjectWithPart("SecretObject", delegate(GameObject obj)
				{
					obj.RemovePart<SecretObject>();
				});
				Z.ForeachObjectWithPart("AIPilgrim", delegate(GameObject obj)
				{
					obj.RemovePart<AIPilgrim>();
				});
				Z.ForeachObjectWithPart("ConvertSpawner", delegate(GameObject obj)
				{
					obj.RemovePart<ConvertSpawner>();
				});
				foreach (GameObject item3 in originalFurniture)
				{
					ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), item3);
				}
				foreach (GameObject originalItem in originalItems)
				{
					ZoneBuilderSandbox.PlaceObject(Z, regionMap.Regions.GetRandomElement(), originalItem);
				}
				Z.ForeachObject(delegate(GameObject o)
				{
					if (o.GetBlueprint().InheritsFrom("Furniture") && o.Physics != null)
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
				});
			}
			if (Z.HasBuilder("RiverBuilder"))
			{
				new RiverBuilder(hardClear: false, originalLiquids?.GetRandomElement()?.Blueprint ?? "SaltyWaterDeepPool").BuildZone(Z);
			}
			if (Z.HasBuilder("RoadBuilder"))
			{
				new RoadBuilder(HardClear: false).BuildZone(Z);
			}
			string damageChance = ((villageSnapshot.GetProperty("abandoned") == "true") ? Stat.Random(5, 25).ToString() : (10 - villageTechTier).ToString());
			PowerGrid powerGrid = new PowerGrid();
			powerGrid.DamageChance = damageChance;
			if ((5 + villageTechTier * 2).in100())
			{
				powerGrid.MissingConsumers = "1d6";
				powerGrid.MissingProducers = "1d3";
			}
			powerGrid.BuildZone(Z);
			Hydraulics hydraulics = new Hydraulics();
			hydraulics.DamageChance = damageChance;
			if ((5 + villageTechTier * 2).in100())
			{
				hydraulics.MissingConsumers = "1d6";
				hydraulics.MissingProducers = "1d3";
			}
			hydraulics.BuildZone(Z);
			MechanicalPower mechanicalPower = new MechanicalPower();
			mechanicalPower.DamageChance = damageChance;
			if ((15 - villageTechTier).in100())
			{
				mechanicalPower.MissingConsumers = "1d6";
				mechanicalPower.MissingProducers = "1d3";
			}
			mechanicalPower.BuildZone(Z);
			Z.SetMusic("Music/Mehmets Book on Strings");
			Z.FireEvent("VillageInit");
			cleanup();
			return true;
		}
	}
}
