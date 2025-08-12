using Qud.API;
using XRL.Core;
using XRL.Language;

namespace XRL.UI
{
	public class GritGateTerminalScreenKnowledge : GritGateTerminalScreen
	{
		private bool bSecretRevealed
		{
			get
			{
				return XRLCore.Core.Game.HasIntGameState("EreshkigalSecret");
			}
			set
			{
				if (value)
				{
					XRLCore.Core.Game.SetIntGameState("EreshkigalSecret", 1);
				}
				else
				{
					XRLCore.Core.Game.IntGameState.Remove("EreshkigalSecret");
				}
			}
		}

		public GritGateTerminalScreenKnowledge()
		{
			if (bSecretRevealed)
			{
				MainText = "I've done as you asked. Your repetition is ungainly.";
				Options.Add("Are you sapient?");
				Options.Add("What is the Thin World?");
				Options.Add("Exit.");
				return;
			}
			MainText = "It is done.";
			Options.Add("Thank you, Ereshkigal.");
			optionActions.Add(delegate
			{
				bSecretRevealed = true;
				IBaseJournalEntry randomUnrevealedNote = JournalAPI.GetRandomUnrevealedNote();
				JournalMapNote obj = randomUnrevealedNote as JournalMapNote;
				string text = "";
				text = ((obj == null) ? randomUnrevealedNote.Text : ("The location of " + Grammar.InitLowerIfArticle(randomUnrevealedNote.Text)));
				Popup.Show("Ereshkigal delivers insight from the Thin World:\n\n" + text);
				randomUnrevealedNote.Reveal("Ereshkigal");
				base.Terminal.CurrentScreen = null;
			});
		}

		public override void Back()
		{
			base.Terminal.CurrentScreen = null;
		}

		public override void Activate()
		{
			if (bSecretRevealed)
			{
				if (base.Terminal.Selected == 0)
				{
					base.Terminal.CurrentScreen = new GritGateTerminalScreenWhoAreYou();
				}
				if (base.Terminal.Selected == 1)
				{
					base.Terminal.CurrentScreen = new GritGateTerminalScreenThinWorld();
				}
				if (base.Terminal.Selected == 2)
				{
					base.Terminal.CurrentScreen = null;
				}
			}
			else
			{
				optionActions[base.Terminal.Selected]();
			}
		}
	}
}
