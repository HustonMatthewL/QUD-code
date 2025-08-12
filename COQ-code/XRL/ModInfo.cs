using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HarmonyLib;
using Kobold;
using Qud.UI;
using UnityEngine;
using XRL.Collections;
using XRL.Rules;
using XRL.UI;

namespace XRL
{
	[HasModSensitiveStaticCache]
	public class ModInfo : IComparable<ModInfo>
	{
		public string Path;

		public string ID = "";

		public ModSource Source;

		public DirectoryInfo Directory;

		public Assembly Assembly;

		public Harmony Harmony;

		public Rack<ModFile> Files = new Rack<ModFile>();

		public int LoadPriority;

		public bool IsScripting;

		public bool IsApproved;

		public long Size;

		public ModSettings Settings;

		public ModManifest Manifest = new ModManifest();

		public SteamWorkshopInfo WorkshopInfo;

		public TextureConfiguration TextureConfiguration = new TextureConfiguration();

		[ModSensitiveStaticCache(true)]
		private static Dictionary<string, Sprite> spriteByPath = new Dictionary<string, Sprite>();

		public bool IsEnabled
		{
			get
			{
				return State == ModState.Enabled;
			}
			set
			{
				Settings.Enabled = value;
			}
		}

		public ModState State
		{
			get
			{
				if (IsScripting && !Options.AllowCSMods)
				{
					return ModState.Disabled;
				}
				if (!Settings.Enabled)
				{
					return ModState.Disabled;
				}
				if (!IsApproved)
				{
					return ModState.NeedsApproval;
				}
				if (Settings.Failed)
				{
					return ModState.Failed;
				}
				return ModState.Enabled;
			}
		}

		public string DisplayTitleStripped => ConsoleLib.Console.ColorUtility.StripFormatting(DisplayTitle);

		public string DisplayTitle
		{
			get
			{
				if (!string.IsNullOrEmpty(Manifest.Title))
				{
					return Manifest.Title;
				}
				return ID;
			}
		}

		public string RelativePath(string path)
		{
			if (!path.StartsWith(Path))
			{
				return path;
			}
			return path.Replace(Path, "");
		}

		public ModInfo(string Path, string ID = null, ModSource Source = ModSource.Unknown, bool Initialize = false)
		{
			this.Path = Path;
			Directory = new DirectoryInfo(Path);
			this.ID = ID;
			this.Source = Source;
			if (Initialize)
			{
				this.Initialize();
			}
		}

		public void Initialize()
		{
			if (Directory.Exists)
			{
				ReadConfigurations();
				LoadSettings();
				InitializeFiles();
				IsApproved = CheckApproval();
				if (IsEnabled)
				{
					Settings.Errors.Clear();
					Settings.Warnings.Clear();
				}
			}
		}

		public void ReadConfigurations()
		{
			foreach (FileInfo item in Directory.EnumerateFiles())
			{
				try
				{
					ReadConfiguration(item);
				}
				catch (Exception msg)
				{
					Error(msg);
				}
			}
			ID = Regex.Replace(Manifest.ID ?? ID, "[^\\w ]", "");
			LoadPriority = Manifest.LoadOrder;
		}

		private void ReadConfiguration(FileInfo File)
		{
			switch (File.Name.ToLower())
			{
			case "manifest.json":
				ModManager.JsonSerializer.Populate(File.FullName, Manifest);
				break;
			case "config.json":
			{
				ModManifest modManifest = ModManager.JsonSerializer.Deserialize<ModManifest>(File.FullName);
				if (Manifest.ID == null)
				{
					Manifest.ID = modManifest.ID;
				}
				if (Manifest.LoadOrder == 0)
				{
					Manifest.LoadOrder = modManifest.LoadOrder;
				}
				Warn("Mod using config.json, please convert to manifest.json and check out https://wiki.cavesofqud.com/Modding:Overview for other options to set");
				break;
			}
			case "workshop.json":
				WorkshopInfo = ModManager.JsonSerializer.Deserialize<SteamWorkshopInfo>(File.FullName);
				if (Manifest.Tags == null)
				{
					Manifest.Tags = WorkshopInfo.Tags;
				}
				if (Manifest.PreviewImage == null)
				{
					Manifest.PreviewImage = WorkshopInfo.ImagePath;
				}
				if (Manifest.Title == null && WorkshopInfo.Title != null)
				{
					Manifest.Title = ConsoleLib.Console.ColorUtility.EscapeFormatting(WorkshopInfo.Title);
				}
				break;
			case "modconfig.json":
				ModManager.JsonSerializer.Populate(File.FullName, TextureConfiguration);
				break;
			}
		}

		public void LoadSettings()
		{
			if (!ModManager.ModSettingsMap.TryGetValue(ID, out Settings))
			{
				Settings = new ModSettings();
				ModManager.ModSettingsMap[ID] = Settings;
			}
			Settings.Title = DisplayTitle;
		}

		public void InitializeFiles()
		{
			IsScripting = false;
			InitializeFiles(Directory);
		}

