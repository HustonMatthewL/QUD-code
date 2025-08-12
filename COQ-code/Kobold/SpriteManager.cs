using System;
using System.Collections.Generic;
using System.IO;
using ConsoleLib.Console;
using UnityEngine;
using XRL;
using XRL.Collections;
using XRL.Core;

namespace Kobold
{
	[HasModSensitiveStaticCache]
	public static class SpriteManager
	{
		private static GameObject _BaseSpritePrefab;

		private static Shader[] Shaders;

		private static GameObject _BaseSplitSpritePrefab;

		private static Dictionary<string, Sprite> UnitySpriteCache;

		[ModSensitiveStaticCache(false)]
		private static Dictionary<exTextureInfo, Sprite> unitySpriteMap;

		[ModSensitiveStaticCache(false)]
		private static StringMap<exTextureInfo> InfoMap;

		private static exTextureInfo InvalidInfo;

		private static Rack<char> KeyBuffer;

		private static GameObject CloneSpritePrefab()
		{
			if (_BaseSpritePrefab == null)
			{
				_BaseSpritePrefab = Resources.Load("KoboldBaseSprite") as GameObject;
				UnityEngine.Object.DontDestroyOnLoad(_BaseSpritePrefab);
			}
			return UnityEngine.Object.Instantiate(_BaseSpritePrefab);
		}

		public static Shader GetShaderMode(int n)
		{
			if (Shaders == null)
			{
				Shaders = new Shader[2];
				Shaders[0] = Shader.Find("Kobold/Alpha Blended Dual Color");
				Shaders[1] = Shader.Find("Kobold/Alpha Blended Truecolor");
			}
			return Shaders[n];
		}

		private static GameObject CloneSplitSpritePrefab()
		{
			if (_BaseSplitSpritePrefab == null)
			{
				_BaseSplitSpritePrefab = Resources.Load("KoboldBaseSlicedSprite") as GameObject;
				UnityEngine.Object.DontDestroyOnLoad(_BaseSplitSpritePrefab);
			}
			return UnityEngine.Object.Instantiate(_BaseSplitSpritePrefab);
		}

		public static Sprite GetUnitySprite(string path, bool reusable = true)
		{
			if (reusable && path != null && UnitySpriteCache.ContainsKey(path))
			{
				return UnitySpriteCache[path];
			}
			exTextureInfo textureInfo = GetTextureInfo(path, returnSpaceOnInvalid: false);
			if (textureInfo == null)
			{
				Texture2D texture2D = Resources.Load(path) as Texture2D;
				if (texture2D != null)
				{
					Sprite unitySprite = GetUnitySprite(texture2D);
					if (reusable)
					{
						UnitySpriteCache.Add(path, unitySprite);
					}
					return unitySprite;
				}
				Path.GetFileName(path);
				Sprite result = null;
				ModManager.ForEachFileIn("Textures", delegate(string f, ModInfo i)
				{
					if (Path.GetFileName(f).EqualsNoCase(path))
					{
						byte[] data = File.ReadAllBytes(path);
						Texture2D texture2D2 = new Texture2D(2, 2);
						texture2D2.LoadImage(data);
						result = GetUnitySprite(texture2D2);
					}
				});
				if (result != null)
				{
					if (reusable)
					{
						UnitySpriteCache.Add(path, result);
					}
					return result;
				}
				if (!string.IsNullOrEmpty(path))
				{
					MetricsManager.LogError("Unknown sprite: " + path);
				}
				return null;
			}
			return GetUnitySprite(textureInfo);
		}

		public static Sprite GetUnitySprite(Texture2D texture)
		{
			return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		}

		public static Sprite GetUnitySprite(exTextureInfo info)
		{
			if (unitySpriteMap == null)
			{
				unitySpriteMap = new Dictionary<exTextureInfo, Sprite>();
			}
			if (unitySpriteMap.ContainsKey(info))
			{
				return unitySpriteMap[info];
			}
			Texture2D texture = info.texture;
			Texture2D texture2D = new Texture2D(info.width, info.height, TextureFormat.ARGB32, mipChain: false);
			texture2D.filterMode = UnityEngine.FilterMode.Point;
			Color[] pixels = texture.GetPixels(info.x, info.y, info.width, info.height, 0);
			texture2D.SetPixels(pixels);
			texture2D.Apply();
			Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 40f);
			unitySpriteMap.Add(info, sprite);
			return sprite;
		}

