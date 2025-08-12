using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using Qud.UI;
using UnityEngine;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts
{
	[Serializable]
	[UIView("FireMissileWeapon", true, false, false, "Targeting", "PickTargetFrame", false, 1, false)]
	public class MissileWeapon : IPart
	{
		public int AnimationDelay = 10;

		public int ShotsPerAction = 1;

		public int AmmoPerAction = 1;

		public int ShotsPerAnimation = 1;

		public int AimVarianceBonus;

		public int WeaponAccuracy;

		public int MaxRange = 999;

		public string VariableMaxRange;

		public string AmmoChar = "ù";

		public bool NoWildfire;

		public bool ShowShotsPerAction = true;

		public bool FiresManually = true;

		public string ProjectilePenetrationStat;

		public string SlotType = "Missile Weapon";

		public int EnergyCost = 1000;

		public int RangeIncrement = 3;

		public string Modifier = "Agility";

		public string Skill = "Rifle";

		[NonSerialized]
		private static Event eModifyAimVariance = new Event("ModifyAimVariance", "Amount", 0);

		[NonSerialized]
		private static Event eModifyIncomingAimVariance = new Event("ModifyIncomingAimVariance", "Amount", 0);

		[NonSerialized]
		private static DieRoll VarianceDieRoll = new DieRoll("2d20");

		[NonSerialized]
		private static bool LockActive = true;

		[NonSerialized]
		private static List<GameObject> LockObjectList = new List<GameObject>();

		[NonSerialized]
		private static List<(string, string)> MenuOptions = new List<(string, string)>();

		[NonSerialized]
		private static MissilePath PlayerMissilePath = new MissilePath();

		[NonSerialized]
		private static MissilePath CalculatedMissilePath = new MissilePath();

		[NonSerialized]
		private static bool CalculatedMissilePathInUse;

		[NonSerialized]
		private static MissilePath SecondCalculatedMissilePath = new MissilePath();

		[NonSerialized]
		private static bool SecondCalculatedMissilePathInUse;

		public static List<Pair> ListOfVisitedSquares(int x1, int y1, int x2, int y2)
		{
			List<Pair> list = new List<Pair>(Math.Abs(x2 - x1) + Math.Abs(y2 - y1));
			int num = y1;
			int num2 = x1;
			int num3 = x2 - x1;
			int num4 = y2 - y1;
			list.Add(new Pair(x1, y1));
			int num5;
			if (num4 < 0)
			{
				num5 = -1;
				num4 = -num4;
			}
			else
			{
				num5 = 1;
			}
			int num6;
			if (num3 < 0)
			{
				num6 = -1;
				num3 = -num3;
			}
			else
			{
				num6 = 1;
			}
			int num7 = 2 * num4;
			int num8 = 2 * num3;
			if (num8 >= num7)
			{
				int num9;
				int num10 = (num9 = num3);
				for (int i = 0; i < num3; i++)
				{
					num2 += num6;
					num9 += num7;
					if (num9 > num8)
					{
						num += num5;
						num9 -= num8;
						if (num9 + num10 < num8)
						{
							list.Add(new Pair(num2, num - num5));
						}
						else if (num9 + num10 > num8)
						{
							list.Add(new Pair(num2 - num6, num));
						}
					}
					list.Add(new Pair(num2, num));
					num10 = num9;
				}
			}
			else
			{
				int num9;
				int num10 = (num9 = num4);
				for (int i = 0; i < num4; i++)
				{
					num += num5;
					num9 += num8;
					if (num9 > num7)
					{
						num2 += num6;
						num9 -= num7;
						if (num9 + num10 < num7)
						{
							list.Add(new Pair(num2 - num6, num));
						}
						else if (num9 + num10 > num7)
						{
							list.Add(new Pair(num2, num - num5));
						}
					}
					list.Add(new Pair(num2, num));
					num10 = num9;
				}
			}
			return list;
		}

		public static void CalculateMissilePath(MissilePath Path, Zone Z, int X0, int Y0, int X1, int Y1, bool IncludeStart = false, bool IncludeCover = false, bool MapCalculated = false, GameObject Actor = null)
		{
			Path.Reset();
			Path.X0 = X0 * 3;
			Path.Y0 = Y0 * 3;
			Path.X1 = X1 * 3 + 1;
			Path.Y1 = Y1 * 3 + 1;
			Path.Angle = (float)Math.Atan2(X1 - X0, Y1 - Y0);
			if (IncludeCover)
			{
				if (Path.Cover == null)
				{
					Path.Cover = new List<float>();
				}
			}
			bool flag = false;
			if (!MapCalculated && IncludeCover)
			{
				Z.CalculateMissileMap(Actor ?? The.Player);
			}
			if (IncludeStart)
			{
				Path.Path.Add(Z.GetCell(X0, Y0));
				if (IncludeCover)
				{
					Path.Cover.Add(Z.GetCoverAt(X0, Y0));
				}
			}
			if (X0 == X1 && Y0 == Y1)
			{
				Path.Path.Add(Z.GetCell(X0, Y0));
				if (IncludeCover)
				{
					Path.Cover.Add(Z.GetCoverAt(X0, Y0));
				}
			}
			else
			{
				bool flag2 = Math.Abs(Y1 - Y0) > Math.Abs(X1 - X0);
				if (flag2)
				{
					int num = X0;
					X0 = Y0;
					Y0 = num;
					int num2 = X1;
					X1 = Y1;
					Y1 = num2;
				}
				if (X0 > X1)
				{
					flag = true;
					int num3 = X1;
					X1 = X0;
					X0 = num3;
					int num4 = Y1;
					Y1 = Y0;
					Y0 = num4;
				}
				double num5 = X1 - X0;
				double num6 = Math.Abs(Y1 - Y0);
				double num7 = 0.0;
				double num8 = num6 / num5;
				int num9 = 0;
				int num10 = Y0;
				num9 = ((Y0 < Y1) ? 1 : (-1));
				int num11 = 0;
				for (int i = X0; i <= X1; i++)
				{
					num11++;
					Cell cell = (flag2 ? Z.GetCell(num10, i) : Z.GetCell(i, num10));
					Path.Path.Add(cell);
					if (IncludeCover)
					{
						float cover = cell.GetCover();
						Path.Cover.Add(cover);
					}
					num7 += num8;
					if (num7 >= 0.5)
					{
						num10 += num9;
						num7 -= 1.0;
					}
				}
			}
			if (flag)
			{
				Path.Path.Reverse();
				Path.Cover?.Reverse();
			}
			if (IncludeCover)
			{
				float num12 = 0f;
				int j = 0;
				for (int count = Path.Cover.Count; j < count; j++)
				{
					num12 += Path.Cover[j];
					Path.Cover[j] = num12;
				}
			}
		}

		public static MissilePath CalculateMissilePath(Zone Z, int X0, int Y0, int X1, int Y1, bool IncludeStart = false, bool IncludeCover = true, bool MapCalculated = false, GameObject Actor = null)
		{
			MissilePath missilePath = new MissilePath();
			CalculateMissilePath(missilePath, Z, X0, Y0, X1, Y1, IncludeStart, IncludeCover, MapCalculated, Actor);
			return missilePath;
		}

		public static void GetObjectListCone(int StartX, int StartY, List<GameObject> ObjectList, string Direction)
		{
			Look.GetObjectListCone(StartX, StartY, ObjectList, Direction);
		}

		public static string GetRoundCooldown(int nCooldown)
		{
			int num = Math.Max((int)Math.Ceiling((double)nCooldown / 10.0), 1);
			if (num == 1)
			{
				return "({{C|1}} turn)";
			}
			return "({{C|" + num + "}} turns)";
		}

		public static MissilePath ShowPicker(int StartX, int StartY, bool Snap, AllowVis VisLevel, int Range, bool BowOrRifle, GameObject Projectile, ref FireType FireType, int MidRange = -1)
		{
			PickTargetWindow.currentMode = PickTargetWindow.TargetMode.PickCells;
			GameManager.Instance.PushGameView("FireMissileWeapon");
			GameObject gameObject = null;
			if (Snap && !The.Player.IsConfused)
			{
				gameObject = Sidebar.CurrentTarget ?? The.Player.GetNearestVisibleObject(Hostile: true, "Combat");
			}
			TextConsole textConsole = Popup._TextConsole;
			TextConsole.LoadScrapBuffers();
			ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
			bool flag = false;
			bool flag2 = true;
			if (gameObject != null)
			{
				Cell cell = gameObject.CurrentCell;
				if (cell != null && The.Player.InSameZone(cell))
				{
					StartX = cell.X;
					StartY = cell.Y;
				}
			}
			Cell cell2 = The.Player.CurrentCell;
			if (cell2 != null)
			{
				PlayerMissilePath.Reset();
				cell2.ParentZone.CalculateMissileMap(The.Player);
				int num = StartX;
				int num2 = StartY;
				bool flag3 = true;
				while (!flag)
				{
					Event.ResetStringbuilderPool();
					Event.ResetGameObjectListPool();
					bool flag4 = false;
					bool flag5 = false;
					bool flag6 = false;
					bool flag7 = false;
					bool flag8 = false;
					bool flag9 = false;
					bool flag10 = false;
					bool flag11 = false;
					The.Core.RenderMapToBuffer(scrapBuffer);
					List<Point> list = Zone.Line(cell2.X, cell2.Y, num, num2);
					CalculateMissilePath(PlayerMissilePath, cell2.ParentZone, cell2.X, cell2.Y, num, num2, IncludeStart: false, IncludeCover: true, MapCalculated: true, The.Player);
					scrapBuffer.Goto(0, 2);
					Cell cell3 = cell2.ParentZone.GetCell(num, num2);
					if (!flag3)
					{
						scrapBuffer.focusPosition = cell3.Pos2D;
					}
					if (list.Count == 0)
					{
						scrapBuffer.Goto(num, num2);
						scrapBuffer.Buffer[num, num2].imposterExtra.Add("Prefabs/Imposters/TargetReticle");
					}
					else
					{
						bool isConfused = The.Player.IsConfused;
						int num3 = 1;
						int count = list.Count;
						while (num3 < count)
						{
							scrapBuffer.Goto(list[num3].X, list[num3].Y);
							Cell cell4 = cell2.ParentZone.GetCell(list[num3].X, list[num3].Y);
							string text = "&y";
							Color gray = The.Color.Gray;
							Color black = The.Color.Black;
							int num4;
							if (!isConfused)
							{
								num4 = (cell4.HasVisibleCombatObject() ? 1 : 0);
								if (num4 != 0)
								{
									if (XRLCore.CurrentFrameLong < 500)
									{
										text = "&R";
										gray = The.Color.Red;
										black = The.Color.DarkRed;
									}
									else
									{
										text = "&r";
										gray = The.Color.DarkRed;
										black = The.Color.Red;
									}
									goto IL_044a;
								}
							}
							else
							{
								num4 = 0;
							}
							if (num3 > Range)
							{
								text = "&K";
								gray = The.Color.Black;
								black = The.Color.Gray;
							}
							else if (MidRange >= 0 && num3 > MidRange)
							{
								text = "&W";
								gray = The.Color.Yellow;
								black = The.Color.Brown;
							}
							else if (!cell4.IsVisible() || !cell4.IsLit())
							{
								text = "&K";
								gray = The.Color.Black;
								black = The.Color.Gray;
							}
							else
							{
								float num5 = (isConfused ? 0f : ((PlayerMissilePath.Cover == null) ? 0f : ((num3 >= PlayerMissilePath.Cover.Count) ? PlayerMissilePath.Cover.Last() : PlayerMissilePath.Cover[num3])));
								if (num5 >= 1f)
								{
									text = "&R";
									gray = The.Color.Red;
									black = The.Color.DarkRed;
								}
								else if ((double)num5 >= 0.8)
								{
									text = "&r";
									gray = The.Color.DarkRed;
									black = The.Color.Red;
								}
								else if ((double)num5 >= 0.5)
								{
									text = "&w";
									gray = The.Color.Brown;
									black = The.Color.Yellow;
								}
								else if ((double)num5 >= 0.2)
								{
									text = "&g";
									gray = The.Color.DarkGreen;
									black = The.Color.Green;
								}
								else
								{
									text = "&G";
									gray = The.Color.Green;
									black = The.Color.DarkGreen;
								}
							}
							goto IL_044a;
							IL_044a:
							if (num4 != 0 || num3 == count - 1)
							{
								ConsoleChar currentChar = scrapBuffer.CurrentChar;
								if (currentChar._Tile != null)
								{
									currentChar._TileForeground = (currentChar._Foreground = gray);
									currentChar._Detail = black;
								}
								else
								{
									currentChar.Background = gray;
									currentChar.Char = currentChar.BackupChar;
								}
							}
							else
							{
								scrapBuffer.Write(text + list[num3].DisplayChar);
							}
							num3++;
						}
						scrapBuffer.Buffer[list.Last().X, list.Last().Y].imposterExtra.Add("Prefabs/Imposters/TargetReticle");
					}
					int x = ((num >= 40) ? 1 : 43);
					scrapBuffer.Goto(x, 0);
					string text2 = "";
					text2 = ((!LockActive) ? ("{{W|space}}-select | lock (" + ControlManager.getCommandInputFormatted("CmdLockUnlock", Options.ModernUI) + ")") : ("{{W|space}}-select | unlock (" + ControlManager.getCommandInputFormatted("CmdLockUnlock", Options.ModernUI) + ")"));
					if (The.Player.IsConfused)
					{
						BowOrRifle = false;
					}
					bool flag12 = false;
					GameObject combatTarget = cell3.GetCombatTarget(The.Player, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, Projectile, null, null, null, null, AllowInanimate: false);
					int num6 = 1;
					MenuOptions.Clear();
					if (BowOrRifle && The.Player.HasSkill("Rifle_DrawABead"))
					{
						Rifle_DrawABead part = The.Player.GetPart<Rifle_DrawABead>();
						RifleMark rifleMark = combatTarget?.GetEffect<RifleMark>();
						scrapBuffer.Goto(x, num6);
						num6++;
						if (rifleMark != null && rifleMark.Marker.IsPlayer())
						{
							if (Options.ModernUI)
							{
								text2 = "{{G|marked target}} " + text2;
							}
							else
							{
								scrapBuffer.Write("{{G|marked target}}");
							}
							flag12 = true;
						}
						else if (part != null)
						{
							flag4 = true;
							if (Options.ModernUI)
							{
								text2 = text2 + " " + ControlManager.getCommandInputFormatted("CmdMarkTarget", Options.ModernUI) + " - mark target";
							}
							else
							{
								scrapBuffer.Write(" " + ControlManager.getCommandInputFormatted("CmdMarkTarget", Options.ModernUI) + " - mark target");
							}
							MenuOptions.Add(("mark target", "MarkTarget"));
						}
					}
					if (BowOrRifle && The.Player.HasSkill("Rifle_DrawABead") && combatTarget != null)
					{
						int num7 = 0;
						if (The.Player.HasSkill("Rifle_SuppressiveFire"))
						{
							scrapBuffer.Goto(x, num6);
							num6++;
							if (The.Player.HasSkill("Rifle_FlatteningFire"))
							{
								flag6 = Rifle_FlatteningFire.MeetsCriteria(combatTarget);
							}
							if (!flag12)
							{
								if (flag6)
								{
									if (!Options.ModernUI)
									{
										scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Flattening Fire (not marked)}}");
									}
									else
									{
										text2 = text2 + " {{K||" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Flattening Fire (not marked)}}";
									}
								}
								else if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Suppressive Fire (not marked)}}");
								}
								else
								{
									text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Suppressive Fire (not marked)}}";
								}
							}
							else if (num7 <= 0)
							{
								flag5 = true;
								if (flag6)
								{
									if (!Options.ModernUI)
									{
										scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + "}} - {{W|Flattening Fire}}");
									}
									else
									{
										text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + "}} - {{W|Flattening Fire}}";
									}
									MenuOptions.Add(("Flattening Fire", "SupressiveFire"));
								}
								else
								{
									if (!Options.ModernUI)
									{
										scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + "}} - Suppressive Fire");
									}
									else
									{
										text2 = text2 + "{{W|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + "}} - Suppressive Fire";
									}
									MenuOptions.Add(("Suppressive Fire", "SupressiveFire"));
								}
							}
							else if (!Options.ModernUI)
							{
								scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Suppressive Fire " + GetRoundCooldown(num7) + "}}");
							}
							else
							{
								text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Suppressive Fire " + GetRoundCooldown(num7) + "}}";
							}
						}
						if (The.Player.HasSkill("Rifle_WoundingFire"))
						{
							scrapBuffer.Goto(x, num6);
							num6++;
							if (The.Player.HasSkill("Rifle_DisorientingFire"))
							{
								flag8 = Rifle_DisorientingFire.MeetsCriteria(combatTarget);
							}
							if (!flag12)
							{
								if (flag8)
								{
									if (!Options.ModernUI)
									{
										scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Disorienting Fire (not marked)}}");
									}
									else
									{
										text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Disorienting Fire (not marked)}}";
									}
								}
								else if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Wounding Fire (not marked)}}");
								}
								else
								{
									text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Wounding Fire (not marked)}}";
								}
							}
							else if (num7 <= 0)
							{
								flag7 = true;
								if (flag8)
								{
									if (!Options.ModernUI)
									{
										scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + "}} - {{W|Disorienting Fire}}");
									}
									else
									{
										text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + "}} - {{W|Disorienting Fire}}";
									}
									MenuOptions.Add(("Disorienting Fire", "WoundingFire"));
								}
								else
								{
									if (!Options.ModernUI)
									{
										scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + "}} - Wounding Fire");
									}
									else
									{
										text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + "}} - Wounding Fire";
									}
									MenuOptions.Add(("Wounding Fire", "WoundingFire"));
								}
							}
							else if (!Options.ModernUI)
							{
								scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Wounding Fire " + GetRoundCooldown(num7) + "}}");
							}
							else
							{
								text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Wounding Fire " + GetRoundCooldown(num7) + "}}";
							}
						}
						if (The.Player.HasSkill("Rifle_SureFire"))
						{
							scrapBuffer.Goto(x, num6);
							num6++;
							if (The.Player.HasSkill("Rifle_BeaconFire"))
							{
								flag10 = Rifle_BeaconFire.MeetsCriteria(combatTarget);
							}
							if (!flag12)
							{
								if (flag10)
								{
									if (!Options.ModernUI)
									{
										scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Beacon Fire (not marked)}}");
									}
									else
									{
										text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Beacon Fire (not marked)}}";
									}
								}
								else if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Sure Fire (not marked)}}");
								}
								else
								{
									text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Sure Fire (not marked)}}";
								}
							}
							else if (num7 <= 0)
							{
								flag9 = true;
								if (The.Player.HasSkill("Rifle_BeaconFire"))
								{
									flag10 = Rifle_BeaconFire.MeetsCriteria(combatTarget);
								}
								if (flag10)
								{
									if (!Options.ModernUI)
									{
										scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + "}} - {{W|Beacon Fire}}");
									}
									else
									{
										text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + "}} - {{W|Beacon Fire}}";
									}
									MenuOptions.Add(("Beacon Fire", "SureFire"));
								}
								else
								{
									if (!Options.ModernUI)
									{
										scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + "}} - Sure Fire");
									}
									else
									{
										text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + "}} - Sure Fire";
									}
									MenuOptions.Add(("Sure Fire", "SureFire"));
								}
							}
							else if (!Options.ModernUI)
							{
								scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Sure Fire " + GetRoundCooldown(num7) + "}}");
							}
							else
							{
								text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Sure Fire " + GetRoundCooldown(num7) + "}}";
							}
						}
						if (The.Player.HasSkill("Rifle_OneShot"))
						{
							scrapBuffer.Goto(x, num6);
							num6++;
							Rifle_OneShot part2 = The.Player.GetPart<Rifle_OneShot>();
							if (!flag12)
							{
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + " - Ultra Fire (not marked)}}");
								}
								else
								{
									text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + " - Ultra Fire (not marked)}}";
								}
							}
							else if (part2.Cooldown <= 0)
							{
								flag11 = true;
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + "}} - Ultra Fire");
								}
								else
								{
									text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + "}} - Ultra Fire";
								}
								MenuOptions.Add(("Ultra Fire", "UltraFire"));
							}
							else if (!Options.ModernUI)
							{
								scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + " - Ultra Fire " + GetRoundCooldown(part2.Cooldown) + "}}");
							}
							else
							{
								text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + " - Ultra Fire " + GetRoundCooldown(part2.Cooldown) + "}}";
							}
						}
					}
					if (MenuOptions.Count > 0)
					{
						scrapBuffer.Goto(x, num6);
						if (Options.ModernUI)
						{
							text2 = text2 + " [{{W|" + ControlManager.getCommandInputDescription("CmdMissileWeaponMenu") + "}}] Menu";
						}
						else
						{
							scrapBuffer.Write("[{{W|" + ControlManager.getCommandInputDescription("CmdMissileWeaponMenu") + "}}] Menu");
						}
					}
					if (Options.ModernUI)
					{
						PickTargetWindow.currentText = (text2.IsNullOrEmpty() ? "" : (text2 + " | ")) + "Fire Missile Weapon";
					}
					else if (!text2.IsNullOrEmpty())
					{
						scrapBuffer.WriteAt(Math.Max(80 - ConsoleLib.Console.ColorUtility.LengthExceptFormatting(text2), 0), 0, text2);
					}
					if (!flag3)
					{
						scrapBuffer.focusPosition = new Point2D(num, num2);
					}
					textConsole.DrawBuffer(scrapBuffer);
					if (!Keyboard.kbhit())
					{
						continue;
					}
					Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
					string text3 = null;
					if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdMissileWeaponMenu")
					{
						int num8 = Popup.PickOption("Select Fire Mode", null, "", "Sounds/UI/ui_notification", MenuOptions.Select(((string, string) m) => m.Item1).ToArray(), null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
						if (num8 >= 0)
						{
							text3 = MenuOptions[num8].Item2;
						}
						else
						{
							keys = Keys.None;
						}
					}
					MenuOptions.Clear();
					if (text3 == "MarkTarget")
					{
						keys = Keys.M;
					}
					if (text3 == "SupressiveFire")
					{
						keys = Keys.D1;
					}
					if (text3 == "WoundingFire")
					{
						keys = Keys.D2;
					}
					if (text3 == "SureFire")
					{
						keys = Keys.D3;
					}
					if (text3 == "UltraFire")
					{
						keys = Keys.D4;
					}
					if (keys == Keys.MouseEvent)
					{
						if (Keyboard.CurrentMouseEvent.Event == "PointerOver" && !flag2)
						{
							num = Keyboard.CurrentMouseEvent.x;
							num2 = Keyboard.CurrentMouseEvent.y;
							flag3 = true;
						}
						if (Keyboard.CurrentMouseEvent.Event == "PointerOver")
						{
							flag2 = false;
						}
					}
					if (keys >= Keys.NumPad1 && keys <= Keys.NumPad9)
					{
						flag3 = false;
					}
					if (keys == Keys.NumPad5 || keys == Keys.Escape || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick"))
					{
						flag = true;
						GameManager.Instance.PopGameView();
						return null;
					}
					if (keys == Keys.U || keys == Keys.L || keys == Keys.F1 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdLockUnlock"))
					{
						LockActive = !LockActive;
					}
					if (LockActive)
					{
						LockObjectList.Clear();
						if (!The.Player.IsConfused)
						{
							if (keys == Keys.NumPad1)
							{
								GetObjectListCone(num - 1, num2 + 1, LockObjectList, "sw");
							}
							if (keys == Keys.NumPad2)
							{
								GetObjectListCone(num, num2 + 1, LockObjectList, "s");
							}
							if (keys == Keys.NumPad3)
							{
								GetObjectListCone(num + 1, num2 + 1, LockObjectList, "se");
							}
							if (keys == Keys.NumPad4)
							{
								GetObjectListCone(num - 1, num2, LockObjectList, "w");
							}
							if (keys == Keys.NumPad6)
							{
								GetObjectListCone(num + 1, num2, LockObjectList, "e");
							}
							if (keys == Keys.NumPad7)
							{
								GetObjectListCone(num - 1, num2 - 1, LockObjectList, "nw");
							}
							if (keys == Keys.NumPad8)
							{
								GetObjectListCone(num, num2 - 1, LockObjectList, "n");
							}
							if (keys == Keys.NumPad9)
							{
								GetObjectListCone(num + 1, num2 - 1, LockObjectList, "ne");
							}
						}
						if (LockObjectList.Count > 0)
						{
							Cell cell5 = LockObjectList[0].CurrentCell;
							LockObjectList.Clear();
							if (Math.Abs(cell5.X - cell2.X) <= Range && Math.Abs(cell5.Y - cell2.Y) <= Range)
							{
								num = cell5.X;
								num2 = cell5.Y;
							}
						}
						else
						{
							if (keys == Keys.NumPad1)
							{
								num--;
								num2++;
							}
							if (keys == Keys.NumPad2)
							{
								num2++;
							}
							if (keys == Keys.NumPad3)
							{
								num++;
								num2++;
							}
							if (keys == Keys.NumPad4)
							{
								num--;
							}
							if (keys == Keys.NumPad6)
							{
								num++;
							}
							if (keys == Keys.NumPad7)
							{
								num--;
								num2--;
							}
							if (keys == Keys.NumPad8)
							{
								num2--;
							}
							if (keys == Keys.NumPad9)
							{
								num++;
								num2--;
							}
						}
					}
					else
					{
						if (keys == Keys.NumPad1)
						{
							num--;
							num2++;
						}
						if (keys == Keys.NumPad2)
						{
							num2++;
						}
						if (keys == Keys.NumPad3)
						{
							num++;
							num2++;
						}
						if (keys == Keys.NumPad4)
						{
							num--;
						}
						if (keys == Keys.NumPad6)
						{
							num++;
						}
						if (keys == Keys.NumPad7)
						{
							num--;
							num2--;
						}
						if (keys == Keys.NumPad8)
						{
							num2--;
						}
						if (keys == Keys.NumPad9)
						{
							num++;
							num2--;
						}
					}
					if ((keys == Keys.Oemplus || keys == Keys.M || keys == Keys.Add || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdMarkTarget")) && flag4)
					{
						FireType = FireType.Mark;
						GameManager.Instance.PopGameView();
						return PlayerMissilePath;
					}
					if (flag5 && (keys == Keys.Oem1 || keys == Keys.D1 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdAltFire1")))
					{
						if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
						{
							Popup.ShowFail("You may only select a visible square!");
						}
						else
						{
							if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
							{
								The.Player.GetPart<Rifle_DrawABead>().ClearMark();
								FireType = FireType.SuppressingFire;
								if (flag6)
								{
									FireType = FireType.FlatteningFire;
								}
								GameManager.Instance.PopGameView();
								return PlayerMissilePath;
							}
							Popup.ShowFail("You may only select an explored square!");
						}
					}
					if (flag7 && (keys == Keys.OemQuestion || keys == Keys.D2 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdAltFire2")))
					{
						if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
						{
							Popup.ShowFail("You may only select a visible square!");
						}
						else
						{
							if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
							{
								The.Player.GetPart<Rifle_DrawABead>().ClearMark();
								FireType = FireType.WoundingFire;
								if (flag8)
								{
									FireType = FireType.DisorientingFire;
								}
								GameManager.Instance.PopGameView();
								return PlayerMissilePath;
							}
							Popup.ShowFail("You may only select an explored square!");
						}
					}
					if (flag9 && (keys == Keys.Oemtilde || keys == Keys.D3 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdAltFire3")))
					{
						if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
						{
							Popup.ShowFail("You may only select a visible square!");
						}
						else
						{
							if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
							{
								The.Player.GetPart<Rifle_DrawABead>().ClearMark();
								FireType = FireType.SureFire;
								if (flag10)
								{
									FireType = FireType.BeaconFire;
								}
								GameManager.Instance.PopGameView();
								return PlayerMissilePath;
							}
							Popup.ShowFail("You may only select an explored square!");
						}
					}
					if (flag11 && (keys == Keys.Oem4 || keys == Keys.D4 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdAltFire4")))
					{
						if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
						{
							Popup.ShowFail("You may only select a visible square!");
						}
						else
						{
							if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
							{
								Rifle_OneShot part3 = The.Player.GetPart<Rifle_OneShot>();
								Event @event = Event.New("BeforeCooldownActivatedAbility", "AbilityEntry", null, "Turns", 1010, "Tags", "Agility");
								if (The.Player.FireEvent(@event) && @event.GetIntParameter("Turns") != 0 && !The.Core.cool)
								{
									int num9 = 0;
									if (The.Player.HasStat("Willpower"))
									{
										num9 = Math.Min(80, (The.Player.Stat("Willpower") - 16) * 5);
									}
									part3.Cooldown = 1000 * (100 - num9) / 100 + 10;
								}
								FireType = FireType.OneShot;
								The.Player.GetPart<Rifle_DrawABead>().ClearMark();
								GameManager.Instance.PopGameView();
								return PlayerMissilePath;
							}
							Popup.ShowFail("You may only select an explored square!");
						}
					}
					if (keys == Keys.F || keys == Keys.Space || keys == Keys.Enter || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdFire") || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick"))
					{
						if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
						{
							Popup.ShowFail("You may only select a visible square!");
						}
						else
						{
							if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
							{
								GameManager.Instance.PopGameView();
								return PlayerMissilePath;
							}
							Popup.ShowFail("You may only select an explored square!");
						}
					}
					if (num < 0)
					{
						num = 0;
					}
					if (num >= cell2.ParentZone.Width)
					{
						num = cell2.ParentZone.Width - 1;
					}
					if (num2 < 0)
					{
						num2 = 0;
					}
					if (num2 >= cell2.ParentZone.Height)
					{
						num2 = cell2.ParentZone.Height - 1;
					}
				}
			}
			GameManager.Instance.PopGameView();
			return null;
		}

		public void CheckHeavyWeaponMovementPenalty(GameObject Subject = null)
		{
			if (Subject == null)
			{
				Subject = ParentObject.Equipped ?? ParentObject.Implantee;
				if (Subject == null)
				{
					return;
				}
			}
			Hampered effect = Subject.GetEffect<Hampered>();
			if (effect != null)
			{
				effect.CheckApplicable(Immediate: true);
			}
			else if (Hampered.Applicable(Subject))
			{
				Subject.ForceApplyEffect(new Hampered());
			}
		}

		private bool ExamineFailure(IExamineEvent E, int Chance)
		{
			if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100() && CheckLoadAmmoEvent.Check(ParentObject, E.Actor, ActivePartsIgnoreSubject: true))
			{
				Cell cell = ParentObject.CurrentCell ?? E.Actor?.CurrentCell;
				Cell cell2 = cell?.GetRandomLocalAdjacentCell(4);
				if (cell2 != null)
				{
					Event @event = Event.New("CommandFireMissile");
					@event.SetParameter("Actor", E.Actor);
					@event.SetParameter("StartCell", cell);
					@event.SetParameter("TargetCell", cell2);
					@event.SetFlag("IncludeStart", E.Actor.CurrentCell == cell && Chance.in100());
					@event.SetFlag("ShowEmitMessage", State: true);
					@event.SetFlag("ActivePartsIgnoreSubject", State: true);
					@event.SetFlag("UsePopups", State: true);
					if (FireEvent(@event))
					{
						E.Identify = true;
					}
				}
				return true;
			}
			return false;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != AfterAddSkillEvent.ID && ID != PooledEvent<AfterRemoveSkillEvent>.ID && ID != EquippedEvent.ID && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != PooledEvent<GenericQueryEvent>.ID && (ID != AdjustTotalWeightEvent.ID || !(Skill == "HeavyWeapons")) && (ID != SingletonEvent<GetEnergyCostEvent>.ID || !(Skill == "HeavyWeapons")) && ID != GetShortDescriptionEvent.ID && ID != QueryEquippableListEvent.ID)
			{
				return ID == UnequippedEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(ExamineFailureEvent E)
		{
			if (ExamineFailure(E, 25))
			{
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(ExamineCriticalFailureEvent E)
		{
			if (ExamineFailure(E, 50))
			{
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AfterAddSkillEvent E)
		{
			if (E.Skill.Name == "HeavyWeapons_Tank")
			{
				CheckHeavyWeaponMovementPenalty();
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AfterRemoveSkillEvent E)
		{
			if (E.Skill.Name == "HeavyWeapons_Tank")
			{
				CheckHeavyWeaponMovementPenalty();
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EquippedEvent E)
		{
			CheckHeavyWeaponMovementPenalty(E.Actor);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(UnequippedEvent E)
		{
			CheckHeavyWeaponMovementPenalty(E.Actor);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GenericQueryEvent E)
		{
			if (E.Query == "PhaseHarmonicEligible" && ModPhaseHarmonic.IsProjectileCompatible(GetProjectileBlueprintEvent.GetFor(ParentObject)))
			{
				E.Result = true;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(QueryEquippableListEvent E)
		{
			if (E.Item == ParentObject && !E.List.Contains(E.Item) && ValidSlotType(E.SlotType))
			{
				if (!E.RequirePossible || E.SlotType == "Floating Nearby")
				{
					E.List.Add(E.Item);
				}
				else
				{
					string usesSlots = E.Item.UsesSlots;
					if (!usesSlots.IsNullOrEmpty() && (E.SlotType != "Thrown Weapon" || usesSlots.Contains("Thrown Weapon")) && (E.SlotType != "Hand" || usesSlots.Contains("Hand")))
					{
						if (E.Actor.IsGiganticCreature)
						{
							if (E.Item.IsGiganticEquipment || E.Item.HasPropertyOrTag("GiganticEquippable") || E.Item.IsNatural())
							{
								E.List.Add(E.Item);
							}
						}
						else if (E.SlotType == "Hand" || E.SlotType == "Missile Weapon" || !E.Item.IsGiganticEquipment || !E.Item.IsNatural())
						{
							E.List.Add(E.Item);
						}
					}
					else if (!E.Actor.IsGiganticCreature || E.Item.IsGiganticEquipment || E.Item.HasPropertyOrTag("GiganticEquippable") || E.Item.IsNatural())
					{
						int slotsRequiredFor = E.Item.GetSlotsRequiredFor(E.Actor, SlotType, FloorAtOne: false);
						if (slotsRequiredFor > 0 && slotsRequiredFor <= E.Actor.GetBodyPartCount(E.SlotType))
						{
							E.List.Add(E.Item);
						}
					}
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetShortDescriptionEvent E)
		{
			E.Postfix.Append(GetDetailedStats());
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AdjustTotalWeightEvent E)
		{
			if (Skill == "HeavyWeapons")
			{
				GameObject gameObject = ParentObject.Equipped ?? ParentObject.InInventory;
				if (gameObject != null && gameObject.HasSkill("HeavyWeapons_StrappingShoulders"))
				{
					E.AdjustWeight(0.5);
				}
			}
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("CommandFireMissile");
			base.Register(Object, Registrar);
		}

		public string GetDetailedStats()
		{
			if (The.Player == null)
			{
				return "";
			}
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("{{rules|");
			if (Skill == "Pistol")
			{
				stringBuilder.Append("\nWeapon Class: Pistol");
			}
			else if (Skill == "Rifle")
			{
				stringBuilder.Append("\nWeapon Class: Bows && Rifles");
			}
			else if (Skill == "HeavyWeapons")
			{
				stringBuilder.Append("\nWeapon Class: Heavy Weapon");
			}
			else if (!Skill.IsNullOrEmpty())
			{
				stringBuilder.Append("\nWeapon Class: ").Append(Skill);
			}
			if (WeaponAccuracy <= 0)
			{
				stringBuilder.Append("\nAccuracy: Very High");
			}
			else if (WeaponAccuracy < 5)
			{
				stringBuilder.Append("\nAccuracy: High");
			}
			else if (WeaponAccuracy < 10)
			{
				stringBuilder.Append("\nAccuracy: Medium");
			}
			else if (WeaponAccuracy < 25)
			{
				stringBuilder.Append("\nAccuracy: Low");
			}
			else
			{
				stringBuilder.Append("\nAccuracy: Very Low");
			}
			if (AmmoPerAction > 1)
			{
				stringBuilder.Append("\nMultiple ammo used per shot: " + AmmoPerAction);
			}
			if (ShotsPerAction > 1 && ShowShotsPerAction)
			{
				stringBuilder.Append("\nMultiple projectiles per shot: " + ShotsPerAction);
			}
			if (NoWildfire)
			{
				stringBuilder.Append("\nSpray fire: This item can be fired while adjacent to multiple enemies without risk of the shot going wild.");
			}
			if (Skill == "HeavyWeapons")
			{
				stringBuilder.Append("\n-25 move speed");
			}
			if (!ProjectilePenetrationStat.IsNullOrEmpty())
			{
				stringBuilder.Append("\nProjectiles fired with this weapon receive bonus penetration based on the wielder's ").Append(ProjectilePenetrationStat).Append('.');
			}
			stringBuilder.Append("}}");
			return stringBuilder.ToString();
		}

		private static int toCoord(float pos)
		{
			return (int)Math.Floor(pos / 3f);
		}

		public static List<Point> CalculateBulletTrajectory(out bool PlayerInvolved, out bool CameNearPlayer, out Cell NearPlayerCell, MissilePath Path, GameObject Projectile = null, GameObject Weapon = null, GameObject Owner = null, Zone Zone = null, string AimVariance = null, int FlatVariance = 0, int WeaponVariance = 0, bool IntendedPathOnly = false)
		{
			PlayerInvolved = false;
			CameNearPlayer = false;
			NearPlayerCell = null;
			double num = Math.Atan2((double)Path.X1 - (double)Path.X0, (double)Path.Y1 - (double)Path.Y0).normalizeRadians();
			List<Pair> list = new List<Pair>(32);
			int num2 = (int)(num * 57.32484076433121);
			Path.Angle = num2;
			int num3 = WeaponVariance + FlatVariance + ((!AimVariance.IsNullOrEmpty()) ? AimVariance.RollCached() : 0);
			if (Weapon != null && Weapon.HasRegisteredEvent("ModifyMissileWeaponAngle"))
			{
				Event @event = Event.New("ModifyMissileWeaponAngle", "Attacker", Owner, "Projectile", Projectile, "Angle", num, "Mod", num3);
				Weapon.FireEvent(@event);
				num = (double)@event.GetParameter("Angle");
				num3 = @event.GetIntParameter("Mod");
			}
			num += (double)num3 * 0.0174532925;
			double num4 = Math.Sin(num);
			double num5 = Math.Cos(num);
			double num6 = Path.X0;
			double num7 = Path.Y0;
			while (Math.Floor(num6) >= 0.0 && Math.Floor(num6) <= 237.0 && Math.Floor(num7) >= 0.0 && Math.Floor(num7) <= 72.0)
			{
				num6 += num4;
				num7 += num5;
			}
			list.AddRange(ListOfVisitedSquares((int)Path.X0, (int)Path.Y0, (int)num6, (int)num7));
			if (Zone != null && Projectile != null && !IntendedPathOnly)
			{
				Cell cell = null;
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					int x = toCoord(list[i].x);
					int y = toCoord(list[i].y);
					Cell cell2 = Zone.GetCell(x, y);
					if (cell2 == null || cell2 == cell)
					{
						continue;
					}
					cell = cell2;
					if (i == 0 || ((!cell2.HasObjectWithRegisteredEvent("RefractLight") || !Projectile.HasTagOrProperty("Light")) && !cell2.HasObjectWithRegisteredEvent("ReflectProjectile")))
					{
						continue;
					}
					bool flag = true;
					GameObject Object = null;
					string clip = null;
					int num8 = -1;
					string verb = null;
					if (cell2.HasObjectWithRegisteredEvent("RefractLight") && Projectile.HasTagOrProperty("Light"))
					{
						Event event2 = Event.New("RefractLight");
						event2.SetParameter("Projectile", Projectile);
						event2.SetParameter("Attacker", Owner);
						event2.SetParameter("Cell", cell2);
						event2.SetParameter("Angle", Path.Angle);
						event2.SetParameter("Direction", Stat.Random(0, 359));
						event2.SetParameter("Verb", null);
						event2.SetParameter("Sound", "sfx_light_refract");
						event2.SetParameter("By", (object)null);
						flag = cell2.FireEvent(event2);
						if (!flag)
						{
							Object = event2.GetGameObjectParameter("By");
							clip = event2.GetParameter<string>("Sound");
							verb = event2.GetStringParameter("Verb") ?? "refract";
							num8 = event2.GetIntParameter("Direction").normalizeDegrees();
						}
					}
					if (flag && cell2.HasObjectWithRegisteredEvent("ReflectProjectile"))
					{
						Event event3 = Event.New("ReflectProjectile");
						event3.SetParameter("Projectile", Projectile);
						event3.SetParameter("Attacker", Owner);
						event3.SetParameter("Cell", cell2);
						event3.SetParameter("Angle", Path.Angle);
						event3.SetParameter("Direction", Stat.Random(0, 359));
						event3.SetParameter("Verb", null);
						event3.SetParameter("Sound", "sfx_light_refract");
						event3.SetParameter("By", (object)null);
						flag = cell2.FireEvent(event3);
						if (!flag)
						{
							Object = event3.GetGameObjectParameter("By");
							clip = event3.GetStringParameter("Sound");
							verb = event3.GetStringParameter("Verb") ?? "reflect";
							num8 = event3.GetIntParameter("Direction").normalizeDegrees();
						}
					}
					if (flag || !GameObject.Validate(ref Object))
					{
						continue;
					}
					if (Object.IsPlayer())
					{
						PlayerInvolved = true;
					}
					else
					{
						GameObject objectContext = Object.GetObjectContext();
						if (objectContext != null && objectContext.IsPlayer())
						{
							PlayerInvolved = true;
						}
					}
					Object?.Physics?.PlayWorldSound(clip, 0.5f, 0f, Combat: true);
					IComponent<GameObject>.XDidYToZ(Object, verb, Projectile, null, "!", null, null, Object);
					float num9 = list[i].x;
					float num10 = list[i].y;
					float num11 = num9;
					float num12 = num10;
					float num13 = (float)Math.Sin((float)num8 * (MathF.PI / 180f));
					float num14 = (float)Math.Cos((float)num8 * (MathF.PI / 180f));
					list.RemoveRange(i, list.Count - i);
					count = list.Count;
					Cell cell3 = cell2;
					do
					{
						num11 += num13;
						num12 += num14;
						Cell cell4 = Zone.GetCell(toCoord(num11), toCoord(num12));
						if (cell4 == null)
						{
							break;
						}
						if (cell4 == cell2)
						{
							continue;
						}
						list.Add(new Pair((int)num11, (int)num12));
						if (cell4 != cell3)
						{
							if (cell4.GetCombatTarget(Owner, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, Projectile, null, null, null, null, AllowInanimate: false) != null || cell4.HasSolidObjectForMissile(Owner, Projectile))
							{
								break;
							}
							cell3 = cell4;
						}
					}
					while (num11 > 0f && num11 < 237f && num12 > 0f && num12 < 72f);
				}
			}
			List<Point> list2 = new List<Point>(list.Count / 2);
			int num15 = int.MinValue;
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				Pair pair = list[j];
				int num16 = toCoord(pair.x) + toCoord(pair.y) * 1000;
				if (num16 != num15)
				{
					list2.Add(new Point(toCoord(pair.x), toCoord(pair.y)));
					num15 = num16;
				}
			}
			if (The.Player != null && Zone != null && list2.Count > 0)
			{
				Cell cell5 = The.Player.GetCurrentCell();
				if (cell5 != null)
				{
					Cell cell6 = Zone.GetCell(list2[0]);
					if (cell6 != null && cell6.PathDistanceTo(cell5) >= 2)
					{
						bool flag2 = false;
						int k = 1;
						for (int count3 = list2.Count; k < count3; k++)
						{
							Cell cell7 = Zone.GetCell(list2[k]);
							if (cell7 == cell5)
							{
								break;
							}
							if (flag2)
							{
								if (cell7.PathDistanceTo(cell5) >= 2)
								{
									CameNearPlayer = true;
									break;
								}
							}
							else if (cell7.PathDistanceTo(cell5) <= 1)
							{
								flag2 = true;
								NearPlayerCell = cell7;
							}
						}
					}
				}
			}
			return list2;
		}

		public static List<Point> CalculateBulletTrajectory(MissilePath Path, GameObject projectile = null, GameObject weapon = null, GameObject owner = null, Zone zone = null, string AimVariance = null, int FlatVariance = 0, int WeaponVariance = 0, bool IntendedPathOnly = false)
		{
			bool PlayerInvolved;
			bool CameNearPlayer;
			Cell NearPlayerCell;
			return CalculateBulletTrajectory(out PlayerInvolved, out CameNearPlayer, out NearPlayerCell, Path, projectile, weapon, owner, zone, AimVariance, FlatVariance, WeaponVariance, IntendedPathOnly);
		}

		public string GetSlotType(bool Cybernetic = false)
		{
			string text = ((!Cybernetic) ? ParentObject.UsesSlots : ParentObject.GetPart<CyberneticsBaseItem>()?.Slots);
			if (text.IsNullOrEmpty())
			{
				text = SlotType;
			}
			if (text.IndexOf(',') != -1)
			{
				return text.CachedCommaExpansion()[0];
			}
			return text;
		}

		public bool ValidSlotType(string Type, bool Cybernetic = false)
		{
			string text = ((!Cybernetic) ? ParentObject.UsesSlots : ParentObject.GetPart<CyberneticsBaseItem>()?.Slots);
			if (text.IsNullOrEmpty())
			{
				text = SlotType;
			}
			if (text.IndexOf(',') != -1)
			{
				List<string> list = text.CachedCommaExpansion();
				if (!list.Contains(Type))
				{
					return list.Contains("*");
				}
				return true;
			}
			if (!(text == Type))
			{
				return text == "*";
			}
			return true;
		}

		public static void SetupProjectile(GameObject Projectile, GameObject Attacker, GameObject Launcher = null, Projectile pProjectile = null)
		{
			Projectile.SetIntProperty("Primed", 1);
			if (pProjectile != null)
			{
				pProjectile.Launcher = Launcher;
			}
			if (Attacker.HasEffect<Phased>() && !Projectile.HasTagOrProperty("IndependentPhaseProjectile") && Projectile.FireEvent("CanApplyPhased") && Projectile.ForceApplyEffect(new Phased(9999)))
			{
				Projectile.ModIntProperty("ProjectilePhaseAdded", 1);
			}
			if (Attacker.HasEffect<Omniphase>() && !Projectile.HasTagOrProperty("IndependentOmniphaseProjectile") && Projectile.FireEvent("CanApplyOmniphase") && Projectile.ForceApplyEffect(new Omniphase(9999)))
			{
				Projectile.ModIntProperty("ProjectileOmniphaseAdded", 1);
			}
			if (Launcher != null && Launcher.HasRegisteredEvent("ProjectileSetup"))
			{
				Launcher.FireEvent(Event.New("ProjectileSetup", "Attacker", Attacker, "Launcher", Launcher, "Projectile", Projectile));
			}
		}

		public static void CleanupProjectile(GameObject Projectile)
		{
			if (!GameObject.Validate(ref Projectile))
			{
				return;
			}
			if (Projectile.Physics.IsReal)
			{
				if (Projectile.GetIntProperty("ProjectilePhaseAdded") > 0)
				{
					Projectile.RemoveEffect<Phased>();
					Projectile.ModIntProperty("ProjectilePhaseAdded", -1, RemoveIfZero: true);
				}
				if (Projectile.GetIntProperty("ProjectileOmniphaseAdded") > 0)
				{
					Projectile.RemoveEffect<Omniphase>();
					Projectile.ModIntProperty("ProjectileOmniphaseAdded", -1, RemoveIfZero: true);
				}
			}
			else
			{
				Projectile.Obliterate();
			}
		}

		private void MissileHit(GameObject Attacker, GameObject Defender, GameObject Owner, GameObject Projectile, Projectile pProjectile, GameObject AimedAt, GameObject ApparentTarget, MissilePath MPath, Cell ImpactCell, FireType FireType, int AimLevel, int NaturalHitResult, int HitResult, bool PathInvolvesPlayer, GameObject MessageAsFrom, bool UsePopups, ref bool Done, ref bool PenetrateCreatures, ref bool PenetrateWalls, bool TargetWasInitiallyUnset, bool ShowUninvolved)
		{
			try
			{
				bool flag = false;
				if (!DefenderMissileHitEvent.Check(ParentObject, Attacker, Defender, Owner, Projectile, pProjectile, AimedAt, ApparentTarget, MPath, FireType, AimLevel, NaturalHitResult, HitResult, PathInvolvesPlayer, MessageAsFrom, ref Done, ref PenetrateCreatures, ref PenetrateWalls))
				{
					return;
				}
				bool flag2 = Defender != ApparentTarget;
				string text = null;
				if (MessageAsFrom != null && MessageAsFrom != Owner)
				{
					text = (MessageAsFrom.IsPlayer() ? "You" : (MessageAsFrom.HasProperName ? ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(MessageAsFrom.ShortDisplayName) : ((MessageAsFrom.Equipped == Owner) ? Owner.Poss(MessageAsFrom, Definite: true, null) : ((MessageAsFrom.Equipped == null) ? ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(MessageAsFrom.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) : ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(MessageAsFrom.Equipped.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " " + MessageAsFrom.ShortDisplayName)))));
				}
				if (Defender == AimedAt && (FireType == FireType.BeaconFire || (FireType == FireType.OneShot && Owner.HasSkill("Rifle_BeaconFire") && Rifle_BeaconFire.MeetsCriteria(Defender))))
				{
					if (Owner.IsPlayer())
					{
						if (text != null)
						{
							EmitMessage(text + MessageAsFrom.GetVerb("hit") + " " + ((Defender == MessageAsFrom) ? MessageAsFrom.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " in a vital area.", ' ', FromDialog: false, UsePopups);
						}
						else
						{
							EmitMessage("You hit " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " in a vital area.", ' ', FromDialog: false, UsePopups);
						}
					}
					Defender.BloodsplatterCone(SelfSplatter: true, MPath.Angle, 45);
					flag = true;
				}
				if (!flag)
				{
					int num = GetCriticalThresholdEvent.GetFor(Attacker, Defender, ParentObject, Projectile, Skill);
					int @for = GetSpecialEffectChanceEvent.GetFor(Attacker, ParentObject, "Missile Critical", 5, Defender, Projectile);
					if (@for != 5)
					{
						num -= (@for - 5) / 5;
					}
					if (NaturalHitResult >= num)
					{
						flag = true;
					}
				}
				int num2 = pProjectile.BasePenetration;
				int num3 = pProjectile.BasePenetration + pProjectile.StrengthPenetration;
				if (flag)
				{
					BaseSkill genericSkill = Skills.GetGenericSkill(Skill, Attacker);
					if (genericSkill != null)
					{
						int weaponCriticalModifier = genericSkill.GetWeaponCriticalModifier(Attacker, Defender, ParentObject);
						if (weaponCriticalModifier != 0)
						{
							num2 += weaponCriticalModifier;
							num3 += weaponCriticalModifier;
						}
					}
				}
				if (!ProjectilePenetrationStat.IsNullOrEmpty() && Attacker != null)
				{
					num2 += Attacker.StatMod(ProjectilePenetrationStat);
				}
				Event @event = Event.New("WeaponMissileWeaponHit");
				@event.SetParameter("Attacker", Attacker);
				@event.SetParameter("Defender", Defender);
				@event.SetParameter("Weapon", ParentObject);
				@event.SetParameter("Penetrations", num2);
				@event.SetParameter("PenetrationCap", num3);
				@event.SetParameter("MessageAsFrom", MessageAsFrom);
				@event.SetFlag("Critical", flag);
				ParentObject.FireEvent(@event);
				num2 = @event.GetIntParameter("Penetrations");
				num3 = @event.GetIntParameter("PenetrationCap");
				flag = @event.HasFlag("Critical");
				@event.ID = "AttackerMissileWeaponHit";
				Attacker?.FireEvent(@event);
				@event.ID = "DefenderMissileWeaponHit";
				Defender?.FireEvent(@event);
				if (flag)
				{
					@event.ID = "MissileAttackerCriticalHit";
					Attacker.FireEvent(@event);
				}
				bool defenderIsCreature = Defender.HasTag("Creature");
				string blueprint = Defender.Blueprint;
				WeaponUsageTracking.TrackMissileWeaponHit(Owner, ParentObject, Projectile, defenderIsCreature, blueprint, flag2);
				GetMissileWeaponPerformanceEvent for2 = GetMissileWeaponPerformanceEvent.GetFor(Owner, ParentObject, Projectile, num2, num3, pProjectile.BaseDamage, null, null, pProjectile.PenetrateCreatures, pProjectile.PenetrateWalls, pProjectile.Quiet, null, null, Active: true);
				if (for2.PenetrateCreatures)
				{
					PenetrateCreatures = true;
				}
				if (for2.PenetrateWalls)
				{
					PenetrateWalls = true;
				}
				Damage damage = new Damage(0);
				damage.AddAttributes(for2.Attributes);
				int num4 = 0;
				if (for2.Attributes.Contains("Mental"))
				{
					if (Defender.Brain == null && (Defender.IsCreature ? for2.PenetrateCreatures : PenetrateWalls))
					{
						return;
					}
					num4 = Stats.GetCombatMA(Defender);
				}
				else
				{
					num4 = Stats.GetCombatAV(Defender);
				}
				int num5 = 0;
				num5 = (for2.Attributes.Contains("NonPenetrating") ? 1 : ((!for2.Attributes.Contains("Vorpal")) ? Stat.RollDamagePenetrations(num4, for2.BasePenetration, for2.PenetrationCap) : (Stat.RollDamagePenetrations(0, 0, 0) + for2.PenetrationBonus)));
				string OutcomeMessageFragment = null;
				MissilePenetrateEvent.Process(ParentObject, Attacker, Defender, Owner, Projectile, pProjectile, AimedAt, ApparentTarget, MPath, FireType, AimLevel, NaturalHitResult, PathInvolvesPlayer, MessageAsFrom, ref num5, ref OutcomeMessageFragment);
				if (Skill == "Pistol" && Owner.HasSkill("Pistol_DisarmingShot") && Owner.StatMod("Agility").in100())
				{
					Disarming.Disarm(Defender, Attacker, 100, "Strength", "Agility", null, ParentObject);
				}
				if (num5 == 0)
				{
					Defender.ParticleBlip("&K\a", 10, 0L);
					if (Owner.IsPlayer())
					{
						if (text != null)
						{
							EmitMessage(text + MessageAsFrom.GetVerb("fail") + " to penetrate " + Defender.poss("armor") + " with " + MessageAsFrom.its_(Projectile) + OutcomeMessageFragment + "!", 'r', FromDialog: false, UsePopups);
						}
						else
						{
							EmitMessage(Owner.Poss(Projectile, Definite: true, null) + Projectile.GetVerb("fail") + " to penetrate " + Defender.poss("armor") + OutcomeMessageFragment + "!", 'r', FromDialog: false, UsePopups);
						}
					}
					else if (Defender.IsPlayer())
					{
						if (text != null)
						{
							EmitMessage(text + MessageAsFrom.GetVerb("fail") + " to penetrate your armor with " + MessageAsFrom.its_(Projectile) + OutcomeMessageFragment + "!", 'g', FromDialog: false, UsePopups);
						}
						else
						{
							EmitMessage(Owner.Poss(Projectile, Definite: true, null) + Projectile.GetVerb("fail") + " to penetrate your armor" + OutcomeMessageFragment + "!", 'g', FromDialog: false, UsePopups);
						}
					}
					Done = true;
					if (Projectile.IsValid())
					{
						ImpactCell?.AddObject(Projectile, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Missile Transit");
					}
					Event event2 = Event.New("ProjectileHit");
					event2.SetParameter("Attacker", Attacker);
					event2.SetParameter("Defender", Defender);
					event2.SetParameter("Skill", Skill);
					event2.SetParameter("Damage", damage);
					event2.SetParameter("AimLevel", AimLevel);
					event2.SetParameter("Owner", Attacker);
					event2.SetParameter("Launcher", ParentObject);
					event2.SetParameter("Projectile", Projectile);
					event2.SetParameter("Path", MPath);
					event2.SetParameter("Penetrations", 0);
					event2.SetParameter("ApparentTarget", ApparentTarget);
					event2.SetParameter("AimedAt", AimedAt);
					event2.SetParameter("ImpactCell", ImpactCell);
					event2.SetFlag("Critical", flag);
					Projectile.FireEvent(event2);
					event2.ID = "DefenderProjectileHit";
					Defender.FireEvent(event2);
					event2.ID = "LauncherProjectileHit";
					ParentObject.FireEvent(event2);
					return;
				}
				if (Defender == AimedAt && Defender.IsCombatObject())
				{
					if (FireType == FireType.SuppressingFire || FireType == FireType.FlatteningFire || (FireType == FireType.OneShot && Attacker.HasSkill("Rifle_SuppressiveFire")))
					{
						if (Defender.ApplyEffect(new Suppressed(Stat.Random(3, 5))))
						{
							if (text != null)
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, Grammar.MakePossessive(text) + " suppressive fire locks " + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " in place.");
							}
							else
							{
								IComponent<GameObject>.EmitMessage(Attacker, Attacker.Poss("suppressive fire locks ") + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " in place.");
							}
						}
						if (Attacker.HasSkill("Rifle_FlatteningFire") && Rifle_FlatteningFire.MeetsCriteria(Defender))
						{
							if (Defender.ApplyEffect(new Prone()))
							{
								if (text != null)
								{
									IComponent<GameObject>.EmitMessage(MessageAsFrom, Grammar.MakePossessive(text) + " flattening fire drops " + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " to the ground!");
								}
								else
								{
									IComponent<GameObject>.EmitMessage(Attacker, Attacker.Poss("flattening fire drops ") + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " to the ground!");
								}
							}
							Disarming.Disarm(Defender, Attacker, 100, "Strength", "Agility", null, ParentObject);
						}
					}
					if (FireType == FireType.WoundingFire || FireType == FireType.DisorientingFire || (FireType == FireType.OneShot && Attacker.HasSkill("Rifle_WoundingFire")))
					{
						string text2 = (Attacker.IsPlayer() ? "You" : Attacker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null));
						if (Defender.ApplyEffect(new Bleeding(num5.ToString(), 20 + for2.BaseDamage.RollMaxCached(), Attacker, Stack: false)))
						{
							if (text != null)
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + MessageAsFrom.GetVerb("wound") + " " + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
							}
							else
							{
								IComponent<GameObject>.EmitMessage(Attacker, text2 + Attacker.GetVerb("wound") + " " + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
							}
							Defender.BloodsplatterCone(SelfSplatter: true, MPath.Angle, 45);
						}
						if (Attacker.HasSkill("Rifle_DisorientingFire") && Rifle_DisorientingFire.MeetsCriteria(Defender) && Defender.ApplyEffect(new Disoriented(Stat.Random(5, 7), 4)))
						{
							if (text != null)
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + MessageAsFrom.GetVerb("disorient") + " " + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
							}
							else
							{
								IComponent<GameObject>.EmitMessage(Attacker, text2 + Attacker.GetVerb("disorient") + " " + Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ".");
							}
						}
					}
				}
				if (Options.ShowMonsterHPHearts)
				{
					Defender.ParticleBlip(Stat.GetResultColor(num5) + "\u0003", 10, 0L);
				}
				bool flag3 = for2.BaseDamage != "0";
				if (for2.Attributes.Contains("Mental") && Defender.Brain == null)
				{
					flag3 = false;
					if (Attacker.IsPlayer())
					{
						OutcomeMessageFragment = ", but your mental attack has no effect" + (OutcomeMessageFragment.IsNullOrEmpty() ? "" : OutcomeMessageFragment);
					}
				}
				string adverb = (flag ? "critically" : null);
				string text3 = (flag ? " critically" : "");
				if (flag3)
				{
					DieRoll possiblyCachedDamageRoll = for2.GetPossiblyCachedDamageRoll();
					int num6 = 0;
					for (int i = 0; i < num5; i++)
					{
						num6 += possiblyCachedDamageRoll.Resolve();
					}
					damage.Amount = num6;
					if (flag2)
					{
						damage.AddAttribute("Accidental");
					}
					int phase = Projectile.GetPhase();
					if (damage.Amount > 0 && flag)
					{
						Defender.ParticleText("*critical hit*", IComponent<GameObject>.ConsequentialColorChar(null, Defender));
						Defender.PlayWorldSound("Sounds/Damage/sfx_damage_critical", 0.5f, 0f, Combat: true);
					}
					if (damage.Amount > 0)
					{
						if (num5 < 2)
						{
							Defender?.PlayWorldSound("Sounds/Damage/sfx_damage_penetrate_low");
						}
						else if (num5 < 4)
						{
							Defender?.PlayWorldSound("Sounds/Damage/sfx_damage_penetrate_med");
						}
						else
						{
							Defender?.PlayWorldSound("Sounds/Damage/sfx_damage_penetrate_high");
						}
						Event event3 = Event.New("DealingMissileDamage");
						event3.SetParameter("Attacker", Attacker);
						event3.SetParameter("Defender", Defender);
						event3.SetParameter("Skill", Skill);
						event3.SetParameter("Damage", damage);
						event3.SetParameter("AimLevel", AimLevel);
						event3.SetParameter("Phase", phase);
						event3.SetFlag("Critical", flag);
						if (!Attacker.FireEvent(event3))
						{
							damage.Amount = 0;
						}
						if (event3.HasFlag("RecheckPhase"))
						{
							phase = Projectile.GetPhase();
						}
					}
					if (damage.Amount > 0)
					{
						Event event4 = Event.New("WeaponDealingMissileDamage");
						event4.SetParameter("Attacker", Attacker);
						event4.SetParameter("Defender", Defender);
						event4.SetParameter("Skill", Skill);
						event4.SetParameter("Damage", damage);
						event4.SetParameter("AimLevel", AimLevel);
						event4.SetParameter("Phase", phase);
						event4.SetFlag("Critical", flag);
						if (!ParentObject.FireEvent(event4))
						{
							damage.Amount = 0;
						}
						if (event4.HasFlag("RecheckPhase"))
						{
							phase = Projectile.GetPhase();
						}
					}
					bool flag4 = false;
					if (damage.Amount > 0)
					{
						Defender.WillCheckHP(true);
						flag4 = true;
						Event event5 = Event.New("TakeDamage");
						event5.SetParameter("Damage", damage);
						event5.SetParameter("Owner", Attacker);
						event5.SetParameter("Attacker", Attacker);
						event5.SetParameter("Weapon", ParentObject);
						event5.SetParameter("Projectile", Projectile);
						event5.SetParameter("Phase", phase);
						event5.SetParameter("OutcomeMessageFragment", OutcomeMessageFragment);
						event5.SetFlag("WillUseOutcomeMessageFragment", State: true);
						if (ParentObject.HasTagOrProperty("NoMissileSetTarget"))
						{
							event5.SetFlag("NoSetTarget", State: true);
						}
						if (!Defender.FireEvent(event5))
						{
							damage.Amount = 0;
						}
						OutcomeMessageFragment = event5.GetStringParameter("OutcomeMessageFragment");
					}
					WeaponUsageTracking.TrackMissileWeaponDamage(Owner, ParentObject, Projectile, defenderIsCreature, blueprint, flag2, damage);
					if (damage.Amount > 0)
					{
						if (Options.ShowMonsterHPHearts)
						{
							Defender.ParticleBlip(Defender.GetHPColor() + "\u0003", 10, 0L);
						}
					}
					else if (flag4)
					{
						Defender.WillCheckHP(false);
						flag4 = false;
					}
					if (Owner.IsPlayer())
					{
						if (OutcomeMessageFragment != null)
						{
							if (Defender.IsVisible())
							{
								if (text != null)
								{
									if (MessageAsFrom.IsVisible())
									{
										IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
									}
									else
									{
										IComponent<GameObject>.EmitMessage(Owner, "Something hits " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
									}
								}
								else
								{
									IComponent<GameObject>.EmitMessage(Owner, "You" + text3 + " hit " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
							}
						}
						else if (damage.Amount > 0 || !damage.SuppressionMessageDone)
						{
							if (text != null)
							{
								if (Defender.IsVisible())
								{
									if (MessageAsFrom.IsVisible())
									{
										IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " for " + damage.Amount + " damage!", Stat.GetResultColorChar(num5), FromDialog: false, UsePopups);
									}
									else
									{
										IComponent<GameObject>.EmitMessage(Owner, "Something hits " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " for " + damage.Amount + " damage!", Stat.GetResultColorChar(num5), FromDialog: false, UsePopups);
									}
								}
								else if (Defender.IsAudible(The.Player, 80))
								{
									IComponent<GameObject>.EmitMessage(Owner, text + MessageAsFrom.GetVerb("hit") + " something " + Owner.DescribeDirectionToward(Defender) + "!", ' ', FromDialog: false, UsePopups);
								}
							}
							else if (Defender.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(Owner, "You" + text3 + " hit " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " for " + damage.Amount + " damage!", Stat.GetResultColorChar(num5), FromDialog: false, UsePopups);
							}
							else if (Defender.IsAudible(The.Player, 80))
							{
								IComponent<GameObject>.EmitMessage(Owner, "You hit something " + Owner.DescribeDirectionToward(Defender) + "!", ' ', FromDialog: false, UsePopups);
							}
						}
					}
					else if (Defender.IsPlayer())
					{
						if (OutcomeMessageFragment != null)
						{
							if (text != null)
							{
								if (MessageAsFrom.IsVisible())
								{
									IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " you with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
								else
								{
									IComponent<GameObject>.EmitMessage(Owner, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you" + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
							}
							else if (Owner.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you " + Defender.DescribeDirectionFrom(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (text != null)
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " you (x" + num5 + ") with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " for " + damage.Amount + " damage!", 'r', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you (x" + num5 + ") " + Defender.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!", 'r', FromDialog: false, UsePopups);
							}
						}
						else if (Owner.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you (x" + num5 + ") with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " for " + damage.Amount + " damage!", 'r', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you (x" + num5 + ") " + Defender.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!", 'r', FromDialog: false, UsePopups);
						}
					}
					else if ((PathInvolvesPlayer || ShowUninvolved) && Defender.IsVisible())
					{
						if (OutcomeMessageFragment != null)
						{
							if (text != null)
							{
								if (MessageAsFrom.IsVisible())
								{
									IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
								else
								{
									IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " " + The.Player.DescribeDirectionFrom(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
							}
							else if (Owner.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " " + The.Player.DescribeDirectionFrom(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (text != null)
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " for " + damage.Amount + " damage!", ' ', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") " + The.Player.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (Owner.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " for " + damage.Amount + " damage!", ' ', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") " + The.Player.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!", ' ', FromDialog: false, UsePopups);
						}
					}
					if (flag4)
					{
						Defender.CheckHP(null, null, null, Preregistered: true);
					}
				}
				else if (!for2.Quiet)
				{
					if (Owner.IsPlayer())
					{
						if (OutcomeMessageFragment != null)
						{
							if (Defender.IsVisible())
							{
								if (text != null)
								{
									if (MessageAsFrom.IsVisible())
									{
										IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
									}
									else
									{
										IComponent<GameObject>.EmitMessage(Owner, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " " + Defender.DescribeDirectionFrom(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
									}
								}
								else
								{
									IComponent<GameObject>.EmitMessage(Owner, "You" + text3 + " hit " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
							}
						}
						else if (text != null)
						{
							if (Defender.IsVisible())
							{
								if (MessageAsFrom.IsVisible())
								{
									IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + "!", Stat.GetResultColorChar(num5), FromDialog: false, UsePopups);
								}
								else
								{
									IComponent<GameObject>.EmitMessage(Owner, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") " + Defender.DescribeDirectionFrom(Owner) + "!", Stat.GetResultColorChar(num5), FromDialog: false, UsePopups);
								}
							}
							else if (MessageAsFrom.IsVisible() && Defender.IsAudible(The.Player, 80))
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + MessageAsFrom.GetVerb("hit") + " something " + Owner.DescribeDirectionToward(Defender) + "!", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (Defender.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(Owner, "You" + text3 + " hit " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + "!", Stat.GetResultColorChar(num5), FromDialog: false, UsePopups);
						}
						else if (Defender.IsAudible(The.Player, 80))
						{
							IComponent<GameObject>.EmitMessage(Owner, "You hit something " + Owner.DescribeDirectionToward(Defender) + "!", ' ', FromDialog: false, UsePopups);
						}
					}
					else if (Defender.IsPlayer())
					{
						if (OutcomeMessageFragment != null)
						{
							if (text != null)
							{
								if (MessageAsFrom.IsVisible())
								{
									IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " you with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
								else
								{
									IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you " + Defender.DescribeDirectionToward(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
							}
							else if (Owner.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you" + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you " + Defender.DescribeDirectionToward(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (text != null)
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + " you with " + Projectile.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + "! (x" + num5 + ")", 'r', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you " + Defender.DescribeDirectionToward(Owner) + "! (x" + num5 + ")", 'r', FromDialog: false, UsePopups);
							}
						}
						else if (Owner.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you! (x" + num5 + ")", 'r', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " you " + Defender.DescribeDirectionToward(Owner) + "! (x" + num5 + ")", 'r', FromDialog: false, UsePopups);
						}
					}
					else if ((PathInvolvesPlayer || ShowUninvolved) && Defender.IsVisible())
					{
						if (OutcomeMessageFragment != null)
						{
							if (text != null)
							{
								if (MessageAsFrom.IsVisible())
								{
									IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
								else
								{
									IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " " + The.Player.DescribeDirectionToward(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
							}
							else if (Owner.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " " + The.Player.DescribeDirectionToward(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (text != null)
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + "! (x" + num5 + ")", ' ', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") " + The.Player.DescribeDirectionToward(Owner) + "!", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (Owner.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + "! (x" + num5 + ")", ' ', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " " + ((Defender == Owner) ? Owner.itself : Defender.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null)) + " (x" + num5 + ") " + The.Player.DescribeDirectionToward(Owner) + "!", ' ', FromDialog: false, UsePopups);
						}
					}
				}
				if (Owner.IsPlayer() && !ParentObject.HasTagOrProperty("NoMissileSetTarget") && Sidebar.CurrentTarget == null && Defender != Owner && Defender.IsCreature && Defender.IsHostileTowards(Owner) && Defender.IsVisible() && TargetWasInitiallyUnset)
				{
					Sidebar.CurrentTarget = Defender;
				}
				if (Projectile.IsValid())
				{
					ImpactCell?.AddObject(Projectile, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Missile Transit");
				}
				Event event6 = Event.New("ProjectileHit");
				event6.SetParameter("Attacker", Attacker);
				event6.SetParameter("Defender", Defender);
				event6.SetParameter("Projectile", Projectile);
				event6.SetParameter("Skill", Skill);
				event6.SetParameter("Damage", damage);
				event6.SetParameter("AimLevel", AimLevel);
				event6.SetParameter("Owner", Attacker);
				event6.SetParameter("Launcher", ParentObject);
				event6.SetParameter("Path", MPath);
				event6.SetParameter("Penetrations", num5);
				event6.SetParameter("ImpactCell", ImpactCell);
				event6.SetParameter("ApparentTarget", ApparentTarget);
				event6.SetParameter("AimedAt", AimedAt);
				event6.SetFlag("critical", flag);
				Projectile.FireEvent(event6);
				event6.ID = "DefenderProjectileHit";
				Defender.FireEvent(event6);
				event6.ID = "LauncherProjectileHit";
				ParentObject.FireEvent(event6);
				if (!for2.PenetrateCreatures)
				{
					Done = true;
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("MissileWeapon:MissileHit", x);
			}
			finally
			{
				if (GameObject.Validate(ref Defender))
				{
					Defender.WillCheckHP(false);
				}
			}
		}

		public bool IsSkilled(GameObject Actor)
		{
			if (Actor != null)
			{
				if (Skill == "Pistol")
				{
					return Actor.HasSkill("Pistol_SteadyHands");
				}
				if (Skill == "Rifle" || Skill == "Bow")
				{
					return Actor.HasSkill("Rifle_SteadyHands");
				}
			}
			return false;
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "CommandFireMissile")
			{
				GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
				if (gameObject == null)
				{
					return false;
				}
				Cell cell = E.GetParameter("TargetCell") as Cell;
				Cell cell2 = E.GetParameter("StartCell") as Cell;
				GameObject Attacker = gameObject;
				FireType fireType = FireType.Normal;
				if (E.HasParameter("FireType"))
				{
					fireType = (FireType)E.GetParameter("FireType");
				}
				if (cell2 == null)
				{
					cell2 = gameObject.CurrentCell;
				}
				if (cell2 == null)
				{
					return false;
				}
				Zone parentZone = cell2.ParentZone;
				if (parentZone == null)
				{
					return false;
				}
				bool flag = parentZone.IsActive();
				bool activePartsIgnoreSubject = E.HasFlag("ActivePartsIgnoreSubject");
				bool flag2 = E.HasFlag("UsePopups");
				ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
				ScreenBuffer screenBuffer = E.GetParameter("ScreenBuffer") as ScreenBuffer;
				if (flag)
				{
					if (screenBuffer != null)
					{
						scrapBuffer.Copy(screenBuffer);
					}
					else
					{
						XRLCore.Core.RenderBaseToBuffer(scrapBuffer);
					}
				}
				GameObject gameObject2 = null;
				int intParameter = E.GetIntParameter("AimLevel");
				MissilePath missilePath = E.GetParameter("Path") as MissilePath;
				if (missilePath == null)
				{
					if (CalculatedMissilePathInUse)
					{
						if (SecondCalculatedMissilePathInUse)
						{
							missilePath = new MissilePath();
						}
						else
						{
							missilePath = SecondCalculatedMissilePath;
							SecondCalculatedMissilePathInUse = true;
						}
					}
					else
					{
						missilePath = CalculatedMissilePath;
						CalculatedMissilePathInUse = true;
					}
					CalculateMissilePath(missilePath, parentZone, cell2.X, cell2.Y, cell.X, cell.Y, E.HasFlag("IncludeStart"), IncludeCover: false, MapCalculated: false, gameObject);
				}
				try
				{
					int intParameter2 = E.GetIntParameter("FlatVariance");
					if (!Attacker.FireEvent("BeginMissileAttack"))
					{
						return false;
					}
					bool flag3 = false;
					if (!NoWildfire)
					{
						int num = 0;
						foreach (Cell adjacentCell in Attacker.CurrentCell.GetAdjacentCells())
						{
							foreach (GameObject item2 in adjacentCell.LoopObjectsWithPart("Combat"))
							{
								if (!item2.IsHostileTowards(Attacker) || !item2.PhaseAndFlightMatches(Attacker) || !item2.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
								{
									continue;
								}
								num++;
								if (num > 1)
								{
									if (50.in100())
									{
										flag3 = true;
									}
									goto end_IL_025b;
								}
							}
							continue;
							end_IL_025b:
							break;
						}
					}
					float num2 = missilePath.X1 - missilePath.X0;
					float num3 = missilePath.Y1 - missilePath.Y0;
					float num4 = 0f;
					string text = "-";
					num4 = ((num2 != 0f) ? (Math.Abs(num3) / Math.Abs(num2)) : 9999f);
					text = ((num4 >= 2f) ? "|" : ((!((double)num4 >= 0.5)) ? "-" : ((num2 < 0f) ? ((!(num3 > 0f)) ? "\\" : "/") : ((!(num3 > 0f)) ? "/" : "\\"))));
					ScreenBuffer scrapBuffer2 = ScreenBuffer.GetScrapBuffer2();
					if (!CheckLoadAmmoEvent.Check(ParentObject, gameObject, out var Message, activePartsIgnoreSubject))
					{
						if (!Message.IsNullOrEmpty() && gameObject.IsPlayer())
						{
							EmitMessage(Message, 'r', FromDialog: false, flag2);
						}
						if (Attacker != null && Attacker.Brain != null && ParentObject.FireEvent("ReloadPossible"))
						{
							Attacker.Brain.NeedToReload = true;
						}
						return false;
					}
					int num5 = 0;
					List<GameObject> list = new List<GameObject>(ShotsPerAction);
					List<Projectile> list2 = new List<Projectile>(ShotsPerAction);
					GameObject gameObject3 = null;
					for (int i = 0; i < AmmoPerAction; i++)
					{
						if (!LoadAmmoEvent.Check(ParentObject, gameObject, out var Projectile, out var LoadedAmmo, out var Message2, activePartsIgnoreSubject))
						{
							if (!Message2.IsNullOrEmpty() && gameObject.IsPlayer())
							{
								EmitMessage(Message2, 'r', FromDialog: false, flag2);
							}
							break;
						}
						if (GameObject.Validate(ref LoadedAmmo))
						{
							gameObject2 = LoadedAmmo;
						}
						num5++;
						if (GameObject.Validate(ref Projectile))
						{
							list.Add(Projectile);
							list2.Add(Projectile.GetPart<Projectile>());
							if (gameObject3 == null)
							{
								gameObject3 = Projectile;
							}
						}
					}
					for (int j = AmmoPerAction; j < ShotsPerAction; j++)
					{
						int num6 = j - AmmoPerAction;
						if (list.Count < num6)
						{
							num6 = 0;
						}
						if (list.Count > num6)
						{
							GameObject Object = list[num6].DeepCopy();
							if (GameObject.Validate(ref Object))
							{
								list.Add(Object);
								list2.Add(Object.GetPart<Projectile>());
							}
						}
					}
					for (int num7 = list.Count - 1; num7 >= 0; num7--)
					{
						SetupProjectile(list[num7], Attacker, ParentObject, list2[num7]);
					}
					if (E.HasFlag("ShowEmitMessage") && list.Count > 0)
					{
						GameObject gameObject4 = ((ParentObject.CurrentCell == null) ? gameObject : ParentObject);
						if (IComponent<GameObject>.Visible(gameObject4))
						{
							if (list.Count == 1)
							{
								GameObject @object = list[0];
								GameObject useVisibilityOf = gameObject4;
								DidXToY("emit", @object, null, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, flag2, useVisibilityOf);
							}
							else
							{
								string[] array = new string[list.Count];
								bool flag4 = true;
								int k = 0;
								for (int count = list.Count; k < count; k++)
								{
									array[k] = list[k].ShortDisplayName;
									if (flag4 && k > 0 && array[k] != array[0])
									{
										flag4 = false;
									}
								}
								if (flag4)
								{
									string extra = list.Count.Things(array[0]);
									GameObject useVisibilityOf = gameObject4;
									DidX("emit", extra, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, flag2, useVisibilityOf);
								}
								else
								{
									int l = 0;
									for (int num8 = array.Length; l < num8; l++)
									{
										array[l] = Grammar.A(array[l]);
									}
									string extra2 = Grammar.MakeAndList(array);
									GameObject useVisibilityOf = gameObject4;
									DidX("emit", extra2, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, flag2, useVisibilityOf);
								}
							}
						}
					}
					if (num5 == 0)
					{
						if (Attacker != null && Attacker.Brain != null && ParentObject.FireEvent("ReloadPossible"))
						{
							Attacker.Brain.NeedToReload = true;
						}
						return false;
					}
					if (flag3)
					{
						if (Attacker.IsPlayer())
						{
							EmitMessage("Your shot goes wild!", 'R', FromDialog: false, flag2);
						}
						else if (IComponent<GameObject>.Visible(Attacker))
						{
							EmitMessage(Attacker.Poss("shot") + " goes wild!", ColorCoding.ConsequentialColorChar(null, Attacker), FromDialog: false, flag2);
						}
					}
					int num9 = 0;
					num9 = ((num5 < AmmoPerAction) ? ((int)Math.Ceiling((float)ShotsPerAction * ((float)num5 / (float)AmmoPerAction))) : ShotsPerAction);
					if (num9 > 0)
					{
						ParentObject.FireEvent("WeaponMissleWeaponFiring");
					}
					string value = ParentObject?.GetTag("MissileFireSound");
					Event @event = Event.New("QueryMissileFireSound");
					@event.SetParameter("Weapon", ParentObject);
					@event.SetParameter("Sound", value);
					@event.SetParameter("Ammo", gameObject2);
					@event.SetParameter("Attacker", Attacker);
					ParentObject?.FireEvent(@event);
					gameObject2?.FireEvent(@event);
					value = @event.GetStringParameter("Sound");
					gameObject?.PlayWorldSound(value, 0.5f, 0f, Combat: true);
					switch (fireType)
					{
					case FireType.SuppressingFire:
						gameObject?.PlayWorldSound("sfx_ability_weaponSkill_suppressiveFire", 0.5f, 0f, Combat: true);
						break;
					case FireType.FlatteningFire:
						gameObject?.PlayWorldSound("sfx_ability_weaponSkill_suppressiveFire_upgraded", 0.5f, 0f, Combat: true);
						break;
					case FireType.SureFire:
						gameObject?.PlayWorldSound("sfx_ability_weaponSkill_sureFire", 0.5f, 0f, Combat: true);
						break;
					case FireType.BeaconFire:
						gameObject?.PlayWorldSound("sfx_ability_weaponSkill_sureFire_upgraded", 0.5f, 0f, Combat: true);
						break;
					case FireType.WoundingFire:
						gameObject?.PlayWorldSound("sfx_ability_weaponSkill_woundingFire", 0.5f, 0f, Combat: true);
						break;
					case FireType.DisorientingFire:
						gameObject?.PlayWorldSound("sfx_ability_weaponSkill_woundingFire_upgraded", 0.5f, 0f, Combat: true);
						break;
					case FireType.OneShot:
						gameObject?.PlayWorldSound("sfx_ability_weaponSkill_ultraFire", 0.5f, 0f, Combat: true);
						break;
					}
					GameObject Object2 = E.GetGameObjectParameter("AimedAt") ?? cell.GetCombatTarget(Attacker, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, gameObject3, null, null, gameObject, null, AllowInanimate: true, InanimateSolidOnly: true);
					if (GameObject.Validate(ref Object2))
					{
						if (Object2.IsInGraveyard())
						{
							Object2 = null;
						}
						else if (Object2.IsPlayer())
						{
							if (AutoAct.IsActive() && IComponent<GameObject>.Visible(Attacker))
							{
								AutoAct.Interrupt("something is shooting at you", null, Attacker, IsThreat: true);
							}
						}
						else if (!Object2.IsHostileTowards(gameObject))
						{
							Object2.AddOpinion<OpinionFriendlyFire>(gameObject);
						}
					}
					else
					{
						Object2 = null;
					}
					GameObject gameObject5 = Object2;
					if (gameObject.IsPlayer())
					{
						MissilePath path = missilePath;
						GameObject useVisibilityOf = Attacker;
						foreach (Point item3 in CalculateBulletTrajectory(path, gameObject3, null, useVisibilityOf, null, null, 0, 0, IntendedPathOnly: true))
						{
							Cell cell3 = parentZone.GetCell(item3.X, item3.Y);
							if (cell3 == Attacker.CurrentCell)
							{
								continue;
							}
							GameObject combatTarget = cell3.GetCombatTarget(Attacker, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, gameObject3, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true);
							if (combatTarget == null || combatTarget == gameObject)
							{
								continue;
							}
							if (combatTarget != gameObject5 && !combatTarget.IsLedBy(gameObject))
							{
								if (gameObject5 == null)
								{
									gameObject5 = combatTarget;
								}
								if (combatTarget.Brain != null && (gameObject.PhaseMatches(combatTarget) || (gameObject3 != null && gameObject3.PhaseMatches(combatTarget))) && !combatTarget.IsHostileTowards(gameObject) && combatTarget.Brain.FriendlyFireIncident(gameObject))
								{
									combatTarget.AddOpinion<OpinionFriendlyFire>(gameObject);
								}
							}
							break;
						}
					}
					List<List<Point>> list3 = new List<List<Point>>();
					int num10 = 0;
					int num11 = -gameObject.StatMod(Modifier);
					if (IsSkilled(Attacker))
					{
						num11 -= 2;
					}
					bool flag5 = E.HasFlag("TargetUnset");
					if (gameObject.IsPlayer() && !ParentObject.HasTagOrProperty("NoMissileSetTarget"))
					{
						if (Object2 != null && Sidebar.CurrentTarget != Object2)
						{
							if (!flag5 && !Object2.IsCreature)
							{
								GameObject currentTarget = Sidebar.CurrentTarget;
								if (currentTarget != null && currentTarget.IsCreature)
								{
									goto IL_0bdc;
								}
							}
							if (Object2.IsVisible())
							{
								Sidebar.CurrentTarget = Object2;
								goto IL_0c04;
							}
						}
						goto IL_0bdc;
					}
					goto IL_0c04;
					IL_0c04:
					if (Object2 != null)
					{
						num11 += Object2.GetIntProperty("IncomingAimModifier");
						if (Rifle_DrawABead.IsCompatibleMissileWeapon(Skill) && Object2.HasEffect((RifleMark fx) => fx.Marker == Attacker))
						{
							num11--;
						}
					}
					num11 -= intParameter;
					num11 -= AimVarianceBonus;
					num11 -= Attacker.GetIntProperty("MissileWeaponAccuracyBonus");
					num11 -= ParentObject.GetIntProperty("MissileWeaponAccuracyBonus");
					eModifyAimVariance.SetParameter("Amount", 0);
					Attacker.FireEvent(eModifyAimVariance);
					ParentObject.FireEvent(eModifyAimVariance);
					num11 += eModifyAimVariance.GetIntParameter("Amount");
					if (Object2 != null && Object2.HasRegisteredEvent("ModifyIncomingAimVariance"))
					{
						eModifyIncomingAimVariance.SetParameter("Amount", 0);
						Object2.FireEvent(eModifyIncomingAimVariance);
						num11 += eModifyIncomingAimVariance.GetIntParameter("Amount");
					}
					int num12 = VarianceDieRoll.Resolve();
					num10 = Math.Abs(num12 - 21) + num11;
					if (fireType == FireType.SureFire || fireType == FireType.BeaconFire || (fireType == FireType.OneShot && Attacker.HasSkill("Rifle_SureFire")))
					{
						num10 = 0;
					}
					if (num10 < 0)
					{
						num10 = 0;
					}
					if (num12 < 25)
					{
						num10 = -num10;
					}
					num10 += intParameter2;
					if (Attacker.HasEffect<Running>() && (Skill != "Pistol" || !Attacker.HasSkill("Pistol_SlingAndRun")) && !Attacker.HasProperty("EnhancedSprint"))
					{
						num10 += Stat.Random(-23, 23);
					}
					if (flag3)
					{
						num10 += Stat.Random(-23, 23);
					}
					bool flag6 = false;
					int Spread = 0;
					int num13 = 0;
					if (num9 > 1)
					{
						flag6 = GetFixedMissileSpreadEvent.GetFor(ParentObject, out Spread);
						if (flag6)
						{
							num13 = Stat.Random(-WeaponAccuracy, WeaponAccuracy);
						}
					}
					List<bool> list4 = new List<bool>(num9);
					List<bool> list5 = new List<bool>(num9);
					List<bool> list6 = new List<bool>(num9);
					List<Cell> list7 = new List<Cell>(num9);
					for (int m = 0; m < num9; m++)
					{
						int num14 = intParameter2;
						int num15 = num10;
						int value2;
						if (flag6)
						{
							value2 = num13;
							int num16 = -Spread / 2 + Spread * m / (num9 - 1);
							num14 += num16;
							num15 += num16;
						}
						else
						{
							value2 = Stat.Random(-WeaponAccuracy, WeaponAccuracy);
						}
						Event event2 = Event.New("WeaponMissileWeaponShot");
						event2.SetParameter("AimVariance", num15);
						event2.SetParameter("FlatVariance", num14);
						event2.SetParameter("WeaponAccuracy", value2);
						ParentObject.FireEvent(event2);
						GameObject gameObject6 = ((list.Count > m) ? list[m] : null);
						if (gameObject6 == null)
						{
							MetricsManager.LogError("had no projectile for shot " + m + " from " + ParentObject.DebugName);
							continue;
						}
						bool PlayerInvolved;
						bool CameNearPlayer;
						Cell NearPlayerCell;
						List<Point> item = CalculateBulletTrajectory(out PlayerInvolved, out CameNearPlayer, out NearPlayerCell, missilePath, gameObject6, ParentObject, Attacker, Attacker.CurrentZone, event2.GetIntParameter("AimVariance").ToString(), event2.GetIntParameter("FlatVariance"), event2.GetIntParameter("WeaponAccuracy"));
						list3.Add(item);
						list4.Add(item: false);
						list5.Add(PlayerInvolved);
						list6.Add(CameNearPlayer);
						list7.Add(NearPlayerCell);
					}
					if (flag)
					{
						scrapBuffer2.Copy(scrapBuffer);
					}
					int num17 = Math.Min(num9, ShotsPerAnimation);
					The.Player.GetCurrentCell();
					Event event3 = Event.New("ProjectileEntering", "Attacker", Attacker);
					Event event4 = Event.New("ProjectileEnteringCell", "Attacker", Attacker);
					List<GameObject> objectsThatWantEvent = cell.ParentZone.GetObjectsThatWantEvent(PooledEvent<ProjectileMovingEvent>.ID, ProjectileMovingEvent.CascadeLevel);
					ProjectileMovingEvent projectileMovingEvent = null;
					if (objectsThatWantEvent.Count > 0)
					{
						projectileMovingEvent = PooledEvent<ProjectileMovingEvent>.FromPool();
						projectileMovingEvent.Attacker = Attacker;
						projectileMovingEvent.Launcher = ParentObject;
						projectileMovingEvent.TargetCell = cell;
						projectileMovingEvent.ApparentTarget = gameObject5;
						projectileMovingEvent.ScreenBuffer = scrapBuffer2;
					}
					GameObject gameObjectParameter = E.GetGameObjectParameter("MessageAsFrom");
					MissileWeaponVFXConfiguration missileWeaponVFXConfiguration = null;
					for (int n = 0; n < list.Count; n += num17)
					{
						int num18 = 0;
						bool flag7 = false;
						for (int num19 = n; num19 < n + num17 && num19 < list3.Count; num19++)
						{
							if (list3[num19].Count > num18)
							{
								num18 = list3[num19].Count;
							}
						}
						int num20 = AnimationDelay - num18 / 5;
						if (num20 > 0 && E.HasParameter("AnimationDelayMultiplier"))
						{
							num20 = (int)((float)num20 * E.GetParameter<float>("AnimationDelayMultiplier"));
						}
						int num21 = cell2.X - cell.X;
						int num22 = cell2.Y - cell.Y;
						_ = (int)Math.Sqrt(num21 * num21 + num22 * num22) / RangeIncrement;
						int num23 = ((VariableMaxRange != null) ? Math.Min(VariableMaxRange.RollCached(), MaxRange) : MaxRange);
						bool flag8 = false;
						for (int num24 = n; num24 < list.Count && num24 < list.Count + num17; num24++)
						{
							GameObjectBlueprint blueprint = list[num24].GetBlueprint();
							bool flag9 = false;
							Dictionary<string, string> value3;
							if (list[num24].HasStringProperty("ProjectileVFX") || blueprint.HasTag("ProjectileVFX"))
							{
								if (missileWeaponVFXConfiguration == null)
								{
									missileWeaponVFXConfiguration = MissileWeaponVFXConfiguration.next();
									CombatJuiceManager.startDelay();
								}
								missileWeaponVFXConfiguration.addStep(num24, list3[num24][0].location);
								missileWeaponVFXConfiguration.setPathProjectileVFX(num24, list[num24].GetPropertyOrTag("ProjectileVFX"), list[num24].GetPropertyOrTag("ProjectileVFXConfiguration"));
								flag9 = true;
							}
							else if (blueprint.xTags != null && blueprint.xTags.TryGetValue("ProjectileVFX", out value3))
							{
								if (missileWeaponVFXConfiguration == null)
								{
									missileWeaponVFXConfiguration = MissileWeaponVFXConfiguration.next();
									CombatJuiceManager.startDelay();
								}
								missileWeaponVFXConfiguration.addStep(num24, list3[num24][0].location);
								missileWeaponVFXConfiguration.setPathProjectileVFX(num24, value3);
								missileWeaponVFXConfiguration.SetPathProjectileRender(num24, list[num24]);
								flag9 = true;
							}
							if (flag9)
							{
								ConfigureMissileVisualEffectEvent.Send(missileWeaponVFXConfiguration, missileWeaponVFXConfiguration.GetPath(num24), Attacker, ParentObject, list[num24]);
							}
						}
						for (int num25 = 1; num25 < num18 && num25 <= num23; num25++)
						{
							if (flag && AmmoChar != "f" && AmmoChar != "m" && AmmoChar != "e")
							{
								scrapBuffer2.Copy(scrapBuffer);
							}
							bool flag10 = true;
							for (int num26 = n; num26 < n + num17 && num26 < list4.Count; num26++)
							{
								if (!list4[num26])
								{
									flag10 = false;
									break;
								}
							}
							if (flag10)
							{
								break;
							}
							for (int num27 = n; num27 < n + num17 && num27 < list3.Count; num27++)
							{
								List<Point> list8 = list3[num27];
								if (num25 >= list8.Count)
								{
									list4[num27] = true;
								}
								if (list4[num27])
								{
									continue;
								}
								Projectile projectile = list2[num27];
								GameObject Object3 = list[num27];
								Cell cell4 = parentZone.GetCell(list8[num25 - 1].X, list8[num25 - 1].Y);
								Cell cell5 = parentZone.GetCell(list8[num25].X, list8[num25].Y);
								if (cell5 != null)
								{
									missileWeaponVFXConfiguration?.addStep(num27, list8[num25].location);
								}
								if (flag && cell5 != null && cell5.IsVisible() && missileWeaponVFXConfiguration == null)
								{
									flag7 = true;
								}
								if (flag7)
								{
									string text2 = projectile.RenderChar ?? AmmoChar;
									scrapBuffer2.Goto(list8[num25].X, list8[num25].Y);
									if (text2 == "sm")
									{
										scrapBuffer2.Goto(list8[num25].X, list8[num25].Y);
										int num28 = Stat.Random(1, 3);
										string s = "+";
										if (num28 == 1)
										{
											s = "&R*";
										}
										if (num28 == 2)
										{
											s = "&W*";
										}
										if (num28 == 3)
										{
											s = "&Y*";
										}
										scrapBuffer2.Write(s);
									}
									else if (text2 == "e")
									{
										float num29 = 0f;
										float num30 = 0f;
										float num31 = (float)Stat.Random(85, 185) / 58f;
										num29 = (float)Math.Sin(num31) / 6f;
										num30 = (float)Math.Cos(num31) / 6f;
										int num32 = Stat.Random(1, 3);
										string text3 = "";
										text3 = ((char)Stat.Random(191, 198)).ToString() ?? "";
										if (num32 == 1)
										{
											text3 = "&Y" + text3;
										}
										if (num32 == 2)
										{
											text3 = "&W*" + text3;
										}
										if (num32 == 3)
										{
											text3 = "&C*" + text3;
										}
										XRLCore.ParticleManager.Add(text3, (float)list8[num25].X + num29 * 2f, (float)list8[num25].Y + num30 * 2f, num29, num30, 2);
										XRLCore.ParticleManager.Frame();
										XRLCore.ParticleManager.Render(scrapBuffer2);
										scrapBuffer2.Goto(list8[num25].X, list8[num25].Y);
										if (num32 == 1)
										{
											text3 = "&Y" + text3;
										}
										if (num32 == 2)
										{
											text3 = "&W*" + text3;
										}
										if (num32 == 3)
										{
											text3 = "&C*" + text3;
										}
										scrapBuffer2.Write(text3);
									}
									else if (text2.Contains("-"))
									{
										scrapBuffer2.Write(text2.Replace("-", text) ?? "");
									}
									else if (text2 == "m")
									{
										float num33 = 0f;
										float num34 = 0f;
										float num35 = (float)Stat.Random(85, 185) / 58f;
										num33 = (float)Math.Sin(num35) / 6f;
										num34 = (float)Math.Cos(num35) / 6f;
										int num36 = Stat.Random(1, 3);
										string text4 = "";
										switch (num36)
										{
										case 1:
											text4 = "°";
											break;
										case 2:
											text4 = "±";
											break;
										case 3:
											text4 = "²";
											break;
										}
										XRLCore.ParticleManager.Add(text4, list8[num25].X, list8[num25].Y, num33, num34);
										XRLCore.ParticleManager.Frame();
										XRLCore.ParticleManager.Render(scrapBuffer2);
										scrapBuffer2.Goto(list8[num25].X, list8[num25].Y);
										switch (num36)
										{
										case 1:
											text4 = "&R*";
											break;
										case 2:
											text4 = "&W*";
											break;
										case 3:
											text4 = "&Y*";
											break;
										}
										scrapBuffer2.Write(text4);
									}
									else if (text2.StartsWith("GG"))
									{
										string text5 = null;
										string text6 = text2.Substring(2);
										int num37 = 1;
										for (int num38 = Math.Min(list8.Count - 1, num25 - 1); num37 <= num38; num37++)
										{
											int x = list8[num37].X;
											int y = list8[num37].Y;
											if (num37 == num38)
											{
												text5 = "X";
											}
											else
											{
												int x2 = list8[num37 - 1].X;
												int y2 = list8[num37 - 1].Y;
												int x3 = list8[num37 + 1].X;
												int y3 = list8[num37 + 1].Y;
												if (y == y3 && y == y2)
												{
													text5 = "-";
												}
												else if (x == x3 && x == x2)
												{
													text5 = "|";
												}
												else if ((x == x3 && x != x2 && y != y3 && y == x2) || (x != x3 && x == x2 && y == y3 && y != x2))
												{
													text5 = null;
												}
												else if (y3 > y2)
												{
													text5 = ((x3 > x2) ? "\\" : "/");
												}
												else if (y3 < y2)
												{
													text5 = ((x3 > x2) ? "/" : "\\");
												}
											}
											if (!text5.IsNullOrEmpty())
											{
												if (!text6.IsNullOrEmpty())
												{
													text5 = "{{" + text6 + "|" + text5 + "}}";
												}
												scrapBuffer2.WriteAt(x, y, text5);
												scrapBuffer2.Draw();
												XRLCore.ParticleManager.Frame();
											}
										}
									}
									else
									{
										switch (text2)
										{
										case "HR":
										{
											for (int num41 = 1; num41 < list8.Count && num41 < num25; num41++)
											{
												scrapBuffer2.Goto(list8[num41].X, list8[num41].Y);
												string text8 = "&b";
												switch (Stat.Random(1, 3))
												{
												case 1:
													text8 = "&r";
													break;
												case 2:
													text8 = "&b";
													break;
												case 3:
													text8 = "&r";
													break;
												}
												switch (Stat.Random(1, 3))
												{
												case 1:
													text8 += "^b";
													break;
												case 2:
													text8 += "^r";
													break;
												case 3:
													text8 += "^b";
													break;
												}
												int num42 = Stat.Random(1, 3);
												scrapBuffer2.Write(text8 + " ");
											}
											break;
										}
										case "FR":
										{
											for (int num39 = 1; num39 < list8.Count && num39 < num25; num39++)
											{
												scrapBuffer2.Goto(list8[num39].X, list8[num39].Y);
												string text7 = "&C";
												switch (Stat.Random(1, 3))
												{
												case 1:
													text7 = "&C";
													break;
												case 2:
													text7 = "&B";
													break;
												case 3:
													text7 = "&Y";
													break;
												}
												switch (Stat.Random(1, 3))
												{
												case 1:
													text7 += "^C";
													break;
												case 2:
													text7 += "^B";
													break;
												case 3:
													text7 += "^Y";
													break;
												}
												int num40 = Stat.Random(1, 3);
												scrapBuffer2.Write(text7 + (char)(219 + Stat.Random(0, 4)));
											}
											break;
										}
										case "f":
										{
											for (int num43 = 1; num43 < list8.Count && num43 < num25; num43++)
											{
												Cell cell6 = parentZone?.GetCell(list8[num43].X, list8[num43].Y);
												if (cell6 != null)
												{
													cell6.Flameburst();
													continue;
												}
												scrapBuffer2.Goto(list8[num43].X, list8[num43].Y);
												string text9 = "&R";
												switch (Stat.Random(1, 3))
												{
												case 1:
													text9 = "&R";
													break;
												case 2:
													text9 = "&W";
													break;
												case 3:
													text9 = "&Y";
													break;
												}
												switch (Stat.Random(1, 3))
												{
												case 1:
													text9 += "^R";
													break;
												case 2:
													text9 += "^W";
													break;
												case 3:
													text9 += "^Y";
													break;
												}
												int num44 = Stat.Random(1, 3);
												scrapBuffer2.Write(text9 + (char)(219 + Stat.Random(0, 4)));
											}
											break;
										}
										default:
											scrapBuffer2.Write(text2 ?? "");
											break;
										}
									}
								}
								GetMissileWeaponPerformanceEvent @for = GetMissileWeaponPerformanceEvent.GetFor(gameObject, ParentObject, Object3, null, null, null, null, null, null, null, null);
								int num45 = 0;
								GameObject SolidObject;
								bool IsSolid;
								bool IsCover;
								GameObject gameObject7;
								while (true)
								{
									cell5.FindSolidObjectForMissile(Attacker, Projectile: Object3, Launcher: ParentObject, SolidObject: out SolidObject, IsSolid: out IsSolid, IsCover: out IsCover, RecheckHit: out var RecheckHit, RecheckPhase: out var _, PenetrateCreatures: @for.PenetrateCreatures, PenetrateWalls: @for.PenetrateWalls, ApparentTarget: gameObject5);
									if (RecheckHit && ++num45 < 100)
									{
										continue;
									}
									gameObject7 = cell5.GetCombatTarget(Attacker, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, Launcher: ParentObject, Projectile: Object3, CheckPhaseAgainst: Object3, Skip: null, SkipList: null, AllowInanimate: true, InanimateSolidOnly: true);
									if (gameObject7 == null)
									{
										break;
									}
									GameObject projectile3 = Object3;
									GameObject useVisibilityOf = ParentObject;
									GameObject attacker3 = Attacker;
									GameObject object2 = gameObject7;
									GameObject apparentTarget = gameObject5;
									bool Recheck;
									bool RecheckPhase2;
									bool flag11 = BeforeProjectileHitEvent.Check(projectile3, attacker3, object2, out Recheck, out RecheckPhase2, @for.PenetrateCreatures, @for.PenetrateWalls, useVisibilityOf, apparentTarget);
									if (!Recheck || ++num45 >= 100)
									{
										if (!flag11)
										{
											gameObject7 = null;
										}
										break;
									}
								}
								bool Done = false;
								bool showUninvolved = false;
								if (!IsSolid)
								{
									cell5.WakeCreaturesInArea();
									event3.SetParameter("Cell", cell5);
									event3.SetParameter("Path", missilePath);
									event3.SetParameter("p", num25);
									Object3.FireEvent(event3);
									event4.SetParameter("Projectile", Object3);
									event4.SetParameter("Cell", cell5);
									event4.SetParameter("Path", missilePath);
									event4.SetParameter("p", num25);
									if (!cell5.FireEvent(event4))
									{
										Done = true;
									}
									else if (!MissileTraversingCellEvent.Check(Object3, cell5, cell4.GetDirectionFromCell(cell5), Attacker))
									{
										Done = true;
									}
									else if (projectileMovingEvent != null)
									{
										projectileMovingEvent.Projectile = Object3;
										projectileMovingEvent.Defender = gameObject7;
										projectileMovingEvent.Cell = cell5;
										projectileMovingEvent.Path = list8;
										projectileMovingEvent.PathIndex = num25;
										foreach (GameObject item4 in objectsThatWantEvent)
										{
											bool num46 = item4.HandleEvent(projectileMovingEvent);
											if (projectileMovingEvent.HitOverride != null)
											{
												gameObject7 = projectileMovingEvent.HitOverride;
												projectileMovingEvent.HitOverride = null;
											}
											if (projectileMovingEvent.ActivateShowUninvolved)
											{
												showUninvolved = true;
											}
											if (!num46)
											{
												Done = true;
												break;
											}
										}
									}
								}
								if (!Done && !GameObject.Validate(ref Object3))
								{
									Done = true;
								}
								bool flag12 = false;
								if (gameObject7 != null && (!flag3 || cell5.DistanceTo(gameObject) >= 2))
								{
									if (AutoAct.IsActive())
									{
										if (gameObject7.IsPlayerControlledAndPerceptible() && !gameObject7.IsTrifling && !Attacker.IsPlayerControlled())
										{
											AutoAct.Interrupt("you " + gameObject7.GetPerceptionVerb() + " something shooting at " + gameObject7.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + (gameObject7.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(gameObject7))), null, gameObject7, IsThreat: true);
										}
										else if (gameObject7.DistanceTo(The.Player) <= 1 && Attacker.IsHostileTowards(The.Player))
										{
											AutoAct.Interrupt("something is shooting at you or " + gameObject7.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null), null, gameObject7, IsThreat: true);
										}
									}
									int num47 = Stat.Random(1, 20);
									int for2 = GetToHitModifierEvent.GetFor(Attacker, gameObject7, ParentObject, 0, Object3, null, Skill, null, Prospective: false, Missile: true);
									int num48 = num47 + for2;
									int combatDV = Stats.GetCombatDV(gameObject7);
									Event event5 = Event.New("WeaponGetDefenderDV");
									event5.SetParameter("Weapon", ParentObject);
									event5.SetParameter("Defender", gameObject7);
									event5.SetParameter("NaturalHitResult", num47);
									event5.SetParameter("Result", num48);
									event5.SetParameter("Skill", Skill);
									event5.SetParameter("DV", combatDV);
									gameObject7?.FireEvent(event5);
									event5.ID = "ProjectileGetDefenderDV";
									projectile?.FireEvent(event5);
									combatDV = event5.GetIntParameter("DV");
									if (!gameObject7.HasSkill("Acrobatics_SwiftReflexes"))
									{
										combatDV -= 5;
									}
									if (!gameObject7.IsMobile())
									{
										combatDV = -100;
									}
									if (num48 > combatDV)
									{
										if (Object3.HasTagOrProperty("NoDodging"))
										{
											if (gameObject7.IsPlayer())
											{
												if (gameObject7.HasPart<Combat>() && gameObject7.CanChangeMovementMode("Dodging"))
												{
													IComponent<GameObject>.XDidYToZ(gameObject7, "attempt", "to flinch away, but", Object3, "is too wide", "!", null, null, null, gameObject7);
												}
											}
											else if (gameObject7.IsVisible() && gameObject7.HasPart<Combat>() && gameObject7.CanChangeMovementMode("Dodging"))
											{
												IComponent<GameObject>.XDidYToZ(gameObject7, "attempt", "to flinch out of the way of", Object3, ", but it's too wide", "!", null, null, null, gameObject7);
											}
										}
										if (IComponent<GameObject>.Visible(gameObject7))
										{
											flag8 = true;
										}
										flag12 = true;
										bool PenetrateCreatures = false;
										bool PenetrateWalls = false;
										MissileHit(Attacker, gameObject7, gameObject, Object3, projectile, Object2, gameObject5, missilePath, cell5, fireType, intParameter, num47, num48, list5[num27], gameObjectParameter, flag2, ref Done, ref PenetrateCreatures, ref PenetrateWalls, flag5, showUninvolved);
									}
									else if (combatDV != -100 && gameObject7.InActiveZone() && !Object3.HasTagOrProperty("NoDodging"))
									{
										string passByVerb = projectile.PassByVerb;
										gameObject7.ParticleBlip("&K\t", 10, 0L);
										if (gameObject7.IsPlayer())
										{
											gameObject7.PlayWorldSound("sfx_missile_generic_flinched");
											if (!passByVerb.IsNullOrEmpty())
											{
												if (gameObject7.HasPart<Combat>() && gameObject7.CanChangeMovementMode("Dodging"))
												{
													IComponent<GameObject>.XDidYToZ(gameObject7, "flinch", "away as", Object3, Object3.GetVerb(passByVerb, PrependSpace: false) + " past " + The.Player.DescribeDirectionFrom(gameObject), "!", null, null, gameObject7, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
												}
												else
												{
													IComponent<GameObject>.XDidYToZ(Object3, passByVerb, "past " + The.Player.DescribeDirectionFrom(gameObject), gameObject7, null, "!", null, null, gameObject7, null, UseFullNames: false, IndefiniteSubject: true);
												}
											}
										}
										else if (gameObject7.IsVisible())
										{
											if (gameObject7.HasPart<Combat>() && gameObject7.CanChangeMovementMode("Dodging"))
											{
												IComponent<GameObject>.XDidYToZ(gameObject7, "flinch", "out of the way of", Object3, gameObject.IsPlayerControlled() ? null : The.Player.DescribeDirectionFrom(gameObject), "!", null, null, gameObject7, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
											}
											else if (!passByVerb.IsNullOrEmpty())
											{
												IComponent<GameObject>.XDidYToZ(Object3, passByVerb, "past", gameObject7, gameObject.IsPlayerControlled() ? null : The.Player.DescribeDirectionFrom(gameObject), "!", null, null, gameObject7, null, UseFullNames: false, IndefiniteSubject: true);
											}
										}
									}
									gameObject7.StopMoving();
								}
								if (!(IsSolid || Done))
								{
									continue;
								}
								bool PenetrateCreatures2 = false;
								bool PenetrateWalls2 = false;
								if (IsSolid || flag12)
								{
									cell5?.PlayWorldSound(Object3?.GetSoundTag("ImpactSound"));
								}
								if (IsSolid && !flag12)
								{
									if (Object3.IsValid())
									{
										cell5?.AddObject(Object3, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Missile Transit");
									}
									if (SolidObject != null)
									{
										int num49 = Stat.Random(1, 20) + Math.Max(0, Attacker.StatMod(Modifier));
										if (IComponent<GameObject>.Visible(SolidObject))
										{
											flag8 = true;
										}
										MissileHit(Attacker, SolidObject, gameObject, Object3, projectile, Object2, gameObject5, missilePath, cell5, fireType, intParameter, num49, num49, list5[num27], gameObjectParameter, flag2, ref Done, ref PenetrateCreatures2, ref PenetrateWalls2, flag5, showUninvolved);
									}
									else
									{
										Event event6 = Event.New("ProjectileHit");
										event6.SetParameter("Attacker", Attacker);
										event6.SetParameter("Defender", (object)null);
										event6.SetParameter("Skill", Skill);
										event6.SetParameter("Damage", (object)null);
										event6.SetParameter("AimLevel", intParameter);
										event6.SetParameter("Owner", Attacker);
										event6.SetParameter("Launcher", ParentObject);
										event6.SetParameter("Path", missilePath);
										event6.SetParameter("Penetrations", 0);
										event6.SetParameter("ApparentTarget", gameObject5);
										event6.SetParameter("AimedAt", Object2);
										event6.SetFlag("Critical", State: false);
										Object3.FireEvent(event6);
										event6.ID = "DefenderProjectileHit";
										gameObject7?.FireEvent(event6);
										event6.ID = "LauncherProjectileHit";
										ParentObject.FireEvent(event6);
									}
								}
								bool flag13 = !Done && ((IsSolid && PenetrateWalls2) || (IsCover && PenetrateCreatures2 && (SolidObject?.IsOrganic ?? false)));
								if (!flag13)
								{
									list4[num27] = true;
								}
								if (IsSolid && !flag13)
								{
									if (Object3.IsValid())
									{
										cell4?.AddObject(Object3, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Missile Transit", null, null, null, null, null, E);
									}
								}
								else if (Done && Object3.IsValid())
								{
									cell5?.AddObject(Object3, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Missile Transit", null, null, null, null, null, E);
								}
								if (!flag13 && Object3 != null && Object3.IsValid())
								{
									Object3.WasThrown(Attacker, gameObject5);
									CleanupProjectile(Object3);
								}
							}
							if (flag7)
							{
								XRLCore._Console.DrawBuffer(scrapBuffer2);
								if (num20 > 0)
								{
									Thread.Sleep(num20);
								}
							}
						}
						if (!flag8 && list6.Count > n && list6[n] && list5.Count > n && !list5[n])
						{
							GameObject Object4 = list[n];
							Projectile projectile4 = list2[n];
							if (GameObject.Validate(ref Object4) && !projectile4.PassByVerb.IsNullOrEmpty())
							{
								IComponent<GameObject>.EmitMessage(The.Player, Object4.Does(projectile4.PassByVerb, int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " past " + The.Player.DescribeDirectionFrom(gameObject) + ".");
							}
							if (!Attacker.IsPlayerLed())
							{
								AutoAct.Interrupt(null, list7[n], null, IsThreat: true);
							}
						}
					}
					for (int num50 = list.Count - 1; num50 >= 0; num50--)
					{
						GameObject Object5 = list[num50];
						if (GameObject.Validate(ref Object5))
						{
							Object5.Obliterate();
						}
					}
					CombatJuiceManager.endDelay();
					if (missileWeaponVFXConfiguration != null && Attacker?.CurrentZone != null && Attacker.CurrentZone.IsActive())
					{
						CombatJuice.missileWeaponVFX(missileWeaponVFXConfiguration);
					}
					float num51 = 1f;
					if (E.HasParameter("EnergyMultiplier"))
					{
						num51 = E.GetParameter<float>("EnergyMultiplier");
					}
					if (Skill == "Pistol" && num51 > 0f)
					{
						if (Attacker.HasEffect<EmptyTheClips>())
						{
							num51 *= 0.5f;
						}
						if (Attacker.HasSkill("Pistol_FastestGun"))
						{
							num51 *= 0.75f;
						}
						int intProperty = Attacker.GetIntProperty("PistolEnergyModifier");
						if (intProperty != 0)
						{
							num51 *= (100f - (float)intProperty) / 100f;
						}
					}
					ShotCompleteEvent.Send(ParentObject, gameObject, gameObject2);
					int num52 = (int)((float)EnergyCost * num51);
					if (num52 > 0)
					{
						gameObject.UseEnergy(num52, "Combat Missile " + Skill, null, null);
					}
					goto end_IL_018a;
					IL_0bdc:
					if (flag5 && gameObject5 != null && gameObject5 != Object2 && gameObject5.IsHostileTowards(gameObject) && gameObject5.IsVisible())
					{
						Sidebar.CurrentTarget = gameObject5;
					}
					goto IL_0c04;
					end_IL_018a:;
				}
				finally
				{
					if (CalculatedMissilePathInUse && missilePath == CalculatedMissilePath)
					{
						CalculatedMissilePath.Reset();
						CalculatedMissilePathInUse = false;
					}
					else if (SecondCalculatedMissilePathInUse && missilePath == SecondCalculatedMissilePath)
					{
						SecondCalculatedMissilePath.Reset();
						SecondCalculatedMissilePathInUse = false;
					}
					CombatJuiceManager.endDelay();
				}
			}
			return base.FireEvent(E);
		}

		public bool ReadyToFire()
		{
			return CheckReadyToFireEvent.Check(ParentObject);
		}

		public string GetNotReadyToFireMessage()
		{
			return GetNotReadyToFireMessageEvent.GetFor(ParentObject);
		}

		public string Status(MissileWeaponArea.MissileWeaponAreaWeaponStatus modernUIStatus = null)
		{
			if (modernUIStatus != null)
			{
				modernUIStatus.renderable = ParentObject.RenderForUI();
				modernUIStatus.text = "";
				modernUIStatus.display = "";
			}
			StringBuilder stringBuilder = Event.NewStringBuilder();
			GetMissileWeaponStatusEvent.Send(ParentObject, stringBuilder, modernUIStatus);
			string text = null;
			if (stringBuilder.Length > 0)
			{
				text = stringBuilder.ToString();
			}
			int num = 23;
			if (text != null)
			{
				num -= ConsoleLib.Console.ColorUtility.LengthExceptFormatting(text);
			}
			string text2;
			if (num > 0)
			{
				text2 = ParentObject.ShortDisplayNameStripped;
				if (text2.Length > num)
				{
					text2 = text2.Substring(0, num).Trim();
				}
				if (text != null)
				{
					text2 += text;
				}
			}
			else
			{
				text2 = text ?? "";
			}
			return text2;
		}

		public static bool IsVorpal(GameObject Object)
		{
			if (Object.TryGetPart<MissilePerformance>(out var Part) && !Part.AddAttributes.IsNullOrEmpty() && Part.AddAttributes.HasDelimitedSubstring(',', "Vorpal"))
			{
				return true;
			}
			if (Object.TryGetPart<PoweredMissilePerformance>(out var Part2) && !Part2.AddAttributes.IsNullOrEmpty() && Part2.AddAttributes.HasDelimitedSubstring(',', "Vorpal"))
			{
				return true;
			}
			GameObject Projectile = null;
			string Blueprint = null;
			GetMissileWeaponProjectileEvent.GetFor(Object, ref Projectile, ref Blueprint);
			if (Projectile != null)
			{
				Projectile part = Projectile.GetPart<Projectile>();
				if (part != null && part.HasAttribute("Vorpal"))
				{
					return true;
				}
			}
			if (Blueprint != null)
			{
				GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(Blueprint);
				if (blueprintIfExists != null && blueprintIfExists.GetPartParameter("Projectile", "Attributes", "").HasDelimitedSubstring(',', "Vorpal"))
				{
					return true;
				}
			}
			return false;
		}
	}
}
