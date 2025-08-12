using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using Newtonsoft.Json;
using RoslynCSharp;
using RoslynCSharp.Compiler;
using Steamworks;
using UnityEngine;
using XRL.Core;
using XRL.UI;
using XRL.World;

namespace XRL
{
	[HasModSensitiveStaticCache]
	public static class ModManager
	{
		public static List<ModInfo> Mods = null;

		public static List<ModInfo> ActiveMods = null;

		public static Dictionary<string, ModInfo> ModMap = null;

		public static Dictionary<string, ModSettings> ModSettingsMap = null;

		public static bool Compiled = false;

		public static bool Initialized = false;

		public static System.Version MarketingVersion = new System.Version("1.0.2");

		private static System.Version _CoreVersion;

		public static JsonSerializer JsonSerializer = new JsonSerializer
		{
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore
		};

		[ModSensitiveStaticCache(true)]
		private static Dictionary<Type, List<Type>> _typesWithAttribute = new Dictionary<Type, List<Type>>();

		[ModSensitiveStaticCache(true)]
		private static Dictionary<(Type, Type), List<FieldInfo>> _fieldsWithAttribute = new Dictionary<(Type, Type), List<FieldInfo>>();

		[ModSensitiveStaticCache(true)]
		private static Dictionary<(Type, Type), List<MethodInfo>> _methodsWithAttribute = new Dictionary<(Type, Type), List<MethodInfo>>();

		[ModSensitiveStaticCache(true)]
		private static Dictionary<Type, List<Type>> _assignableTypes = new Dictionary<Type, List<Type>>();

		[ModSensitiveStaticCache(true)]
		private static Dictionary<string, Type> _typeResolutions = new Dictionary<string, Type>();

		private static List<Assembly> _ModAssemblies = null;

		public static Dictionary<Type, string> typeNames = new Dictionary<Type, string>();

		private static Harmony harmony = new Harmony("com.freeholdgames.cavesofqud");

		public const BindingFlags ATTRIBUTE_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public static System.Version CoreVersion => _CoreVersion ?? (_CoreVersion = Assembly.GetExecutingAssembly().GetName().Version);

		public static IEnumerable<Type> ActiveTypes
		{
			get
			{
				foreach (Assembly activeAssembly in ActiveAssemblies)
				{
					Type[] types = activeAssembly.GetTypes();
					for (int i = 0; i < types.Length; i++)
					{
						yield return types[i];
					}
				}
			}
		}

		public static IEnumerable<Assembly> ActiveAssemblies
		{
			get
			{
				yield return Assembly.GetExecutingAssembly();
				foreach (Assembly modAssembly in ModAssemblies)
				{
					yield return modAssembly;
				}
			}
		}

		public static List<Assembly> ModAssemblies
		{
			get
			{
				if (_ModAssemblies == null)
				{
					Init();
					_ModAssemblies = new List<Assembly>();
					foreach (ModInfo mod in Mods)
					{
						if (mod.IsEnabled && !(mod.Assembly == null))
						{
							_ModAssemblies.Add(mod.Assembly);
						}
					}
				}
				return _ModAssemblies;
			}
		}

		public static bool RegisterMod(ModInfo Mod)
		{
			if (ModMap.ContainsKey(Mod.ID))
			{
				Mod.Warn("A mod with the ID \"" + Mod.ID + "\" already exists in " + DataManager.SanitizePathForDisplay(ModMap[Mod.ID].Path) + ", skipping.");
				return false;
			}
			ModMap[Mod.ID] = Mod;
			Mods.Add(Mod);
			return true;
		}

		public static bool DoesModDefineType(Type T)
		{
			return ModAssemblies.Contains(T.Assembly);
		}

		public static bool DoesModDefineType(string TypeID)
		{
			if (_typeResolutions.TryGetValue(TypeID, out var value))
			{
				return ModAssemblies.Contains(value.Assembly);
			}
			return ModAssemblies.Any((Assembly x) => x.GetType(TypeID) != null);
		}

		public static Type ResolveType(string TypeID, bool IgnoreCase = false, bool ThrowOnError = false, bool Cache = true)
		{
			return ResolveType(null, TypeID, IgnoreCase, ThrowOnError, Cache);
		}

