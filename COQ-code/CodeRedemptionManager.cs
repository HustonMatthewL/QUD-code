using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Ionic.Zip;
using Qud.UI;
using UnityEngine;
using XRL;
using XRL.Core;
using XRL.UI;

public static class CodeRedemptionManager
{
	public static async void redeemNoProgress(string code)
	{
		await SingletonWindowBase<LoadingStatusWindow>.instance.SetLoadingTextAsync("hm");
		await Task.Yield();
		if (string.IsNullOrEmpty(code) || Uri.EscapeUriString(code) != code)
		{
			await Popup.ShowAsync("That code is invalid.");
			return;
		}
		code = code.ToUpper();
		if (code[0] != 'P')
		{
			await Popup.ShowAsync("That code is invalid.");
			return;
		}
		await SingletonWindowBase<LoadingStatusWindow>.instance.SetLoadingTextAsync("Redeeming code...");
		await Task.Yield();
		if (code[0] != 'P')
		{
			return;
		}
		code = code.Substring(1);
		WebClient webClient = new WebClient();
		string text = DataManager.SavePath("Temp");
		Directory.CreateDirectory(text);
		string fileName = code + ".zip";
		string downloadPath = Path.Combine(text, fileName).Replace('\\', '/');
		if (File.Exists(downloadPath))
		{
			Debug.Log("Deleting existing file " + downloadPath + "...");
			File.Delete(downloadPath);
		}
		await SingletonWindowBase<LoadingStatusWindow>.instance.SetLoadingTextAsync("Downloading pet...");
		await Task.Yield();
		try
		{
			string address = "http://s3.us-east-2.amazonaws.com/cavesofqud/pets/" + fileName;
			webClient.DownloadFile(address, downloadPath);
		}
		catch (Exception ex)
		{
			await Popup.ShowAsync("Error downloading pet: " + ex.ToString(), CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: true, PushView: true);
			return;
		}
		string modPath = DataManager.SavePath("Mods");
		Directory.CreateDirectory(modPath);
		await SingletonWindowBase<LoadingStatusWindow>.instance.SetLoadingTextAsync("Installing pet...");
		await Task.Yield();
		using (ZipFile zipFile = ZipFile.Read(downloadPath))
		{
			foreach (ZipEntry item in zipFile)
			{
				item.Extract(modPath, ExtractExistingFileAction.OverwriteSilently);
			}
		}
		await SingletonWindowBase<LoadingStatusWindow>.instance.SetLoadingTextAsync("Reloading configuration...");
		await Task.Yield();
		ModManager.Init(Reload: true);
		ModManager.BuildScriptMods();
		XRLCore.Core.HotloadConfiguration();
		await Popup.ShowAsync("Your new pet is ready to love.", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: true, PushView: true);
	}

	public static void redeem(string code)
	{
		_ = Popup.ShowProgressAsync(async delegate(Progress p)
		{
			if (string.IsNullOrEmpty(code) || Uri.EscapeUriString(code) != code)
			{
				await Popup.ShowAsync("That code is invalid.", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: true, PushView: true);
				return false;
			}
			code = code.ToUpper();
			if (code[0] != 'P')
			{
				await Popup.ShowAsync("That code is invalid.", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: true, PushView: true);
				return false;
			}
			p.setCurrentProgressText("Redeeming code...");
			p.setCurrentProgress(20);
			if (code[0] == 'P')
			{
				code = code.Substring(1);
				WebClient webClient = new WebClient();
				string text = DataManager.SavePath("Temp");
				Directory.CreateDirectory(text);
				string text2 = code + ".zip";
				string text3 = Path.Combine(text, text2).Replace('\\', '/');
				if (File.Exists(text3))
				{
					Debug.Log("Deleting existing file " + text3 + "...");
					File.Delete(text3);
				}
				p.setCurrentProgressText("Downloading pet...");
				p.setCurrentProgress(40);
				try
				{
					string address = "http://s3.us-east-2.amazonaws.com/cavesofqud/pets/" + text2;
					webClient.DownloadFile(address, text3);
				}
				catch (Exception ex)
				{
					await Popup.ShowAsync("Error downloading pet: " + ex.ToString(), CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: true, PushView: true);
					return false;
				}
				p.setCurrentProgressText("The pet finished downloading!");
				p.setCurrentProgress(60);
				p.setCurrentProgressText("Installing pet...");
				string text4 = DataManager.SavePath("Mods");
				Directory.CreateDirectory(text4);
				using (ZipFile zipFile = ZipFile.Read(text3))
				{
					foreach (ZipEntry item in zipFile)
					{
						item.Extract(text4, ExtractExistingFileAction.OverwriteSilently);
					}
				}
				p.setCurrentProgress(80);
				p.setCurrentProgressText("Reloading configuration...");
				ModManager.Init(Reload: true);
				ModManager.BuildScriptMods();
				await The.GameContext;
				XRLCore.Core.HotloadConfiguration();
				p.setCurrentProgress(100);
				await Popup.ShowAsync("Your new pet is ready to love.", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: true, PushView: true);
				return true;
			}
			return false;
		}).Result;
	}
}
