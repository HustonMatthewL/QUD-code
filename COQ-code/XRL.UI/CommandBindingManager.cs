using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace XRL.UI
{
	[HasModSensitiveStaticCache]
	public static class CommandBindingManager
	{
		[Serializable]
		public class SerializedBindings
		{
			public int Version = 1;

			public Dictionary<string, List<string>> Commands = new Dictionary<string, List<string>>();
		}

		[Flags]
		public enum ModifierFlags
		{
			none = 0,
			ctrl = 1,
			alt = 2,
			shift = 4,
			gamepadAlt = 8
		}

		[ModSensitiveStaticCache(false)]
		public static Dictionary<string, List<GameCommand>> CommandsByCategory;

		[ModSensitiveStaticCache(false)]
		public static Dictionary<string, GameCommand> CommandsByID;

		[ModSensitiveStaticCache(false)]
		public static List<string> CategoriesInOrder;

		[ModSensitiveStaticCache(true)]
		public static Dictionary<string, NavCategory> NavCategories;

		public static KeyMap CurrentMap;

		private static Dictionary<string, ControlManager.InputDeviceType> ControlPrefixType = new Dictionary<string, ControlManager.InputDeviceType>(StringComparer.InvariantCultureIgnoreCase)
		{
			{
				"<Keyboard>",
				ControlManager.InputDeviceType.Keyboard
			},
			{
				"<Mouse>",
				ControlManager.InputDeviceType.Keyboard
			},
			{
				"<Pointer>",
				ControlManager.InputDeviceType.Keyboard
			},
			{
				"<Gamepad>",
				ControlManager.InputDeviceType.Gamepad
			}
		};

		private static string[] default_exclusions = new string[1] { "Chargen" };

		public static readonly string[] CoreLoopExcludes = new string[2] { "Chargen", "Actions" };

		private static Dictionary<string, Action<XmlDataHelper>> _Nodes = new Dictionary<string, Action<XmlDataHelper>>
		{
			{ "commands", HandleNodes },
			{ "command", HandleCommandNode },
			{ "navcategory", HandleNavCategoryNode }
		};

		private static Dictionary<string, Action<XmlDataHelper>> _CommandSubnodes = new Dictionary<string, Action<XmlDataHelper>>
		{
			{ "keyboardBind", HandleKeyboardBind },
			{ "gamepadBind", HandleGamepadBind },
			{ "mouseBind", HandleMouseBind }
		};

		private static Dictionary<string, Action<XmlDataHelper>> _NavCategorySubnodes = new Dictionary<string, Action<XmlDataHelper>> { { "layer", HandleLayerNode } };

		private static NavCategory CurrentLoadingNavCategory;

		private static GameCommand CurrentParsingCommand;

		public static InputActionMap currentActionMap;

		public static Dictionary<string, HashSet<Keys>> ConsumedKeyCodesByLayer;

		public static Dictionary<string, CommandBinding> CommandBindings;

		public static Dictionary<string, CommandBindingLayer> CommandBindingLayers;

		public static List<CommandBindingLayer> CommandBindingLayersList;

		public static List<CommandBinding> AutoDownAdventureInputActions;

		public static List<CommandBinding> AutoDownInputActions;

		public static List<CommandBinding> AutoDownPassInputActions;

		public static List<CommandBinding> AutoDownUIInputActions;

		public static List<string> AutoRepeatInputActions;

		private static KeyMap _legacyKeymap;

		public static int BindingRefreshIndex = 0;

		private static InputActionAsset dummyAsset = null;

		public static Dictionary<string, ModifierFlags> keyboardModifierFlags = new Dictionary<string, ModifierFlags>
		{
			{
				"<Keyboard>/ctrl",
				ModifierFlags.ctrl
			},
			{
				"<Keyboard>/shift",
				ModifierFlags.shift
			},
			{
				"<Keyboard>/alt",
				ModifierFlags.alt
			},
			{
				"<Keyboard>/leftCtrl",
				ModifierFlags.ctrl
			},
			{
				"<Keyboard>/rightCtrl",
				ModifierFlags.ctrl
			},
			{
				"<Keyboard>/leftShift",
				ModifierFlags.shift
			},
			{
				"<Keyboard>/rightShift",
				ModifierFlags.shift
			},
			{
				"<Keyboard>/leftAlt",
				ModifierFlags.alt
			},
			{
				"<Keyboard>/rightAlt",
				ModifierFlags.alt
			}
		};

		private static string[] KeyboardModifiers = new string[10] { "<Keyboard>/ctrl", "<Keyboard>/shift", "<Keyboard>/alt", "<Keyboard>/leftCtrl", "<Keyboard>/rightCtrl", "<Keyboard>/leftShift", "<Keyboard>/rightShift", "<Keyboard>/leftAlt", "<Keyboard>/rightAlt", "<Keyboard>/anyKey" };

		private static string[] IgnoredControls = new string[8] { "<Gamepad>/leftStick/x", "<Gamepad>/leftStick/y", "<Gamepad>/rightStick/x", "<Gamepad>/rightStick/y", "<Gamepad>/dpad/x", "<Gamepad>/dpad/y", "<Gamepad>/leftTriggerButton", "<Gamepad>/rightTriggerButton" };

		public static Dictionary<Keys, string> KeysToBindings = new Dictionary<Keys, string>
		{
			{
				Keys.None,
				null
			},
			{
				Keys.Back,
				"<Keyboard>/backspace"
			},
			{
				Keys.Tab,
				"<Keyboard>/tab"
			},
			{
				Keys.Enter,
				"<Keyboard>/enter"
			},
			{
				Keys.ShiftKey,
				"<Keyboard>/shift"
			},
			{
				Keys.ControlKey,
				"<Keyboard>/ctrl"
			},
			{
				Keys.Menu,
				"<Keyboard>/alt"
			},
			{
				Keys.Pause,
				"<Keyboard>/pause"
			},
			{
				Keys.CapsLock,
				"<Keyboard>/capsLock"
			},
			{
				Keys.Escape,
				"<Keyboard>/escape"
			},
			{
				Keys.Space,
				"<Keyboard>/space"
			},
			{
				Keys.Prior,
				"<Keyboard>/pageup"
			},
			{
				Keys.Next,
				"<Keyboard>/pagedown"
			},
			{
				Keys.End,
				"<Keyboard>/end"
			},
			{
				Keys.Home,
				"<Keyboard>/home"
			},
			{
				Keys.Left,
				"<Keyboard>/leftArrow"
			},
			{
				Keys.Up,
				"<Keyboard>/upArrow"
			},
			{
				Keys.Right,
				"<Keyboard>/rightArrow"
			},
			{
				Keys.Down,
				"<Keyboard>/downArrow"
			},
			{
				Keys.PrintScreen,
				"<Keyboard>/printScreen"
			},
			{
				Keys.Insert,
				"<Keyboard>/insert"
			},
			{
				Keys.Delete,
				"<Keyboard>/delete"
			},
			{
				Keys.D0,
				"<Keyboard>/0"
			},
			{
				Keys.D1,
				"<Keyboard>/1"
			},
			{
				Keys.D2,
				"<Keyboard>/2"
			},
			{
				Keys.D3,
				"<Keyboard>/3"
			},
			{
				Keys.D4,
				"<Keyboard>/4"
			},
			{
				Keys.D5,
				"<Keyboard>/5"
			},
			{
				Keys.D6,
				"<Keyboard>/6"
			},
			{
				Keys.D7,
				"<Keyboard>/7"
			},
			{
				Keys.D8,
				"<Keyboard>/8"
			},
			{
				Keys.D9,
				"<Keyboard>/9"
			},
			{
				Keys.A,
				"<Keyboard>/a"
			},
			{
				Keys.B,
				"<Keyboard>/b"
			},
			{
				Keys.C,
				"<Keyboard>/c"
			},
			{
				Keys.D,
				"<Keyboard>/d"
			},
			{
				Keys.E,
				"<Keyboard>/e"
			},
			{
				Keys.F,
				"<Keyboard>/f"
			},
			{
				Keys.G,
				"<Keyboard>/g"
			},
			{
				Keys.H,
				"<Keyboard>/h"
			},
			{
				Keys.I,
				"<Keyboard>/i"
			},
			{
				Keys.J,
				"<Keyboard>/j"
			},
			{
				Keys.K,
				"<Keyboard>/k"
			},
			{
				Keys.L,
				"<Keyboard>/l"
			},
			{
				Keys.M,
				"<Keyboard>/m"
			},
			{
				Keys.N,
				"<Keyboard>/n"
			},
			{
				Keys.O,
				"<Keyboard>/o"
			},
			{
				Keys.P,
				"<Keyboard>/p"
			},
			{
				Keys.Q,
				"<Keyboard>/q"
			},
			{
				Keys.R,
				"<Keyboard>/r"
			},
			{
				Keys.S,
				"<Keyboard>/s"
			},
			{
				Keys.T,
				"<Keyboard>/t"
			},
			{
				Keys.U,
				"<Keyboard>/u"
			},
			{
				Keys.V,
				"<Keyboard>/v"
			},
			{
				Keys.W,
				"<Keyboard>/w"
			},
			{
				Keys.X,
				"<Keyboard>/x"
			},
			{
				Keys.Y,
				"<Keyboard>/y"
			},
			{
				Keys.Z,
				"<Keyboard>/z"
			},
			{
				Keys.LWin,
				"<Keyboard>/leftMeta"
			},
			{
				Keys.RWin,
				"<Keyboard>/rightMeta"
			},
			{
				Keys.Apps,
				"<Keyboard>/contextMenu"
			},
			{
				Keys.NumPad0,
				"<Keyboard>/numpad0"
			},
			{
				Keys.NumPad1,
				"<Keyboard>/numpad1"
			},
			{
				Keys.NumPad2,
				"<Keyboard>/numpad2"
			},
			{
				Keys.NumPad3,
				"<Keyboard>/numpad3"
			},
			{
				Keys.NumPad4,
				"<Keyboard>/numpad4"
			},
			{
				Keys.NumPad5,
				"<Keyboard>/numpad5"
			},
			{
				Keys.NumPad6,
				"<Keyboard>/numpad6"
			},
			{
				Keys.NumPad7,
				"<Keyboard>/numpad7"
			},
			{
				Keys.NumPad8,
				"<Keyboard>/numpad8"
			},
			{
				Keys.NumPad9,
				"<Keyboard>/numpad9"
			},
			{
				Keys.Multiply,
				"<Keyboard>/numpadMultiply"
			},
			{
				Keys.Divide,
				"<Keyboard>/numpadDivide"
			},
			{
				Keys.Add,
				"<Keyboard>/numpadPlus"
			},
			{
				Keys.Subtract,
				"<Keyboard>/numpadMinus"
			},
			{
				Keys.Decimal,
				"<Keyboard>/numpadPeriod"
			},
			{
				Keys.F1,
				"<Keyboard>/F1"
			},
			{
				Keys.F2,
				"<Keyboard>/F2"
			},
			{
				Keys.F3,
				"<Keyboard>/F3"
			},
			{
				Keys.F4,
				"<Keyboard>/F4"
			},
			{
				Keys.F5,
				"<Keyboard>/F5"
			},
			{
				Keys.F6,
				"<Keyboard>/F6"
			},
			{
				Keys.F7,
				"<Keyboard>/F7"
			},
			{
				Keys.F8,
				"<Keyboard>/F8"
			},
			{
				Keys.F9,
				"<Keyboard>/F9"
			},
			{
				Keys.F10,
				"<Keyboard>/F10"
			},
			{
				Keys.F11,
				"<Keyboard>/F11"
			},
			{
				Keys.F12,
				"<Keyboard>/F12"
			},
			{
				Keys.NumLock,
				"<Keyboard>/numLock"
			},
			{
				Keys.Scroll,
				"<Keyboard>/scrollLock"
			},
			{
				Keys.LShiftKey,
				"<Keyboard>/leftShift"
			},
			{
				Keys.RShiftKey,
				"<Keyboard>/rightShift"
			},
			{
				Keys.LControlKey,
				"<Keyboard>/leftCtrl"
			},
			{
				Keys.RControlKey,
				"<Keyboard>/rightCtrl"
			},
			{
				Keys.LMenu,
				"<Keyboard>/leftMeta"
			},
			{
				Keys.RMenu,
				"<Keyboard>/rightMeta"
			},
			{
				Keys.Oem1,
				"<Keyboard>/semicolon"
			},
			{
				Keys.Oemplus,
				"<Keyboard>/equals"
			},
			{
				Keys.Oemcomma,
				"<Keyboard>/comma"
			},
			{
				Keys.OemPeriod,
				"<Keyboard>/period"
			},
			{
				Keys.OemQuestion,
				"<Keyboard>/slash"
			},
			{
				Keys.Oemtilde,
				"<Keyboard>/backquote"
			},
			{
				Keys.Oem4,
				"<Keyboard>/leftBracket"
			},
			{
				Keys.Oem6,
				"<Keyboard>/rightBracket"
			},
			{
				Keys.OemPipe,
				"<Keyboard>/backslash"
			},
			{
				Keys.Oem7,
				"<Keyboard>/quote"
			},
			{
				Keys.Oem102,
				"<Keyboard>/backslash"
			}
		};

		private static InputAction ctrlAction;

		private static InputAction shiftAction;

		private static InputAction altAction;

		private static int _FrameKeyboardModifier = -1;

		private static int _FrameGamepadModifier = -1;

		public static InputAction gamepadAltAction;

		private static int FrameKeyboardModifier
		{
			get
			{
				if (_FrameKeyboardModifier != -1)
				{
					return _FrameKeyboardModifier;
				}
				_FrameKeyboardModifier = 0;
				InputAction inputAction = ctrlAction;
				if (inputAction != null && inputAction.IsPressed())
				{
					_FrameKeyboardModifier |= 1;
				}
				InputAction inputAction2 = shiftAction;
				if (inputAction2 != null && inputAction2.IsPressed())
				{
					_FrameKeyboardModifier |= 4;
				}
				InputAction inputAction3 = altAction;
				if (inputAction3 != null && inputAction3.IsPressed())
				{
					_FrameKeyboardModifier |= 2;
				}
				return _FrameKeyboardModifier;
			}
		}

		private static int FrameGamepadModifier
		{
			get
			{
				if (_FrameGamepadModifier != -1)
				{
					return _FrameGamepadModifier;
				}
				_FrameGamepadModifier = 0;
				InputAction inputAction = gamepadAltAction;
				if (inputAction != null && inputAction.IsPressed())
				{
					_FrameGamepadModifier |= 8;
				}
				return _FrameGamepadModifier;
			}
		}

		public static ControlManager.InputDeviceType GetInputDeviceTypeFromBindingPath(string path)
		{
			if (path != null && path.Contains("Gamepad"))
			{
				return ControlManager.InputDeviceType.Gamepad;
			}
			foreach (KeyValuePair<string, ControlManager.InputDeviceType> item in ControlPrefixType)
			{
				if (path.StartsWith(item.Key, StringComparison.InvariantCultureIgnoreCase))
				{
					return item.Value;
				}
			}
			return ControlManager.InputDeviceType.Unknown;
		}

		public static IEnumerable<string> GetCommandBindings(string cmd)
		{
			if (!CommandBindings.TryGetValue(cmd, out var action))
			{
				yield break;
			}
			for (int x = 0; x < action._inputAction.bindings.Count; x++)
			{
				if (!action._inputAction.bindings[x].isPartOfComposite)
				{
					yield return action._inputAction.GetBindingDisplayString(x);
				}
			}
		}

		public static void GetCommandBindings(string cmd, ControlManager.InputDeviceType controllerType, out string bind1)
		{
			IEnumerator<string> enumerator = GetCommandBindings(cmd, controllerType).GetEnumerator();
			bind1 = null;
			if (enumerator.MoveNext())
			{
				bind1 = enumerator.Current;
			}
			enumerator.Dispose();
		}

		public static void GetCommandBindings(string cmd, ControlManager.InputDeviceType controllerType, out string bind1, out string bind2)
		{
			IEnumerator<string> enumerator = GetCommandBindings(cmd, controllerType).GetEnumerator();
			bind1 = null;
			bind2 = null;
			if (enumerator.MoveNext())
			{
				bind1 = enumerator.Current;
			}
			if (enumerator.MoveNext())
			{
				bind2 = enumerator.Current;
			}
			enumerator.Dispose();
		}

		public static void GetCommandBindings(string cmd, ControlManager.InputDeviceType controllerType, out string bind1, out string bind2, out string bind3, out string bind4)
		{
			IEnumerator<string> enumerator = GetCommandBindings(cmd, controllerType).GetEnumerator();
			bind1 = null;
			bind2 = null;
			bind3 = null;
			bind4 = null;
			if (enumerator.MoveNext())
			{
				bind1 = enumerator.Current;
			}
			if (enumerator.MoveNext())
			{
				bind2 = enumerator.Current;
			}
			if (enumerator.MoveNext())
			{
				bind3 = enumerator.Current;
			}
			if (enumerator.MoveNext())
			{
				bind4 = enumerator.Current;
			}
			enumerator.Dispose();
		}

		public static IEnumerable<string> GetCommandBindings(string cmd, ControlManager.InputDeviceType byType)
		{
			if (!CommandBindings.TryGetValue(cmd, out var action))
			{
				yield break;
			}
			for (int x = 0; x < action._inputAction.bindings.Count; x++)
			{
				if (action._inputAction.bindings[x].isPartOfComposite)
				{
					continue;
				}
				if (action._inputAction.bindings[x].isComposite)
				{
					if (GetInputDeviceTypeFromBindingPath(action._inputAction.bindings[x + 1].path) != byType)
					{
						continue;
					}
				}
				else if (GetInputDeviceTypeFromBindingPath(action._inputAction.bindings[x].path) != byType)
				{
					continue;
				}
				yield return action._inputAction.GetBindingDisplayString(x);
			}
		}

		public static IEnumerable<GameCommand> GetBindingCommands(InputBinding binding, string layer = null)
		{
			foreach (KeyValuePair<string, CommandBinding> commandBinding in CommandBindings)
			{
				if (commandBinding.Value._inputAction.GetBindingIndex(binding) >= 0)
				{
					yield return CommandsByID[commandBinding.Key];
				}
			}
		}

		public static string GetCommandMappedTo(int key, string[] layersToInclude = null)
		{
			return CurrentMap.GetCommandMappedTo(key, layersToInclude);
		}

		public static string GetCommandFromKey(int c)
		{
			foreach (KeyValuePair<string, Dictionary<int, string>> item in CurrentMap.PrimaryMapKeyToCommandLayer)
			{
				if (item.Value.ContainsKey(c))
				{
					return item.Value[c];
				}
			}
			foreach (KeyValuePair<string, Dictionary<int, string>> item2 in CurrentMap.SecondaryMapKeyToCommandLayer)
			{
				if (item2.Value.ContainsKey(c))
				{
					return item2.Value[c];
				}
			}
			return "CmdUnknown";
		}

		public static string GetCommandFromKey(Keys k)
		{
			return GetCommandFromKey((int)k);
		}

		public static string MapKeyToCommand(int Meta, string[] exclusions = null)
		{
			if (exclusions == null)
			{
				exclusions = default_exclusions;
			}
			foreach (KeyValuePair<string, Dictionary<int, string>> item in CurrentMap.PrimaryMapKeyToCommandLayer)
			{
				if (!exclusions.Contains(item.Key) && item.Value.ContainsKey(Meta))
				{
					return item.Value[Meta];
				}
			}
			foreach (KeyValuePair<string, Dictionary<int, string>> item2 in CurrentMap.SecondaryMapKeyToCommandLayer)
			{
				if (!exclusions.Contains(item2.Key) && item2.Value.ContainsKey(Meta))
				{
					return item2.Value[Meta];
				}
			}
			return "CmdUnknown";
		}

		public static string GetNextCommand(string[] exclusions = null)
		{
			return MapKeyToCommand(ConsoleLib.Console.Keyboard.getmeta(MapDirectionToArrows: false), exclusions);
		}

		public static (int, int) GetAllKeysFromCommand(string Cmd)
		{
			int value = -1;
			int value2 = -1;
			if (CurrentMap?.PrimaryMapCommandToKeyLayer != null)
			{
				using Dictionary<string, Dictionary<string, int>>.Enumerator enumerator = CurrentMap.PrimaryMapCommandToKeyLayer.GetEnumerator();
				while (enumerator.MoveNext() && !enumerator.Current.Value.TryGetValue(Cmd, out value))
				{
				}
			}
			if (CurrentMap?.SecondaryMapCommandToKeyLayer != null)
			{
				using Dictionary<string, Dictionary<string, int>>.Enumerator enumerator = CurrentMap.SecondaryMapCommandToKeyLayer.GetEnumerator();
				while (enumerator.MoveNext() && !enumerator.Current.Value.TryGetValue(Cmd, out value2))
				{
				}
			}
			return (value, value2);
		}

		public static int GetKeyFromCommand(string Cmd)
		{
			if (CurrentMap?.PrimaryMapCommandToKeyLayer != null)
			{
				foreach (KeyValuePair<string, Dictionary<string, int>> item in CurrentMap.PrimaryMapCommandToKeyLayer)
				{
					if (item.Value.TryGetValue(Cmd, out var value))
					{
						return value;
					}
				}
			}
			if (CurrentMap?.SecondaryMapCommandToKeyLayer != null)
			{
				foreach (KeyValuePair<string, Dictionary<string, int>> item2 in CurrentMap.SecondaryMapCommandToKeyLayer)
				{
					if (item2.Value.TryGetValue(Cmd, out var value2))
					{
						return value2;
					}
				}
			}
			return 0;
		}

		public static string GetLegacyKeymapPath()
		{
			return DataManager.SyncedPath(Environment.UserName + ".Keymap.json");
		}

		public static string GetCurrentKeymapPath()
		{
			return DataManager.SyncedPath(Environment.UserName + ".Keymap2.json");
		}

		public static async Task SaveCurrentKeymapAsync()
		{
			await The.UiContext;
			SaveCurrentKeymap();
		}

		public static void SaveCurrentKeymap()
		{
			SaveKeymap(CurrentMap, GetCurrentKeymapPath());
		}

		public static void SaveKeymap(KeyMap Map, string FileName)
		{
			foreach (KeyValuePair<string, CommandBinding> commandBinding in CommandBindings)
			{
				List<string> list = commandBinding.Value.SerializedFormat();
				if (list != null)
				{
					Map.CommandToSerializedInputBindings[commandBinding.Key] = list;
				}
			}
			Map.GamepadAltBind = (Map.CommandToSerializedInputBindings.ContainsKey("GamepadAlt") ? Map.CommandToSerializedInputBindings["GamepadAlt"].First() : "<Gamepad>/leftTrigger");
			File.WriteAllText(FileName, JsonConvert.SerializeObject(Map));
		}

		public static async Task LoadCurrentKeymapAsync(bool restoreLayers = false)
		{
			await The.UiContext;
			LoadCurrentKeymap(restoreLayers);
		}

		public static void LoadCurrentKeymap(bool restoreLayers = false)
		{
			LoadCurrentKeymap(GetCurrentKeymapPath(), AllowLegacyUpgrade: false, restoreLayers);
		}

		public static void LoadCurrentKeymap(string FileName, bool AllowLegacyUpgrade = true, bool restoreLayers = false, string targetSet = null)
		{
			CurrentMap = LoadKeymap(FileName) ?? new KeyMap();
			InitializeInputManager(AllowLegacyUpgrade, restoreLayers, targetSet);
		}

		public static async Task RestoreDefaults()
		{
			int num = await Popup.PickOptionAsync("Selected Bind Set", null, "", new string[2] { "Normal", "HJKL" });
			if (num < 0)
			{
				return;
			}
			string targetSet = "";
			if (num == 1)
			{
				targetSet = "hjkl";
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, CommandBindingLayer> commandBindingLayer in CommandBindingLayers)
			{
				if (commandBindingLayer.Value.enabled)
				{
					list.Add(commandBindingLayer.Key);
				}
			}
			LoadCurrentKeymap(DataManager.FilePath("DefaultKeymap.json"), AllowLegacyUpgrade: false, restoreLayers: false, targetSet);
			foreach (KeyValuePair<string, CommandBindingLayer> commandBindingLayer2 in CommandBindingLayers)
			{
				if (list.Contains(commandBindingLayer2.Key))
				{
					commandBindingLayer2.Value.Enable();
				}
				else
				{
					commandBindingLayer2.Value.Disable();
				}
			}
		}

		public static Predicate<GameCommand> ConflictChecker(GameCommand with)
		{
			if (with.IgnoreConflicts)
			{
				return (GameCommand command) => false;
			}
			if (Options.GetOption("OptionDisableBindConflictChecking") == "Yes")
			{
				return (GameCommand command) => false;
			}
			HashSet<string> Conflicts = new HashSet<string>();
			foreach (NavCategory value in NavCategories.Values)
			{
				if (!value.Layers.Contains(with.Layer))
				{
					continue;
				}
				foreach (string layer in value.Layers)
				{
					Conflicts.Add(layer);
				}
			}
			return delegate(GameCommand command)
			{
				if (command != with && command.Bindable && !command.IgnoreConflicts && Conflicts.Contains(command.Layer))
				{
					List<string> canShareBindsWith = command.CanShareBindsWith;
					if (canShareBindsWith == null || !canShareBindsWith.Contains(with.ID))
					{
						return !(with.CanShareBindsWith?.Contains(command.ID) ?? false);
					}
				}
				return false;
			};
		}

		public static Predicate<GameCommand> DynamicLayerConflictChecker()
		{
			HashSet<string> Conflicts = new HashSet<string>();
			foreach (NavCategory value in NavCategories.Values)
			{
				if (!value.Layers.Contains("Dynamic"))
				{
					continue;
				}
				foreach (string layer in value.Layers)
				{
					Conflicts.Add(layer);
				}
			}
			return (GameCommand command) => command.Bindable && !command.IgnoreConflicts && Conflicts.Contains(command.Layer);
		}

		public static async Task RestoreDefaultsAsync()
		{
			await The.UiContext;
			await RestoreDefaults();
		}

		public static KeyMap LoadKeymap(string FileName)
		{
			if (!File.Exists(FileName))
			{
				return null;
			}
			KeyMap keyMap = JsonConvert.DeserializeObject<KeyMap>(File.ReadAllText(FileName));
			if (keyMap.CommandToSerializedInputBindings == null)
			{
				keyMap.CommandToSerializedInputBindings = new Dictionary<string, List<string>>();
			}
			return keyMap;
		}

		public static void SaveLegacyKeymap(KeyMap Map, string FileName)
		{
			FileStream fileStream = File.OpenWrite(FileName);
			((IFormatter)new BinaryFormatter()).Serialize((Stream)fileStream, (object)Map);
			fileStream.Close();
		}

		public static KeyMap LoadLegacyKeymap(string FileName)
		{
			Stream stream = File.OpenRead(FileName);
			KeyMap keyMap = ((IFormatter)new BinaryFormatter()).Deserialize(stream) as KeyMap;
			keyMap.PrimaryMapCommandToKeyLayer = new Dictionary<string, Dictionary<string, int>>();
			keyMap.PrimaryMapKeyToCommandLayer = new Dictionary<string, Dictionary<int, string>>();
			keyMap.SecondaryMapKeyToCommandLayer = new Dictionary<string, Dictionary<int, string>>();
			keyMap.SecondaryMapCommandToKeyLayer = new Dictionary<string, Dictionary<string, int>>();
			keyMap.PrimaryMapCommandToKeyLayer.Add("*default", keyMap.PrimaryMapCommandToKey);
			keyMap.PrimaryMapKeyToCommandLayer.Add("*default", keyMap.PrimaryMapKeyToCommand);
			keyMap.SecondaryMapKeyToCommandLayer.Add("*default", keyMap.SecondaryMapKeyToCommand);
			keyMap.SecondaryMapCommandToKeyLayer.Add("*default", keyMap.SecondaryMapCommandToKey);
			keyMap.PrimaryMapCommandToKey = null;
			keyMap.PrimaryMapKeyToCommand = null;
			keyMap.SecondaryMapCommandToKey = null;
			keyMap.SecondaryMapKeyToCommand = null;
			stream.Close();
			keyMap.upgradeLayers();
			CleanKeymapCommands(keyMap);
			return keyMap;
		}

		public static void CleanKeymapCommands(KeyMap Map)
		{
		}

		public static void HandleNodes(XmlDataHelper xml)
		{
			xml.HandleNodes(_Nodes);
		}

		public static void HandleNavCategoryNode(XmlDataHelper xml)
		{
			string text = xml.ParseAttribute<string>("ID", null, required: true);
			if (text == null)
			{
				xml.DoneWithElement();
				return;
			}
			if (!NavCategories.TryGetValue(text, out CurrentLoadingNavCategory))
			{
				CurrentLoadingNavCategory = new NavCategory
				{
					ID = text
				};
				NavCategories.Add(text, CurrentLoadingNavCategory);
			}
			CurrentLoadingNavCategory.Layers = new List<string>();
			xml.HandleNodes(_NavCategorySubnodes);
			CurrentLoadingNavCategory = null;
		}

		public static void HandleLayerNode(XmlDataHelper xml)
		{
			if (CurrentLoadingNavCategory == null)
			{
				xml.ParseWarning("Layer node detected outside nav category context");
				xml.DoneWithElement();
				return;
			}
			string text = xml.ParseAttribute<string>("ID", null, required: true);
			if (text != null)
			{
				CurrentLoadingNavCategory.Layers.Add(text);
			}
			xml.DoneWithElement();
		}

		public static void HandleKeyboardBind(XmlDataHelper xml)
		{
			GameCommand.KeyboardBinding keyboardBinding = new GameCommand.KeyboardBinding();
			keyboardBinding.Key = xml.ParseAttribute<string>("Key", null, required: true);
			keyboardBinding.Modifier = xml.ParseAttribute<string>("Modifier", null);
			keyboardBinding.Set = xml.ParseAttribute<string>("Set", null);
			CurrentParsingCommand.keyboardBindings.Add(keyboardBinding);
		}

		public static void HandleGamepadBind(XmlDataHelper xml)
		{
			GameCommand.GamepadBinding gamepadBinding = new GameCommand.GamepadBinding();
			gamepadBinding.Button = xml.ParseAttribute<string>("Button", null, required: true);
			gamepadBinding.Alt = xml.ParseAttribute("Alt", defaultValue: false);
			CurrentParsingCommand.gamepadBindings.Add(gamepadBinding);
		}

		public static void HandleMouseBind(XmlDataHelper xml)
		{
			GameCommand.MouseBinding mouseBinding = new GameCommand.MouseBinding();
			mouseBinding.Button = xml.ParseAttribute<string>("Button", null, required: true);
			mouseBinding.Modifier = xml.ParseAttribute<string>("Modifier", null);
			CurrentParsingCommand.mouseBindings.Add(mouseBinding);
		}

		private static InputActionType ParseCommandType(string s)
		{
			return s switch
			{
				"Button" => InputActionType.Button, 
				"PassThrough" => InputActionType.PassThrough, 
				"Value" => InputActionType.Value, 
				_ => throw new Exception("Unknown command type " + s), 
			};
		}

		public static void HandleCommandNode(XmlDataHelper xml)
		{
			CurrentParsingCommand = new GameCommand();
			CurrentParsingCommand.ID = xml.ParseAttribute<string>("ID", null, required: true);
			CurrentParsingCommand.DisplayText = xml.GetAttribute("DisplayText");
			CurrentParsingCommand.Category = xml.GetAttribute("Category");
			CurrentParsingCommand.Layer = xml.GetAttribute("Layer");
			CurrentParsingCommand.Auto = xml.GetAttribute("Auto");
			CurrentParsingCommand.Instrument = xml.GetAttribute("Instrument");
			CurrentParsingCommand.InputModuleBind = xml.GetAttribute("InputModuleBind");
			CurrentParsingCommand.SkipUpgrade = xml.GetAttribute("SkipUpgrade") == "true";
			CurrentParsingCommand.Bindable = xml.ParseAttribute("Bindable", CurrentParsingCommand.Bindable);
			CurrentParsingCommand.Display = xml.ParseAttribute("Display", CurrentParsingCommand.Display);
			CurrentParsingCommand.Required = xml.ParseAttribute("Required", CurrentParsingCommand.Required);
			CurrentParsingCommand.CanShareBindsWith = xml.ParseAttribute("CanShareBindsWith", CurrentParsingCommand.CanShareBindsWith);
			CurrentParsingCommand.Type = xml.ParseAttribute("Type", CurrentParsingCommand.Type, required: false, ParseCommandType);
			CurrentParsingCommand.UpgradeFrom = xml.ParseAttribute("UpgradeFrom", CurrentParsingCommand.ID);
			if (string.IsNullOrEmpty(CurrentParsingCommand.Layer))
			{
				CurrentParsingCommand.Layer = "*default";
			}
			xml.HandleNodes(_CommandSubnodes);
			if (!CommandsByCategory.ContainsKey(CurrentParsingCommand.Category))
			{
				CommandsByCategory.Add(CurrentParsingCommand.Category, new List<GameCommand>());
			}
			CommandsByCategory[CurrentParsingCommand.Category].Add(CurrentParsingCommand);
			if (CommandsByID.ContainsKey(CurrentParsingCommand.ID))
			{
				CommandsByID[CurrentParsingCommand.ID] = CurrentParsingCommand;
			}
			else
			{
				CommandsByID.Add(CurrentParsingCommand.ID, CurrentParsingCommand);
			}
			if (!CategoriesInOrder.Contains(CurrentParsingCommand.Category))
			{
				CategoriesInOrder.Add(CurrentParsingCommand.Category);
			}
			CurrentParsingCommand = null;
		}

		public static bool DoesAnyLayerConsumeKeycode(Keys code, List<string> layersToInclude)
		{
			if (layersToInclude == null)
			{
				return DoesAnyActiveLayerConsumeKeycode(code);
			}
			for (int i = 0; i < layersToInclude.Count; i++)
			{
				if (layersToInclude[i] != null && ConsumedKeyCodesByLayer.ContainsKey(layersToInclude[i]) && ConsumedKeyCodesByLayer[layersToInclude[i]].Contains(code))
				{
					return true;
				}
			}
			return false;
		}

		public static bool DoesAnyActiveLayerConsumeKeycode(Keys code)
		{
			return ConsumedKeyCodesByLayer.Any((KeyValuePair<string, HashSet<Keys>> l) => CommandBindingLayers[l.Key].enabled && CommandBindingLayers.ContainsKey(l.Key) && l.Value.Contains(code));
		}

		public static bool AddKeysValueToActionAsBinding(int keycode, InputAction action, bool doExpansions = true)
		{
			if (keycode == 0)
			{
				return false;
			}
			bool num = (keycode & 0x40000) > 0;
			bool flag = (keycode & 0x20000) > 0;
			bool flag2 = (keycode & 0x10000) > 0;
			int num2 = (num ? 262144 : 0) + (flag ? 131072 : 0) + (flag2 ? 65536 : 0);
			List<string> list = new List<string>();
			if (num)
			{
				list.Add("<Keyboard>/alt");
			}
			if (flag)
			{
				list.Add("<Keyboard>/ctrl");
			}
			if (flag2)
			{
				list.Add("<Keyboard>/shift");
			}
			int num3 = keycode & 0xFFFF;
			if (doExpansions)
			{
				List<int> list2 = new List<int>();
				if (num3 == 38)
				{
					list2.Add(104);
				}
				if (num3 == 104)
				{
					list2.Add(38);
				}
				if (num3 == 40)
				{
					list2.Add(98);
				}
				if (num3 == 98)
				{
					list2.Add(40);
				}
				if (num3 == 39)
				{
					list2.Add(102);
				}
				if (num3 == 102)
				{
					list2.Add(39);
				}
				if (num3 == 37)
				{
					list2.Add(100);
				}
				if (num3 == 100)
				{
					list2.Add(37);
				}
				if (num3 == 101 && !flag2)
				{
					list2.Add(65568);
				}
				if (num3 == 103 && !flag2)
				{
					list2.Add(65573);
				}
				if (num3 == 105 && !flag2)
				{
					list2.Add(65574);
				}
				if (num3 == 97 && !flag2)
				{
					list2.Add(65576);
				}
				if (num3 == 99 && !flag2)
				{
					list2.Add(65575);
				}
				if (num3 == 33 && !flag)
				{
					list2.Add(131177);
				}
				if (num3 == 34 && !flag)
				{
					list2.Add(131171);
				}
				foreach (int item in list2)
				{
					AddKeysValueToActionAsBinding(item + num2, action, doExpansions: false);
				}
			}
			if (KeysToBindings.TryGetValue((Keys)num3, out var value))
			{
				if (list.Count == 0)
				{
					action.AddBinding(value);
				}
				else if (list.Count == 1)
				{
					action.AddCompositeBinding("OneModifier").With("Binding", value).With("Modifier", list[0]);
				}
				else if (list.Count == 2)
				{
					action.AddCompositeBinding("TwoModifiers").With("Binding", value).With("Modifier1", list[0])
						.With("Modifier2", list[1]);
				}
				else
				{
					MetricsManager.LogWarning($"Couldn't upgrade keybind for {action.id} because it had too many modifiers.");
				}
				return true;
			}
			return false;
		}

		public static KeyMap GetLegacyKeymap()
		{
			if (_legacyKeymap == null && File.Exists(GetLegacyKeymapPath()))
			{
				_legacyKeymap = LoadKeymap(GetLegacyKeymapPath());
			}
			return _legacyKeymap;
		}

		public static void InitializeInputManager(bool AllowLegacyUpgrade = false, bool restoreLayers = false, string targetSet = null)
		{
			Debug.Log("Initializing input manager...");
			if (targetSet == null)
			{
				targetSet = Options.GetOption("KeybindSet");
			}
			Options.SetOption("KeybindSet", targetSet);
			BindingRefreshIndex++;
			List<string> list = null;
			if (restoreLayers && CommandBindingLayers != null)
			{
				list = (from kv in CommandBindingLayers
					where kv.Value.enabled
					select kv.Key).ToList();
			}
			if (CommandBindings != null)
			{
				foreach (CommandBinding value2 in CommandBindings.Values)
				{
					value2.Disable();
					value2.Dispose();
				}
				CommandBindings.Clear();
				currentActionMap.Disable();
				currentActionMap.Dispose();
				CommandBindingLayers.Clear();
				CommandBindingLayersList.Clear();
			}
			GetLegacyKeymapPath();
			PlayerInputManager.ready = false;
			currentActionMap = new InputActionMap("global");
			currentActionMap.Disable();
			AutoRepeatInputActions = new List<string>();
			AutoDownAdventureInputActions = new List<CommandBinding>();
			AutoDownInputActions = new List<CommandBinding>();
			AutoDownPassInputActions = new List<CommandBinding>();
			AutoDownUIInputActions = new List<CommandBinding>();
			CommandBindings = new Dictionary<string, CommandBinding>();
			CommandBindingLayers = new Dictionary<string, CommandBindingLayer>();
			HashSet<string> hashSet = new HashSet<string>();
			foreach (KeyValuePair<string, GameCommand> item in CommandsByID)
			{
				string layer = item.Value.Layer;
				if (!CommandBindingLayers.ContainsKey(layer))
				{
					CommandBindingLayers.Add(layer, new CommandBindingLayer(layer));
				}
				CommandBindingLayers[layer].Disable();
				InputAction newAction = CommandBindingLayers[layer].AddAction(item.Value.ID, item.Value.Type);
				if (item.Value.Instrument == "Yes")
				{
					newAction.started += delegate
					{
						Debug.Log("started: " + newAction.ToString());
					};
					newAction.canceled += delegate
					{
						Debug.Log("cancelled: " + newAction.ToString());
					};
					newAction.performed += delegate
					{
						Debug.Log("performed: " + newAction.ToString());
					};
				}
				hashSet.Add(item.Value.ID);
				List<string> value = null;
				CurrentMap?.CommandToSerializedInputBindings?.TryGetValue(item.Value.ID, out value);
				CommandBinding commandBinding = CommandBinding.FromSerializedFormat(newAction, item.Value, value, AllowLegacyUpgrade, targetSet);
				CommandBindings.Add(item.Value.ID, commandBinding);
				if (item.Value.Auto == "Repeat")
				{
					AutoRepeatInputActions.Add(item.Value.ID);
				}
				else if (item.Value.Auto == "Down")
				{
					if (item.Value.Layer.Contains("Adventure"))
					{
						AutoDownAdventureInputActions.Add(commandBinding);
					}
					else if (item.Value.Layer == "UI")
					{
						AutoDownUIInputActions.Add(commandBinding);
					}
					else
					{
						AutoDownInputActions.Add(commandBinding);
					}
				}
				else if (item.Value.Auto == "DownPass")
				{
					AutoDownPassInputActions.Add(commandBinding);
				}
				CommandBindingLayers[layer].AddBinding(commandBinding);
			}
			if (AllowLegacyUpgrade && CurrentMap?.CommandToSerializedInputBindings == null)
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (KeyValuePair<string, List<int>> item2 in HotkeyFavorites.LoadForUpgrade())
				{
					foreach (int cmdkv in item2.Value)
					{
						if (!dictionary.Any((KeyValuePair<string, int> kv) => kv.Value == cmdkv))
						{
							dictionary.Add(item2.Key, cmdkv);
							break;
						}
					}
				}
				foreach (KeyValuePair<string, int> item3 in dictionary)
				{
					string text = "Dynamic";
					if (!CommandBindingLayers.ContainsKey(text))
					{
						CommandBindingLayers.Add(text, new CommandBindingLayer(text));
					}
					CommandBindingLayers[text].Disable();
					InputAction action = CommandBindingLayers[text].AddAction(item3.Key, InputActionType.Button);
					AddKeysValueToActionAsBinding(item3.Value, action);
					CommandBinding commandBinding2 = CommandBinding.FromInputAction(action);
					CommandBindings.Add(item3.Key, commandBinding2);
					CommandBindingLayers[text].AddBinding(commandBinding2);
				}
			}
			if (CurrentMap != null && CurrentMap.CommandToSerializedInputBindings != null)
			{
				foreach (KeyValuePair<string, List<string>> item4 in CurrentMap?.CommandToSerializedInputBindings)
				{
					_ = item4.Key;
					if (!hashSet.Contains(item4.Key))
					{
						hashSet.Add(item4.Key);
						string text2 = "Dynamic";
						if (!CommandBindingLayers.ContainsKey(text2))
						{
							CommandBindingLayers.Add(text2, new CommandBindingLayer(text2));
						}
						CommandBindingLayers[text2].Disable();
						InputAction action2 = CommandBindingLayers[text2].AddAction(item4.Key, InputActionType.Button);
						action2.LoadSerializedBindings(item4.Value);
						CommandBinding commandBinding3 = CommandBinding.FromInputAction(action2);
						CommandBindings.Add(item4.Key, commandBinding3);
						CommandBindingLayers[text2].AddBinding(commandBinding3);
					}
				}
			}
			currentActionMap.Enable();
			foreach (KeyValuePair<string, CommandBinding> commandBinding5 in CommandBindings)
			{
				if (!commandBinding5.Value.enabled)
				{
					commandBinding5.Value.Enable();
				}
			}
			foreach (KeyValuePair<string, CommandBindingLayer> commandBindingLayer in CommandBindingLayers)
			{
				if (!commandBindingLayer.Value.enabled)
				{
					commandBindingLayer.Value.Enable();
				}
			}
			UnityEngine.InputSystem.Keyboard.current.upArrowKey.SetDisplayName("\u0018");
			UnityEngine.InputSystem.Keyboard.current.downArrowKey.SetDisplayName("\u0019");
			UnityEngine.InputSystem.Keyboard.current.rightArrowKey.SetDisplayName("\u001a");
			UnityEngine.InputSystem.Keyboard.current.leftArrowKey.SetDisplayName("\u001b");
			UnityEngine.InputSystem.Keyboard.current.pageUpKey.SetDisplayName("PgUp");
			UnityEngine.InputSystem.Keyboard.current.pageDownKey.SetDisplayName("PgDown");
			UnityEngine.InputSystem.Keyboard.current.homeKey.SetDisplayName("Home");
			UnityEngine.InputSystem.Keyboard.current.endKey.SetDisplayName("End");
			UnityEngine.InputSystem.Keyboard.current.insertKey.SetDisplayName("Insert");
			UnityEngine.InputSystem.Keyboard.current.deleteKey.SetDisplayName("Delete");
			UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.SetDisplayName("NumEnter");
			ConsumedKeyCodesByLayer = new Dictionary<string, HashSet<Keys>>();
			foreach (KeyValuePair<string, CommandBindingLayer> commandBindingLayer2 in CommandBindingLayers)
			{
				ConsumedKeyCodesByLayer.Add(commandBindingLayer2.Key, new HashSet<Keys>());
				HashSet<Keys> keyLayer = ConsumedKeyCodesByLayer[commandBindingLayer2.Key];
				foreach (CommandBinding action3 in commandBindingLayer2.Value.actions)
				{
					action3.GetConsoleKeycodes().ToList().ForEach(delegate(int b)
					{
						if ((b & 0xFFFF) == 187)
						{
							keyLayer.Add((Keys)(107 + (b & -65536)));
						}
						if ((b & 0xFFFF) == 189)
						{
							keyLayer.Add((Keys)(109 + (b & -65536)));
						}
						if ((b & 0xFFFF) == 190)
						{
							keyLayer.Add((Keys)(110 + (b & -65536)));
						}
						keyLayer.Add((Keys)b);
					});
				}
			}
			ControlManager.DisableAllLayers();
			foreach (KeyValuePair<string, CommandBindingLayer> commandBindingLayer3 in CommandBindingLayers)
			{
				ControlManager.EnableLayer(commandBindingLayer3.Key);
			}
			foreach (KeyValuePair<string, GameCommand> item5 in CommandsByID)
			{
				if (string.IsNullOrEmpty(item5.Value.InputModuleBind))
				{
					continue;
				}
				try
				{
					CommandBinding commandBinding4 = CommandBindings[item5.Key];
					InputSystemUIInputModule component = GameObject.Find("EventSystem").GetComponent<InputSystemUIInputModule>();
					if (item5.Value.InputModuleBind == "submit")
					{
						component.submit = InputActionReferenceCreateNaughty(commandBinding4._inputAction);
					}
					if (item5.Value.InputModuleBind == "cancel")
					{
						component.cancel = InputActionReferenceCreateNaughty(commandBinding4._inputAction);
					}
					if (item5.Value.InputModuleBind == "move")
					{
						component.move = InputActionReferenceCreateNaughty(commandBinding4._inputAction);
					}
					if (item5.Value.InputModuleBind == "leftClick")
					{
						component.leftClick = InputActionReferenceCreateNaughty(commandBinding4._inputAction);
					}
					if (item5.Value.InputModuleBind == "rightClick")
					{
						component.rightClick = InputActionReferenceCreateNaughty(commandBinding4._inputAction);
					}
					if (item5.Value.InputModuleBind == "scrollWheel")
					{
						component.scrollWheel = InputActionReferenceCreateNaughty(commandBinding4._inputAction);
					}
				}
				catch (Exception x)
				{
					KeyValuePair<string, GameCommand> keyValuePair = item5;
					MetricsManager.LogException("Exception setting up nav on command " + keyValuePair, x);
				}
			}
			CommandBindingLayersList = CommandBindingLayers.Values.ToList();
			ControlManager.ResetBindingCaches();
			PlayerInputManager.ready = true;
			if (list != null)
			{
				foreach (KeyValuePair<string, CommandBindingLayer> commandBindingLayer4 in CommandBindingLayers)
				{
					if (list.Contains(commandBindingLayer4.Key))
					{
						commandBindingLayer4.Value.Enable();
					}
					else
					{
						commandBindingLayer4.Value.Disable();
					}
				}
			}
			if (ctrlAction == null)
			{
				ctrlAction = new InputAction("_internal_CtrlModifierAction");
				ctrlAction.AddBinding("<Keyboard>/ctrl");
				ctrlAction.Enable();
			}
			if (altAction == null)
			{
				altAction = new InputAction("_internal_AltModifierAction");
				altAction.AddBinding("<Keyboard>/alt");
				altAction.Enable();
			}
			if (shiftAction == null)
			{
				shiftAction = new InputAction("_internal_ShiftModifierAction");
				shiftAction.AddBinding("<Keyboard>/shift");
				shiftAction.Enable();
			}
		}

		public static InputActionReference InputActionReferenceCreateNaughty(InputAction from)
		{
			if (dummyAsset == null)
			{
				dummyAsset = new InputActionAsset();
			}
			InputActionReference inputActionReference = ScriptableObject.CreateInstance<InputActionReference>();
			SetPrivateFieldValue(inputActionReference, "m_Action", from);
			SetPrivateFieldValue(inputActionReference, "m_Asset", dummyAsset);
			SetPrivateFieldValue(inputActionReference, "m_ActionId", from.id.ToString());
			return inputActionReference;
		}

		public static void SetPrivatePropertyValue<T>(object obj, string propName, T val)
		{
			Type type = obj.GetType();
			if (type.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) == null)
			{
				throw new ArgumentOutOfRangeException("propName", $"Property {propName} was not found in Type {obj.GetType().FullName}");
			}
			type.InvokeMember(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty, null, obj, new object[1] { val });
		}

		public static void SetPrivateFieldValue<T>(object obj, string propName, T val)
		{
			FieldInfo field = obj.GetType().GetField(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				throw new ArgumentOutOfRangeException("propName", $"Property {propName} was not found in Type {obj.GetType().FullName}");
			}
			field.SetValue(obj, val);
		}

		public static string GetBindingDisplayString(List<string> binding)
		{
			using InputAction action = new InputAction();
			action.LoadSerializedBindings(binding);
			return action.GetBindingDisplayString(0);
		}

		[ModSensitiveCacheInit]
		public static void LoadCommands()
		{
			CommandsByCategory = new Dictionary<string, List<GameCommand>>();
			CommandsByID = new Dictionary<string, GameCommand>();
			CategoriesInOrder = new List<string>();
			foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("commands"))
			{
				HandleNodes(item);
			}
			CurrentMap = null;
			bool allowLegacyUpgrade = false;
			try
			{
				if (File.Exists(GetCurrentKeymapPath()))
				{
					try
					{
						CurrentMap = LoadKeymap(GetCurrentKeymapPath());
					}
					catch (Exception x)
					{
						allowLegacyUpgrade = true;
						MetricsManager.LogException("Load keymap2.json", x);
					}
				}
				else
				{
					allowLegacyUpgrade = true;
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogException("Loading keymapping", x2);
			}
			if (CurrentMap == null)
			{
				CurrentMap = new KeyMap();
			}
			InitializeInputManager(allowLegacyUpgrade);
		}

		public static string GetGamepadAltBindings()
		{
			if (CommandBindings.ContainsKey("GamepadAlt"))
			{
				return GetSerializedBindingsForCommand("GamepadAlt")?.FirstOrDefault() ?? "<Gamepad>/leftTrigger";
			}
			return "<Gamepad>/leftTrigger";
		}

		public static int GetFrameKeyboardModifier()
		{
			return FrameKeyboardModifier;
		}

		public static void LateUpdate()
		{
			_FrameGamepadModifier = -1;
			_FrameKeyboardModifier = -1;
		}

		public static int GetFrameGamepadModifier()
		{
			return FrameGamepadModifier;
		}

		public static async Task<List<string>> GetRebindAsync(ControlManager.InputDeviceType deviceType, bool AllowGamepadAlt = true, InputActionType type = InputActionType.Button)
		{
			_ = 1;
			try
			{
				await The.UiContext;
				using InputActionRebindingExtensions.RebindingOperation rebind = new InputActionRebindingExtensions.RebindingOperation();
				TaskCompletionSource<List<string>> taskComplete = new TaskCompletionSource<List<string>>();
				rebind.WithTimeout(10f);
				rebind.WithExpectedControlType((type == InputActionType.Button) ? "Button" : "Stick");
				foreach (KeyValuePair<string, ControlManager.InputDeviceType> item in ControlPrefixType)
				{
					if (item.Value == deviceType)
					{
						rebind.WithControlsHavingToMatchPath(item.Key);
					}
				}
				string[] keyboardModifiers = KeyboardModifiers;
				foreach (string path in keyboardModifiers)
				{
					rebind.WithControlsExcluding(path);
				}
				keyboardModifiers = IgnoredControls;
				foreach (string path2 in keyboardModifiers)
				{
					rebind.WithControlsExcluding(path2);
				}
				string gamepadbind = null;
				if (AllowGamepadAlt)
				{
					foreach (InputBinding binding in CommandBindings["GamepadAlt"]._inputAction.bindings)
					{
						gamepadbind = binding.path;
						rebind.WithControlsExcluding(binding.path);
					}
				}
				foreach (InputControl control in CommandBindings["Cancel"]._inputAction.controls)
				{
					if (control.device is UnityEngine.InputSystem.Keyboard)
					{
						rebind.WithCancelingThrough(control);
					}
				}
				MetricsManager.LogEditorInfo("gamepad: " + gamepadbind);
				rebind.OnCancel(delegate
				{
					taskComplete.TrySetResult(null);
					ControlManager.WaitForKeyup("Cancel");
				});
				rebind.OnApplyBinding(delegate(InputActionRebindingExtensions.RebindingOperation _, string bind)
				{
					bool isPressed = UnityEngine.InputSystem.Keyboard.current.ctrlKey.isPressed;
					bool isPressed2 = UnityEngine.InputSystem.Keyboard.current.altKey.isPressed;
					bool isPressed3 = UnityEngine.InputSystem.Keyboard.current.shiftKey.isPressed;
					bool flag = gamepadbind != null && InputSystem.FindControl(gamepadbind).IsPressed();
					MetricsManager.LogInfo($"Got bind {bind} c:{isPressed} a:{isPressed2} s:{isPressed3} g:{flag}");
					int num = (isPressed ? 1 : 0) + (isPressed2 ? 1 : 0) + (isPressed3 ? 1 : 0) + (flag ? 1 : 0);
					using InputAction action = new InputAction();
					switch (num)
					{
					case 0:
						action.AddBinding(bind);
						break;
					case 1:
					{
						InputActionSetupExtensions.CompositeSyntax compositeSyntax2 = action.AddCompositeBinding("OneModifier");
						compositeSyntax2.With("Binding", bind);
						if (flag)
						{
							compositeSyntax2.With("Modifier", gamepadbind);
						}
						if (isPressed)
						{
							compositeSyntax2.With("Modifier", "<Keyboard>/ctrl");
						}
						if (isPressed3)
						{
							compositeSyntax2.With("Modifier", "<Keyboard>/shift");
						}
						if (isPressed2)
						{
							compositeSyntax2.With("Modifier", "<Keyboard>/alt");
						}
						break;
					}
					default:
					{
						InputActionSetupExtensions.CompositeSyntax compositeSyntax = action.AddCompositeBinding("TwoModifiers");
						compositeSyntax.With("Binding", bind);
						int num2 = 1;
						if (isPressed)
						{
							compositeSyntax.With($"Modifier{num2++}", "<Keyboard>/ctrl");
						}
						if (isPressed3)
						{
							compositeSyntax.With($"Modifier{num2++}", "<Keyboard>/shift");
						}
						if (isPressed2 && num2 != 3)
						{
							compositeSyntax.With($"Modifier{num2++}", "<Keyboard>/alt");
						}
						break;
					}
					}
					taskComplete.SetResult(action.SerializedFormat());
				});
				rebind.Start();
				return await taskComplete.Task;
			}
			finally
			{
				MetricsManager.LogEditorInfo("Rebind complete - finally");
			}
		}

		public static IEnumerable<string> GetSerializedBindingsForCommand(string command)
		{
			if (CurrentMap?.CommandToSerializedInputBindings != null && CurrentMap.CommandToSerializedInputBindings.TryGetValue(command, out var value) && value != null)
			{
				return value;
			}
			if (CommandBindings.TryGetValue(command, out var value2))
			{
				return value2._inputAction.SerializedFormat();
			}
			return new List<string>();
		}

		public static void ReplaceCommandBindingIndex(string command, int index, List<string> binding, ControlManager.InputDeviceType deviceType)
		{
			List<string> value = ReplaceSerializedBindingFormatIndex(GetSerializedBindingsForCommand(command), index, binding, deviceType).ToList();
			KeyMap currentMap = CurrentMap;
			if (currentMap.CommandToSerializedInputBindings == null)
			{
				currentMap.CommandToSerializedInputBindings = new Dictionary<string, List<string>>();
			}
			CurrentMap.CommandToSerializedInputBindings[command] = value;
		}

		public static void RemoveCommandBinding(string command, List<string> binding)
		{
			List<string> list = new List<string>();
			foreach (IEnumerable<string> item in SplitSerializedBindings(GetSerializedBindingsForCommand(command)))
			{
				if (item.SequenceEqual(binding))
				{
					continue;
				}
				foreach (string item2 in item)
				{
					list.Add(item2);
				}
			}
			KeyMap currentMap = CurrentMap;
			if (currentMap.CommandToSerializedInputBindings == null)
			{
				currentMap.CommandToSerializedInputBindings = new Dictionary<string, List<string>>();
			}
			CurrentMap.CommandToSerializedInputBindings[command] = list;
		}

		public static async Task RemoveCommandBindingAsync(string command, int index)
		{
			await The.UiContext;
			RemoveCommandBinding(command, index);
			InitializeInputManager(AllowLegacyUpgrade: false, restoreLayers: true);
			SaveCurrentKeymap();
		}

		public static void RemoveCommandBinding(string command, int index)
		{
			List<string> list = new List<string>();
			int num = 0;
			foreach (IEnumerable<string> item in SplitSerializedBindings(GetSerializedBindingsForCommand(command)))
			{
				if (num++ == index)
				{
					continue;
				}
				foreach (string item2 in item)
				{
					list.Add(item2);
				}
			}
			KeyMap currentMap = CurrentMap;
			if (currentMap.CommandToSerializedInputBindings == null)
			{
				currentMap.CommandToSerializedInputBindings = new Dictionary<string, List<string>>();
			}
			CurrentMap.CommandToSerializedInputBindings[command] = list;
		}

		private static IEnumerable<IEnumerable<string>> SplitSerializedBindings(IEnumerable<string> original)
		{
			if (original == null)
			{
				yield break;
			}
			int numRemaining = 0;
			int index = 0;
			int num = 0;
			foreach (string item in original)
			{
				if (item == InputSystemExtensions.COMPOSITE)
				{
					numRemaining += 2;
				}
				else if (item == "OneModifier")
				{
					numRemaining++;
				}
				else if (item == "TwoModifiers")
				{
					numRemaining += 2;
				}
				index++;
				if (numRemaining-- <= 0)
				{
					numRemaining = 0;
					yield return original.Skip(num).Take(index - num);
					num = index;
				}
			}
		}

		private static ControlManager.InputDeviceType GetInputDeviceTypeFromBinding(IEnumerable<string> binding)
		{
			foreach (string item in binding)
			{
				ControlManager.InputDeviceType inputDeviceTypeFromBindingPath = GetInputDeviceTypeFromBindingPath(item);
				if (inputDeviceTypeFromBindingPath != ControlManager.InputDeviceType.Unknown)
				{
					return inputDeviceTypeFromBindingPath;
				}
			}
			return ControlManager.InputDeviceType.Unknown;
		}

		private static IEnumerable<string> ReplaceSerializedBindingFormatIndex(IEnumerable<string> original, int replaceIndex, IEnumerable<string> replacement, ControlManager.InputDeviceType deviceType)
		{
			bool replaceYielded = false;
			int index = 0;
			foreach (IEnumerable<string> item in SplitSerializedBindings(original))
			{
				if (GetInputDeviceTypeFromBinding(item) == deviceType && index++ == replaceIndex)
				{
					foreach (string item2 in replacement)
					{
						yield return item2;
					}
					replaceYielded = true;
					continue;
				}
				foreach (string item3 in item)
				{
					yield return item3;
				}
			}
			if (replaceYielded)
			{
				yield break;
			}
			foreach (string item4 in replacement)
			{
				yield return item4;
			}
		}

		public static IEnumerable<GameCommand> GetCommandsWithBinding(IEnumerable<string> binding, Predicate<GameCommand> filter = null)
		{
			foreach (GameCommand value in CommandsByID.Values)
			{
				if ((filter == null || filter(value)) && CommandUsesBinding(value, binding))
				{
					yield return value;
				}
			}
		}

		public static bool TryGetCommandFromBinding(IEnumerable<string> binding, Predicate<GameCommand> filter, out GameCommand result)
		{
			result = null;
			using (IEnumerator<GameCommand> enumerator = GetCommandsWithBinding(binding, filter).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					GameCommand current = enumerator.Current;
					result = current;
					return true;
				}
			}
			return false;
		}

		public static bool CommandUsesBinding(GameCommand command, IEnumerable<string> binding)
		{
			return CommandUsesBinding(command.ID, binding);
		}

		public static bool CommandUsesBinding(string commandId, IEnumerable<string> binding)
		{
			return SplitSerializedBindings(GetSerializedBindingsForCommand(commandId)).Any((IEnumerable<string> s) => s.SequenceEqual(binding));
		}
	}
}
