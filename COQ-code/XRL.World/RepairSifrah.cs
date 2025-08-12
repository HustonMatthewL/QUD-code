using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;

namespace XRL.World
{
	[Serializable]
	public class RepairSifrah : TinkeringSifrah
	{
		public int Complexity;

		public int Difficulty;

		public int RepairRating;

		public bool Abort;

		public bool AsMaster;

		public bool Bypass;

		public bool InterfaceExitRequested;

		public string HandlerID;

		public string HandlerPartName;

		private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
		{
			new SifrahSlotConfiguration("replacing gearworks", "Items/sw_gears2.bmp", "é", "&c", 'c'),
			new SifrahSlotConfiguration("reconnecting spark vines", "Items/sw_copper_wire.bmp", "í", "&W", 'G'),
			new SifrahSlotConfiguration("restoring humour flows", "Items/sw_sparktick_plasma.bmp", "÷", "&Y", 'y'),
			new SifrahSlotConfiguration("sealing glow modules", "Items/sw_lens.bmp", "\u001f", "&B", 'b'),
			new SifrahSlotConfiguration("appeasing spirits", "Items/sw_computer.bmp", "Ñ", "&c", 'G'),
			new SifrahSlotConfiguration("invoking the beyond", "Items/sw_wind_turbine_3.bmp", "ì", "&m", 'K')
		};

