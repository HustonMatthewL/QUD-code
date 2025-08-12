using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class Panhumor : IPart
	{
		public int Level = 1;

		public int MoveBoost;

		public string Form;

		public string Blueprint;

		public GameObject Pod1;

		public GameObject Pod2;

		public GameObject Pod3;

		public GameObject Pod4;

		public override bool SameAs(IPart p)
		{
			ElementalJelly elementalJelly = p as ElementalJelly;
			if (elementalJelly.Level != Level)
			{
				return false;
			}
			if (elementalJelly.MoveBoost != MoveBoost)
			{
				return false;
			}
			if (elementalJelly.Form != Form)
			{
				return false;
			}
			if (elementalJelly.Blueprint != Blueprint)
			{
				return false;
			}
			return base.SameAs(p);
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("BeginTakeAction");
			Registrar.Register("ObjectCreated");
			Registrar.Register("BeforeDeathRemoval");
			Registrar.Register("EnteredCell");
			base.Register(Object, Registrar);
		}

		public void NewForm()
		{
			string form = Form;
			switch (Stat.Random(1, 4))
			{
			case 1:
				Form = "Fire";
				break;
			case 2:
				Form = "Ice";
				break;
			case 3:
				Form = "Acid";
				break;
			case 4:
				Form = "Shocking";
				break;
			}
			if (!(Form == form))
			{
				if (MoveBoost != 0)
				{
					ParentObject.Statistics["MoveSpeed"].Penalty -= MoveBoost;
					MoveBoost = 0;
				}
				MoveBoost = 5 * Level;
				ParentObject.Statistics["MoveSpeed"].Penalty += MoveBoost;
				List<string> list = new List<string> { "Fire", "Ice", "Acid", "Shocking" };
				list.ShuffleInPlace();
				Form = list[0];
				CheckPodInit(ref Pod1);
				SetupPod(Pod1);
				Form = list[1];
				CheckPodInit(ref Pod2);
				SetupPod(Pod2);
				Form = list[2];
				CheckPodInit(ref Pod3);
				SetupPod(Pod3);
				Form = list[3];
				CheckPodInit(ref Pod4);
				SetupPod(Pod4);
			}
		}

		private void CheckPodInit(ref GameObject obj)
		{
			if (obj == null || obj.IsInvalid())
			{
				obj = GameObject.Create(Blueprint);
				obj.SetStringProperty("NeverStack", "1");
				ParentObject.ReceiveObject(obj);
			}
		}

		private void SetupPod(GameObject obj)
		{
			obj.RemovePart<TemperatureOnHit>();
			obj.RemovePart<DischargeOnHit>();
			ElementalDamage part = obj.GetPart<ElementalDamage>();
			if (Form == "Fire")
			{
				obj.Render.DisplayName = "{{R|flaming pseudopod}}";
				if (part != null)
				{
					part.Attributes = "Fire";
				}
				obj.AddPart(new TemperatureOnHit(Level + "d20+" + Level * 50, 400));
			}
			else if (Form == "Ice")
			{
				obj.Render.DisplayName = "{{C|hoary pseudopod}}";
				if (part != null)
				{
					part.Attributes = "Cold";
				}
				obj.AddPart(new TemperatureOnHit("-" + Level + "d20-" + Level * 20, 400));
			}
			else if (Form == "Acid")
			{
				obj.Render.DisplayName = "{{G|acidic pseudopod}}";
				if (part != null)
				{
					part.Attributes = "Acid";
				}
			}
			else if (Form == "Shocking")
			{
				obj.Render.DisplayName = "{{W|sparking pseudopod}}";
				if (part != null)
				{
					part.Attributes = "Shock";
				}
				obj.AddPart(new DischargeOnHit("3d3", "1d6"));
			}
		}

		public override bool FireEvent(Event E)
		{
			if (!(E.ID == "BeginTakeAction"))
			{
				if (E.ID == "EnteredCell")
				{
					ParentObject.CurrentCell.AddObject("SaltyAcidPool");
				}
				else if (E.ID == "ObjectCreated")
				{
					NewForm();
				}
				else if (E.ID == "BeforeDeathRemoval")
				{
					if (ParentObject.Physics.CurrentCell == null || ParentObject.Physics.CurrentCell.ParentZone == null || !ParentObject.Physics.CurrentCell.ParentZone.IsActive())
					{
						return true;
					}
					bool flag = true;
					if (Form == "Shocking" || flag)
					{
						ParentObject.Discharge(ParentObject.CurrentCell.GetRandomLocalAdjacentCell() ?? ParentObject.CurrentCell, 20, 0, Level + "d20", null, ParentObject, ParentObject);
					}
					if (Form == "Fire" || flag)
					{
						DidX("explode", null, "!", null, null, null, IComponent<GameObject>.ThePlayer);
						Physics physics = ParentObject.Physics;
						if (physics != null && physics.CurrentCell != null)
						{
							List<Cell> adjacentCells = physics.CurrentCell.GetAdjacentCells();
							adjacentCells.Add(physics.CurrentCell);
							int num = 1000 * Level;
							TextConsole.LoadScrapBuffers();
							TextConsole textConsole = Look._TextConsole;
							ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
							XRLCore.Core.RenderMapToBuffer(scrapBuffer);
							GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
							int phase = ParentObject.GetPhase();
							foreach (Cell item in adjacentCells)
							{
								item.TemperatureChange(num, gameObjectParameter, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, phase);
								if (num > 0)
								{
									scrapBuffer.Goto(item.X, item.Y);
									switch (Stat.Random(1, 3))
									{
									case 1:
										scrapBuffer.Write("&R*");
										break;
									case 2:
										scrapBuffer.Write("&W*");
										break;
									default:
										scrapBuffer.Write("&r*");
										break;
									}
								}
								else
								{
									scrapBuffer.Goto(item.X, item.Y);
									switch (Stat.Random(1, 3))
									{
									case 1:
										scrapBuffer.Write("&C*");
										break;
									case 2:
										scrapBuffer.Write("&Y*");
										break;
									default:
										scrapBuffer.Write("&c*");
										break;
									}
								}
								if (item.GetObjectCount() <= 0)
								{
									continue;
								}
								GameObject objectInCell = item.GetObjectInCell(0);
								if (num > 0)
								{
									for (int i = 0; i < 5; i++)
									{
										objectInCell.ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
									for (int j = 0; j < 5; j++)
									{
										objectInCell.ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
									for (int k = 0; k < 5; k++)
									{
										objectInCell.ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
								}
								else
								{
									for (int l = 0; l < 5; l++)
									{
										objectInCell.ParticleText("&Y" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
									for (int m = 0; m < 5; m++)
									{
										objectInCell.ParticleText("&C" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
									for (int n = 0; n < 5; n++)
									{
										objectInCell.ParticleText("&c" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
								}
							}
							textConsole.DrawBuffer(scrapBuffer);
						}
					}
					if (Form == "Ice" || flag)
					{
						DidX("explode", null, "!", null, null, null, IComponent<GameObject>.ThePlayer);
						Physics physics2 = ParentObject.Physics;
						if (physics2 != null && physics2.CurrentCell != null)
						{
							List<Cell> adjacentCells2 = physics2.CurrentCell.GetAdjacentCells();
							adjacentCells2.Add(physics2.CurrentCell);
							int num2 = -1000 * Level;
							TextConsole.LoadScrapBuffers();
							TextConsole textConsole2 = Look._TextConsole;
							ScreenBuffer scrapBuffer2 = TextConsole.ScrapBuffer;
							XRLCore.Core.RenderMapToBuffer(scrapBuffer2);
							GameObject gameObjectParameter2 = E.GetGameObjectParameter("Owner");
							int phase2 = ParentObject.GetPhase();
							foreach (Cell item2 in adjacentCells2)
							{
								item2.TemperatureChange(num2, gameObjectParameter2, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, phase2);
								if (num2 > 0)
								{
									scrapBuffer2.Goto(item2.X, item2.Y);
									switch (Stat.Random(1, 3))
									{
									case 1:
										scrapBuffer2.Write("&R*");
										break;
									case 2:
										scrapBuffer2.Write("&W*");
										break;
									default:
										scrapBuffer2.Write("&r*");
										break;
									}
								}
								else
								{
									scrapBuffer2.Goto(item2.X, item2.Y);
									switch (Stat.Random(1, 3))
									{
									case 1:
										scrapBuffer2.Write("&C*");
										break;
									case 2:
										scrapBuffer2.Write("&Y*");
										break;
									default:
										scrapBuffer2.Write("&c*");
										break;
									}
								}
								if (item2.GetObjectCount() <= 0)
								{
									continue;
								}
								GameObject objectInCell2 = item2.GetObjectInCell(0);
								if (num2 > 0)
								{
									for (int num3 = 0; num3 < 5; num3++)
									{
										objectInCell2.ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
									for (int num4 = 0; num4 < 5; num4++)
									{
										objectInCell2.ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
									for (int num5 = 0; num5 < 5; num5++)
									{
										objectInCell2.ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
								}
								else
								{
									for (int num6 = 0; num6 < 5; num6++)
									{
										objectInCell2.ParticleText("&Y" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
									for (int num7 = 0; num7 < 5; num7++)
									{
										objectInCell2.ParticleText("&C" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
									for (int num8 = 0; num8 < 5; num8++)
									{
										objectInCell2.ParticleText("&c" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
									}
								}
							}
							textConsole2.DrawBuffer(scrapBuffer2);
						}
					}
					if ((Form == "Acid" || flag) && ParentObject.Physics.CurrentCell != null)
					{
						List<Cell> adjacentCells3 = ParentObject.Physics.CurrentCell.GetAdjacentCells();
						adjacentCells3.Add(ParentObject.Physics.CurrentCell);
						foreach (Cell item3 in adjacentCells3)
						{
							if (!item3.IsOccluding() && Stat.Random(0, 100) <= 75)
							{
								item3.AddObject(GameObjectFactory.Factory.CreateObject("SaltyAcidPool"));
							}
						}
					}
				}
			}
			return base.FireEvent(E);
		}
	}
}