		public static Type ResolveType(string Namespace, string TypeID, bool IgnoreCase = false, bool ThrowOnError = false, bool Cache = true)
		{
			if (TypeID.IsNullOrEmpty())
			{
				return null;
			}
			string text = ((Namespace != null) ? (Namespace + "." + TypeID) : TypeID);
			if (_typeResolutions.TryGetValue(text, out var value))
			{
				return value;
			}
			foreach (Assembly modAssembly in ModAssemblies)
			{
				if (Namespace != null)
				{
					value = modAssembly.GetType(text, throwOnError: false, IgnoreCase);
					if (value != null)
					{
						break;
					}
				}
				value = modAssembly.GetType(TypeID, throwOnError: false, IgnoreCase);
				if (value != null)
				{
					break;
				}
			}
			if ((object)value == null)
			{
				value = Type.GetType(text, ThrowOnError, IgnoreCase);
			}
			if (!IgnoreCase && Cache)
			{
				_typeResolutions[text] = value;
			}
			return value;
		}

		public static string ResolveTypeName(Type T)
		{
			if (typeNames.TryGetValue(T, out var value))
			{
				return value;
			}
			typeNames.Add(T, T.Name);
			return typeNames[T];
		}

		public static void ResetModSensitiveStaticCaches()
		{
			ActiveMods.Clear();
			foreach (ModInfo mod in Mods)
			{
				if (mod.IsEnabled)
				{
					ActiveMods.Add(mod);
				}
			}
			Type typeFromHandle = typeof(ModSensitiveStaticCacheAttribute);
			Type typeFromHandle2 = typeof(ModSensitiveCacheInitAttribute);
			Type typeFromHandle3 = typeof(HasModSensitiveStaticCacheAttribute);
			foreach (FieldInfo item in GetFieldsWithAttribute(typeFromHandle, typeFromHandle3, Cache: false))
			{
				if (item.IsStatic)
				{
					try
					{
						bool flag = item.FieldType.IsValueType || item.GetCustomAttribute<ModSensitiveStaticCacheAttribute>().CreateEmptyInstance;
						item.SetValue(null, flag ? Activator.CreateInstance(item.FieldType) : null);
					}
					catch (Exception arg)
					{
						MetricsManager.LogAssemblyError(item, $"Error initializing {item.DeclaringType.FullName}.{item.Name}: {arg}");
					}
				}
			}
			foreach (MethodInfo item2 in GetMethodsWithAttribute(typeFromHandle2, typeFromHandle3, Cache: false))
			{
				try
				{
					item2.Invoke(null, new object[0]);
				}
				catch (Exception arg2)
				{
					MetricsManager.LogAssemblyError(item2, $"Error invoking {item2.DeclaringType.FullName}.{item2.Name}: {arg2}");
				}
			}
			XRLCore.Core?.ReloadUIViews();
		}

