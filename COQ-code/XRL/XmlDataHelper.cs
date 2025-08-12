using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using ConsoleLib.Console;
using UnityEngine;

namespace XRL
{
	[HasModSensitiveStaticCache]
	public class XmlDataHelper : XmlTextReader
	{
		public class AttributeParser
		{
			public delegate object ParseDelegate(string value);

			public ParseDelegate _Parse;

			public ParseDelegate Parse
			{
				get
				{
					return _Parse;
				}
				set
				{
					_Parse = value;
				}
			}

			public AttributeParser()
			{
			}

			public AttributeParser(AttributeParser parser)
			{
				_Parse = parser._Parse;
			}

			public object Invoke(string s)
			{
				return _Parse(s);
			}

			public AttributeParser<T> AsGeneric<T>()
			{
				if (this is AttributeParser<T> result)
				{
					return result;
				}
				return new AttributeParser<T>(this);
			}
		}

		public class AttributeParser<AttributeType> : AttributeParser
		{
			public new delegate AttributeType ParseDelegate(string value);

			public new ParseDelegate Parse
			{
				set
				{
					_Parse = (string str) => value(str);
				}
			}

			public AttributeParser()
			{
			}

			public AttributeParser(AttributeParser parser)
				: base(parser)
			{
			}

			public new AttributeType Invoke(string s)
			{
				return (AttributeType)_Parse(s);
			}
		}

		[AttributeUsage(AttributeTargets.Method)]
		public class AttributeParserAttribute : Attribute
		{
			public readonly Type type;

			public AttributeParserAttribute(Type T)
			{
				type = T;
			}
		}

		public readonly ModInfo modInfo;

		public bool sanityChecks = true;

		public static readonly Regex TrimSpacePerLine = new Regex("^[ \\t\\r]+|[ \\t\\r]+$", RegexOptions.Multiline);

		public static readonly Regex TrimNewlineRegex = new Regex("^\\n+|\\n+$");

		private bool hasReadFirstElement;

		protected HashSet<string> attributeChecked = new HashSet<string>();

		private static readonly Dictionary<string, Action<XmlDataHelper>> noNodesExpected = new Dictionary<string, Action<XmlDataHelper>>();

		[ModSensitiveStaticCache(true)]
		private static Dictionary<Type, AttributeParser> AttributeParsers = new Dictionary<Type, AttributeParser>();

		public XmlDataHelper(Stream input, ModInfo modInfo = null)
			: base(input)
		{
			this.modInfo = modInfo;
			base.WhitespaceHandling = WhitespaceHandling.None;
		}

		public XmlDataHelper(string uri, ModInfo modInfo = null)
			: base(uri)
		{
			this.modInfo = modInfo;
			base.WhitespaceHandling = WhitespaceHandling.None;
		}

		public virtual bool IsMod()
		{
			return modInfo != null;
		}

		public void HandleException(Exception e)
		{
			if (modInfo != null)
			{
				modInfo.Error(e);
			}
			else
			{
				MetricsManager.LogException(GetSourcePoint(), e, "XML_Parse");
			}
		}

		public string GetSourcePoint()
		{
			return GetType().Name + ":: " + BaseURI + " line " + base.LineNumber + " char " + base.LinePosition;
		}

		public void ParseWarning(object msg)
		{
			string msg2 = GetSourcePoint() + "\n" + msg;
			if (modInfo != null)
			{
				modInfo.Warn(msg2);
			}
			else
			{
				MetricsManager.LogException(GetSourcePoint(), new Exception(msg.ToString()), "XML_Parse");
			}
		}

		public override bool Read()
		{
			attributeChecked.Clear();
			bool result = base.Read();
			if (!hasReadFirstElement && NodeType == XmlNodeType.Element)
			{
				hasReadFirstElement = true;
				attributeChecked.Add("LoadPriority");
			}
			return result;
		}

		public static void Parse(string path, Dictionary<string, Action<XmlDataHelper>> handlers, bool includeMods = false)
		{
			List<(string, ModInfo)> Paths = new List<(string, ModInfo)>();
			Paths.Add((DataManager.FilePath(path), null));
			if (includeMods)
			{
				ModManager.ForEachFile(path, delegate(string modPath, ModInfo modInfo)
				{
					Paths.Add((modPath, modInfo));
				});
			}
			foreach (var (fileName, modInfo2) in Paths)
			{
				using XmlDataHelper xmlDataHelper = DataManager.GetXMLStream(fileName, modInfo2);
				xmlDataHelper.HandleNodes(handlers);
				xmlDataHelper.Close();
			}
		}

		public void AssertExtraAttributes()
		{
			if (!sanityChecks || AttributeCount == 0)
			{
				return;
			}
			for (int i = 0; i < AttributeCount; i++)
			{
				MoveToAttribute(i);
				if (!attributeChecked.Contains(Name))
				{
					ParseWarning($"Unused attribute \"{Name}\" detected.");
				}
			}
			MoveToElement();
		}

