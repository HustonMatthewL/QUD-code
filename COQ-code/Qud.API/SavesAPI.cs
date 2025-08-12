using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using XRL;

namespace Qud.API
{
	public static class SavesAPI
	{
		private static List<string> _SavedGamePaths;

		private static readonly string[] InfoFiles = new string[2] { "Primary.json", "Primary.sav.json" };

		public static List<string> SavedGamePaths
		{
			get
			{
				object obj = _SavedGamePaths;
				if (obj == null)
				{
					obj = new List<string>
					{
						DataManager.SyncedPath("Saves"),
						DataManager.SavePath("Saves")
					};
					_SavedGamePaths = (List<string>)obj;
				}
				return (List<string>)obj;
			}
		}

		private static long GetDirectorySize(string p)
		{
			string[] files = Directory.GetFiles(p, "*.*");
			long num = 0L;
			string[] array = files;
			for (int i = 0; i < array.Length; i++)
			{
				FileInfo fileInfo = new FileInfo(array[i]);
				num += fileInfo.Length;
			}
			return num;
		}

		public static SaveGameInfo ReadSaveJson(string Dir, string file)
		{
			SaveGameJSON saveGameJSON = null;
			try
			{
				saveGameJSON = JsonUtility.FromJson<SaveGameJSON>(File.ReadAllText(file));
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Loading sav json " + file, x);
			}
			if (saveGameJSON == null)
			{
				return new SaveGameInfo
				{
					Name = "&RCorrupt info file",
					Size = "Total size: " + GetDirectorySize(Dir) / 1000000 + "mb",
					Info = "",
					Directory = Dir
				};
			}
			SaveGameInfo saveGameInfo = new SaveGameInfo
			{
				json = saveGameJSON,
				Directory = Dir,
				Size = "Total size: " + GetDirectorySize(Dir) / 1000000 + "mb",
				ID = saveGameJSON.ID,
				Version = saveGameJSON.GameVersion,
				Name = saveGameJSON.Name,
				Description = $"Level {saveGameJSON.Level} {saveGameJSON.GenoSubType} [{saveGameJSON.GameMode}]",
				Info = $"{saveGameJSON.Location}, {saveGameJSON.InGameTime} turn {saveGameJSON.Turn}",
				SaveTime = saveGameJSON.SaveTime,
				ModsEnabled = saveGameJSON.ModsEnabled
			};
			if (saveGameJSON.SaveVersion < 395 || saveGameJSON.SaveVersion > 396)
			{
				saveGameInfo.Name = "{{R|Older Version (" + saveGameJSON.GameVersion + ")}} " + saveGameInfo.Name;
			}
			return saveGameInfo;
		}

		public static bool HasSavedGameInfo()
		{
			foreach (string savedGamePath in SavedGamePaths)
			{
				if (!Directory.Exists(savedGamePath))
				{
					continue;
				}
				foreach (string item in Directory.EnumerateDirectories(savedGamePath))
				{
					string[] infoFiles = InfoFiles;
					foreach (string path in infoFiles)
					{
						if (File.Exists(Path.Combine(item, path)))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public static List<SaveGameInfo> GetSavedGameInfo()
		{
			List<SaveGameInfo> list = new List<SaveGameInfo>();
			foreach (string savedGamePath in SavedGamePaths)
			{
				if (!Directory.Exists(savedGamePath))
				{
					continue;
				}
				foreach (string item in Directory.EnumerateDirectories(savedGamePath))
				{
					SaveGameInfo directoryInfo = GetDirectoryInfo(item);
					if (directoryInfo != null)
					{
						list.Add(directoryInfo);
					}
				}
			}
			list.Sort(SortGameByDate);
			return list;
		}

		private static SaveGameInfo GetDirectoryInfo(string Directory)
		{
			try
			{
				if (Path.GetFileNameWithoutExtension(Directory).EqualsNoCase("mods") || Path.GetFileNameWithoutExtension(Directory).EqualsNoCase("textures"))
				{
					return null;
				}
				string[] infoFiles = InfoFiles;
				foreach (string path in infoFiles)
				{
					string text = Path.Combine(Directory, path);
					if (File.Exists(text))
					{
						return ReadSaveJson(Directory, text);
					}
				}
			}
			catch (ThreadInterruptedException ex)
			{
				throw ex;
			}
			catch (Exception ex2)
			{
				MetricsManager.LogWarning(ex2.ToString());
			}
			Debug.LogWarning("Weird save directory with no .json file present: " + Directory);
			return null;
		}

		private static int SortGameByDate(SaveGameInfo I1, SaveGameInfo I2)
		{
			try
			{
				if (string.IsNullOrEmpty(I1.SaveTime) || !I1.SaveTime.Contains(" at "))
				{
					return 1;
				}
				if (string.IsNullOrEmpty(I2.SaveTime) || !I2.SaveTime.Contains(" at "))
				{
					return -1;
				}
				string text = I1.SaveTime.Substring(0, I1.SaveTime.IndexOf(" at "));
				string text2 = I1.SaveTime.Substring(I1.SaveTime.IndexOf(" at ") + 4);
				string text3 = I2.SaveTime.Substring(0, I2.SaveTime.IndexOf(" at "));
				string text4 = I2.SaveTime.Substring(I2.SaveTime.IndexOf(" at ") + 4);
				DateTime value = DateTime.Parse(text + " " + text2);
				return DateTime.Parse(text3 + " " + text4).CompareTo(value);
			}
			catch
			{
				return 0;
			}
		}
	}
}
