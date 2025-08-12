using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Conversations.Parts
{
	public class WaterRitualBuySecret : IWaterRitualSecretPart
	{
		public override bool Available => WaterRitual.Record.secretsRemaining > 0;

		public override bool IsBuy => true;

		public override bool Filter(IBaseJournalEntry Entry)
		{
			return Entry.CanBuy();
		}

		public override int GetWeight(IBaseJournalEntry Entry)
		{
			return WaterRitual.RecordFaction.GetSellSecretWeight(Entry);
		}

		public override void Awake()
		{
			base.Awake();
			Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Secret", 50);
		}

		public virtual void Share()
		{
			IBaseJournalEntry Note = GetDistinctNote();
			HistoryAPI.OnWaterRitualBuySecret(WaterRitual.Record, ref Note);
			RevealEntry(Note);
		}

		public virtual void RevealEntry(IBaseJournalEntry Entry)
		{
			bool flag = true;
			Entry.Attributes.Add(WaterRitual.RecordFaction.NoBuySecretString);
			WaterRitual.Record.secretsRemaining--;
			if (!(Entry is JournalSultanNote journalSultanNote))
			{
				if (!(Entry is JournalMapNote journalMapNote))
				{
					if (!(Entry is JournalObservation journalObservation))
					{
						if (Entry is JournalRecipeNote)
						{
							Popup.Show(The.Speaker.Does("share", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " a recipe with you.");
						}
					}
					else
					{
						string text = The.Speaker.Does("share", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " some gossip with you.";
						if (Entry.Has("gossip"))
						{
							text += "\n\n\"";
							string text2 = HistoricStringExpander.ExpandString("<spice.gossip.leadIns.!random>", null, The.Game.sultanHistory);
							text = ((!text2.Contains('?') && !text2.Contains('.') && !journalObservation.Rumor) ? (text + text2 + " " + Grammar.InitLower(journalObservation.Text)) : (text + text2 + " " + journalObservation.Text));
							text += "\"";
						}
						Popup.Show(text);
					}
				}
				else
				{
					Popup.Show(The.Speaker.Does("share", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " the location of " + Markup.Wrap(Grammar.LowerArticles(journalMapNote.Text)) + ".");
				}
			}
			else
			{
				HistoricEvent @event = HistoryAPI.GetEvent(journalSultanNote.EventID);
				Popup.Show(The.Speaker.Does("share", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null) + " an event from the life of a sultan with you.\n\n\"" + @event.GetEventProperty("gospel") + "\"");
				@event.Reveal();
				flag = false;
			}
			if (WaterRitual.RecordFaction.Visible)
			{
				Entry.AppendHistory(" {{K|-learned from " + WaterRitual.RecordFaction.GetFormattedName() + "}}");
			}
			if (flag || !Entry.Revealed)
			{
				Entry.Reveal(WaterRitual.RecordFaction.GetFormattedName());
			}
			Entry.Updated();
		}

		public override bool WantEvent(int ID, int Propagation)
		{
			if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
			{
				return ID == EnteredElementEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(EnteredElementEvent E)
		{
			if (WaterRitual.Record.secretsRemaining <= 0)
			{
				Popup.ShowFail(The.Speaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + " has no more secrets to share.");
			}
			else if (UseReputation())
			{
				Share();
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetChoiceTagEvent E)
		{
			E.Tag = "{{" + Lowlight + "|[{{" + Numeric + "|" + GetReputationCost() + "}} reputation]}}";
			return false;
		}
	}
}