		public static void CallAfterGameLoaded()
		{
			foreach (MethodInfo item in GetMethodsWithAttribute(typeof(CallAfterGameLoadedAttribute), typeof(HasCallAfterGameLoadedAttribute)))
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

		public static void BuildScriptMods()
		{
			if (!Application.isPlaying)
			{
				MetricsManager.LogEditorWarning("Script build initiated in edit mode.");
			}
			if (Thread.CurrentThread == XRLCore.CoreThread)
			{
				MetricsManager.LogInfo("Awaiting script build attempt on UI thread");
				GameManager.Instance.uiQueue.awaitTask(BuildScriptMods);
			}
			else
			{
				Loading.LoadTask("Building script mods", DoBuildScriptMods);
				Loading.LoadTask("Resetting static caches", ResetModSensitiveStaticCaches);
			}
		}

		private static void UnapplyAllHarmonyPatches()
		{
			try
			{
				if (Harmony.GetAllPatchedMethods().Any())
				{
					Logger.buildLog.Info("Unapplying all Harmony patches...");
					harmony.UnpatchAll("com.freeholdgames.cavesofqud");
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("unapply harmony patches", x);
			}
		}

		private static bool MainAssemblyPredicate(Assembly assembly)
		{
			if (assembly.IsDynamic)
			{
				return false;
			}
			if (assembly.Location.IsNullOrEmpty())
			{
				return false;
			}
			if (assembly.Location.Contains("ModAssemblies"))
			{
				return false;
			}
			if (assembly.Location.Contains("UIElements"))
			{
				return false;
			}
			if (assembly.Location.Contains("UnityEditor."))
			{
				return false;
			}
			if (assembly.FullName.Contains("ExCSS"))
			{
				return false;
			}
			return true;
		}

		private static bool ScriptModPredicate(ModInfo Mod)
		{
			if (Mod.IsScripting)
			{
				return Mod.IsEnabled;
			}
			return false;
		}

		private static RoslynCSharpCompiler GetCompilerService()
		{
			RoslynCSharpCompiler roslynCompilerService = ScriptDomain.CreateDomain("ModsDomain").RoslynCompilerService;
			roslynCompilerService.GenerateInMemory = !Options.OutputModAssembly;
			roslynCompilerService.OutputDirectory = DataManager.SavePath("ModAssemblies");
			roslynCompilerService.OutputPDBExtension = ".pdb";
			roslynCompilerService.GenerateSymbols = !roslynCompilerService.GenerateInMemory;
			foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies().Where(MainAssemblyPredicate))
			{
				roslynCompilerService.ReferenceAssemblies.Add(AssemblyReference.FromAssembly(item));
			}
			System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
			string text = $"BUILD_{version.Major}_{version.Minor}_{version.Build}";
			roslynCompilerService.DefineSymbols.Add(text);
			Logger.buildLog.Info("Defined symbol: " + text);
			return roslynCompilerService;
		}

		private static void DoBuildScriptMods()
		{
			if (Compiled)
			{
				return;
			}
			Harmony.DEBUG = Options.HarmonyDebug;
			UnapplyAllHarmonyPatches();
			Compiled = true;
			if (!Options.EnableMods || !Options.AllowCSMods || !Mods.Any(ScriptModPredicate))
			{
				return;
			}
			Logger.buildLog.Info("==== BUILDING SCRIPT MODS ====");
			RoslynCSharpCompiler compilerService = GetCompilerService();
			List<string> list = new List<string>();
			foreach (ModInfo mod in Mods)
			{
				mod.Assembly = null;
				if (!ScriptModPredicate(mod))
				{
					continue;
				}
				try
				{
					list.Clear();
					foreach (ModFile file in mod.Files)
					{
						if (file.Type == ModFileType.CSharp)
						{
							list.Add(file.OriginalName);
						}
					}
					string[] array = list.ToArray();
					string arg = ((array.Length == 1) ? "file" : "files");
					Logger.buildLog.Info("=== " + mod.DisplayTitleStripped.ToUpper() + " ===");
					Logger.buildLog.Info($"Compiling {array.Length} {arg}...");
					compilerService.OutputName = mod.ID;
					CompilationResult compilationResult = compilerService.CompileFromFiles(array);
					if (compilationResult.Success)
					{
						Logger.buildLog.Info("Success :)");
						mod.Assembly = compilationResult.OutputAssembly;
						mod.Settings.Failed = false;
						string text = "MOD_" + mod.ID.ToUpperInvariant().Replace(" ", "_");
						compilerService.DefineSymbols.Add(text);
						Logger.buildLog.Info("Defined symbol: " + text);
						if (compilationResult.OutputFile.IsNullOrEmpty())
						{
							compilerService.ReferenceAssemblies.Add(AssemblyReference.FromImage(compilationResult.OutputAssemblyImage));
						}
						else
						{
							compilerService.ReferenceAssemblies.Add(AssemblyReference.FromNameOrFile(compilationResult.OutputFile));
							Logger.buildLog.Info("Location: " + compilationResult.OutputFile);
						}
					}
					else
					{
						Logger.buildLog.Info("Failure :(");
						mod.Settings.Failed = true;
					}
					if (compilationResult.ErrorCount > 0)
					{
						Logger.buildLog.Info("== COMPILER ERRORS ==");
						CompilationError[] errors = compilationResult.Errors;
						foreach (CompilationError compilationError in errors)
						{
							if (compilationError.IsError)
							{
								string text2 = DataManager.SanitizePathForDisplay(compilationError.ToString());
								Logger.buildLog.Error(text2);
								mod.Error(text2);
							}
						}
					}
					if (compilationResult.WarningCount > 0)
					{
						Logger.buildLog.Info("== COMPILER WARNINGS ==");
						CompilationError[] errors = compilationResult.Errors;
						foreach (CompilationError compilationError2 in errors)
						{
							if (compilationError2.IsWarning)
							{
								string text3 = DataManager.SanitizePathForDisplay(compilationError2.ToString());
								Logger.buildLog.Info(text3);
								mod.Warn(text3);
							}
						}
					}
					mod.ApplyHarmonyPatches();
				}
				catch (Exception ex)
				{
					mod.Error("Exception compiling mod assembly: " + ex);
					mod.Settings.Failed = mod.Assembly == null;
				}
			}
			MinEvent.ResetEvents();
		}

		public static void Refresh()
		{
			Init(Reload: true);
		}

		public static void RefreshModDirectory(string Path, bool Create = false, ModSource Source = ModSource.Local)
		{
			MetricsManager.LogInfo("RefreshModDirectory " + ((Path != null) ? Path : "(null)"));
			DirectoryInfo directoryInfo = (Create ? Directory.CreateDirectory(Path) : new DirectoryInfo(Path));
			if (!directoryInfo.Exists)
			{
				return;
			}
			foreach (DirectoryInfo item in directoryInfo.EnumerateDirectories())
			{
				if ((item.Attributes & FileAttributes.Hidden) <= (FileAttributes)0)
				{
					try
					{
						RegisterMod(new ModInfo(item.FullName, item.Name, Source, Initialize: true));
					}
					catch (Exception x)
					{
						MetricsManager.LogError("Exception reading local mod directory " + item.Name, x);
					}
				}
			}
		}

		private static void RefreshWorkshopSubscriptions()
		{
			if (!PlatformManager.SteamInitialized)
			{
				MetricsManager.LogInfo("Skipping workshop subscription info because steam isn't connected");
				return;
			}
			PublishedFileId_t[] array = new PublishedFileId_t[4096];
			uint subscribedItems = SteamUGC.GetSubscribedItems(array, 4096u);
			MetricsManager.LogInfo("Subscribed workshop items: " + subscribedItems);
			for (int i = 0; i < subscribedItems; i++)
			{
				try
				{
					if (SteamUGC.GetItemInstallInfo(array[i], out var _, out var pchFolder, 4096u, out var _))
					{
						if (!Directory.Exists(pchFolder))
						{
							MetricsManager.LogError("Mod directory does not exist: " + pchFolder);
						}
						else
						{
							RegisterMod(new ModInfo(pchFolder, array[i].ToString(), ModSource.Steam, Initialize: true));
						}
					}
				}
				catch (Exception x)
				{
					PublishedFileId_t publishedFileId_t = array[i];
					MetricsManager.LogError("Exception reading workshop mod subscription " + publishedFileId_t.ToString(), x);
				}
			}
		}

		public static void ReadModSettings(bool Reload = false)
		{
			if (ModSettingsMap != null && !Reload)
			{
				return;
			}
			try
			{
				string text = DataManager.LocalPath("ModSettings.json");
				if (File.Exists(text))
				{
					ModSettingsMap = JsonSerializer.Deserialize<Dictionary<string, ModSettings>>(text);
				}
				else
				{
					ModSettingsMap = new Dictionary<string, ModSettings>();
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Failed reading ModSettings.json", x);
				ModSettingsMap = ModSettingsMap ?? new Dictionary<string, ModSettings>();
			}
			if (Reload)
			{
				Init(Reload: true);
			}
		}

		public static void WriteModSettings()
		{
			if (ModSettingsMap != null)
			{
				string file = DataManager.LocalPath("ModSettings.json");
				JsonSerializer.Serialize(file, ModSettingsMap);
			}
		}

		public static int SortModInfo(ModInfo f1, ModInfo f2)
		{
			if (f1.LoadPriority == f2.LoadPriority)
			{
				return f1.ID.CompareTo(f2.ID);
			}
			return f1.LoadPriority.CompareTo(f2.LoadPriority);
		}

		public static void Init(bool Reload = false)
		{
			if (!Reload && Mods != null)
			{
				return;
			}
			Compiled = false;
			Mods = new List<ModInfo>();
			ModMap = new Dictionary<string, ModInfo>();
			ActiveMods = new List<ModInfo>();
			_ModAssemblies = null;
			if (!Application.isPlaying)
			{
				MetricsManager.LogEditorWarning("Mod initialization executed in edit mode.");
				return;
			}
			if (Options.Bag == null)
			{
				MetricsManager.LogError("Mod initialization executed early, options not initialized.");
				return;
			}
			if (!Options.EnableMods)
			{
				Initialized = true;
				return;
			}
			ReadModSettings();
			try
			{
				RefreshModDirectory(DataManager.DLCPath(), Create: false, ModSource.Embedded);
			}
			catch (Exception message)
			{
				MetricsManager.LogError(message);
			}
			try
			{
				RefreshModDirectory(DataManager.EmbeddedModsPath(), Create: false, ModSource.Embedded);
			}
			catch (Exception message2)
			{
				MetricsManager.LogError(message2);
			}
			try
			{
				RefreshModDirectory(DataManager.SavePath("Mods"), Create: true);
			}
			catch (Exception message3)
			{
				MetricsManager.LogError(message3);
			}
			try
			{
				RefreshModDirectory(DataManager.LocalPath("Mods"), Create: true);
			}
			catch (Exception message4)
			{
				MetricsManager.LogError(message4);
			}
			try
			{
				RefreshWorkshopSubscriptions();
			}
			catch (Exception message5)
			{
				MetricsManager.LogError(message5);
			}
			Mods.Sort(SortModInfo);
			Initialized = true;
		}

		public static ModInfo GetMod(string ID)
		{
			if (ID == null)
			{
				return null;
			}
			if (ModMap == null)
			{
				Init();
			}
			if (ModMap.TryGetValue(ID, out var value))
			{
				return value;
			}
			return null;
		}

		public static ModInfo GetMod(ulong WorkshopID)
		{
			if (WorkshopID == 0)
			{
				return null;
			}
			if (Mods == null)
			{
				Init();
			}
			int i = 0;
			for (int count = Mods.Count; i < count; i++)
			{
				SteamWorkshopInfo workshopInfo = Mods[i].WorkshopInfo;
				if (workshopInfo != null && workshopInfo.WorkshopId == WorkshopID)
				{
					return Mods[i];
				}
			}
			return null;
		}

		public static ModInfo GetMod(Assembly Assembly)
		{
			if (Assembly == null)
			{
				return null;
			}
			if (Mods == null)
			{
				Init();
			}
			foreach (ModInfo mod in Mods)
			{
				if (!(mod.Assembly != Assembly))
				{
					return mod;
				}
			}
			return null;
		}

		public static ModInfo GetModBySpec(string Spec)
		{
			try
			{
				ModInfo mod = GetMod(Convert.ToUInt64(Spec));
				if (mod != null)
				{
					return mod;
				}
			}
			catch
			{
			}
			return GetMod(Spec);
		}

		public static bool ModLoadedBySpec(string Spec)
		{
			return GetModBySpec(Spec) != null;
		}

		public static bool TryGetCallingMod(out ModInfo Mod, out StackFrame Frame)
		{
			return TryGetStackMod(new StackTrace(1), out Mod, out Frame);
		}

		public static bool TryGetStackMod(Exception Exception, out ModInfo Mod, out StackFrame Frame)
		{
			return TryGetStackMod(new StackTrace(Exception), out Mod, out Frame);
		}

		public static bool TryGetStackMod(StackTrace Trace, out ModInfo Mod, out StackFrame Frame)
		{
			try
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				StackFrame[] frames = Trace.GetFrames();
				foreach (StackFrame stackFrame in frames)
				{
					Assembly assembly = stackFrame.GetMethod().DeclaringType.Assembly;
					if (!(assembly == executingAssembly))
					{
						ModInfo modInfo = Mods.FirstOrDefault((ModInfo x) => x.Assembly == assembly);
						if (modInfo != null)
						{
							Mod = modInfo;
							Frame = stackFrame;
							return true;
						}
					}
				}
			}
			catch
			{
			}
			Mod = null;
			Frame = null;
			return false;
		}

		public static string GetModTitle(string ID)
		{
			ModInfo mod = GetMod(ID);
			if (mod != null)
			{
				return mod.DisplayTitle;
			}
			ReadModSettings();
			if (ModSettingsMap.TryGetValue(ID, out var value) && !value.Title.IsNullOrEmpty())
			{
				return value.Title;
			}
			return ID;
		}

		public static void LogRunningMods()
		{
			string text = "Enabled mods: ";
			text = ((Mods != null && Mods.Count != 0) ? (text + string.Join(", ", from m in Mods
				where m.IsEnabled
				select m.DisplayTitleStripped)) : (text + "None"));
			MetricsManager.LogInfo(text);
		}

		public static IEnumerable<string> GetRunningMods()
		{
			if (ActiveMods == null)
			{
				yield break;
			}
			foreach (ModInfo activeMod in ActiveMods)
			{
				yield return activeMod.ID;
			}
		}

		public static List<string> GetAvailableMods()
		{
			List<string> result = new List<string>();
			ForEachMod(delegate(ModInfo mod)
			{
				result.Add(mod.ID);
			}, IncludeDisabled: true);
			return result;
		}

		public static bool AreAnyModsUnapproved()
		{
			foreach (ModInfo mod in Mods)
			{
				if (mod.State == ModState.NeedsApproval)
				{
					return true;
				}
			}
			return false;
		}

		public static bool AreAnyModsFailed()
		{
			foreach (ModInfo mod in Mods)
			{
				if (mod.State == ModState.Failed)
				{
					return true;
				}
			}
			return false;
		}

		public static void ForEachMod(Action<ModInfo> ModAction, bool IncludeDisabled = false)
		{
			Init();
			foreach (ModInfo mod in Mods)
			{
				if (mod.IsEnabled || IncludeDisabled)
				{
					try
					{
						ModAction(mod);
					}
					catch (Exception ex)
					{
						mod.Error(DataManager.SanitizePathForDisplay(ex.ToString()));
					}
				}
			}
		}

		public static void ForEveryFile(Action<string, ModInfo> FileAction, bool IncludeDisabled = false)
		{
			ForEachMod(delegate(ModInfo mod)
			{
				if (!mod.Directory.Exists)
				{
					return;
				}
				foreach (FileInfo item in mod.Directory.EnumerateFiles())
				{
					FileAction(item.FullName, mod);
				}
			}, IncludeDisabled);
		}

		public static void ForEveryFileRecursive(Action<string, ModInfo> FileAction, string SearchPattern = "*.*", bool IncludeDisabled = false)
		{
			ForEachMod(delegate(ModInfo mod)
			{
				if (!mod.Directory.Exists)
				{
					return;
				}
				foreach (string item in Directory.EnumerateFiles(mod.Path, SearchPattern, SearchOption.AllDirectories))
				{
					FileAction(item, mod);
				}
			}, IncludeDisabled);
		}

		public static void ForEachFile(string FileName, Action<string> FileAction, bool IncludeDisabled = false)
		{
			ForEachFile(FileName, delegate(string f, ModInfo i)
			{
				FileAction(f);
			}, IncludeDisabled);
		}

		public static void ForEachFile(string FileName, Action<string, ModInfo> FileAction, bool IncludeDisabled = false)
		{
			string name = FileName.ToLower();
			ForEachMod(delegate(ModInfo mod)
			{
				if (!mod.Directory.Exists)
				{
					return;
				}
				foreach (FileInfo item in mod.Directory.EnumerateFiles())
				{
					if (!(item.Name.ToLower() != name))
					{
						try
						{
							FileAction(item.FullName, mod);
							break;
						}
						catch (Exception ex)
						{
							mod.Error(DataManager.SanitizePathForDisplay(mod.Path + "/" + FileName + ": " + ex.ToString()));
						}
					}
				}
			}, IncludeDisabled);
		}

		public static void ForEachFileIn(string Subdirectory, Action<string, ModInfo> FileAction, bool bIncludeBase = false, bool bIncludeDisabled = false)
		{
			Init();
			if (bIncludeBase)
			{
				_ForEachFileIn(DataManager.FilePath(Subdirectory), FileAction, null);
			}
			foreach (ModInfo mod in Mods)
			{
				if (bIncludeDisabled || mod.IsEnabled)
				{
					_ForEachFileIn(Path.Combine(mod.Path, Subdirectory), FileAction, mod);
				}
			}
		}

		private static void _ForEachFileIn(string Subdirectory, Action<string, ModInfo> FileAction, ModInfo mod)
		{
			if (!Directory.Exists(Subdirectory))
			{
				return;
			}
			foreach (string item in Directory.EnumerateFiles(Subdirectory))
			{
				try
				{
					FileAction(item, mod);
				}
				catch (Exception ex)
				{
					mod.Error(DataManager.SanitizePathForDisplay(Subdirectory + ": " + ex.ToString()));
				}
			}
			string[] directories = Directory.GetDirectories(Subdirectory);
			for (int i = 0; i < directories.Length; i++)
			{
				_ForEachFileIn(directories[i], FileAction, mod);
			}
		}

		public static object CreateInstance(string className)
		{
			Type type = ResolveType(className);
			if (type == null)
			{
				throw new TypeLoadException("No class with name \"" + className + "\" could be found. A full name including namespaces is required.");
			}
			return Activator.CreateInstance(type);
		}

		public static T CreateInstance<T>(string className) where T : class
		{
			return CreateInstance(className) as T;
		}

		public static T CreateInstance<T>(Type type) where T : class
		{
			return Activator.CreateInstance(type) as T;
		}

		public static IEnumerable<Type> GetClassesWithAttribute(Type attributeToSearchFor, Type classFilterAttribute = null)
		{
			List<Type> list = new List<Type>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.FullName.Contains("Assembly-CSharp"))
				{
					list.AddRange(from m in assembly.GetTypes().Concat(assembly.GetTypes().SelectMany((Type type) => type.GetNestedTypes()))
						where m.IsDefined(attributeToSearchFor, inherit: false) && m.IsClass
						select m);
				}
			}
			return list;
		}