		private void InitializeFiles(DirectoryInfo Directory)
		{
			foreach (FileSystemInfo item in Directory.EnumerateFileSystemInfos())
			{
				if ((item.Attributes & FileAttributes.Hidden) > (FileAttributes)0)
				{
					continue;
				}
				if ((item.Attributes & FileAttributes.Directory) > (FileAttributes)0)
				{
					InitializeFiles((DirectoryInfo)item);
					continue;
				}
				FileInfo fileInfo = (FileInfo)item;
				Size += fileInfo.Length;
				Files.Add(new ModFile(this, fileInfo));
				if (!IsScripting && fileInfo.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
				{
					IsScripting = true;
				}
			}
		}

		public bool CheckApproval()
		{
			if (!IsScripting)
			{
				return true;
			}
			if (!Options.ApproveCSMods)
			{
				return true;
			}
			if (Settings.FilesHash == null)
			{
				return false;
			}
			if (Settings.SourceHash == null)
			{
				return false;
			}
			string text = Settings.CalcFilesHash(Files, Path);
			if (Settings.FilesHash != text)
			{
				return false;
			}
			text = Settings.CalcSourceHash(Files);
			if (Settings.SourceHash != text)
			{
				return false;
			}
			return true;
		}

		public void Approve()
		{
			Settings.FilesHash = Settings.CalcFilesHash(Files, Path);
			Settings.SourceHash = Settings.CalcSourceHash(Files);
			Settings.Failed = false;
			IsApproved = true;
		}

		public void ConfirmFailure()
		{
			int count = Settings.Errors.Count;
			if (count > 0)
			{
				LogToClipboard();
				string title = DisplayTitle + " - {{R|Errors}}";
				string text = string.Join("\n", Settings.Errors.Take(3));
				if (count > 3)
				{
					text = text + "\n(... {{R|+" + (count - 3) + "}} more)";
				}
				text = text + "\n\nAutomatically on your clipboard should you wish to forward it to " + (Manifest.Author ?? "the mod author") + ".";
				List<QudMenuItem> list = new List<QudMenuItem>(PopupMessage.CancelButton);
				list.Add(new QudMenuItem
				{
					text = "{{W|[R]}} {{y|Retry}}",
					command = "retry",
					hotkey = "R"
				});
				if (WorkshopInfo != null)
				{
					list.Add(new QudMenuItem
					{
						text = "{{W|[W]}} {{y|Workshop}}",
						command = "workshop",
						hotkey = "W"
					});
				}
				Popup.WaitNewPopupMessage(text, list, delegate(QudMenuItem i)
				{
					if (i.command == "workshop")
					{
						WorkshopInfo?.OpenWorkshopPage();
					}
					else if (i.command == "retry")
					{
						Settings.Failed = false;
					}
				}, null, title);
			}
			else
			{
				Settings.Failed = false;
			}
		}

		public void LogToClipboard()
		{
			StringBuilder stringBuilder = Strings.SB.Clear().Append("=== ").Append(DisplayTitleStripped);
			if (!Manifest.Version.IsZero())
			{
				stringBuilder.Append(" ").Append(Manifest.Version.ToString());
			}
			stringBuilder.Append(" Errors ===\n");
			if (Settings.Errors.Any())
			{
				stringBuilder.AppendRange(Settings.Errors, "\n");
			}
			else
			{
				stringBuilder.Append("None");
			}
			stringBuilder.Append("\n== Warnings ==\n");
			if (Settings.Warnings.Any())
			{
				stringBuilder.AppendRange(Settings.Warnings, "\n");
			}
			else
			{
				stringBuilder.Append("None");
			}
			ClipboardHelper.SetClipboardData(stringBuilder.ToString());
		}

		public Sprite GetDefaultSprite()
		{
			return SpriteManager.GetUnitySprite("Text/0.bmp");
		}

		public Sprite GetSprite()
		{
			string text = Manifest.PreviewImage ?? WorkshopInfo?.ImagePath;
			if (string.IsNullOrEmpty(text))
			{
				return GetDefaultSprite();
			}
			string text2 = System.IO.Path.Combine(Path, text);
			if (!text2.StartsWith(Path))
			{
				return GetDefaultSprite();
			}
			Sprite value = null;
			if (spriteByPath.TryGetValue(text2, out value))
			{
				return value;
			}
			Texture2D texture2D = null;
			if (File.Exists(text2))
			{
				byte[] data = File.ReadAllBytes(text2);
				texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
				texture2D.LoadImage(data);
				texture2D.filterMode = UnityEngine.FilterMode.Trilinear;
			}
			if (texture2D != null)
			{
				value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
			}
			spriteByPath.Add(text2, value);
			return value;
		}

		public void Warn(object msg)
		{
			MetricsManager.LogModWarning(this, msg);
		}

		public void Error(object msg)
		{
			MetricsManager.LogModError(this, msg);
		}

		public void ApplyHarmonyPatches()
		{
			try
			{
				if (!(Assembly == null) && Assembly.GetTypes().Any((Type x) => x.IsDefined(typeof(HarmonyAttribute), inherit: true)))
				{
					Logger.buildLog.Info("Applying Harmony patches...");
					Harmony = Harmony ?? new Harmony(ID);
					Harmony.PatchAll(Assembly);
					Logger.buildLog.Info("Success :)");
				}
			}
			catch (Exception ex)
			{
				Error("Exception applying harmony patches: " + ex);
				Logger.buildLog.Info("Failure :(");
			}
		}

		public void UnapplyHarmonyPatches()
		{
			try
			{
				if (Harmony.GetPatchedMethods().Any())
				{
					Logger.buildLog.Info("Unapplying Harmony patches...");
					Harmony.UnpatchAll(Harmony.Id);
				}
			}
			catch (Exception ex)
			{
				Error("Exception unapplying harmony patches: " + ex);
			}
		}

		public void InitializeWorkshopInfo(ulong PublishedFileId)
		{
			WorkshopInfo = new SteamWorkshopInfo();
			WorkshopInfo.WorkshopId = PublishedFileId;
			SaveWorkshopInfo();
		}

		public void SaveWorkshopInfo()
		{
			if (WorkshopInfo != null)
			{
				ModManager.JsonSerializer.Serialize(System.IO.Path.Combine(Path, "workshop.json"), WorkshopInfo);
			}
		}

		public int CompareTo(ModInfo Other)
		{
			int num = LoadPriority.CompareTo(Other.LoadPriority);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(ID, Other.ID, StringComparison.Ordinal);
		}
	}
}
