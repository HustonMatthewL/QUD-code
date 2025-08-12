using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace XRL.UI
{
	public class GameOption
	{
		public class RequiresSpec
		{
			private string original;

			private Func<bool> test;

			private static readonly Regex partParse = new Regex("^\\s*(?<Option>.*?)\\s*(?<Test>[!=]=)\\s*(?<Value>.*?)\\s*$", RegexOptions.Compiled);

			public bool RequirementsMet => test();

			public override string ToString()
			{
				return original;
			}

			[XmlDataHelper.AttributeParser(typeof(RequiresSpec))]
			public static RequiresSpec ParseString(string input)
			{
				if (string.IsNullOrWhiteSpace(input))
				{
					return null;
				}
				Exception e = null;
				(string, string, bool)[] parsed = input.Split(',').Select(delegate(string part)
				{
					Match match = partParse.Match(part);
					if (!match.Success)
					{
						e = new Exception("\"" + part + "\" could not be parsed as a option == value or option != value", e);
						return ((string, string, bool))(null, null, true);
					}
					return (match.Groups["Option"].Value, match.Groups["Value"].Value, match.Groups["Test"].Value == "==");
				}).ToArray();
				if (e != null)
				{
					throw e;
				}
				return new RequiresSpec
				{
					original = input,
					test = delegate
					{
						(string, string, bool)[] array = parsed;
						for (int i = 0; i < array.Length; i++)
						{
							var (iD, text, flag) = array[i];
							if (Options.GetOption(iD) == text != flag)
							{
								return false;
							}
						}
						return true;
					}
				};
			}
		}

		public string ID;

		public string DisplayText;

		public string Category;

		public string Type;

		public string Default;

		public string SearchKeywords;

		public string HelpText;

		public MethodInfo OnClick;

		public RequiresSpec Requires;

		public List<string> Values;

		public int Min;

		public int Max;

		public int Increment;
	}
}
