using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using XRL.Core;
using XRL.UI;

namespace XRL.CharacterBuilds.Qud
{
	public class QudChartypeModule : QudEmbarkBuilderModule<QudChartypeModuleData>
	{
		public class GameTypeDescriptor
		{
			public string ID;

			public string Title;

			public string IconTile;

			public string IconForeground;

			public string IconDetail;

			public string Description;
		}

		public Dictionary<string, GameTypeDescriptor> GameTypes = new Dictionary<string, GameTypeDescriptor>();

		protected GameTypeDescriptor CurrentReadingGameTypeDescriptor;

		public override Dictionary<string, Action<XmlDataHelper>> XmlNodes
		{
			get
			{
				Dictionary<string, Action<XmlDataHelper>> xmlNodes = base.XmlNodes;
				xmlNodes.Add("types", HandleTypesNode);
				return xmlNodes;
			}
		}

		public Dictionary<string, Action<XmlDataHelper>> XmlTypesNodes => new Dictionary<string, Action<XmlDataHelper>>
		{
			{ "type", HandleTypeNode },
			{ "icon", HandleTypeIconNode },
			{ "description", HandleTypeDescriptionNode }
		};

		public override bool IncludeInBuildCodes()
		{
			return base.data?.type == "Pregen";
		}

		public override bool shouldBeEnabled()
		{
			return builder.GetModule<QudGamemodeModule>().IsDataValid();
		}

		public override bool shouldBeEditable()
		{
			if (builder.IsEditableGameMode())
			{
				return builder?.GetModule<QudGamemodeModule>()?.data?.Mode != "Tutorial";
			}
			return false;
		}

		public void selectType(string type)
		{
			setData(new QudChartypeModuleData(type));
			builder.ResetForwardViews();
			switch (type)
			{
			case "Last":
				try
				{
					string code = File.ReadAllText(DataManager.SavePath("lastcharacter.txt"));
					builder.InitModulesFromCode(code);
					builder.advanceToViewId("Chargen/BuildSummary");
					break;
				}
				catch (Exception x)
				{
					MetricsManager.LogError("Error loading lastcharacter.txt", x);
					Popup.ShowAsync("There is no valid last character to use.");
					break;
				}
			case "Random":
			{
				EmbarkBuilderModuleWindowDescriptor activeWindow = builder.activeWindow;
				do
				{
					builder.advance();
					if (builder.activeWindow != activeWindow)
					{
						activeWindow = builder.activeWindow;
						activeWindow.window.RandomSelection();
						continue;
					}
					break;
				}
				while (builder?.activeWindow?.viewID != "Chargen/BuildSummary");
				break;
			}
			default:
				builder.advance();
				break;
			}
		}

		public string getSelectedType()
		{
			return base.data?.type;
		}

		public void HandleTypesNode(XmlDataHelper xml)
		{
			xml.HandleNodes(XmlTypesNodes);
		}

		public override void bootGame(XRLGame game, EmbarkInfo info)
		{
			if (getSelectedType() == "Daily")
			{
				int dayOfYear = new CultureInfo("en-US").Calendar.GetDayOfYear(DateTime.Now);
				XRLCore.Core.Game.SetStringGameState("leaderboardMode", "daily:" + DateTime.Now.Year + ":" + dayOfYear);
				Debug.Log(">>> " + XRLCore.Core.Game.GetStringGameState("leaderboardMode"));
			}
		}

		protected void HandleTypeNode(XmlDataHelper xml)
		{
			string attribute = xml.GetAttribute("ID");
			if (!GameTypes.TryGetValue(attribute, out CurrentReadingGameTypeDescriptor))
			{
				CurrentReadingGameTypeDescriptor = new GameTypeDescriptor
				{
					ID = attribute
				};
				GameTypes.Add(attribute, CurrentReadingGameTypeDescriptor);
			}
			CurrentReadingGameTypeDescriptor.Title = xml.GetAttribute("Title");
			xml.HandleNodes(XmlTypesNodes);
			CurrentReadingGameTypeDescriptor = null;
		}

		protected void HandleTypeIconNode(XmlDataHelper xml)
		{
			CurrentReadingGameTypeDescriptor.IconTile = xml.GetAttribute("Tile");
			CurrentReadingGameTypeDescriptor.IconDetail = xml.GetAttributeString("Detail", "W");
			CurrentReadingGameTypeDescriptor.IconForeground = xml.GetAttributeString("Foreground", "y");
			xml.DoneWithElement();
		}

		protected void HandleTypeDescriptionNode(XmlDataHelper xml)
		{
			CurrentReadingGameTypeDescriptor.Description = xml.GetTextNode();
		}

		public override void InitFromSeed(string seed)
		{
			throw new NotImplementedException();
		}
	}
}