		public override string GetAttribute(string name)
		{
			attributeChecked.Add(name);
			return base.GetAttribute(name);
		}

		public virtual Type ParseType(string attribute, string defaultNameSpace, bool required = false)
		{
			return ParseAttribute(attribute, null, required, (string value) => ModManager.ResolveType(defaultNameSpace, value) ?? throw new Exception("Error finding " + defaultNameSpace + "." + value + " or " + value + " Type."));
		}

		public virtual ResultType ParseAttribute<ResultType>(string attribute, ResultType defaultValue, bool required = false, AttributeParser<ResultType>.ParseDelegate parse = null)
		{
			if (parse == null)
			{
				AttributeParser<ResultType> attributeParser = TryGetAttributeParser<ResultType>();
				if (attributeParser != null)
				{
					parse = attributeParser.Invoke;
				}
				if (parse == null)
				{
					throw new Exception("No default parser for type " + typeof(ResultType).FullName);
				}
			}
			string attribute2 = GetAttribute(attribute);
			if (attribute2 == null)
			{
				if (required)
				{
					ParseWarning("Required attribute \"" + attribute + "\" missing");
				}
				return defaultValue;
			}
			try
			{
				return parse(attribute2);
			}
			catch (Exception innerException)
			{
				HandleException(new Exception("Error parsing attribute " + attribute + "=\"" + attribute2 + "\":", innerException));
				return defaultValue;
			}
		}

		public virtual bool HasAttribute(string name)
		{
			return GetAttribute(name) != null;
		}

		public virtual string GetAttributeString(string name, string defaultValue, bool required = false)
		{
			return ParseAttribute(name, defaultValue, required);
		}

		public virtual int GetAttributeInt(string name, int defaultValue, bool required = false)
		{
			return ParseAttribute(name, defaultValue, required);
		}

		public virtual bool GetAttributeBool(string name, bool defaultValue, bool required = false)
		{
			return ParseAttribute(name, defaultValue, required);
		}

		public void DoneWithElement()
		{
			HandleNodes(noNodesExpected);
		}

		public string GetTextNode()
		{
			AssertExtraAttributes();
			string name = Name;
			Read();
			string result = null;
			if (NodeType == XmlNodeType.Text)
			{
				result = TrimSpacePerLine.Replace(Value, "");
				result = TrimNewlineRegex.Replace(result, "");
			}
			else
			{
				ParseWarning("Unexpected node type: " + NodeType.ToString() + " expecting Text.");
			}
			Read();
			if (NodeType != XmlNodeType.EndElement || Name != name)
			{
				ParseWarning("Expected closing tag for " + name + ".");
			}
			return result;
		}

		public bool TryParseTextNode<T>(out T Value, bool FailOnEmpty = true, bool FailOnWhiteSpace = true)
		{
			string textNode = GetTextNode();
			if (textNode == null || (FailOnEmpty && textNode == "") || (FailOnWhiteSpace && string.IsNullOrWhiteSpace(textNode)))
			{
				Value = default(T);
				return false;
			}
			return TryParse<T>(textNode, out Value);
		}

		public bool TryParse<T>(string Text, out T Value)
		{
			try
			{
				AttributeParser attributeParser = TryGetAttributeParser(typeof(T));
				if (attributeParser != null)
				{
					Value = (T)attributeParser.Invoke(Text);
					return true;
				}
			}
			catch (Exception innerException)
			{
				HandleException(new Exception("Error parsing text \"" + Text + "\":", innerException));
			}
			Value = default(T);
			return false;
		}

		public void HandleNodes(IDictionary<string, Action<XmlDataHelper>> nodeHandlers)
		{
			AssertExtraAttributes();
			string name = Name;
			if (IsEmptyElement || NodeType == XmlNodeType.EndElement)
			{
				return;
			}
			Action<XmlDataHelper> value = null;
			while (Read())
			{
				switch (NodeType)
				{
				case XmlNodeType.EndElement:
					if (Name == name)
					{
						return;
					}
					ParseWarning($"Unexpected EndElement for \"{Name}\"");
					break;
				case XmlNodeType.Text:
					if (nodeHandlers.TryGetValue("text", out value))
					{
						try
						{
							value(this);
						}
						catch (Exception ex2)
						{
							HandleException(ex2);
							throw ex2;
						}
					}
					else
					{
						ParseWarning("Unexpected text node");
					}
					break;
				case XmlNodeType.Element:
					if (nodeHandlers.TryGetValue(Name, out value))
					{
						try
						{
							value(this);
						}
						catch (Exception ex)
						{
							HandleException(ex);
							throw ex;
						}
					}
					else
					{
						ParseWarning($"Unexpected \"{Name}\" node");
					}
					break;
				default:
					ParseWarning("Unexpected node type: " + NodeType);
					break;
				case XmlNodeType.Comment:
				case XmlNodeType.Whitespace:
				case XmlNodeType.SignificantWhitespace:
				case XmlNodeType.XmlDeclaration:
					break;
				}
			}
		}