		public static void SetSprite(GameObject Sprite, string Path)
		{
			Sprite.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path);
		}

		public static Vector2i GetSpriteSize(string Path)
		{
			exTextureInfo textureInfo = GetTextureInfo(Path);
			Debug.Log(Path + " " + textureInfo.rawWidth + "x" + textureInfo.rawWidth + "   " + textureInfo.trim_x + "x" + textureInfo.trim_y);
			return new Vector2i(textureInfo.width, textureInfo.height);
		}

		static SpriteManager()
		{
			_BaseSpritePrefab = null;
			Shaders = null;
			_BaseSplitSpritePrefab = null;
			UnitySpriteCache = new Dictionary<string, Sprite>();
			unitySpriteMap = null;
			InfoMap = null;
			KeyBuffer = new Rack<char>(128);
			"assets_content_textures_".AsSpan().CopyTo(KeyBuffer.FillSpan(24));
		}

		public static void Initialize()
		{
			MemoryHelper.GCCollect();
			InfoMap = new StringMap<exTextureInfo>();
			KoboldDatabaseScriptable koboldDatabaseScriptable = Resources.Load<KoboldDatabaseScriptable>("KoboldDatabase");
			if (koboldDatabaseScriptable == null)
			{
				exTextureInfo[] array = Resources.LoadAll<exTextureInfo>("TextureInfo");
				foreach (exTextureInfo exTextureInfo in array)
				{
					if (exTextureInfo != null)
					{
						try
						{
							InfoMap.Add(exTextureInfo.name, exTextureInfo);
						}
						catch (Exception ex)
						{
							Debug.Log("Error adding - " + exTextureInfo.name + " ... " + ex.Message);
						}
					}
				}
			}
			else
			{
				string[] koboldTextureInfos = koboldDatabaseScriptable.koboldTextureInfos;
				foreach (string text in koboldTextureInfos)
				{
					if (text.IsNullOrEmpty())
					{
						Debug.LogWarning("Info in koboldTextureInfos is null");
						continue;
					}
					try
					{
						InfoMap.Add(text, null);
					}
					catch (Exception x)
					{
						MetricsManager.LogException("SpriteManager", x);
					}
				}
			}
			ModManager.ForEachFileIn("Textures", delegate(string name, ModInfo mod)
			{
				if (mod.IsEnabled && name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				{
					Texture2D texture2D = new Texture2D(mod.TextureConfiguration.TextureWidth, mod.TextureConfiguration.TextureHeight);
					byte[] data = File.ReadAllBytes(name);
					texture2D.LoadImage(data);
					texture2D.filterMode = UnityEngine.FilterMode.Point;
					name = name.ToLowerInvariant();
					name = "assets_content_" + name.Substring(name.IndexOf("textures", StringComparison.Ordinal)).Replace('\\', '_').Replace('/', '_');
					exTextureInfo exTextureInfo2 = ScriptableObject.CreateInstance<exTextureInfo>();
					exTextureInfo2.texture = texture2D;
					exTextureInfo2.width = texture2D.width;
					exTextureInfo2.height = texture2D.height;
					exTextureInfo2.x = 0;
					exTextureInfo2.y = 0;
					exTextureInfo2.ShaderMode = mod.TextureConfiguration.ShaderMode;
					InfoMap[name] = exTextureInfo2;
					InfoMap[name.Replace(".png", ".bmp")] = exTextureInfo2;
					InfoMap[name.Replace(".png", "")] = exTextureInfo2;
				}
			});
		}

		public static exTextureInfo GetTextureInfo(string Path, bool returnSpaceOnInvalid = true)
		{
			if (TryGetTextureInfo(Path, out var Info))
			{
				return Info;
			}
			if (!returnSpaceOnInvalid)
			{
				return null;
			}
			Debug.LogError("SpriteManager: No texture found by ID '" + Path + "'.");
			Info = InvalidInfo ?? (InvalidInfo = GetTextureInfo("Text_32.bmp"));
			InfoMap.Add(Path, Info);
			return Info;
		}

		public static bool TryGetTextureInfo(string Path, out exTextureInfo Info)
		{
			if (Path == null)
			{
				Info = null;
				return false;
			}
			if (InfoMap == null)
			{
				Initialize();
			}
			if (InfoMap.TryGetValue(Path, out Info))
			{
				if (Info == null)
				{
					Info = Resources.Load<exTextureInfo>("TextureInfo/" + Path);
					InfoMap[Path] = Info;
				}
				return true;
			}
			int length = Path.Length;
			char[] array = KeyBuffer.GetArray(length + 24);
			Span<char> span = array.AsSpan(24, length);
			for (int i = 0; i < length; i++)
			{
				char c = Path[i];
				if (c == '/' || c == '\\')
				{
					span[i] = '_';
				}
				else
				{
					span[i] = char.ToLowerInvariant(c);
				}
			}
			if (InfoMap.TryGetValue(span, out Info))
			{
				if (Info == null)
				{
					string text = new string(span);
					Info = Resources.Load<exTextureInfo>("TextureInfo/" + text);
					InfoMap[text] = Info;
				}
				InfoMap.Add(Path, Info);
				return true;
			}
			span = array.AsSpan(0, length + 24);
			if (InfoMap.TryGetValue(span, out Info))
			{
				if (Info == null)
				{
					string text2 = new string(span);
					Info = Resources.Load<exTextureInfo>("TextureInfo/" + text2);
					InfoMap[text2] = Info;
				}
				InfoMap.Add(Path, Info);
				return true;
			}
			return false;
		}

		public static bool HasTextureInfo(string Path)
		{
			if (Path == null)
			{
				return false;
			}
			if (InfoMap == null)
			{
				if (GameManager.IsOnUIContext())
				{
					Initialize();
				}
				else
				{
					GameManager.Instance.uiQueue.awaitTask(Initialize);
				}
			}
			if (InfoMap.ContainsKey(Path))
			{
				return true;
			}
			int length = Path.Length;
			char[] array = KeyBuffer.GetArray(length + 24);
			Span<char> span = array.AsSpan(24, length);
			for (int i = 0; i < length; i++)
			{
				char c = Path[i];
				if (c == '/' || c == '\\')
				{
					span[i] = '_';
				}
				else
				{
					span[i] = char.ToLowerInvariant(c);
				}
			}
			if (InfoMap.ContainsKey(span))
			{
				return true;
			}
			return InfoMap.ContainsKey(array.AsSpan(0, length + 24));
		}

		public static GameObject CreateEmptySprite()
		{
			return CloneSpritePrefab();
		}

		public static GameObject CreateSplitSprite(string Path)
		{
			GameObject gameObject = CloneSplitSpritePrefab();
			gameObject.GetComponent<ex3DSprite2>().anchor = Anchor.MidCenter;
			gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
			gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
			return gameObject;
		}

		public static GameObject CreateCollidableSprite(string Path, Anchor _Anchor, bool bReusable = false)
		{
			GameObject gameObject = CloneSpritePrefab();
			gameObject.GetComponent<ex3DSprite2>().anchor = _Anchor;
			gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
			gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
			gameObject.GetComponent<ex3DSprite2>().bCollide = true;
			return gameObject;
		}

		public static GameObject CreateCollidableSprite(string Path, bool bReusable = false)
		{
			GameObject gameObject = CloneSpritePrefab();
			gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
			gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
			gameObject.GetComponent<ex3DSprite2>().bCollide = true;
			return gameObject;
		}

		public static GameObject CreateSprite(string Path, Anchor _Anchor, bool bReusable = false)
		{
			GameObject gameObject = CloneSpritePrefab();
			gameObject.GetComponent<ex3DSprite2>().anchor = _Anchor;
			gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
			gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
			return gameObject;
		}

		public static GameObject CreateSprite(string Path, Color Foreground, bool bReusable = false)
		{
			GameObject gameObject = CloneSpritePrefab();
			gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
			gameObject.GetComponent<ex3DSprite2>().color = Foreground;
			gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
			return gameObject;
		}

		public static GameObject CreateSprite(string Path, Color Foreground, Color Background, bool bReusable = false)
		{
			GameObject gameObject = CloneSpritePrefab();
			gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
			gameObject.GetComponent<ex3DSprite2>().color = Foreground;
			gameObject.GetComponent<ex3DSprite2>().backcolor = Background;
			return gameObject;
		}

		public static GameObject CreateSprite(string Path, bool bReusable = false)
		{
			GameObject gameObject = CloneSpritePrefab();
			gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
			gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
			return gameObject;
		}

		public static ex3DSprite2 GetPooledSprite(string Path, Color Foreground, Color Background, Color Detail, bool HFlip = false, bool VFlip = false)
		{
			ex3DSprite2 component = PooledPrefabManager.Instantiate("KoboldBaseSprite", null).GetComponent<ex3DSprite2>();
			component.textureInfo = GetTextureInfo(Path);
			component.shader = GetShaderMode(component.textureInfo.ShaderMode);
			component.color = Foreground;
			component.backcolor = Background;
			component.detailcolor = Detail;
			BoxCollider component2 = component.GetComponent<BoxCollider>();
			if (HFlip)
			{
				if (VFlip)
				{
					component.transform.localScale = new Vector3(-1f, -1f, -1f);
					component2.size = new Vector3(0f - Math.Abs(component2.size.x), 0f - Math.Abs(component2.size.y), 0f - Math.Abs(component2.size.z));
				}
				else
				{
					component.transform.localScale = new Vector3(-1f, 1f, 1f);
					component2.size = new Vector3(0f - Math.Abs(component2.size.x), Math.Abs(component2.size.y), Math.Abs(component2.size.z));
				}
			}
			else if (VFlip)
			{
				component.transform.localScale = new Vector3(1f, -1f, 1f);
				component2.size = new Vector3(Math.Abs(component2.size.x), 0f - Math.Abs(component2.size.y), Math.Abs(component2.size.z));
			}
			else
			{
				component.transform.localScale = new Vector3(1f, 1f, 1f);
				component2.size = new Vector3(Math.Abs(component2.size.x), Math.Abs(component2.size.y), Math.Abs(component2.size.z));
			}
			return component;
		}

		public static ex3DSprite2 GetPooledSprite(IRenderable Tile, bool Transparent = false)
		{
			ColorChars colorChars = Tile.getColorChars();
			string text = Tile.getTile();
			Color color = ConsoleLib.Console.ColorUtility.ColorMap.GetValue(colorChars.foreground).WithAlpha((!Transparent || colorChars.foreground != 'k') ? 1 : 0);
			Color color2 = ConsoleLib.Console.ColorUtility.ColorMap.GetValue(colorChars.background).WithAlpha((!Transparent || colorChars.background != 'k') ? 1 : 0);
			Color color3 = ConsoleLib.Console.ColorUtility.ColorMap.GetValue(colorChars.detail).WithAlpha((!Transparent || colorChars.detail != 'k') ? 1 : 0);
			if (Globals.RenderMode == RenderModeType.Text || text.IsNullOrEmpty())
			{
				int num = 32;
				string renderString = Tile.getRenderString();
				if (!renderString.IsNullOrEmpty())
				{
					num = renderString[0];
					if (num < 0 || num > 255)
					{
						num = 32;
					}
				}
				text = $"assets_content_textures_text_{num}.bmp";
				color3 = color;
				color = color2;
				color2 = color3;
			}
			return GetPooledSprite(text, color, color2, color3, Tile.getHFlip(), Tile.getVFlip());
		}

		public static void Return(ex3DSprite2 Sprite)
		{
			PooledPrefabManager.Return(Sprite.gameObject);
		}

		public static void Return(GameObject Object)
		{
			PooledPrefabManager.Return(Object);
		}
	}
}
