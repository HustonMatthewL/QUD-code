using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Language;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;

namespace XRL.UI
{
	public class TinkeringScreen : IScreen
	{
		public ScreenReturn Show(GameObject GO)
		{
			return Show(GO, null, null);
		}

		public ScreenReturn Show(GameObject GO, GameObject ForModdingOf = null, IEvent FromEvent = null)
		{
			if (Options.ModernCharacterSheet)
			{
				return ScreenReturn.Exit;
			}
			GameManager.Instance.PushGameView("Tinkering");
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			Keys keys = Keys.None;
			bool flag = false;
			string text = "Build";
			int num = 0;
			int num2 = 0;
			List<GameObject> list = null;
			if (ForModdingOf != null)
			{
				list = new List<GameObject> { ForModdingOf };
				text = "Mod";
			}
			BitLocker bitLocker = ((GO.GetPart<Tinkering>() == null) ? GO.GetPart<BitLocker>() : GO.RequirePart<BitLocker>());
			List<TinkerData> list2 = new List<TinkerData>(64);
			List<TinkerData> ModRecipes = new List<TinkerData>(64);
			List<GameObject> ModdableItems = null;
			Dictionary<GameObject, List<TinkerData>> ItemMods = null;
			Dictionary<GameObject, bool> ItemExpanded = null;
			List<TinkerData> list3 = null;
			int TotalModLines = 0;
			int num3 = 15;
			int num4 = 0;
			foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
			{
				if (knownRecipe.Type == "Build")
				{
					list2.Add(knownRecipe);
				}
				else if (knownRecipe.Type == "Mod")
				{
					ModRecipes.Add(knownRecipe);
				}
			}
			list2.Sort((TinkerData a, TinkerData b) => ColorUtility.CompareExceptFormattingAndCase(a.DisplayName, b.DisplayName));
			ModRecipes.Sort((TinkerData a, TinkerData b) => ColorUtility.CompareExceptFormattingAndCase(a.DisplayName, b.DisplayName));
			string text2 = "< {{W|7}} Journal | Skills {{W|9}} >";
			if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
			{
				text2 = "< {{W|" + ControlManager.getCommandInputDescription("Page Left", mapGlyphs: false) + "}} Journal | Skills {{W|" + ControlManager.getCommandInputDescription("Page Right", mapGlyphs: false) + "}} >";
			}
			BitCost bitCost = new BitCost();
			BitCost bitCost2 = new BitCost();
			while (!flag)
			{
				Event.ResetPool();
				scrapBuffer.Clear();
				bool num5 = GO.AreHostilesNearby();
				scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				scrapBuffer.Goto(35, 0);
				scrapBuffer.Write("[ {{W|Tinkering}} ]");
				if (num5)
				{
					scrapBuffer.WriteAt(10, 0, " {{R|hostiles nearby}} ");
				}
				if (bitLocker != null)
				{
					scrapBuffer.SingleBox(51, 0, 79, 16, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				}
				if (list == null)
				{
					if (text == "Build")
					{
						scrapBuffer.Goto(2, 1);
						scrapBuffer.Write("{{Y|>}} {{W|Build}}    {{w|Mod}}");
					}
					else
					{
						scrapBuffer.Goto(2, 1);
						scrapBuffer.Write("  {{w|Build}}  {{Y|>}} {{W|Mod}}");
					}
				}
				TinkerData tinkerData = null;
				GameObject gameObject = null;
				bitCost.Clear();
				if (text == "Mod")
				{
					bool flag2 = false;
					if (!GO.HasSkill("Tinkering"))
					{
						scrapBuffer.Goto(4, 4);
						scrapBuffer.Write("You don't have the Tinkering skill.");
					}
					else if (ModRecipes.Count == 0)
					{
						scrapBuffer.Goto(4, 4);
						scrapBuffer.Write("You don't have any modification schematics.");
					}
					else
					{
						if (ModdableItems == null)
						{
							TotalModLines = 0;
							ModdableItems = new List<GameObject>(128);
							ItemExpanded = new Dictionary<GameObject, bool>(128);
							ItemMods = new Dictionary<GameObject, List<TinkerData>>(64);
							list3 = new List<TinkerData>(ModRecipes);
							Action<GameObject> action = delegate(GameObject obj)
							{
								string text3 = ItemModding.ModKey(obj);
								if (text3 != null && obj.Understood())
								{
									for (int i = 0; i < ModRecipes.Count; i++)
									{
										if (ModRecipes[i].CanMod(text3) && ItemModding.ModificationApplicable(ModRecipes[i].PartName, obj, The.Player))
										{
											if (!ItemMods.ContainsKey(obj))
											{
												ModdableItems.Add(obj);
												ItemMods.Add(obj, new List<TinkerData>(8));
												ItemExpanded.Add(obj, value: false);
												TotalModLines++;
											}
											ItemMods[obj].Add(ModRecipes[i]);
											TotalModLines++;
										}
									}
								}
							};
							if (list == null)
							{
								GO.Inventory.ForeachObject(action);
								GO.Body.ForeachEquippedObject(action);
								if (list3.Count > 0)
								{
									GameObject gameObject2 = GameObject.CreateSample("DummyTinkerScreenObject");
									ModdableItems.Add(gameObject2);
									ItemMods.Add(gameObject2, new List<TinkerData>());
									ItemExpanded.Add(gameObject2, value: false);
									TotalModLines++;
									for (int j = 0; j < list3.Count; j++)
									{
										ItemMods[gameObject2].Add(list3[j]);
										TotalModLines++;
									}
								}
							}
							else
							{
								foreach (GameObject item in list)
								{
									action(item);
								}
							}
						}
						if (ModdableItems.Count == 0)
						{
							scrapBuffer.Goto(4, 4);
							if (list != null)
							{
								flag = true;
								break;
							}
							scrapBuffer.Write("You don't have any moddable items.");
						}
						else
						{
							int num6 = 3;
							int num7 = 0;
							for (int k = 0; k < ModdableItems.Count; k++)
							{
								if (num6 >= num3)
								{
									break;
								}
								if (num7 >= num2)
								{
									scrapBuffer.Goto(4, num6);
									scrapBuffer.Write(StringFormat.ClipLine(ModdableItems[k].DisplayName, 46));
									if (num7 == num)
									{
										scrapBuffer.Goto(2, num6);
										scrapBuffer.Write("{{Y|>}}");
										flag2 = true;
									}
									num6++;
								}
								num7++;
								for (int l = 0; l < ItemMods[ModdableItems[k]].Count; l++)
								{
									if (num6 >= num3)
									{
										break;
									}
									if (num7 >= num2)
									{
										int key = Tier.Constrain(ItemMods[ModdableItems[k]][l].Tier);
										int key2 = Tier.Constrain(ModdableItems[k].GetModificationSlotsUsed() - ModdableItems[k].GetIntProperty("NoCostMods") + ModdableItems[k].GetTechTier());
										bitCost2.Clear();
										bitCost2.Increment(BitType.TierBits[key]);
										bitCost2.Increment(BitType.TierBits[key2]);
										ModifyBitCostEvent.Process(GO, bitCost2, "Mod");
										if (num7 == num)
										{
											scrapBuffer.Goto(2, num6);
											scrapBuffer.Write("{{Y|>}}");
											bitCost2.CopyTo(bitCost);
											tinkerData = ItemMods[ModdableItems[k]][l];
											gameObject = ModdableItems[k];
											flag2 = true;
										}
										scrapBuffer.Goto(4, num6);
										scrapBuffer.Write("  ");
										scrapBuffer.Write(ItemMods[ModdableItems[k]][l].DisplayName);
										scrapBuffer.Write(" <");
										scrapBuffer.Write(bitCost2.ToString());
										scrapBuffer.Write(">");
										num6++;
									}
									num7++;
								}
							}
						}
					}
					if (!flag2 && num > 0)
					{
						num--;
						continue;
					}
					scrapBuffer.SingleBox(0, 16, 80, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
					if (tinkerData != null)
					{
						string s = StringFormat.ClipText("{{rules|" + ItemModding.GetModificationDescription(tinkerData.Blueprint, gameObject) + "}}", 76, KeepNewlines: true);
						scrapBuffer.Goto(2, 18);
						scrapBuffer.WriteBlockWithNewlines(s);
					}
					scrapBuffer.WriteAt(3, 24, " " + Markup.Color("keybind", ControlManager.getCommandInputDescription("Accept", mapGlyphs: false)) + " Mod Item  " + Markup.Color("keybind", ControlManager.getCommandInputDescription("CmdInsert", mapGlyphs: false)) + " List Mods  " + Markup.Color("keybind", ControlManager.getCommandInputDescription("Cancel", mapGlyphs: false)) + " Exit ");
				}
				else if (text == "Build")
				{
					if (list2.Count > 0)
					{
						tinkerData = list2[num];
					}
					if (tinkerData != null)
					{
						bitCost.Import(TinkerItem.GetBitCostFor(tinkerData.Blueprint));
						ModifyBitCostEvent.Process(GO, bitCost, "Build");
					}
					if (!GO.HasSkill("Tinkering"))
					{
						scrapBuffer.Goto(4, 4);
						scrapBuffer.Write("You don't have the Tinkering skill.");
					}
					else if (list2.Count == 0)
					{
						scrapBuffer.Goto(4, 4);
						scrapBuffer.Write("You don't have any item schematics.");
					}
					else
					{
						for (int m = num2; m < list2.Count && m - num2 < 12; m++)
						{
							scrapBuffer.Goto(4, 3 + (m - num2));
							if (list2[m].DisplayName.IsNullOrEmpty())
							{
								scrapBuffer.Write(list2[m].Blueprint);
							}
							else
							{
								scrapBuffer.Write(list2[m].DisplayName);
							}
							string text4;
							if (m == num)
							{
								text4 = bitCost.ToString();
							}
							else
							{
								bitCost2.Clear();
								bitCost2.Import(TinkerItem.GetBitCostFor(list2[m].Blueprint));
								ModifyBitCostEvent.Process(GO, bitCost2, "Build");
								text4 = bitCost2.ToString();
							}
							scrapBuffer.Goto(50 - ColorUtility.LengthExceptFormatting(text4), 3 + (m - num2));
							if (m == num)
							{
								scrapBuffer.Write("{{^K|" + text4 + "}}");
							}
							else
							{
								scrapBuffer.Write(text4);
							}
							if (m == num)
							{
								scrapBuffer.Goto(2, 3 + (m - num2));
								scrapBuffer.Write("{{Y|>}}");
							}
						}
						scrapBuffer.SingleBox(0, 16, 80, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
						if (tinkerData != null)
						{
							scrapBuffer.Goto(2, 16);
							scrapBuffer.Write(list2[num].LongDisplayName);
							scrapBuffer.Goto(2, 17);
							scrapBuffer.WriteBlockWithNewlines(list2[num].Description, 8, num4, drawIndicators: true);
						}
					}
					scrapBuffer.WriteAt(3, 24, " " + Markup.Color("keybind", ControlManager.getCommandInputDescription("Accept", mapGlyphs: false)) + " Build  " + Markup.Color("keybind", ControlManager.getCommandInputDescription("V Positive", mapGlyphs: false)) + "/" + Markup.Color("keybind", ControlManager.getCommandInputDescription("V Negative", mapGlyphs: false)) + " Scroll  " + Markup.Color("keybind", ControlManager.getCommandInputDescription("Cancel", mapGlyphs: false)) + " Exit ");
				}
				if (bitLocker != null)
				{
					scrapBuffer.WriteAt(53, 0, " Bit Locker ");
					int num6 = 1;
					foreach (char item2 in BitType.BitOrder)
					{
						BitType bitType = BitType.BitMap[item2];
						string s2 = "{{" + bitType.Color + "|" + (Options.AlphanumericBits ? BitType.CharTranslateBit(bitType.Color) : '\a') + " " + bitType.Description + "}}";
						int bitCount = bitLocker.GetBitCount(item2);
						string text5 = ((bitCount == 0) ? "{{K|0}}" : ((bitCount >= 1000000) ? ("{{C|" + bitCount / 1000000 + "M}}") : ((bitCount < 10000) ? ("{{C|" + bitCount + "}}") : ("{{C|" + bitCount / 1000 + "K}}"))));
						scrapBuffer.Goto(52, num6);
						scrapBuffer.Write(s2);
						scrapBuffer.Goto(79 - ColorUtility.LengthExceptFormatting(text5), num6);
						scrapBuffer.Write(text5);
						if (tinkerData != null)
						{
							scrapBuffer.Goto(52, num6);
							if (bitCost.TryGetValue(item2, out var value) && value > 0)
							{
								if (bitCount >= value)
								{
									scrapBuffer.Write("{{G|û}}");
								}
								else
								{
									scrapBuffer.Write("{{R|X}}");
								}
							}
							else
							{
								scrapBuffer.Write("{{K|-}}");
							}
						}
						num6++;
					}
					if (tinkerData != null && !tinkerData.Ingredient.IsNullOrEmpty())
					{
						bool flag3 = false;
						string[] array = tinkerData.Ingredient.Split(',');
						int num8 = 14;
						if (array.Length > 1)
						{
							num8--;
						}
						num6 = num8;
						string[] array2 = array;
						foreach (string blueprint in array2)
						{
							if (GO.Inventory.FindObjectByBlueprint(blueprint) != null)
							{
								scrapBuffer.Goto(52, num6);
								num6++;
								scrapBuffer.Write("{{G|û}} ");
								scrapBuffer.Write(TinkeringHelpers.TinkeredItemShortDisplayName(blueprint));
								flag3 = true;
								break;
							}
						}
						if (!flag3)
						{
							array2 = array;
							foreach (string blueprint2 in array2)
							{
								if (num6 != num8)
								{
									scrapBuffer.Goto(52, num6);
									scrapBuffer.Write("-or-");
									num6++;
								}
								scrapBuffer.Goto(52, num6);
								num6++;
								scrapBuffer.Write("{{R|X}} ");
								scrapBuffer.Write(TinkeringHelpers.TinkeredItemShortDisplayName(blueprint2));
							}
						}
					}
				}
				if (list == null)
				{
					scrapBuffer.Goto(79 - ColorUtility.StripFormatting(text2).Length, 24);
					scrapBuffer.Write(text2);
				}
				scrapBuffer.WriteAt(51, 0, "Â");
				scrapBuffer.WriteAt(0, 16, "Ã");
				scrapBuffer.WriteAt(79, 16, "\u00b4");
				scrapBuffer.WriteAt(51, 16, "Á");
				Popup._TextConsole.DrawBuffer(scrapBuffer);
				keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
				if (list == null && (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && Keyboard.RawCode != Keys.Prior && Keyboard.RawCode != Keys.Next)))
				{
					flag = true;
				}
				if (keys == Keys.Escape || keys == Keys.NumPad5)
				{
					flag = true;
				}
				if (list == null && (keys == Keys.NumPad4 || keys == Keys.NumPad6))
				{
					num4 = 0;
					num2 = 0;
					num = 0;
					text = ((!(text == "Build")) ? "Build" : "Mod");
				}
				if ((keys == Keys.L || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdInsert")) && text == "Mod" && ModRecipes.Count > 0)
				{
					string text6 = "";
					foreach (TinkerData item3 in ModRecipes)
					{
						if (text6 != "")
						{
							text6 += "\n";
						}
						text6 += item3.DisplayName;
					}
					Popup.Show(text6, null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
				}
				if ((keys == Keys.Oemplus || keys == Keys.Add || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Positive")) && tinkerData != null && num4 + 6 < tinkerData.DescriptionLineCount)
				{
					num4++;
				}
				if ((keys == Keys.OemMinus || keys == Keys.Subtract || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Positive")) && num4 > 0)
				{
					num4--;
				}
				if (keys == Keys.NumPad8)
				{
					num4 = 0;
					if (num == num2)
					{
						if (num2 > 0)
						{
							num2--;
							num--;
						}
					}
					else if (num > 0)
					{
						num--;
					}
				}
				if (keys == Keys.NumPad2)
				{
					num4 = 0;
					int num9 = list2.Count - 1;
					if (text == "Mod")
					{
						num9 = TotalModLines - 1;
					}
					if (num < num9)
					{
						num++;
					}
					if (num - num2 >= num3 - 3)
					{
						num2++;
					}
				}
				if (keys == Keys.Prior)
				{
					num4 = 0;
					num = ((num != num2) ? num2 : (num2 = Math.Max(num2 - num3 + 3, 0)));
				}
				if (keys == Keys.Next)
				{
					num4 = 0;
					int val = ((text == "Mod") ? (TotalModLines - 1) : (list2.Count - 1));
					int num10 = num2 + num3 - 4;
					if (num == num10)
					{
						num = Math.Min(num + num3 - 3, val);
						num2 = Math.Max(num - num3 + 4, 0);
					}
					else
					{
						num = Math.Min(num10, val);
					}
				}
				if (text == "Mod" && (keys == Keys.Space || keys == Keys.Enter))
				{
					bool didMod = false;
					if (!PerformUITinkerMod(GO, gameObject, tinkerData, bitCost, FromEvent, ref didMod, list) || FromEvent.InterfaceExitRequested())
					{
						flag = true;
					}
					if (didMod)
					{
						ModdableItems = null;
					}
				}
				if (text == "Build" && (keys == Keys.Space || keys == Keys.Enter) && (!PerformUITinkerBuild(GO, tinkerData, FromEvent) || FromEvent.InterfaceExitRequested()))
				{
					flag = true;
				}
			}
			GameManager.Instance.PopGameView();
			if (list == null)
			{
				if (keys == Keys.NumPad7 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Left"))
				{
					return ScreenReturn.Previous;
				}
				if (keys == Keys.NumPad9 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Right"))
				{
					return ScreenReturn.Next;
				}
			}
			return ScreenReturn.Exit;
		}

		public static bool PerformUITinkerMod(GameObject GO, GameObject ModObject, TinkerData Data, BitCost ActiveCost, IEvent FromEvent, ref bool didMod, List<GameObject> Scope = null)
		{
			if (ModObject == null)
			{
				return true;
			}
			if (Data == null)
			{
				return true;
			}
			BitLocker bitLocker = ((GO.GetPart<Tinkering>() == null) ? GO.GetPart<BitLocker>() : GO.RequirePart<BitLocker>());
			if (bitLocker == null)
			{
				return true;
			}
			bool flag = GO.AreHostilesNearby();
			if (Data != null && ModObject != null && ModObject.Blueprint != "DummyTinkerScreenObject")
			{
				GameObject player = The.Player;
				Inventory inventory = player.Inventory;
				BodyPart bodyPart = null;
				GameObject gameObject = null;
				GameObject gameObject2 = null;
				if (!Data.Ingredient.IsNullOrEmpty())
				{
					List<string> list = Data.Ingredient.CachedCommaExpansion();
					foreach (string item in list)
					{
						gameObject = inventory.FindObjectByBlueprint(item, XRL.World.Parts.Temporary.IsNotTemporary);
						if (gameObject != null)
						{
							break;
						}
						if (gameObject2 == null)
						{
							gameObject2 = inventory.FindObjectByBlueprint(item);
						}
					}
					if (gameObject == null)
					{
						if (gameObject2 != null)
						{
							Popup.ShowFail((gameObject2.HasProperName ? "" : "Your ") + gameObject2.ShortDisplayName + gameObject2.Is + " too unstable to craft with.");
						}
						else
						{
							string text = "";
							foreach (string item2 in list)
							{
								if (text != "")
								{
									text += " or ";
								}
								text += TinkeringHelpers.TinkeredItemShortDisplayName(item2);
							}
							Popup.ShowFail("You don't have the required ingredient: " + text + "!");
						}
						return true;
					}
				}
				bool flag2 = bitLocker.HasBits(ActiveCost);
				string sifrahItemModding = Options.SifrahItemModding;
				if (!flag2 && sifrahItemModding == "Never")
				{
					Popup.ShowFail("You don't have the required <" + ActiveCost.ToString() + "> bits! You have:\n\n " + bitLocker.GetBitsString());
					return true;
				}
				if (flag && GO.FireEvent("CombatPreventsTinkering"))
				{
					GO.Fail("You can't tinker with hostiles nearby!");
					return true;
				}
				if (!GO.CheckFrozen())
				{
					return true;
				}
				int num = 0;
				bool flag3 = false;
				if (sifrahItemModding == "Always")
				{
					flag3 = true;
				}
				else if (sifrahItemModding != "Never")
				{
					DialogResult dialogResult = Popup.ShowYesNoCancel("Do you want to play a game of Sifrah to mod " + ModObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + "? You can potentially improve the mod's performance and add capabilities to the item, and the cost of playing Sifrah will replace the normal modding cost." + (flag2 ? "" : (" You do not have the required <" + ActiveCost.ToString() + " bits to perform the mod normally.")));
					if (dialogResult == DialogResult.Yes)
					{
						flag3 = true;
					}
					else if (dialogResult == DialogResult.Cancel || (!flag2 && dialogResult == DialogResult.No))
					{
						return true;
					}
				}
				if (flag3)
				{
					ItemModdingSifrah itemModdingSifrah = new ItemModdingSifrah(ModObject, ActiveCost.GetHighestTier(), ModObject.GetModificationSlotsUsed(), The.Player.Stat("Intelligence"));
					itemModdingSifrah.Play(ModObject);
					if (itemModdingSifrah.InterfaceExitRequested)
					{
						return false;
					}
					if (!itemModdingSifrah.ApplyMod)
					{
						return true;
					}
					num = itemModdingSifrah.Performance;
				}
				try
				{
					if (ModObject.Equipped == player)
					{
						bodyPart = player.FindEquippedObject(ModObject);
						if (bodyPart == null)
						{
							MetricsManager.LogError("could not find equipping part for " + ModObject.Blueprint + " " + ModObject.DebugName + " tracked as equipped on player");
							return true;
						}
						Event @event = Event.New("CommandUnequipObject");
						@event.SetParameter("BodyPart", bodyPart);
						@event.SetParameter("EnergyCost", 0);
						@event.SetParameter("Context", "Tinkering");
						@event.SetFlag("NoStack", State: true);
						if (!player.FireEvent(@event))
						{
							Popup.ShowFail("You can't unequip " + ModObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
							return true;
						}
					}
					if (!Data.Ingredient.IsNullOrEmpty())
					{
						gameObject.SplitStack(1, GO);
						if (!inventory.FireEvent(Event.New("CommandRemoveObject", "Object", gameObject)))
						{
							Popup.ShowFail("You cannot use the ingredient!");
							return true;
						}
					}
					if (!flag3)
					{
						bitLocker.UseBits(ActiveCost);
					}
					GameObject gameObject3 = ModObject.SplitStack(1, GO);
					if (gameObject3 != null)
					{
						Scope?.Add(gameObject3);
					}
					int Tier = ModObject.GetTier();
					int num2 = 0;
					if (num != 0)
					{
						if (num > 0)
						{
							for (int i = 0; i < num; i++)
							{
								if ((num * 5 + ((Tier >= 8) ? 25 : 5)).in100() && RelicGenerator.ApplyBasicBestowal(ModObject, null, 1, null, Standard: false, ShowInShortDescription: true))
								{
									num2++;
								}
								else
								{
									Tier++;
								}
							}
						}
						else
						{
							Tier += num;
						}
						XRL.World.Capabilities.Tier.Constrain(ref Tier);
					}
					string text2 = ModObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null);
					didMod = ItemModding.ApplyModification(ModObject, Data.PartName, out var ModPart, Tier, DoRegistration: true, The.Player);
					if (didMod)
					{
						ModObject.MakeUnderstood();
						SoundManager.PlayUISound("Sounds/Abilities/sfx_ability_tinkerModItem");
						Popup.Show("You mod " + text2 + " to be {{C|" + (ModPart.GetModificationDisplayName() ?? Data.DisplayName) + "}}.");
					}
					if (num2 > 0)
					{
						Popup.Show(ModObject.Does("seem", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " to have taken on new qualities.");
						InventoryActionEvent.Check(ModObject, player, ModObject, "Look");
					}
					if (ModObject.Equipped == null && ModObject.InInventory == null)
					{
						player.ReceiveObject(ModObject, NoStack: false, "Tinkering");
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogError("Exception applying mod", x);
				}
				finally
				{
					if (GameObject.Validate(ref ModObject) && bodyPart != null && bodyPart.Equipped == null)
					{
						Event event2 = Event.New("CommandEquipObject");
						event2.SetParameter("Object", ModObject);
						event2.SetParameter("BodyPart", bodyPart);
						event2.SetParameter("EnergyCost", 0);
						event2.SetParameter("Context", "Tinkering");
						player.FireEvent(event2);
					}
				}
			}
			GO.UseEnergy(1000, "Skill Tinkering Mod", null, null);
			if (flag)
			{
				FromEvent?.RequestInterfaceExit();
				return false;
			}
			return true;
		}

		public static bool PerformUITinkerBuild(GameObject GO, TinkerData Data, IEvent FromEvent)
		{
			bool flag = GO.AreHostilesNearby();
			if (Data != null)
			{
				BitLocker bitLocker = ((GO.GetPart<Tinkering>() == null) ? GO.GetPart<BitLocker>() : GO.RequirePart<BitLocker>());
				if (bitLocker == null)
				{
					return true;
				}
				BitCost bitCost = new BitCost();
				bitCost.Import(TinkerItem.GetBitCostFor(Data.Blueprint));
				ModifyBitCostEvent.Process(GO, bitCost, "Build");
				Inventory inventory = GO.Inventory;
				GameObject gameObject = null;
				bool flag2 = true;
				if (!Data.Ingredient.IsNullOrEmpty())
				{
					string[] array = Data.Ingredient.Split(',');
					string[] array2 = array;
					foreach (string blueprint in array2)
					{
						gameObject = inventory.FindObjectByBlueprint(blueprint);
						if (gameObject != null)
						{
							break;
						}
					}
					if (gameObject == null)
					{
						string text = "";
						array2 = array;
						foreach (string blueprint2 in array2)
						{
							if (text != "")
							{
								text += " or ";
							}
							text += TinkeringHelpers.TinkeredItemShortDisplayName(blueprint2);
						}
						Popup.ShowFail("You don't have the required ingredient: " + text + "!");
						flag2 = false;
					}
				}
				if (flag2)
				{
					if (!GO.HasSkill(DataDisk.GetRequiredSkill(Data.Tier)))
					{
						GO.Fail("You don't have the required skill: " + DataDisk.GetRequiredSkillHumanReadable(Data.Tier) + "!");
					}
					else if (!bitLocker.HasBits(bitCost))
					{
						GO.Fail("You don't have the required <" + bitCost.ToString() + "> bits! You have:\n\n" + bitLocker.GetBitsString());
					}
					else if (flag && GO.FireEvent("CombatPreventsTinkering"))
					{
						GO.Fail("You can't tinker with hostiles nearby!");
					}
					else if (GO.CanMoveExtremities("Tinker", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
					{
						GameObject Object = GameObject.CreateSample(Data.Blueprint);
						try
						{
							Object.MakeUnderstood();
							bool Interrupt = false;
							int @for = GetTinkeringBonusEvent.GetFor(GO, Object, "BonusMod", 0, 0, ref Interrupt);
							if (Interrupt)
							{
								return false;
							}
							if (!Data.Ingredient.IsNullOrEmpty())
							{
								gameObject.SplitStack(1, GO);
								if (!inventory.FireEvent(Event.New("CommandRemoveObject", "Object", gameObject)))
								{
									GO.Fail("You cannot use the ingredient!");
									gameObject.CheckStack();
									return false;
								}
							}
							bitLocker.UseBits(bitCost);
							TinkerItem part = Object.GetPart<TinkerItem>();
							GameObject gameObject2 = null;
							for (int j = 0; j < Math.Max(part.NumberMade, 1); j++)
							{
								gameObject2 = GameObject.Create(Data.Blueprint, 0, @for.in100() ? 1 : 0, null, null, null, "Tinkering");
								TinkeringHelpers.ProcessTinkeredItem(gameObject2, The.Player);
								inventory.AddObject(gameObject2);
							}
							SoundManager.PlayUISound("sfx_ability_buildRecipeItem");
							if (part.NumberMade > 1)
							{
								Popup.Show("You tinker up " + Grammar.Cardinal(part.NumberMade) + " " + Grammar.Pluralize(Object.ShortDisplayName) + "!");
							}
							else
							{
								Popup.Show("You tinker up " + gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + "!");
							}
							GO.UseEnergy(1000, "Skill Tinkering Build", null, null);
							if (flag && FromEvent != null)
							{
								FromEvent.RequestInterfaceExit();
								return false;
							}
						}
						finally
						{
							if (GameObject.Validate(ref Object))
							{
								Object.Obliterate();
							}
						}
					}
				}
			}
			return true;
		}
	}
}