		public static void AddAttributeParser(Type T, AttributeParser parser)
		{
			AttributeParsers.Set(T, parser);
		}

		public static void AddDefaultParsers()
		{
			AddAttributeParser(typeof(bool), new AttributeParser<bool>
			{
				Parse = Convert.ToBoolean
			});
			AddAttributeParser(typeof(bool?), new AttributeParser<bool?>
			{
				Parse = (string s) => (s != null) ? new bool?(Convert.ToBoolean(s)) : ((bool?)null)
			});
			AddAttributeParser(typeof(double), new AttributeParser<double>
			{
				Parse = Convert.ToDouble
			});
			AddAttributeParser(typeof(short), new AttributeParser<short>
			{
				Parse = short.Parse
			});
			AddAttributeParser(typeof(int), new AttributeParser<int>
			{
				Parse = int.Parse
			});
			AddAttributeParser(typeof(long), new AttributeParser<long>
			{
				Parse = long.Parse
			});
			AddAttributeParser(typeof(float), new AttributeParser<float>
			{
				Parse = Convert.ToSingle
			});
			AddAttributeParser(typeof(char), new AttributeParser<char>
			{
				Parse = (string s) => s?[0] ?? '\0'
			});
			AddAttributeParser(typeof(string), new AttributeParser<string>
			{
				Parse = (string s) => string.Intern(s.Replace("\r", ""))
			});
			AddAttributeParser(typeof(object), new AttributeParser<object>
			{
				Parse = (string s) => s
			});
			AddAttributeParser(typeof(List<string>), new AttributeParser<List<string>>
			{
				Parse = (string s) => (s != null) ? new List<string>(s.Split(',')) : null
			});
			AddAttributeParser(typeof(Color), new AttributeParser<Color>
			{
				Parse = delegate(string s)
				{
					Exception innerException = null;
					try
					{
						if (s[0] == '#')
						{
							return ConsoleLib.Console.ColorUtility.FromWebColor(s.Substring(1));
						}
						if (s.Contains(','))
						{
							string[] array = s.Split(',');
							return new Color(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
						}
						if (s.Length == 6)
						{
							return ConsoleLib.Console.ColorUtility.FromWebColor(s);
						}
					}
					catch (Exception ex)
					{
						innerException = ex;
					}
					throw new Exception("Could not figure out color format.  Supported formats are #rrggbb hex style; rrr,ggg,bbb from 0 -> 1", innerException);
				}
			});
			AddAttributeParser(typeof(MethodInfo), new AttributeParser<MethodInfo>
			{
				Parse = delegate(string value)
				{
					int num = value.LastIndexOf('.');
					if (num == -1)
					{
						throw new ArgumentException("No path for type and namespace was specified for method '" + value + "'");
					}
					Type type = ModManager.ResolveType(value.Substring(0, num), IgnoreCase: false, ThrowOnError: true);
					MethodInfo method = type.GetMethod(value.Substring(num + 1));
					if (method == null)
					{
						throw new KeyNotFoundException("No method by name '" + value + "' was found for type '" + type.FullName + "'");
					}
					return method;
				}
			});
		}

		public static AttributeParser<T> TryGetAttributeParser<T>()
		{
			return TryGetAttributeParser(typeof(T))?.AsGeneric<T>();
		}

		public static AttributeParser TryGetAttributeParser(Type type)
		{
			if (type == null)
			{
				return null;
			}
			if (AttributeParsers.Count == 0)
			{
				AddDefaultParsers();
			}
			if (AttributeParsers.TryGetValue(type, out var value))
			{
				return value;
			}
			if (type.IsEnum)
			{
				AttributeParser attributeParser = new AttributeParser
				{
					Parse = (string propertyValue) => Enum.Parse(type, propertyValue)
				};
				AddAttributeParser(type, attributeParser);
				return attributeParser;
			}
			MethodInfo reflectedParser = type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault((MethodInfo methodInfo) => methodInfo.CustomAttributes.Any((CustomAttributeData s) => s.AttributeType.IsAssignableFrom(typeof(AttributeParserAttribute))) && methodInfo.ReturnType == type);
			if (reflectedParser != null)
			{
				object[] @params = new object[1];
				AttributeParser attributeParser2 = new AttributeParser
				{
					Parse = delegate(string s)
					{
						@params[0] = s;
						return reflectedParser.Invoke(null, @params);
					}
				};
				AddAttributeParser(type, attributeParser2);
				return attributeParser2;
			}
			foreach (KeyValuePair<Type, AttributeParser> attributeParser3 in AttributeParsers)
			{
				if (type.IsAssignableFrom(attributeParser3.Key))
				{
					MetricsManager.LogWarning("Making a parser for " + type.FullName + " from " + attributeParser3.Key.FullName);
					AddAttributeParser(type, attributeParser3.Value);
					return attributeParser3.Value;
				}
			}
			AddAttributeParser(type, null);
			return null;
		}
	}
}
