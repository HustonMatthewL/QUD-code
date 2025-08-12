using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL
{
	[HasModSensitiveStaticCache]
	public static class CompatManager
	{
		public static bool Loaded;

		[ModSensitiveStaticCache(false, CreateEmptyInstance = true)]
		public static Dictionary<string, string> Skills = new Dictionary<string, string>();

		[ModSensitiveStaticCache(false, CreateEmptyInstance = true)]
		public static Dictionary<string, string> Mutations = new Dictionary<string, string>();

		[ModSensitiveStaticCache(false, CreateEmptyInstance = true)]
		public static Dictionary<string, string> Factions = new Dictionary<string, string>();

		private static readonly Dictionary<string, Action<XmlDataHelper>> _outerNodes = new Dictionary<string, Action<XmlDataHelper>> { { "compat", HandleInnerNode } };

		private static readonly Dictionary<string, Action<XmlDataHelper>> _innerNodes = new Dictionary<string, Action<XmlDataHelper>>
		{
			{ "skills", HandleInnerNode },
			{ "skill", HandleSkillNode },
			{ "mutations", HandleInnerNode },
			{ "mutation", HandleMutationNode },
			{ "factions", HandleInnerNode },
			{ "faction", HandleFactionNode }
		};

		public static void CheckInit()
		{
			if (!Loaded)
			{
				Init();
			}
		}

		[ModSensitiveCacheInit]
		private static void Init()
		{
			Loaded = false;
			Skills.Clear();
			Loading.LoadTask("Loading Compat.xml", delegate
			{
				Loaded = true;
				foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("compat"))
				{
					item.HandleNodes(_outerNodes);
				}
			});
		}

		public static void HandleInnerNode(XmlDataHelper xml)
		{
			xml.HandleNodes(_innerNodes);
		}

		public static void HandleSkillNode(XmlDataHelper Reader)
		{
			string text = Reader.ParseAttribute<string>("Old", null, required: true);
			if (text.IsNullOrEmpty())
			{
				throw new Exception(Reader.Name + " tag had missing or empty Old attribute");
			}
			string value = Reader.ParseAttribute<string>("New", null, required: true);
			if (value.IsNullOrEmpty())
			{
				throw new Exception(Reader.Name + " tag had missing or empty New attribute");
			}
			Skills[text] = value;
			Reader.DoneWithElement();
		}

		public static void HandleMutationNode(XmlDataHelper Reader)
		{
			string text = Reader.ParseAttribute<string>("Old", null, required: true);
			if (text.IsNullOrEmpty())
			{
				throw new Exception(Reader.Name + " tag had missing or empty Old attribute");
			}
			string value = Reader.ParseAttribute<string>("New", null, required: true);
			if (value.IsNullOrEmpty())
			{
				throw new Exception(Reader.Name + " tag had missing or empty New attribute");
			}
			Mutations[text] = value;
			Reader.DoneWithElement();
		}

		public static void HandleFactionNode(XmlDataHelper Reader)
		{
			string text = Reader.ParseAttribute<string>("Old", null, required: true);
			if (text.IsNullOrEmpty())
			{
				throw new Exception(Reader.Name + " tag had missing or empty Old attribute");
			}
			string value = Reader.ParseAttribute<string>("New", null, required: true);
			if (value.IsNullOrEmpty())
			{
				throw new Exception(Reader.Name + " tag had missing or empty New attribute");
			}
			Factions[text] = value;
			Reader.DoneWithElement();
		}

		public static void ProcessPart(ref string Part)
		{
			ProcessSkill(ref Part);
			ProcessMutation(ref Part);
		}

		public static string ProcessPart(string Part)
		{
			ProcessPart(ref Part);
			return Part;
		}

		public static string GetNewPart(string Part)
		{
			string Part2 = Part;
			ProcessPart(ref Part2);
			if (Part2 == Part)
			{
				return null;
			}
			return Part2;
		}

		public static bool TryGetPart(string Part, out string NewPart, out string Type)
		{
			CheckInit();
			if (Skills.TryGetValue(Part, out NewPart))
			{
				Type = "Skill";
				return true;
			}
			if (Mutations.TryGetValue(Part, out NewPart))
			{
				Type = "Mutation";
				return true;
			}
			Type = null;
			return false;
		}

		public static string GetNewSkill(string Skill)
		{
			CheckInit();
			if (Skills.TryGetValue(Skill, out var value))
			{
				return value;
			}
			return null;
		}

		public static void ProcessSkill(ref string Skill)
		{
			CheckInit();
			if (Skills.TryGetValue(Skill, out var value))
			{
				Skill = value;
			}
		}

		public static string ProcessSkill(string Skill)
		{
			CheckInit();
			if (Skills.TryGetValue(Skill, out var value))
			{
				Skill = value;
			}
			return Skill;
		}

		public static string GetNewMutation(string Mutation)
		{
			CheckInit();
			if (Mutations.TryGetValue(Mutation, out var value))
			{
				return value;
			}
			return null;
		}

		public static void ProcessMutation(ref string Mutation)
		{
			CheckInit();
			if (Mutations.TryGetValue(Mutation, out var value))
			{
				Mutation = value;
			}
		}

		public static string ProcessMutation(string Mutation)
		{
			CheckInit();
			if (Mutations.TryGetValue(Mutation, out var value))
			{
				Mutation = value;
			}
			return Mutation;
		}

		public static string GetNewFaction(string Faction)
		{
			CheckInit();
			if (Factions.TryGetValue(Faction, out var value))
			{
				return value;
			}
			return null;
		}

		public static void ProcessFaction(ref string Faction)
		{
			CheckInit();
			if (Factions.TryGetValue(Faction, out var value))
			{
				Faction = value;
			}
		}

		public static string ProcessFaction(string Faction)
		{
			CheckInit();
			if (Factions.TryGetValue(Faction, out var value))
			{
				Faction = value;
			}
			return Faction;
		}
	}
}