		public RepairSifrah(GameObject ContextObject, int Complexity, int Difficulty, int RepairRating)
		{
			Description = "Repairing " + ContextObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: false, BaseOnly: false, IndicateHidden: false, SecondPerson: true, Reflexive: false, null);
			this.Complexity = Complexity;
			this.Difficulty = Difficulty;
			this.RepairRating = RepairRating;
			int num = 3 + Complexity + Difficulty;
			int num2 = 4;
			if (Complexity >= 3)
			{
				num2++;
			}
			if (Complexity >= 7)
			{
				num2++;
				num += 4;
			}
			if (num2 > slotConfigs.Count)
			{
				num2 = slotConfigs.Count;
			}
			MaxTurns = 3;
			bool Interrupt = false;
			bool PsychometryApplied = false;
			int SecondaryBonus = 0;
			int @for = GetTinkeringBonusEvent.GetFor(The.Player, ContextObject, "Repair", MaxTurns, 0, ref SecondaryBonus, ref Interrupt, ref PsychometryApplied, Interruptable: true, ForSifrah: true);
			if (Interrupt)
			{
				Finished = true;
				Abort = true;
			}
			MaxTurns += @for;
			if (SecondaryBonus != 0)
			{
				RepairRating += SecondaryBonus;
				this.RepairRating = RepairRating;
			}
			List<SifrahToken> list = new List<SifrahToken>(num);
			if (list.Count < num && (Complexity < 4 || RepairRating > 10 + Complexity * 2))
			{
				list.Add(new TinkeringSifrahTokenPhysicalManipulation());
			}
			if (list.Count < num && The.Player.HasSkill("TenfoldPath_Hok"))
			{
				list.Add(new TinkeringSifrahTokenTenfoldPathHok());
			}
			if (list.Count < num && The.Player.HasPart<Telekinesis>() && (Complexity < 3 || RepairRating > Difficulty + Complexity * 2))
			{
				list.Add(new TinkeringSifrahTokenTelekinesis());
			}
			int num3 = num - list.Count;
			if (num3 > 0)
			{
				List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
				if (Complexity < 5 || RepairRating > 5 + Complexity * 3)
				{
					list2.Add(new TinkeringSifrahTokenToolkit());
				}
				if (TinkeringSifrahTokenCreationKnowledge.IsPotentiallyAvailableFor(ContextObject))
				{
					list2.Add(new TinkeringSifrahTokenCreationKnowledge(ContextObject));
				}
				list2.Add(new TinkeringSifrahTokenComputePower((Complexity + Difficulty) * (Complexity + Difficulty)));
				list2.Add(new TinkeringSifrahTokenCharge(Math.Max(Math.Min(250 * (Complexity + Difficulty), 10000), 250)));
				foreach (BitType bitType in BitType.BitTypes)
				{
					if (bitType.Level > Complexity)
					{
						break;
					}
					list2.Add(new TinkeringSifrahTokenBit(bitType));
				}
				list2.Add(new TinkeringSifrahTokenLiquid("oil"));
				list2.Add(new TinkeringSifrahTokenLiquid("gel"));
				list2.Add(new TinkeringSifrahTokenLiquid("acid"));
				list2.Add(new TinkeringSifrahTokenCopperWire());
				AssignPossibleTokens(list2, list, num3, num);
			}
			if (num > list.Count)
			{
				num = list.Count;
			}
			List<SifrahSlot> slots = SifrahSlot.GenerateListFromConfigurations(slotConfigs, num2, num);
			Slots = slots;
			Tokens = list;
			if (!AnyUsableTokens(ContextObject))
			{
				Popup.ShowFail("You have no usable options to employ for repairing " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null) + ", giving you no chance of success. You can remedy this situation by improving your Intelligence and tinkering skills, or by obtaining items useful for tinkering.");
				Finished = true;
				Abort = true;
				return;
			}
			bool? asMaster = true;
			int? slots2 = Slots.Count;
			int? tokens = Tokens.Count;
			if (GetGameCount(null, slots2, tokens, asMaster, null) > 0)
			{
				AsMaster = true;
			}
			else
			{
				tokens = Slots.Count;
				slots2 = Tokens.Count;
				int gameCount = GetGameCount(null, tokens, slots2, null, null);
				int num4 = GetGameCount(Slots: Slots.Count, Tokens: Tokens.Count, Outcome: GameOutcome.Failure, AsMaster: null, Mastered: null);
				int num5 = GetGameCount(Slots: Slots.Count, Tokens: Tokens.Count, Outcome: GameOutcome.CriticalFailure, AsMaster: null, Mastered: null);
				int num6 = Tokens.Count + num4 / 2 + num5 + gameCount / 20;
				slots2 = Slots.Count;
				tokens = Tokens.Count;
				asMaster = true;
				int gameCount2 = GetGameCount(null, slots2, tokens, null, asMaster);
				asMaster = true;
				bool? asMaster2 = false;
				int gameCount3 = GetGameCount(null, null, null, asMaster2, asMaster);
				if (gameCount2 + gameCount3 / 50 >= num6)
				{
					AsMaster = true;
				}
			}
			if (!AsMaster)
			{
				return;
			}
			string sifrahRepairAuto = Options.SifrahRepairAuto;
			if (!(sifrahRepairAuto == "Always"))
			{
				if (sifrahRepairAuto == "Never")
				{
					return;
				}
				DialogResult dialogResult = Popup.ShowYesNoCancel("You have mastered repairs of this complexity. Do you want to perform detailed repairs anyway, with an enhanced chance of exceptional success? If you answer 'No', you will automatically succeed at the repairs.");
				if (dialogResult != 0)
				{
					Finished = true;
					if (dialogResult == DialogResult.No)
					{
						Bypass = true;
					}
					else
					{
						Abort = true;
					}
				}
			}
			else
			{
				Finished = true;
				Bypass = true;
			}
		}

		public override bool CheckEarlyExit(GameObject ContextObject)
		{
			CalculateOutcomeChances(out var Success, out var _, out var _, out var CriticalSuccess, out var CriticalFailure);
			if (Success <= 0 && CriticalSuccess <= 0 && Turn == 1)
			{
				Abort = true;
				return true;
			}
			string text = "Do you want to try to complete the repair in its current state?";
			if (Turn > 1)
			{
				int num = MaxTurns - Turn + 1;
				int num2 = CriticalFailure - num;
				if (num2 > 0)
				{
					text = text + " Answering 'No' will abort the repair, but will still have " + (Grammar.IndefiniteArticleShouldBeAn(num2) ? "an" : "a") + " {{C|" + num2 + "%}} chance of causing a critical failure.";
				}
			}
			switch (Popup.ShowYesNoCancel(text))
			{
			case DialogResult.Yes:
				return true;
			case DialogResult.No:
				Abort = true;
				return true;
			default:
				return false;
			}
		}

		public override bool CheckOutOfOptions(GameObject ContextObject)
		{
			CalculateOutcomeChances(out var _, out var _, out var _, out var _, out var CriticalFailure);
			string text = "You have no more usable options. Do you want to try to complete the repair in its current state?";
			if (Turn > 1)
			{
				int num = MaxTurns - Turn + 1;
				int num2 = CriticalFailure - num;
				if (num2 > 0)
				{
					text = text + " Answering 'No' will abort the repair, but will still have " + (Grammar.IndefiniteArticleShouldBeAn(num2) ? "an" : "a") + " {{C|" + num2 + "%}} chance of causing a critical failure.";
				}
			}
			if (Popup.ShowYesNo(text) != 0)
			{
				Abort = true;
			}
			return true;
		}

		public override void Finish(GameObject ContextObject)
		{
			GameObject gameObject = GameObject.FindByID(HandlerID);
			if (gameObject == null || !(gameObject.GetPart(HandlerPartName) is IRepairSifrahHandler repairSifrahHandler))
			{
				return;
			}
			if (Bypass)
			{
				PlayOutcomeSound(GameOutcome.Success);
				repairSifrahHandler.RepairResultSuccess(The.Player, ContextObject);
				return;
			}
			if (base.PercentSolved == 100)
			{
				ContextObject.ModIntProperty("ItemNamingBonus", 1);
			}
			if (!Abort)
			{
				GameOutcome gameOutcome = DetermineOutcome();
				PlayOutcomeSound(gameOutcome);
				bool mastered = false;
				switch (gameOutcome)
				{
				case GameOutcome.CriticalFailure:
					repairSifrahHandler.RepairResultCriticalFailure(The.Player, ContextObject);
					break;
				case GameOutcome.Failure:
					repairSifrahHandler.RepairResultFailure(The.Player, ContextObject);
					break;
				case GameOutcome.PartialSuccess:
					repairSifrahHandler.RepairResultPartialSuccess(The.Player, ContextObject);
					break;
				case GameOutcome.Success:
				{
					if (base.PercentSolved >= 100)
					{
						mastered = true;
					}
					repairSifrahHandler.RepairResultSuccess(The.Player, ContextObject);
					int num2 = (Tokens.Count * Tokens.Count - Slots.Count) * (Complexity + Difficulty);
					if (base.PercentSolved < 100)
					{
						num2 = num2 * (100 - (100 - base.PercentSolved) * 3) / 100;
					}
					if (num2 > 0)
					{
						The.Player.AwardXP(num2, -1, 0, int.MaxValue, null, ContextObject);
					}
					break;
				}
				case GameOutcome.CriticalSuccess:
				{
					if (base.PercentSolved >= 100)
					{
						mastered = true;
					}
					repairSifrahHandler.RepairResultExceptionalSuccess(The.Player, ContextObject);
					TinkeringSifrah.AwardInsight();
					int num = (Tokens.Count * Tokens.Count - Slots.Count) * (Complexity + Difficulty) * 3;
					if (base.PercentSolved < 100)
					{
						num = num * (100 - (100 - base.PercentSolved) * 3) / 100;
					}
					if (num > 0)
					{
						The.Player.AwardXP(num, -1, 0, int.MaxValue, null, ContextObject);
					}
					ContextObject.ModIntProperty("ItemNamingBonus", (base.PercentSolved != 100) ? 1 : 2);
					break;
				}
				}
				SifrahGame.RecordOutcome(this, gameOutcome, Slots.Count, Tokens.Count, AsMaster, mastered);
			}
			else if (Turn > 1)
			{
				CalculateOutcomeChances(out var _, out var _, out var _, out var _, out var CriticalFailure);
				int num3 = MaxTurns - Turn + 1;
				if ((CriticalFailure - num3).in100())
				{
					repairSifrahHandler.RepairResultCriticalFailure(The.Player, ContextObject);
					RequestInterfaceExit();
					AutoAct.Interrupt();
				}
			}
		}

		public override void CalculateOutcomeChances(out int Success, out int Failure, out int PartialSuccess, out int CriticalSuccess, out int CriticalFailure)
		{
			double num = (double)(RepairRating + base.PercentSolved) * 0.01;
			double num2 = num;
			if (num2 < 1.0)
			{
				int i = 1;
				for (int num3 = Complexity.DiminishingReturns(1.0); i < num3; i++)
				{
					num2 *= num;
				}
			}
			if (num2 < 0.0)
			{
				num2 = 0.0;
			}
			double num4 = 0.01;
			if (Turn > 1)
			{
				num4 += 0.01 * (double)(MaxTurns - Turn);
			}
			if (AsMaster)
			{
				num4 *= 4.0;
			}
			double num5 = num2 * num4;
			if (num2 > 1.0)
			{
				num2 = 1.0;
			}
			double num6 = 1.0 - num2;
			double num7 = num6 * 0.5;
			double num8 = num6 * 0.1;
			num2 -= num5;
			num6 -= num7;
			num6 -= num8;
			Success = (int)(num2 * 100.0);
			Failure = (int)(num6 * 100.0);
			PartialSuccess = (int)(num7 * 100.0);
			CriticalSuccess = (int)(num5 * 100.0);
			CriticalFailure = (int)(num8 * 100.0);
			while (Success + Failure + PartialSuccess + CriticalSuccess + CriticalFailure < 100)
			{
				if (AsMaster)
				{
					CriticalSuccess++;
				}
				else
				{
					Success++;
				}
			}
		}

		public void RequestInterfaceExit()
		{
			InterfaceExitRequested = true;
		}
	}
}
