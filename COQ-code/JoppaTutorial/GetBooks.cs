using Qud.UI;
using XRL;
using XRL.World;

namespace JoppaTutorial
{
	public class GetBooks : TutorialStep
	{
		private GameObject book1;

		private GameObject book2;

		private bool saw;

		public override bool BeforePlayerEnterCell(Cell cell)
		{
			return ConstrainToCurrentZone(cell);
		}

		public override bool AllowSelectedPopupCommand(string title, QudMenuItem command)
		{
			return true;
		}

		public override bool AllowCommand(string id)
		{
			if (step == 0 && base.CurrentGameView == "Stage" && The.Player.DistanceTo(The.PlayerCell.ParentZone.GetCell(38, 16)) <= 1)
			{
				if ((id == null || !id.Contains("CmdGetFrom")) && (id == null || !id.Contains("Cancel")))
				{
					return id?.Contains("AdventureMouseInteractAll") ?? false;
				}
				return true;
			}
			return true;
		}

		public override void GameSync()
		{
			if (book1 == null)
			{
				book1 = The.Player.CurrentZone.FindObject("TutorialAphorisms");
				book1?.RequireID();
				book2 = The.Player.CurrentZone.FindObject("TutorialMarkovBook");
				book2?.RequireID();
			}
		}

		public override void LateUpdate()
		{
			manager.ClearHighlight();
			if (The.Player.CurrentZone.GetCell(38, 16).GetObjectCount((GameObject o) => o.Blueprint == "TutorialMarkovBook") == 0)
			{
				TutorialManager.AdvanceStep(new ExploreStairs());
				return;
			}
			if (step == 0)
			{
				if (base.CurrentGameView == "Stage")
				{
					if (The.Player.DistanceTo(The.PlayerCell.ParentZone.GetCell(38, 16)) > 1)
					{
						manager.HighlightCell(38, 16, "There's something else over here.", "nw");
					}
					else
					{
						manager.HighlightCell(38, 16, "Sometimes squares have multiple items in them.\n\nTo see everything in a space, press ~CmdGetFrom and then choose a direction or press ~AdventureMouseInteractAll.", "nw");
					}
				}
				else if (base.CurrentGameView == "PickGameObject" && ControlId.Get("PickGameObject:Item:" + book1.ID) != null)
				{
					step = 1;
					TutorialManager.ShowCIDPopupAsync("PickGameObject:Item:" + book1.ID, "Whoever this was, they had some books. All books can be read in game.\n\nBooks with {{Y|white}} titles are generated and usually contain nonsense, but the nonsense can occasionally be useful.\n\nBooks with {{W|gold}} titles are more valuable: they are handwritten by in-world beings and contain interesting takes on the world.", "nw", "[~Accept] Continue", 16, 36, -18f);
				}
				if (book1 != null && (!book1.IsValid() || !book2.IsValid() || book1.InInventory != null || book2.InInventory != null))
				{
					step = 1;
				}
			}
			if (step != 1 && step != 2)
			{
				return;
			}
			if (base.CurrentGameView == "Stage")
			{
				if (step == 1)
				{
					manager.HighlightCell(38, 16, "Sometimes squares have multiple items in them.\n\nTo see everything in a space, press ~CmdGetFrom and then choose a direction or press ~AdventureMouseInteractAll.", "nw");
				}
				else
				{
					TutorialManager.AdvanceStep(new ExploreStairs());
				}
			}
			else if (base.CurrentGameView == "PickGameObject")
			{
				if (ControlId.Get("PickGameObject:Item:" + book1.ID) != null)
				{
					manager.HighlightByCID("PickGameObject:Item:" + book1.ID, "Get the books.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
					return;
				}
				if (ControlId.Get("PickGameObject:Item:" + book2.ID) != null)
				{
					manager.HighlightByCID("PickGameObject:Item:" + book2.ID, "Get the books.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
					return;
				}
				step = 2;
				manager.HighlightByCID("TAKE_ALL", "This water is valuable too. Take it all.", "ne", 64, 36, -18f);
			}
		}
	}
}
