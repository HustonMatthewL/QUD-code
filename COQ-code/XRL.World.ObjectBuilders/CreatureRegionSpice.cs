using System;
using System.Collections.Generic;
using HistoryKit;
using SimpleJSON;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ObjectBuilders
{
	public class CreatureRegionSpice : IObjectBuilder
	{
		public string TileColor;

		public string DetailColor;

		public override void Initialize()
		{
			TileColor = null;
			DetailColor = null;
		}

		public override void Apply(GameObject Object, string Context)
		{
			if (Object.Render == null)
			{
				return;
			}
			string text = ZoneManager.ZoneGenerationContext?.GetTerrainRegion();
			if (text.IsNullOrEmpty())
			{
				return;
			}
			Render render = Object.Render;
			JSONNode jSONNode = HistoricSpice.root["history"]["regions"]["terrain"][text];
			JSONArray jSONArray = (JSONArray)jSONNode["creatureRegionAdjective"];
			JSONArray jSONArray2 = (JSONArray)jSONNode["creatureRegionNoun"];
			JSONArray jSONArray3 = (JSONArray)jSONNode["creatureAlteredLocale"];
			JSONArray jSONArray4 = (JSONArray)jSONNode["creatureAlteredCast"];
			Random seededRandomGenerator = Stat.GetSeededRandomGenerator(Object.Blueprint);
			string text2 = jSONArray[seededRandomGenerator.Next(jSONArray.Count)];
			string text3 = jSONArray2[seededRandomGenerator.Next(jSONArray2.Count)];
			render.DisplayName = render.DisplayName.StartReplace().AddObject(Object).AddReplacer("creatureRegionAdjective", text2)
				.AddReplacer("creatureRegionNoun", text3)
				.ToString();
			if (Object.TryGetPart<Description>(out var Part))
			{
				string text4 = jSONArray3[seededRandomGenerator.Next(jSONArray3.Count)];
				string text5 = jSONArray4[seededRandomGenerator.Next(jSONArray4.Count)];
				string[] value = Object.GetxTag("TextFragments", "PoeticFeatures").Split(',');
				Part._Short = Part._Short.Replace("=creatureRegionAdjective=", text2).Replace("=creatureRegionNoun=", text3);
				Description description = Part;
				description._Short = description._Short + " Time in " + text4 + " has altered =pronouns.possessive= features -- " + string.Join(", ", value) + " -- and given them " + text5 + ".";
			}
			if (!TileColor.IsNullOrEmpty())
			{
				string text6 = TileColor.CachedCommaExpansion().GetRandomElement(seededRandomGenerator);
				if (text6.Length == 1)
				{
					text6 = "&" + text6;
				}
				render.ColorString = text6;
				if (!render.TileColor.IsNullOrEmpty())
				{
					render.TileColor = render.ColorString;
				}
			}
			else
			{
				JSONArray jSONArray5 = (JSONArray)jSONNode["baseColor"];
				render.ColorString = "&" + jSONArray5[seededRandomGenerator.Next(jSONArray5.Count)];
				if (!render.TileColor.IsNullOrEmpty())
				{
					render.TileColor = render.ColorString;
				}
			}
			if (!DetailColor.IsNullOrEmpty())
			{
				List<string> list = DetailColor.CachedCommaExpansion();
				char tileForegroundColorChar = Object.Render.GetTileForegroundColorChar();
				int num = 0;
				do
				{
					render.DetailColor = list.GetRandomElement(seededRandomGenerator);
				}
				while (tileForegroundColorChar == render.getDetailColor() && num++ < list.Count);
			}
		}
	}
}
