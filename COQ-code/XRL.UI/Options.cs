using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModelShark;
using Qud.API;
using Qud.UI;
using UnityEngine;
using XRL.Core;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.UI
{
	[HasModSensitiveStaticCache]
	public class Options
	{
		public enum PlayAreaScaleTypes
		{
			Fit,
			Cover,
			PixelPerfect
		}

		public static NameValueBag Bag;

		public static GameOptions Map;

		public static Dictionary<string, List<GameOption>> OptionsByCategory;

		public static Dictionary<string, GameOption> OptionsByID;

		public static List<OptionValueCacheEntry> ValueCache = new List<OptionValueCacheEntry>();

		private static Dictionary<string, bool> lastOptionsRequirementState;

		public static int _MessageLogLineSizeAdjustment;

		private static readonly Dictionary<string, Action<XmlDataHelper>> _Nodes = new Dictionary<string, Action<XmlDataHelper>>
		{
			{ "options", HandleNodes },
			{ "option", HandleOptionNode }
		};

		public static string OptionPlayScaleOverride = null;

		public static float DockOpacity = 1f;

		private static bool _UseFireSounds;

		public static double StageScale = 1.0;

		public static bool _AutogetPrimitiveAmmo;

		public static bool _AutogetAmmo = false;

		public static bool _AutogetArtifacts;

		public static bool _DisableFullscreenColorEffects;

		public static bool _DisableFullscreenWarpEffects;

		public static bool _UseCombatText;

		public static bool _OptionUseParticleVFX;

		private static bool _InterruptHeldMovement;

		public static bool DisableTextAnimationEffects;

		public static int MessageLogLineSizeAdjustment => _MessageLogLineSizeAdjustment;

		public static int MasterVolume => Convert.ToInt32(GetOption("OptionMasterVolume", "0"));

		public static bool Sound => GetOption("OptionSound").EqualsNoCase("Yes");

		public static int SoundVolume => Convert.ToInt32(GetOption("OptionSoundVolume", "0"));

		public static bool Music => GetOption("OptionMusic").EqualsNoCase("Yes");

		public static bool Ambient => GetOption("OptionAmbient").EqualsNoCase("Yes");

		public static int AmbientVolume => Convert.ToInt32(GetOption("OptionAmbientVolume", "0"));

		public static int InterfaceVolume => Convert.ToInt32(GetOption("OptionInterfaceVolume", "0"));

		public static int CombatVolume => Convert.ToInt32(GetOption("OptionCombatVolume", "0"));

		public static int MusicVolume => Convert.ToInt32(GetOption("OptionMusicVolume", "0"));

		public static bool MusicBackground => GetOption("OptionMusicBackground").EqualsNoCase("Yes");

		public static bool ModernUI => GetOption("OptionModernUI").EqualsNoCase("Yes");

		public static bool ModernCharacterSheet
		{
			get
			{
				if (ModernUI)
				{
					return GetOption("OptionModernCharacterSheet").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static string StageViewID => "Stage";

		public static bool UseTiles => Globals.RenderMode == RenderModeType.Tiles;

		public static PlayAreaScaleTypes PlayScale => ((OptionPlayScaleOverride == null) ? GetOption("OptionPlayScale") : OptionPlayScaleOverride) switch
		{
			"Fit" => PlayAreaScaleTypes.Fit, 
			"Cover" => PlayAreaScaleTypes.Cover, 
			"Pixel Perfect" => PlayAreaScaleTypes.PixelPerfect, 
			_ => PlayAreaScaleTypes.Fit, 
		};

		public static int TileScale
		{
			get
			{
				int result;
				if (PlayScale == PlayAreaScaleTypes.PixelPerfect)
				{
					return (!int.TryParse(GetOption("OptionTileScale"), out result)) ? 1 : result;
				}
				return 0;
			}
		}

		public static int DockMovable => GetOption("OptionDockMovable") switch
		{
			"Flip" => 3, 
			"Right" => 2, 
			"Left" => 1, 
			_ => 0, 
		};

		public static bool ShowErrorPopups => GetOption("OptionShowErrorPopups").EqualsNoCase("Yes");

		public static bool UseCombatSounds => GetOption("OptionUseCombatSounds").EqualsNoCase("Yes");

		public static bool UseInterfaceSounds => GetOption("OptionUseInterfaceSounds").EqualsNoCase("Yes");

		public static bool UseFireSounds => _UseFireSounds;

		public static bool DisplayVignette => GetOption("OptionDisplayVignette").EqualsNoCase("Yes");

		public static bool DisplayScanlines => GetOption("OptionDisplayScanlines").EqualsNoCase("Yes");

		public static bool ShowModSelectionNewGame => GetOption("OptionShowModSelectionNewGame").EqualsNoCase("Yes");

		public static int DisplayBrightness => Convert.ToInt32(GetOption("OptionDisplayBrightness", "0"));

		public static int DisplayContrast => Convert.ToInt32(GetOption("OptionDisplayContrast", "0"));

		public static string FullscreenResolution => GetOption("OptionDisplayResolution");

		public static bool DisplayFullscreen => GetOption("OptionDisplayFullscreen").EqualsNoCase("Yes");

		public static string DisplayFramerate => GetOption("OptionDisplayFramerate");

		public static bool PrereleaseInputManager => true;

		public static int KeyRepeatDelay => Convert.ToInt32(GetOption("OptionKeyRepeatDelay", "10"));

		public static int KeyRepeatRate => Convert.ToInt32(GetOption("OptionKeyRepeatRate", "90"));

		public static bool OverlayUI
		{
			get
			{
				if (ModernUI)
				{
					return GetOption("OptionOverlayUI").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static bool MouseInput
		{
			get
			{
				return GetOption("OptionMouseInput").EqualsNoCase("Yes");
			}
			set
			{
				SetOption("OptionMouseInput", value);
			}
		}

		public static bool MouseMovement => GetOption("OptionMouseMovement").EqualsNoCase("Yes");

		public static bool MouseScrollWheel => GetOption("OptionMouseScrollWheel").EqualsNoCase("Yes");

		public static int MinimapScale => Convert.ToInt32(GetOption("OptionMinimapScale", "0"));

		public static bool OverlayMinimap
		{
			get
			{
				return GetOption("OptionOverlayMinimap").EqualsNoCase("Yes");
			}
			set
			{
				SetOption("OptionOverlayMinimap", value);
			}
		}

		public static bool OverlayNearbyObjects
		{
			get
			{
				return GetOption("OptionOverlayNearbyObjects").EqualsNoCase("Yes");
			}
			set
			{
				SetOption("OptionOverlayNearbyObjects", value);
			}
		}

		public static bool OverlayNearbyObjectsLocal => GetOption("OptionOverlayNearbyObjectsLocal").EqualsNoCase("Yes");

		public static bool OverlayNearbyObjectsTakeable => GetOption("OptionOverlayNearbyObjectsTakeable").EqualsNoCase("Yes");

		public static bool OverlayNearbyObjectsPools => GetOption("OptionOverlayNearbyObjectsPools").EqualsNoCase("Yes");

		public static bool OverlayNearbyObjectsPlants => GetOption("OptionOverlayNearbyObjectsPlants").EqualsNoCase("Yes");

		public static bool UseOverlayCombatEffects => GetOption("OptionUseOverlayCombatEffects").EqualsNoCase("Yes");

		public static bool AutoSip => GetOption("OptionAutoSip").EqualsNoCase("Yes");

		public static string AutoSipLevel => GetOption("OptionAutoSipLevel", "Thirsty");

		public static int AutosaveInterval
		{
			get
			{
				if (!int.TryParse(GetOption("OptionAutosaveInterval", "5"), out var result))
				{
					return int.MaxValue;
				}
				return result;
			}
		}

		public static bool AutoTorch => GetOption("OptionAutoTorch").EqualsNoCase("Yes");

		public static bool AutoDisassembleScrap => GetOption("OptionAutoDisassembleScrap").EqualsNoCase("Yes");

		public static bool ShowScavengeItemAsMessage => GetOption("OptionShowScavengeItemAsMessage").EqualsNoCase("Yes");

		public static bool AutogetPrimitiveAmmo => _AutogetPrimitiveAmmo;

		public static bool AutogetAmmo => _AutogetAmmo;

		public static bool AutogetNuggets => GetOption("OptionAutogetNuggets").EqualsNoCase("Yes");

		public static bool AutogetTradeGoods => GetOption("OptionAutogetTradeGoods").EqualsNoCase("Yes");

		public static bool AutogetFreshWater => GetOption("OptionAutogetFreshWater").EqualsNoCase("Yes");

		public static bool AutogetArtifacts => _AutogetArtifacts;

		public static bool AutogetSpecialItems => GetOption("OptionAutogetSpecialItems").EqualsNoCase("Yes");

		public static bool AutogetScrap => GetOption("OptionAutogetScrap").EqualsNoCase("Yes");

		public static bool AutogetFood => GetOption("OptionAutogetFood").EqualsNoCase("Yes");

		public static bool AutogetBooks => GetOption("OptionAutogetBooks").EqualsNoCase("Yes");

		public static bool AutogetZeroWeight => GetOption("OptionAutogetZeroWeight").EqualsNoCase("Yes");

		public static bool AutogetIfHostiles => GetOption("OptionAutogetIfHostiles").EqualsNoCase("Yes");

		public static bool AutogetFromNearby => GetOption("OptionAutogetFromNearby").EqualsNoCase("Yes");

		public static bool AutogetNoDroppedLiquid => GetOption("OptionAutogetNoDroppedLiquid").EqualsNoCase("Yes");

		public static bool TakeallCorpses => GetOption("OptionTakeallCorpses").EqualsNoCase("Yes");

		public static int AutoexploreRate
		{
			get
			{
				if (!int.TryParse(GetOption("OptionAutoexploreRate", "10"), out var result))
				{
					return 0;
				}
				return result;
			}
		}

		public static int AutoexploreIgnoreEasyEnemies => DifficultyEvaluation.GetDifficultyFromDescription(GetOption("OptionAutoexploreIgnoreEasyEnemies"));

		public static int AutoexploreIgnoreDistantEnemies
		{
			get
			{
				if (!int.TryParse(GetOption("OptionAutoexploreIgnoreDistantEnemies", "None"), out var result))
				{
					return 9999999;
				}
				return result;
			}
		}

		public static bool AutoexploreAttackIgnoredAdjacentEnemies => GetOption("OptionAutoexploreAttackIgnoredAdjacentEnemies").EqualsNoCase("Yes");

		public static bool AutoexploreChests => GetOption("OptionAutoexploreChests").EqualsNoCase("Yes");

		public static bool AskForWorldmap => GetOption("OptionAskForWorldmap").EqualsNoCase("Yes");

		public static bool AskForOneItem => GetOption("OptionAskForOneItem").EqualsNoCase("Yes");

		public static bool AskAutostair => GetOption("OptionAskAutostair").EqualsNoCase("Yes");

		public static bool ConfirmSwimming => GetOption("OptionConfirmSwimming").EqualsNoCase("Yes");

		public static bool ConfirmDangerousLiquid => GetOption("OptionConfirmDangerousLiquid").EqualsNoCase("Yes");

		public static bool DisplayLedLevelUp => GetOption("OptionDisplayLedLevelUp").EqualsNoCase("Yes");

		public static bool PopupJournalNote => !GetOption("OptionPopupJournalNote").EqualsNoCase("No");

		public static bool AlwaysHPColor => GetOption("Option@AlwaysHPColor").EqualsNoCase("Yes");

		public static bool HPColor => GetOption("Option@HPColor").EqualsNoCase("Yes");

		public static bool MutationColor => GetOption("Option@MutationColor").EqualsNoCase("Yes");

		public static bool StripUIColorText => GetOption("OptionStripUIColorText").EqualsNoCase("Yes");

		public static bool ShowSidebarAbilities => GetOption("OptionShowSidebarAbilities").EqualsNoCase("Yes");

		public static bool ShowCurrentCellPopup => GetOption("OptionShowCurrentCellPopup").EqualsNoCase("Yes");

		public static bool ShowDetailedWeaponStats => GetOption("OptionShowDetailedWeaponStats").EqualsNoCase("Yes");

		public static bool ShowMonsterHPHearts => GetOption("OptionShowMonsterHPHearts").EqualsNoCase("Yes");

		public static bool ShiftHidesSidebar => GetOption("OptionShiftHidesSidebar").EqualsNoCase("Yes");

		public static bool ShowNumberOfItems => GetOption("OptionShowNumberOfItems").EqualsNoCase("Yes");

		public static bool DisableFloorTextures => GetOption("OptionDisableFloorTextures").EqualsNoCase("Yes");

		public static bool HighlightStairs => GetOption("OptionHighlightStairs").EqualsNoCase("Yes");

		public static bool LocationIntseadOfName => GetOption("OptionLocationIntseadOfName").EqualsNoCase("Yes");

		public static bool AlphanumericBits => GetOption("OptionAlphanumericBits").EqualsNoCase("Yes");

		public static bool DisableFullscreenColorEffects => _DisableFullscreenColorEffects;

		public static bool DisableFullscreenWarpEffects => _DisableFullscreenWarpEffects;

		public static bool LookLocked
		{
			get
			{
				return GetOption("LookLocked", "Yes").EqualsNoCase("Yes");
			}
			set
			{
				SetOption("LookLocked", value ? "Yes" : "No");
			}
		}

		public static bool PickTargetLocked
		{
			get
			{
				return GetOption("PickTargetLocked", "Yes").EqualsNoCase("Yes");
			}
			set
			{
				SetOption("PickTargetLocked", value ? "Yes" : "No");
			}
		}

		public static bool MapDirectionsToKeypad => GetOption("OptionMapDirectionsToKeypad").EqualsNoCase("Yes");

		public static bool CapInputBuffer => GetOption("OptionCapInputBuffer").EqualsNoCase("Yes");

		public static bool LogTurnSeparator => GetOption("OptionLogTurnSeparator").EqualsNoCase("Yes");

		public static bool IndentBodyParts => GetOption("OptionIndentBodyParts").EqualsNoCase("Yes");

		public static bool AbilityCooldownWarningAsMessage => GetOption("OptionAbilityCooldownWarningAsMessage").EqualsNoCase("Yes");

		public static bool PressingRightInInventoryEquips => GetOption("OptionPressingRightInInventoryEquips").EqualsNoCase("Yes");

		public static bool AllowFramelessZoomOut => GetOption("OptionAllowFramelessZoomOut").EqualsNoCase("Yes");

		public static bool DropAll => GetOption("OptionDropAll").EqualsNoCase("Yes");

		public static bool UseTextAutoactInterruptionIndicator => GetOption("OptionUseTextAutowalkThreatIndicator").EqualsNoCase("Yes");

		public static bool DigOnMove => GetOption("OptionDigOnMove").EqualsNoCase("Yes");

		public static bool EnableMods => GetOption("OptionEnableMods").EqualsNoCase("Yes");

		public static bool AllowCSMods => GetOption("OptionAllowCSMods").EqualsNoCase("Yes");

		public static bool HarmonyDebug => GetOption("OptionHarmonyDebug").EqualsNoCase("Yes");

		public static bool ApproveCSMods => GetOption("OptionApproveCSMods").EqualsNoCase("Yes");

		public static bool OutputModAssembly => GetOption("OptionOutputModAssembly").EqualsNoCase("Yes");

		public static bool DisableCacheCompression => GetOption("OptionDisableCacheCompression").EqualsNoCase("Yes");

		public static bool CacheEarly => GetOption("OptionCacheEarly").EqualsNoCase("Yes");

		public static bool CollectEarly => GetOption("OptionCollectEarly").EqualsNoCase("Yes");

		public static bool DisableFloorTextureObjects => GetOption("OptionDisableFloorTextureObjects").EqualsNoCase("Yes");

		public static bool ThrottleAnimation => GetOption("OptionThrottleAnimation").EqualsNoCase("Yes");

		public static bool Analytics => GetOption("OptionAnalytics").EqualsNoCase("Yes");

		public static bool DisableBloodsplatter => GetOption("OptionDisableBloodsplatter").EqualsNoCase("Yes");

		public static bool DisableSmoke => GetOption("OptionDisableSmoke").EqualsNoCase("Yes");

		public static bool UseCombatText => _UseCombatText;

		public static bool UseParticleVFX
		{
			get
			{
				if (UseImposters)
				{
					return _OptionUseParticleVFX;
				}
				return false;
			}
		}

		public static bool UseImposters => !DisableImposters;

		public static bool DisableImposters => GetOption("OptionDisableImposters").EqualsNoCase("Yes");

		public static bool DisableAchievements => GetOption("OptionDisableAchievements").EqualsNoCase("Yes");

		public static bool CheckMemory => GetOption("OptionCheckMemory").EqualsNoCase("Yes");

		public static bool DrawPopulationHintMaps => GetOption("OptionDrawPopulationHintMaps").EqualsNoCase("Yes");

		public static bool DrawInfluenceMaps => GetOption("OptionDrawInfluenceMaps").EqualsNoCase("Yes");

		public static bool DrawPathfinder => GetOption("OptionDrawPathfinder").EqualsNoCase("Yes");

		public static bool DrawPathfinderHalt => GetOption("OptionDrawPathfinderHalt").EqualsNoCase("Yes");

		public static bool DrawNavigationWeightMaps => GetOption("OptionDrawNavigationWeightMaps").EqualsNoCase("Yes");

		public static bool DrawCASystems => GetOption("OptionDrawCASystems").EqualsNoCase("Yes");

		public static bool DrawFloodVis => GetOption("OptionDrawFloodVis").EqualsNoCase("Yes");

		public static bool DrawFloodAud => GetOption("OptionDrawFloodAud").EqualsNoCase("Yes");

		public static bool DrawFloodOlf => GetOption("OptionDrawFloodOlf").EqualsNoCase("Yes");

		public static bool DrawArcs => GetOption("OptionDrawArcs").EqualsNoCase("Yes");

		public static bool DisablePlayerbrain => GetOption("OptionDisablePlayerbrain").EqualsNoCase("Yes");

		public static bool DisableZoneCaching2 => GetOption("OptionDisableZoneCaching2").EqualsNoCase("Yes");

		public static bool DebugShowConversationNode => GetOption("OptionDebugShowConversationNode").EqualsNoCase("Yes");

		public static bool InterruptHeldMovement => _InterruptHeldMovement;

		public static bool DebugShowFullZoneDuringBuild => GetOption("OptionDebugShowFullZoneDuringBuild").EqualsNoCase("Yes");

		public static bool DebugDamagePenetrations => GetOption("OptionDebugDamagePenetrations").EqualsNoCase("Yes");

		public static bool DebugSavingThrows => GetOption("OptionDebugSavingThrows").EqualsNoCase("Yes");

		public static bool DebugGetLostChance => GetOption("OptionDebugGetLostChance").EqualsNoCase("Yes");

		public static bool DebugStatShift => GetOption("OptionDebugStatShift").EqualsNoCase("Yes");

		public static bool DebugEncounterChance => GetOption("OptionDebugEncounterChance").EqualsNoCase("Yes");

		public static bool DebugTravelSpeed => GetOption("OptionDebugTravelSpeed").EqualsNoCase("Yes");

		public static bool DebugInternals => GetOption("OptionDebugInternals").EqualsNoCase("Yes");

		public static bool InventoryConsistencyCheck => GetOption("OptionInventoryConsistencyCheck").EqualsNoCase("Yes");

		public static bool ShowReachable => GetOption("OptionShowReachable").EqualsNoCase("Yes");

		public static bool ShowOverlandEncounters => GetOption("OptionShowOverlandEncounters").EqualsNoCase("Yes");

		public static bool ShowOverlandRegions => GetOption("OptionShowOverlandRegions").EqualsNoCase("Yes");

		public static bool ShowQuickstartOption => GetOption("OptionShowQuickstart").EqualsNoCase("Yes");

		public static bool AllowReallydie => GetOption("OptionAllowReallydie").EqualsNoCase("Yes");

		public static bool AllowSaveLoad => GetOption("OptionAllowSaveLoad").EqualsNoCase("Yes");

		public static bool DisablePermadeath => GetOption("OptionDisablePermadeath").EqualsNoCase("Yes");

		public static bool EnablePrereleaseContent => GetOption("OptionEnablePrereleaseContent").EqualsNoCase("Yes");

		public static bool EnableWishRegionNames => GetOption("OptionEnableWishRegionNames").EqualsNoCase("Yes");

		public static bool DisableTryLimit => GetOption("OptionDisableTryLimit").EqualsNoCase("Yes");

		public static bool DisableDefectLimit => GetOption("OptionDisableDefectLimit").EqualsNoCase("Yes");

		public static bool GivesRepShowsCurrentRep => GetOption("OptionGivesRepShowsCurrentRep").EqualsNoCase("Yes");

		public static bool SifrahExamine
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahExamine").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static string SifrahExamineAuto
		{
			get
			{
				string text = GetOption("OptionSifrahExamineAuto");
				if (text.EqualsNoCase("Yes"))
				{
					text = "Always";
				}
				else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
				{
					text = "Ask";
				}
				return text;
			}
		}

		public static bool SifrahRepair
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahRepair").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static string SifrahRepairAuto
		{
			get
			{
				string text = GetOption("OptionSifrahRepairAuto");
				if (text.EqualsNoCase("Yes"))
				{
					text = "Always";
				}
				else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
				{
					text = "Ask";
				}
				return text;
			}
		}

		public static bool SifrahReverseEngineer
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahReverseEngineer").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static bool SifrahDisarming
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahDisarming").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static string SifrahDisarmingAuto
		{
			get
			{
				string text = GetOption("OptionSifrahDisarmingAuto");
				if (text.EqualsNoCase("Yes"))
				{
					text = "Always";
				}
				else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
				{
					text = "Ask";
				}
				return text;
			}
		}

		public static bool SifrahHaggling
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahHaggling").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static bool SifrahRecruitment
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahRecruitment").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static string SifrahRecruitmentAuto
		{
			get
			{
				string text = GetOption("OptionSifrahRecruitmentAuto");
				if (text.EqualsNoCase("Yes"))
				{
					text = "Always";
				}
				else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
				{
					text = "Ask";
				}
				return text;
			}
		}

		public static bool SifrahHacking
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahHacking").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static string SifrahHackingAuto
		{
			get
			{
				string text = GetOption("OptionSifrahHackingAuto");
				if (text.EqualsNoCase("Yes"))
				{
					text = "Always";
				}
				else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
				{
					text = "Ask";
				}
				return text;
			}
		}

		public static string SifrahHackingLowLevel
		{
			get
			{
				string text = GetOption("OptionSifrahHackingLowLevel");
				if (text.EqualsNoCase("Yes"))
				{
					text = "Always";
				}
				else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
				{
					text = "Ask";
				}
				return text;
			}
		}

		public static bool SifrahItemNaming
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahItemNaming").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static string SifrahItemModding
		{
			get
			{
				if (!SifrahGame.Installed)
				{
					return "Never";
				}
				string text = GetOption("OptionSifrahItemModding");
				if (text.IsNullOrEmpty())
				{
					text = "Never";
				}
				return text;
			}
		}

		public static bool SifrahRealityDistortion
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahRealityDistortion").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static string SifrahRealityDistortionAuto
		{
			get
			{
				string text = GetOption("OptionSifrahRealityDistortionAuto");
				if (text.EqualsNoCase("Yes"))
				{
					text = "Always";
				}
				else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
				{
					text = "Ask";
				}
				return text;
			}
		}

		public static bool SifrahPsychicCombat
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahPsychicCombat").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static string SifrahPsychicCombatAuto
		{
			get
			{
				string text = GetOption("OptionSifrahPsychicCombatAuto");
				if (text.EqualsNoCase("Yes"))
				{
					text = "Always";
				}
				else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
				{
					text = "Ask";
				}
				return text;
			}
		}

		public static string SifrahWaterRitual
		{
			get
			{
				if (!SifrahGame.Installed)
				{
					return "Never";
				}
				string text = GetOption("OptionSifrahWaterRitual");
				if (text.IsNullOrEmpty())
				{
					text = "Never";
				}
				return text;
			}
		}

		public static bool SifrahBaetylOfferings
		{
			get
			{
				if (SifrahGame.Installed)
				{
					return GetOption("OptionSifrahBaetylOfferings").EqualsNoCase("Yes");
				}
				return false;
			}
		}

		public static bool AnySifrah
		{
			get
			{
				if (SifrahGame.Installed)
				{
					if (!SifrahExamine && !SifrahRepair && !SifrahReverseEngineer && !SifrahDisarming && !SifrahHaggling && !SifrahRecruitment && !SifrahHacking && !SifrahItemNaming && !(SifrahItemModding != "Never") && !SifrahRealityDistortion && !SifrahPsychicCombat && !(SifrahWaterRitual != "Never"))
					{
						return SifrahBaetylOfferings;
					}
					return true;
				}
				return false;
			}
		}

		public static void SetOption(string ID, bool Value)
		{
			SetOption(ID, Value ? "Yes" : "No");
		}

		public static void SetOption(string ID, string Value)
		{
			lock (ValueCache)
			{
				Bag.SetValue(ID, Value);
				UpdateValueCacheEntry(ID, Value);
				UpdateFlags();
			}
		}

		public static void AddValueCache(string ID, string Value)
		{
			lock (ValueCache)
			{
				ValueCache.Add(new OptionValueCacheEntry(ID, Value));
			}
		}

		public static void UpdateValueCacheEntry(string ID, string Value)
		{
			for (int i = 0; i < ValueCache.Count; i++)
			{
				if (ValueCache[i].Key == ID)
				{
					ValueCache[i].Value = Value;
					return;
				}
			}
			AddValueCache(ID, Value);
		}

		public static bool HasOption(string ID)
		{
			lock (ValueCache)
			{
				for (int i = 0; i < ValueCache.Count; i++)
				{
					if (ValueCache[i].Key == ID)
					{
						return true;
					}
				}
				if (OptionsByID.TryGetValue(ID, out var _))
				{
					return true;
				}
				return false;
			}
		}

		public static string GetOption(string ID, string Default = "")
		{
			string text = TutorialManager.OverrideOption(ID);
			if (text != null)
			{
				return text;
			}
			if (Bag == null)
			{
				Debug.LogWarning("accessign options pre-init: " + ID);
				return Default;
			}
			lock (ValueCache)
			{
				for (int i = 0; i < ValueCache.Count; i++)
				{
					if (ValueCache[i].Key == ID)
					{
						return ValueCache[i].Value;
					}
				}
				string value = Bag.GetValue(ID);
				if (value != null)
				{
					UpdateValueCacheEntry(ID, value);
					return value;
				}
				if (OptionsByID.TryGetValue(ID, out var value2))
				{
					UpdateValueCacheEntry(ID, value2.Default);
					return value2.Default;
				}
				UpdateValueCacheEntry(ID, Default);
				return Default;
			}
		}

		public static bool GetOptionBool(string ID)
		{
			return GetOption(ID).EqualsNoCase("Yes");
		}

		public static bool ShouldCheckRequirements()
		{
			if (lastOptionsRequirementState == null)
			{
				lastOptionsRequirementState = new Dictionary<string, bool>();
			}
			bool result = false;
			foreach (KeyValuePair<string, GameOption> item in OptionsByID)
			{
				bool flag = item.Value.Requires?.RequirementsMet ?? true;
				if (lastOptionsRequirementState.TryGetValue(item.Key, out var value))
				{
					if (value != flag)
					{
						result = true;
						lastOptionsRequirementState.Set(item.Key, flag);
					}
				}
				else
				{
					result = true;
					lastOptionsRequirementState.Set(item.Key, flag);
				}
			}
			return result;
		}

		public static void UpdateFlags()
		{
			string option = GetOption("OptionPrereleaseStageScale", "1.0");
			double result = 1.0;
			if (option.StartsWith("auto"))
			{
				if (option == "auto x1.25")
				{
					result = 1.25;
				}
				if (option == "auto x1.5")
				{
					result = 1.5;
				}
				StageScale = Math.Min((double)Screen.width * result / 1920.0, (double)Screen.height * result / 640.0);
			}
			else if (double.TryParse(option, out result))
			{
				StageScale = result;
			}
			else
			{
				StageScale = 1.0;
			}
			ObjectFinder.instance?.ReadOptions();
			SingletonWindowBase<MouseBlocker>.instance?.UpdateOptions();
			CursorManager.instance?.UpdateOptions();
			if (CapInputBuffer)
			{
				GameManager.bCapInputBuffer = true;
			}
			else
			{
				GameManager.bCapInputBuffer = false;
			}
			_AutogetAmmo = GetOption("OptionAutogetAmmo").EqualsNoCase("Yes");
			_AutogetArtifacts = GetOption("OptionAutogetArtifacts").EqualsNoCase("Yes");
			_AutogetPrimitiveAmmo = GetOption("OptionAutogetPrimitiveAmmo").EqualsNoCase("Yes");
			_DisableFullscreenColorEffects = GetOption("OptionDisableFullscreenColorEffects").EqualsNoCase("Yes");
			_DisableFullscreenWarpEffects = GetOption("OptionDisableFullscreenWarpEffects").EqualsNoCase("Yes");
			_UseFireSounds = GetOption("OptionPlayFireSounds").EqualsNoCase("Yes");
			_UseCombatText = GetOption("OptionUseOverlayDamageText").EqualsNoCase("Yes");
			_OptionUseParticleVFX = GetOption("OptionUseParticleVFX").EqualsNoCase("Yes");
			int.TryParse(GetOption("OptionMessageLineLogScale"), out _MessageLogLineSizeAdjustment);
			try
			{
				DockOpacity = (float)Convert.ToInt32(GetOption("OptionDockOpacity", "100")) / 100f;
			}
			catch
			{
				DockOpacity = 1f;
			}
			_InterruptHeldMovement = GetOption("OptionInterruptHeldMovement").EqualsNoCase("Yes");
			if (ModernUI)
			{
				GameManager.Instance.ModernUI = true;
			}
			else
			{
				GameManager.Instance.ModernUI = false;
			}
			if (GetOption("OptionUseTiles").EqualsNoCase("Yes"))
			{
				Globals.RenderMode = RenderModeType.Tiles;
			}
			else
			{
				Globals.RenderMode = RenderModeType.Text;
			}
			if (Analytics)
			{
				Globals.EnableMetrics = true;
			}
			else
			{
				Globals.EnableMetrics = false;
			}
			if (Sound)
			{
				Globals.EnableSound = true;
			}
			else
			{
				Globals.EnableSound = false;
			}
			if (Music)
			{
				Globals.EnableMusic = true;
			}
			else
			{
				Globals.EnableMusic = false;
			}
			if (Ambient)
			{
				Globals.EnableAmbient = true;
			}
			else
			{
				Globals.EnableAmbient = false;
			}
			Globals.AmbientVolume = (float)AmbientVolume / 100f * 0.5f;
			Globals.InterfaceVolume = (float)InterfaceVolume / 100f;
			Globals.CombatVolume = (float)CombatVolume / 100f;
			if (int.TryParse(GetOption("OptionDisplayHPWarning", "40%").TrimEnd('%'), out var result2))
			{
				Globals.HPWarningThreshold = result2;
			}
			else
			{
				Globals.HPWarningThreshold = int.MinValue;
			}
			if (MouseInput)
			{
				GameManager.Instance.MouseInput = true;
			}
			else
			{
				GameManager.Instance.MouseInput = false;
			}
			SoundManager.WriteSoundsToLog = GetOption("OptionWriteSoundsToLog").EqualsNoCase("Yes");
			AchievementManager.Enabled = !DisableAchievements;
			int masterVolume = MasterVolume;
			int musicVolume = MusicVolume;
			int soundVolume = SoundVolume;
			GameManager.Instance.compassScale = (float)Convert.ToInt32(GetOption("OptionOverlayCompassScale", "100")) / 100f;
			GameManager.Instance.nearbyObjectsListScale = (float)Convert.ToInt32(GetOption("OptionOverlayNearbyObjectsScale", "100")) / 100f;
			GameManager.Instance.minimapScale = (float)Convert.ToInt32(GetOption("OptionMinimapScale", "100")) / 100f;
			GameManager.Instance.TileScale = TileScale;
			GameManager.Instance.StageScale = StageScale;
			GameManager.Instance.DockMovable = DockMovable;
			GameManager.Instance.DisplayMinimap = OverlayMinimap;
			int result3 = 2000;
			int.TryParse(GetOption("OptionTooltipDelay"), out result3);
			TooltipManager.Instance.tooltipDelay = (float)result3 / 1000f;
			BaseLineWithTooltip.TOOLTIP_DELAY = (float)result3 / 1000f;
			lock (SoundManager.SoundRequests)
			{
				SoundManager.MasterVolume = (float)masterVolume / 100f;
				SoundManager.MusicVolume = (float)musicVolume / 100f;
				SoundManager.SoundVolume = (float)soundVolume / 100f;
				SoundManager.MusicSources.StopFadeAsync();
			}
			float num = (float)Convert.ToInt32(GetOption("OptionKeyRepeatDelay")) / 100f;
			float num2 = (float)Convert.ToInt32(GetOption("OptionKeyRepeatRate")) / 100f;
			ControlManager.delaytime = 0.1f + 2f * num;
			ControlManager.repeattime = 0f + 0.2f * (1f - num2);
			ControlManager.updateFont = true;
			ZoneManager.ZoneTransitionSaveInterval = AutosaveInterval;
			Leveler.PlayerLedPrompt = DisplayLedLevelUp;
			IBaseJournalEntry.NotedPrompt = PopupJournalNote;
			DisableTextAnimationEffects = GetOption("OptionDisableTextAnimationEffects").EqualsNoCase("Yes");
			if (ModManager.Initialized)
			{
				foreach (MethodInfo item in ModManager.GetMethodsWithAttribute(typeof(OptionFlagUpdate), typeof(HasOptionFlagUpdate)))
				{
					try
					{
						item.Invoke(null, Array.Empty<object>());
					}
					catch (Exception arg)
					{
						MetricsManager.LogAssemblyError(item, $"Error invoking {item.DeclaringType.FullName}.{item.Name}: {arg}");
					}
				}
			}
			CursorManager.instance?.Sync();
			GameManager.Instance.uiQueue.queueTask(delegate
			{
				UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_AnalogTV>().enabled = DisplayScanlines;
				UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_FastVignette>().enabled = DisplayVignette;
				UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = Math.Max(-70, DisplayBrightness);
				UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().contrast = Math.Max(-70, DisplayContrast);
				if (DisplayFullscreen)
				{
					string text = FullscreenResolution;
					if (text == "*Max")
					{
						Resolution resolution = GameManager.resolutions.Last();
						text = resolution.width + "x" + resolution.height;
					}
					if (text == "Screen")
					{
						text = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
					}
					if (text == "Unset")
					{
						Screen.fullScreen = true;
					}
					else
					{
						string[] array = text.Split('x');
						int width = Convert.ToInt32(array[0]);
						int height = Convert.ToInt32(array[0]);
						Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
						Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
					}
				}
				else
				{
					Screen.fullScreen = false;
				}
				if (MusicBackground)
				{
					Application.runInBackground = true;
					SoundManager.MusicSources.SetMusicBackground(State: true);
				}
				else
				{
					Application.runInBackground = false;
					SoundManager.MusicSources.SetMusicBackground(State: false);
				}
				string displayFramerate = DisplayFramerate;
				if (displayFramerate == "Unlimited")
				{
					QualitySettings.vSyncCount = 0;
					Application.targetFrameRate = 0;
				}
				else
				{
					if (!(displayFramerate == "VSync"))
					{
						try
						{
							Application.targetFrameRate = Convert.ToInt16(displayFramerate);
							QualitySettings.vSyncCount = 0;
							return;
						}
						catch
						{
							Application.targetFrameRate = 60;
							QualitySettings.vSyncCount = 0;
							return;
						}
					}
					QualitySettings.vSyncCount = 1;
					Application.targetFrameRate = 60;
				}
			});
		}

		[ModSensitiveCacheInit]
		public static void LoadAllOptions()
		{
			if (GameManager.AwakeComplete)
			{
				LoadOptions();
				LoadModOptions();
				UpdateFlags();
			}
		}

		public static void LoadOptions()
		{
			OptionsByCategory = new Dictionary<string, List<GameOption>>();
			OptionsByID = new Dictionary<string, GameOption>();
			Bag = new NameValueBag(DataManager.LocalPath("PlayerOptions.json"));
			foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Options", IncludeBase: true, IncludeMods: false))
			{
				try
				{
					HandleNodes(item);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Error loading base options", x);
				}
			}
			Bag.Load();
			lock (ValueCache)
			{
				ValueCache.Clear();
			}
		}

		private static void HandleNodes(XmlDataHelper xml)
		{
			xml.HandleNodes(_Nodes);
		}

		public static void LoadModOptions()
		{
			foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Options", IncludeBase: false))
			{
				try
				{
					HandleNodes(item);
				}
				catch (Exception msg)
				{
					item.modInfo.Error(msg);
				}
			}
			lock (ValueCache)
			{
				ValueCache.Clear();
			}
			SifrahGame.Installed = GlobalConfig.GetBoolSetting("EnableSifrah");
		}

		private static void HandleOptionNode(XmlDataHelper xml)
		{
			GameOption gameOption = LoadOptionNode(xml);
			if (!OptionsByCategory.ContainsKey(gameOption.Category))
			{
				OptionsByCategory.Add(gameOption.Category, new List<GameOption>());
			}
			OptionsByCategory[gameOption.Category].Add(gameOption);
			OptionsByID.Add(gameOption.ID, gameOption);
		}

		private static GameOption LoadOptionNode(XmlDataHelper xml)
		{
			string text = xml.ParseAttribute<string>("ID", null, required: true);
			if (OptionsByID.TryGetValue(text, out var Option))
			{
				OptionsByCategory[Option.Category].Remove(Option);
				OptionsByID.Remove(text);
			}
			else
			{
				Option = new GameOption();
				Option.ID = text;
			}
			Option.DisplayText = xml.ParseAttribute("DisplayText", Option.DisplayText);
			Option.Category = xml.ParseAttribute("Category", Option.Category ?? "No Category", required: true);
			Option.Requires = xml.ParseAttribute<GameOption.RequiresSpec>("Requires", null);
			Option.Type = xml.ParseAttribute("Type", Option.Type);
			Option.SearchKeywords = xml.ParseAttribute("SearchKeywords", Option.SearchKeywords);
			Option.Default = CapabilityManager.instance.GetDefaultOptionOverrideForCapabilities(Option.ID, xml.ParseAttribute("Default", Option.Default));
			if (Option.Type == "Button")
			{
				Option.OnClick = xml.ParseAttribute<MethodInfo>("OnClick", null, required: true);
			}
			if (!xml.GetAttribute("Values").IsNullOrEmpty())
			{
				if (xml.GetAttribute("Values") == "*Resolution")
				{
					Option.Values = new List<string>();
					HashSet<string> hashSet = new HashSet<string>();
					foreach (Resolution resolution in GameManager.resolutions)
					{
						string item = resolution.width + "x" + resolution.height;
						if (!hashSet.Contains(item))
						{
							Option.Values.Add(item);
							hashSet.Add(item);
						}
					}
					Option.Values.Add("Screen");
					Option.Values.Add("Unset");
				}
				else
				{
					Option.Values = new List<string>(xml.GetAttribute("Values").Split(','));
				}
			}
			Option.Min = xml.GetAttributeInt("Min", Option.Min);
			Option.Max = xml.GetAttributeInt("Max", Option.Max);
			Option.Increment = xml.GetAttributeInt("Increment", Option.Increment);
			xml.HandleNodes(new Dictionary<string, Action<XmlDataHelper>> { 
			{
				"helptext",
				delegate(XmlDataHelper xml)
				{
					Option.HelpText = xml.GetTextNode();
				}
			} });
			return Option;
		}
	}
}
