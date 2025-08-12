using System;
using ConsoleLib.Console;
using XRL.World.Capabilities;

namespace XRL.World.Parts
{
	[Serializable]
	public class StrideMason : IPoweredPart
	{
		public const int BP_COST_DIVISOR = 500;

		public string Source;

		public bool DynamicCharge;

		public bool Imitate;

		public string Blueprint = "Sandstone";

		public string DisplayName;

		public string Description;

		public Renderable Renderable;

		[NonSerialized]
		public int BlueprintChargeUse;

		public StrideMason()
		{
			ChargeUse = 10;
			WorksOnEquipper = true;
			ResetImitation();
		}

		public void ResetImitation()
		{
			BlueprintChargeUse = -1;
			DisplayName = null;
			Description = null;
			Renderable = null;
		}

		public bool IsReady(bool UseCharge = false)
		{
			if (Blueprint != null)
			{
				return IsReady(UseCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, GetChargeUse(), UseChargeIfUnpowered: false, 0L, null);
			}
			return false;
		}

		public bool IsValidCell(Cell C)
		{
			if (C != null && !C.OnWorldMap())
			{
				return C.IsEmpty();
			}
			return false;
		}

		public bool IsValidWall(GameObject Wall)
		{
			if (Wall != null && Wall.Blueprint != Blueprint && Wall.IsWall() && !Wall.HasTag("Plant") && !Wall.HasTag("PlantLike") && !Wall.IsTemporary && Wall.Physics.IsReal)
			{
				return !Wall.HasPart<Forcefield>();
			}
			return false;
		}

		public bool IsImitable(GameObject Wall)
		{
			if (Wall.Render != null && !Wall.HasProperName && (Wall.HasIntProperty("ForceMutableSave") || !Wall.HasTag("Immutable")))
			{
				return !Wall.HasTagOrProperty("QuestItem");
			}
			return false;
		}

		public void ApplyRenderable(GameObject Wall)
		{
			Render render = Wall.Render;
			render.DisplayName = DisplayName ?? render.DisplayName;
			render.Tile = Renderable.Tile ?? render.Tile;
			render.RenderString = Renderable.RenderString ?? render.RenderString;
			render.ColorString = Renderable.ColorString ?? render.ColorString;
			render.TileColor = Renderable.TileColor ?? render.TileColor;
			if (Renderable.DetailColor != 0)
			{
				render.DetailColor = Renderable.DetailColor.ToString();
			}
			if (Wall.TryGetPart<Description>(out var Part))
			{
				Part.Short = Description ?? Part._Short;
			}
			if (Wall.HasTag("Immutable"))
			{
				Wall.SetIntProperty("ForceMutableSave", 1);
			}
		}

		public int GetChargeUse()
		{
			if (!DynamicCharge)
			{
				return ChargeUse;
			}
			if (BlueprintChargeUse == -1 && GameObjectFactory.Factory.Blueprints.TryGetValue(Blueprint, out var value))
			{
				BlueprintChargeUse = value.Stat("AV", 1) * value.Stat("Hitpoints", 1) / 500;
			}
			return Math.Max(ChargeUse, BlueprintChargeUse);
		}

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID)
			{
				return ID == UnequippedEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(EquippedEvent E)
		{
			E.Actor.RegisterEvent(this, LeftCellEvent.ID, 0, Serialize: true);
			if (Source == "Look")
			{
				E.Actor.RegisterPartEvent(this, "LookedAt");
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(UnequippedEvent E)
		{
			E.Actor.UnregisterEvent(this, LeftCellEvent.ID);
			E.Actor.UnregisterPartEvent(this, "LookedAt");
			return base.HandleEvent(E);
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

		public override bool HandleEvent(LeftCellEvent E)
		{
			if (IsValidCell(E.Cell) && IsReady(UseCharge: true))
			{
				E.Cell.AddObject(GenerateProduct());
				E.Cell.SetReachable(State: false);
			}
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return false;
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "LookedAt" && Source == "Look")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
				if (IsValidWall(gameObjectParameter) && IsReady())
				{
					ResetImitation();
					Blueprint = gameObjectParameter.Blueprint;
					if (Imitate && IsImitable(gameObjectParameter))
					{
						DisplayName = gameObjectParameter.Render.DisplayName;
						Description = gameObjectParameter.GetPart<Description>()?._Short;
						Renderable = new Renderable(gameObjectParameter.Render);
					}
				}
			}
			return base.FireEvent(E);
		}

		private GameObject GenerateProduct(GameObject Actor = null)
		{
			GameObject gameObject = GameObject.Create(Blueprint);
			if (Imitate && Renderable != null)
			{
				ApplyRenderable(gameObject);
			}
			Phase.carryOver(Actor ?? ParentObject.Equipped ?? ParentObject.Implantee, gameObject);
			return gameObject;
		}

		private bool ExamineFailure(IExamineEvent E, int Chance)
		{
			if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, null))
			{
				for (int i = 0; i < 3; i++)
				{
					Cell cell = ParentObject.GetCurrentCell()?.GetRandomLocalAdjacentCell();
					if (IsValidCell(cell))
					{
						GameObject @object = GenerateProduct(E.Actor);
						cell.AddObject(@object);
						DidXToY("extrude", @object, null, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, E.Actor.IsPlayer());
						E.Identify = true;
						return true;
					}
				}
			}
			return false;
		}
	}
}
