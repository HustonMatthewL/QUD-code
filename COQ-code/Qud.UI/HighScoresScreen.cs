using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.CharacterBuilds.UI;
using XRL.Core;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI
{
	[UIView("HighScoresScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "HighScoresScreen", UICanvasHost = 1)]
	public class HighScoresScreen : SingletonWindowBase<HighScoresScreen>, ControlManager.IControllerChangedEvent
	{
		public enum Modes
		{
			Achievements,
			Local,
			Daily,
			DailyFriends
		}

		public RectTransform safeArea;

		public RectTransform rightFiller;

		public Image background;

		public FrameworkScroller hotkeyBar;

		public FrameworkScroller scoresScroller;

		public FrameworkScroller leftSideScroller;

		public UITextSkin titleText;

		public EmbarkBuilderModuleBackButton backButton;

		public TaskCompletionSource<XRLGame> completionSource;

		public NavigationContext globalContext = new NavigationContext();

		public ScrollContext<NavigationContext> midHorizNav = new ScrollContext<NavigationContext>();

		public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

		public List<HighScoresDataElement> scores = new List<HighScoresDataElement>();

		public FrameworkUnityScrollChild LocalHighScoresRowPrefab;

		public FrameworkUnityScrollChild SteamHighScoresRowPrefab;

		public FrameworkUnityScrollChild AchivementRowPrefab;

		public GameObject SpacerPrefab;

		public float lastWidth;

		public float breakpointBackButtonWidth;

		public float breakpointRightFiller;

		public Modes currentMode = Modes.Local;

		private bool SelectFirst = true;

		public readonly Action WrappedHandleDelete;

		public readonly Action WrappedHandleRevisit;

		public static readonly MenuOption ACHIEVEMENTS = new MenuOption
		{
			Id = "ACH",
			InputCommand = null,
			Description = "Achievements"
		};

		public static readonly MenuOption LOCAL_SCORES = new MenuOption
		{
			Id = "LOCAL_SCORES",
			InputCommand = null,
			Description = "Ended Runs"
		};

		public static readonly MenuOption GLOBAL_DAILY = new MenuOption
		{
			Id = "GLOBAL_DAILY",
			InputCommand = null,
			Description = "Daily (steam)"
		};

		public static readonly MenuOption FRIENDS_DAILY = new MenuOption
		{
			Id = "FRIENDS_DAILY",
			InputCommand = "V Negative",
			Description = "Daily (friends)"
		};

		public static List<MenuOption> leftSideMenuOptions = new List<MenuOption> { LOCAL_SCORES, GLOBAL_DAILY, FRIENDS_DAILY, ACHIEVEMENTS };

		public static readonly MenuOption PREVIOUS_DAY = new MenuOption
		{
			Id = "PREVIOUS_DAY",
			InputCommand = "Page Left",
			Description = "Previous Day"
		};

		public static readonly MenuOption NEXT_DAY = new MenuOption
		{
			Id = "NEXT_DAY",
			InputCommand = "Page Right",
			Description = "Next Day"
		};

		public static readonly MenuOption BACK_BUTTON = KeybindsScreen.BACK_BUTTON;

		public int daysAgo;

		public string leaderboardID;

		public bool friendsOnly;

		private List<HighScoresDataElement> selection = new List<HighScoresDataElement>();

		private List<FrameworkDataElement> achSelection = new List<FrameworkDataElement>();

		private CancellationTokenSource cancelLast = new CancellationTokenSource();

		public bool wasInScroller;

		public Scoreboard2 scoreboard => Scores.Scoreboard;

		public bool breakBackButton => lastWidth <= breakpointBackButtonWidth;

		public bool breakRightFiller => lastWidth <= breakpointRightFiller;

		public bool isLocalMode => currentMode == Modes.Local;

		public HighScoresScreen()
		{
			WrappedHandleDelete = XRL.UI.Framework.Event.Helpers.Handle(HandleDelete);
			WrappedHandleRevisit = XRL.UI.Framework.Event.Helpers.Handle(HandleRevisit);
		}

		public void SetupContext()
		{
			globalContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
			globalContext.buttonHandlers.Set(InputButtonTypes.CancelButton, XRL.UI.Framework.Event.Helpers.Handle(Exit));
			globalContext.axisHandlers = new Dictionary<InputAxisTypes, Action>();
			if (currentMode == Modes.Daily || currentMode == Modes.DailyFriends)
			{
				globalContext.axisHandlers.Set(InputAxisTypes.NavigationPageXAxis, XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(NextPage, PrevPage)));
			}
			vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
			vertNav.contexts.Clear();
			vertNav.contexts.Add(midHorizNav);
			vertNav.contexts.Add(hotkeyBar.GetNavigationContext());
			vertNav.parentContext = globalContext;
			midHorizNav.SetAxis(InputAxisTypes.NavigationXAxis);
			vertNav.axisHandlers.Set(InputAxisTypes.NavigationXAxis, midHorizNav.axisHandlers[InputAxisTypes.NavigationXAxis]);
			midHorizNav.contexts.Clear();
			if (!breakBackButton)
			{
				midHorizNav.contexts.Add(backButton.navigationContext);
			}
			midHorizNav.contexts.Add(leftSideScroller.GetNavigationContext());
			midHorizNav.contexts.Add(scoresScroller.GetNavigationContext());
			vertNav.Setup();
		}

		public void NextPage()
		{
			HandleMenuOption(NEXT_DAY);
		}

		public void PrevPage()
		{
			HandleMenuOption(PREVIOUS_DAY);
		}

		public void PostSetupScore(FrameworkUnityScrollChild child, ScrollChildContext context, FrameworkDataElement element, int index)
		{
			HighScoresRow component = child.GetComponent<HighScoresRow>();
			if ((object)component != null)
			{
				NavigationContext context2 = component.deleteButton.context;
				if (context2.buttonHandlers == null)
				{
					context2.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
				}
				component.deleteButton.context.buttonHandlers.Clear();
				component.deleteButton.context.buttonHandlers.Add(InputButtonTypes.AcceptButton, WrappedHandleDelete);
				context2 = component.revisitCodaButton.context;
				if (context2.buttonHandlers == null)
				{
					context2.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
				}
				component.revisitCodaButton.context.buttonHandlers.Clear();
				component.revisitCodaButton.context.buttonHandlers.Add(InputButtonTypes.AcceptButton, WrappedHandleRevisit);
				component.context.context.commandHandlers = new Dictionary<string, Action>
				{
					{ "U Negative", WrappedHandleDelete },
					{ "CmdDelete", WrappedHandleDelete },
					{ "CmdInsert", WrappedHandleRevisit }
				};
			}
		}

		public void HandleMenuOption(FrameworkDataElement option)
		{
			if (option is MenuOption menuOption)
			{
				if (menuOption == BACK_BUTTON)
				{
					Exit();
				}
				if (menuOption == ACHIEVEMENTS && currentMode != 0)
				{
					currentMode = Modes.Achievements;
					SelectFirst = true;
					Show();
				}
				if (menuOption == LOCAL_SCORES && currentMode != Modes.Local)
				{
					currentMode = Modes.Local;
					SelectFirst = true;
					Show();
				}
				if (menuOption == GLOBAL_DAILY && currentMode != Modes.Daily)
				{
					currentMode = Modes.Daily;
					SelectFirst = true;
					leaderboardID = LeaderboardManager.GetDailyID(daysAgo = 0);
					friendsOnly = false;
					Show();
				}
				if (menuOption == FRIENDS_DAILY && currentMode != Modes.DailyFriends)
				{
					currentMode = Modes.DailyFriends;
					SelectFirst = true;
					leaderboardID = LeaderboardManager.GetDailyID(daysAgo = 0);
					friendsOnly = true;
					Show();
				}
				if (menuOption == PREVIOUS_DAY)
				{
					leaderboardID = LeaderboardManager.GetDailyID(++daysAgo);
					Show();
				}
				if (menuOption == NEXT_DAY)
				{
					daysAgo = Math.Max(0, daysAgo - 1);
					leaderboardID = LeaderboardManager.GetDailyID(daysAgo);
					Show();
				}
			}
		}

		public override void Show()
		{
			if (currentMode == Modes.Achievements)
			{
				if (scoresScroller.selectionPrefab != AchivementRowPrefab)
				{
					scoresScroller.ChangePrefabs(AchivementRowPrefab, SpacerPrefab);
				}
				selection.Clear();
				achSelection.Clear();
				Dictionary<string, AchievementInfo> achievements = AchievementManager.State.Achievements;
				if (achSelection == null)
				{
					achSelection = new List<FrameworkDataElement>(achievements.Count);
				}
				achSelection.Clear();
				int num = 0;
				foreach (AchievementInfo item in from x in achievements.Values
					orderby x.Achieved descending, x.TimeStamp descending
					select x)
				{
					if (item.Hidden && !item.Achieved)
					{
						num++;
					}
					else
					{
						achSelection.Add(new AchievementInfoData(item));
					}
				}
				if (num > 0)
				{
					achSelection.Add(new HiddenAchievementData(num));
				}
				titleText.SetText("{{W|ACHIEVEMENTS}}");
			}
			else if (currentMode == Modes.Local)
			{
				if (scoresScroller.selectionPrefab != LocalHighScoresRowPrefab)
				{
					scoresScroller.ChangePrefabs(LocalHighScoresRowPrefab, SpacerPrefab);
				}
				selection.Clear();
				selection.AddRange(scoreboard.Scores.Select((ScoreEntry2 s) => new HighScoresDataElement
				{
					entry = s
				}));
				titleText.SetText("{{W|ENDED RUNS}}");
			}
			else
			{
				if (scoresScroller.selectionPrefab != SteamHighScoresRowPrefab)
				{
					scoresScroller.ChangePrefabs(SteamHighScoresRowPrefab);
				}
				selection.Clear();
				selection.Add(new HighScoresDataElement
				{
					message = "Fetching leaderboard"
				});
				cancelLast.Cancel();
				cancelLast = new CancellationTokenSource();
				titleText.SetText("{{W|" + LeaderboardManager.GetLeaderboardName(leaderboardID).ToUpper() + (friendsOnly ? " (friends only)" : "") + "}}");
				FetchLeaderboard(cancelLast.Token);
			}
			leftSideMenuOptions.Clear();
			leftSideMenuOptions.Add(LOCAL_SCORES);
			if (LeaderboardManager.isConnected)
			{
				leftSideMenuOptions.Add(GLOBAL_DAILY);
				leftSideMenuOptions.Add(FRIENDS_DAILY);
			}
			leftSideMenuOptions.Add(ACHIEVEMENTS);
			base.Show();
			backButton?.gameObject.SetActive(value: true);
			if (backButton.navigationContext == null)
			{
				backButton.Awake();
			}
			backButton.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
			backButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, XRL.UI.Framework.Event.Helpers.Handle(Exit));
			scores.Clear();
			scores.AddRange(selection);
			scoresScroller.scrollContext.wraps = true;
			scoresScroller.PostSetup.RemoveAllListeners();
			scoresScroller.PostSetup.AddListener(PostSetupScore);
			if (SelectFirst)
			{
				SelectFirst = false;
				scoresScroller.scrollContext.selectedPosition = 0;
			}
			FrameworkScroller frameworkScroller = scoresScroller;
			IEnumerable<FrameworkDataElement> selections;
			if (currentMode != 0)
			{
				IEnumerable<FrameworkDataElement> enumerable = scores;
				selections = enumerable;
			}
			else
			{
				IEnumerable<FrameworkDataElement> enumerable = achSelection;
				selections = enumerable;
			}
			frameworkScroller.BeforeShow(null, selections);
			leftSideScroller.BeforeShow(null, leftSideMenuOptions);
			leftSideScroller.onSelected.RemoveAllListeners();
			leftSideScroller.onSelected.AddListener(HandleMenuOption);
			hotkeyBar.onSelected.RemoveAllListeners();
			hotkeyBar.onSelected.AddListener(HandleMenuOption);
			scoresScroller.onSelected.RemoveAllListeners();
			scoresScroller.onSelected.AddListener(SelectedInfo);
			SetupContext();
			EnableNavContext();
			UpdateMenuBars();
		}

		public async void FetchLeaderboard(CancellationToken ct)
		{
			_ = 1;
			try
			{
				await Task.Delay(100);
				if (ct.IsCancellationRequested)
				{
					return;
				}
				List<(int, ulong, int)> source = await LeaderboardManager.GetLeaderboardListAsync(leaderboardID, 0, 9999, friendsOnly, ct);
				if (!ct.IsCancellationRequested)
				{
					scores.Clear();
					scores.AddRange(source.Select<(int, ulong, int), HighScoresDataElement>(((int rank, ulong steamID, int score) s) => new HighScoresDataElement
					{
						rank = s.rank,
						score = s.score,
						steamID = s.steamID
					}));
					int index = Math.Max(0, scores.FindIndex((HighScoresDataElement s) => s.steamID == (ulong)SteamUser.GetSteamID()));
					if (scores.Count == 0)
					{
						scores.Add(new HighScoresDataElement
						{
							message = "Empty leaderboard"
						});
					}
					scoresScroller.BeforeShow(null, scores);
					(scoresScroller as VisibleWindowScroller).ScrollIndexIntoView(index);
					scoresScroller.scrollContext.SelectIndex(index);
				}
			}
			catch (Exception ex)
			{
				if (!ct.IsCancellationRequested)
				{
					scores.Clear();
					scores.Add(new HighScoresDataElement
					{
						message = "{{R|Error: \n" + ex.StackTrace + "}}"
					});
					scoresScroller.BeforeShow(null, scores);
				}
			}
		}

		public override void Hide()
		{
			base.Hide();
			DisableNavContext();
			base.gameObject.SetActive(value: false);
		}

		public void EnableNavContext()
		{
			globalContext.disabled = false;
			List<HighScoresDataElement> list = scores;
			if (list != null && list.Count > 0)
			{
				scoresScroller.GetNavigationContext().Activate();
			}
			else
			{
				backButton?.navigationContext?.ActivateAndEnable();
			}
		}

		public void DisableNavContext(bool deactivate = true)
		{
			if (deactivate)
			{
				NavigationContext activeContext = NavigationController.instance.activeContext;
				if (activeContext != null && activeContext.IsInside(globalContext))
				{
					NavigationController.instance.activeContext = null;
				}
			}
			globalContext.disabled = true;
		}

		public async void SelectedInfo(FrameworkDataElement data)
		{
			if (data is HighScoresDataElement highScoresDataElement && currentMode == Modes.Local)
			{
				await Popup.ShowAsync(highScoresDataElement.entry.Details);
			}
		}

		public async Task<XRLGame> ShowScreen()
		{
			SelectFirst = true;
			completionSource?.TrySetCanceled();
			completionSource = new TaskCompletionSource<XRLGame>();
			await The.UiContext;
			ControlManager.ResetInput();
			Show();
			XRLGame result = await completionSource.Task;
			DisableNavContext();
			Hide();
			return result;
		}

		public void Exit()
		{
			MetricsManager.LogEditorInfo("Exiting high scores screen");
			completionSource?.TrySetResult(null);
			ControlManager.ResetInput();
		}

		public void HandleRevisit()
		{
			if (NavigationController.instance.activeContext.IsInside(scoresScroller.GetNavigationContext()))
			{
				HighScoresDataElement highScoresDataElement = scores[scoresScroller.selectedPosition];
				try
				{
					XRLGame result = highScoresDataElement?.entry?.LoadCoda();
					completionSource.TrySetResult(result);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Deleting coda save", x);
				}
			}
		}

		public async void HandleDelete()
		{
			if (!NavigationController.instance.activeContext.IsInside(scoresScroller.GetNavigationContext()))
			{
				return;
			}
			HighScoresDataElement score = scores[scoresScroller.selectedPosition];
			if (await Popup.ShowYesNoAsync("Are you sure you want to delete this?\n\n" + score.entry.Details.Split(new char[1] { '\n' }, 2)[0]) == DialogResult.Yes)
			{
				try
				{
					score?.entry?.DeleteCoda();
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Deleting coda save", x);
				}
				scoreboard.Scores.Remove(score.entry);
				scoreboard.Save();
				Show();
			}
		}

		public void UpdateMenuBars()
		{
			List<MenuOption> list = new List<MenuOption>();
			if (breakBackButton)
			{
				list.Add(BACK_BUTTON);
			}
			list.Add(new MenuOption
			{
				InputCommand = "NavigationXYAxis",
				Description = "navigate",
				disabled = true
			});
			if (currentMode == Modes.Local)
			{
				list.Add(new MenuOption
				{
					KeyDescription = ControlManager.getCommandInputDescription("Accept"),
					Description = "select",
					disabled = true
				});
			}
			if (currentMode == Modes.Daily || currentMode == Modes.DailyFriends)
			{
				list.Add(PREVIOUS_DAY);
				if (daysAgo > 0)
				{
					list.Add(NEXT_DAY);
				}
			}
			hotkeyBar.GetNavigationContext().disabled = false;
			hotkeyBar.BeforeShow(null, list);
		}

		public void Update()
		{
			if (globalContext.IsActive())
			{
				bool num = NavigationController.instance.activeContext?.IsInside(scoresScroller.GetNavigationContext()) ?? false;
				float width = base.rectTransform.rect.width;
				if (num != wasInScroller || lastWidth != width)
				{
					lastWidth = width;
					backButton.gameObject.SetActive(!breakBackButton);
					rightFiller.gameObject.SetActive(!breakRightFiller);
					safeArea.offsetMin = new Vector2(breakBackButton ? 10 : 150, safeArea.offsetMin.y);
					safeArea.offsetMax = new Vector2(breakBackButton ? (-10) : (-150), safeArea.offsetMax.y);
					UpdateMenuBars();
				}
			}
		}

		public void ControllerChanged()
		{
			UpdateMenuBars();
		}
	}
}