		public static List<T> GetInstancesWithAttribute<T>(Type attributeType) where T : class
		{
			List<T> list = new List<T>();
			foreach (Type item2 in GetTypesWithAttribute(attributeType))
			{
				if (Activator.CreateInstance(item2) is T item)
				{
					list.Add(item);
				}
			}
			return list;
		}

		public static List<Type> GetTypesWithAttribute(Type AttributeType, bool Cache = true)
		{
			if (Cache && _typesWithAttribute.TryGetValue(AttributeType, out var value))
			{
				return value;
			}
			value = new List<Type>(32);
			foreach (Type activeType in ActiveTypes)
			{
				if (activeType.IsDefined(AttributeType, inherit: true))
				{
					value.Add(activeType);
				}
			}
			if (Cache)
			{
				_typesWithAttribute[AttributeType] = value;
				value.TrimExcess();
			}
			return value;
		}

		public static List<MethodInfo> GetMethodsWithAttribute(Type AttributeType, Type ClassFilterType = null, bool Cache = true)
		{
			if (_methodsWithAttribute.TryGetValue((AttributeType, ClassFilterType), out var value))
			{
				return value;
			}
			value = new List<MethodInfo>(128);
			IEnumerable<Type> enumerable;
			if (!(ClassFilterType == null))
			{
				IEnumerable<Type> typesWithAttribute = GetTypesWithAttribute(ClassFilterType, Cache);
				enumerable = typesWithAttribute;
			}
			else
			{
				enumerable = ActiveTypes;
			}
			foreach (Type item in enumerable)
			{
				MethodInfo[] methods = item.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					if (methodInfo.IsDefined(AttributeType, inherit: false))
					{
						value.Add(methodInfo);
					}
				}
			}
			if (Cache)
			{
				_methodsWithAttribute.Add((AttributeType, ClassFilterType), value);
				value.TrimExcess();
			}
			return value;
		}

		public static List<FieldInfo> GetFieldsWithAttribute(Type AttributeType, Type ClassFilterType = null, bool Cache = true)
		{
			if (Cache && _fieldsWithAttribute.TryGetValue((AttributeType, ClassFilterType), out var value))
			{
				return value;
			}
			value = new List<FieldInfo>(128);
			IEnumerable<Type> enumerable;
			if (!(ClassFilterType == null))
			{
				IEnumerable<Type> typesWithAttribute = GetTypesWithAttribute(ClassFilterType, Cache);
				enumerable = typesWithAttribute;
			}
			else
			{
				enumerable = ActiveTypes;
			}
			foreach (Type item in enumerable)
			{
				FieldInfo[] fields = item.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (fieldInfo.IsDefined(AttributeType, inherit: false))
					{
						value.Add(fieldInfo);
					}
				}
			}
			if (Cache)
			{
				_fieldsWithAttribute.Add((AttributeType, ClassFilterType), value);
				value.TrimExcess();
			}
			return value;
		}

		public static List<Type> GetTypesAssignableFrom(Type AssignableType, bool Cache = true)
		{
			if (Cache && _assignableTypes.TryGetValue(AssignableType, out var value))
			{
				return value;
			}
			value = new List<Type>(64);
			GetTypesAssignableFrom(AssignableType, value);
			if (Cache)
			{
				_assignableTypes.Add(AssignableType, value);
				value.TrimExcess();
			}
			return value;
		}

		public static void GetTypesAssignableFrom(Type AssignableType, List<Type> Result)
		{
			Result.Clear();
			foreach (Type activeType in ActiveTypes)
			{
				if (AssignableType.IsAssignableFrom(activeType))
				{
					Result.Add(activeType);
				}
			}
		}
	}
}
