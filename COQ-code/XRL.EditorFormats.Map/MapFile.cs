using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace XRL.EditorFormats.Map
{
	public class MapFile
	{
		public MapFileRegion Cells;

		public int width
		{
			get
			{
				return Cells.width;
			}
			set
			{
				MapFileRegion cells = new MapFileRegion(value, height);
				cells.SetRegion(Cells);
				Cells = cells;
			}
		}

		public int height
		{
			get
			{
				return Cells.height;
			}
			set
			{
				MapFileRegion cells = new MapFileRegion(width, value);
				cells.SetRegion(Cells);
				Cells = cells;
			}
		}

		public int RightmostObject()
		{
			return Cells.MaxX(requireObjects: true);
		}

		public int BottommostObject()
		{
			return Cells.MaxY(requireObjects: true);
		}

		public static MapFile LoadWithMods(string filename)
		{
			MapFile mapFile = LoadStreaming(filename);
			if (mapFile == null)
			{
				mapFile = new MapFile(0, 0);
			}
			ModManager.ForEachFile(filename, mapFile.LoadMod);
			mapFile.Cells.FillEmptyCells();
			return mapFile;
		}

		public MapFile(int width = 80, int height = 25)
		{
			Cells = new MapFileRegion(width, height);
		}

		public HashSet<string> UsedBlueprints(HashSet<string> result = null)
		{
			if (result == null)
			{
				result = new HashSet<string>();
			}
			foreach (MapFileObjectReference item in Cells.AllObjects())
			{
				result.Add(item.blueprint.Name);
			}
			return result;
		}

		public void FlipHorizontal()
		{
			Cells = Cells.FlippedHorizontal();
		}

		public void FlipVertical()
		{
			Cells = Cells.FlippedVertical();
		}

		public void NewMap(int width = 80, int height = 25)
		{
			Cells = new MapFileRegion(width, height);
		}

		public MapFile Load(XmlTextReader Reader)
		{
			Reader.WhitespaceHandling = WhitespaceHandling.None;
			while (Reader.Read())
			{
				if (Reader.Name == "Map")
				{
					LoadMapNode(Reader);
				}
			}
			Reader.Close();
			return this;
		}

		public static MapFile LoadStreaming(string filename)
		{
			if (File.Exists(DataManager.FilePath(filename)))
			{
				return new MapFile(0, 0).Load(DataManager.GetStreamingAssetsXMLStream(filename));
			}
			return null;
		}

		public static MapFile LoadFile(string filename)
		{
			return new MapFile(0, 0).Load(new XmlTextReader(filename));
		}

		public void LoadMod(string FileName)
		{
			Load(new XmlTextReader(FileName));
		}

		private void LoadMapNode(XmlTextReader Reader, bool Mod = false)
		{
			bool merge = false;
			string attribute = Reader.GetAttribute("Load");
			if (attribute != null && attribute.EqualsNoCase("merge"))
			{
				merge = true;
			}
			attribute = Reader.GetAttribute("width");
			if (attribute != null)
			{
				width = Convert.ToInt32(attribute);
			}
			attribute = Reader.GetAttribute("height");
			if (attribute != null)
			{
				height = Convert.ToInt32(attribute);
			}
			while (Reader.Read())
			{
				if (Reader.Name == "cell")
				{
					LoadCellNode(Reader, merge);
				}
			}
		}

		private void LoadCellNode(XmlTextReader Reader, bool Merge = false)
		{
			int num = Convert.ToInt32(Reader.GetAttribute("X"));
			int num2 = Convert.ToInt32(Reader.GetAttribute("Y"));
			if (num + 1 > width)
			{
				width = num + 1;
			}
			if (num2 + 1 > height)
			{
				height = num2 + 1;
			}
			MapFileCell orCreateCellAt = Cells.GetOrCreateCellAt(num, num2);
			if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
			{
				return;
			}
			if (!Merge)
			{
				orCreateCellAt.Objects.Clear();
			}
			orCreateCellAt.Clear = Reader.GetAttribute("Clear").EqualsNoCase("true");
			while (Reader.Read())
			{
				if (Reader.Name == "object")
				{
					orCreateCellAt.Objects.Add(LoadObjectNode(Reader));
				}
				if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == "cell"))
				{
					break;
				}
			}
		}

		private MapFileObjectBlueprint LoadObjectNode(XmlTextReader Reader)
		{
			MapFileObjectBlueprint mapFileObjectBlueprint = new MapFileObjectBlueprint("");
			mapFileObjectBlueprint.Name = Reader.GetAttribute("Name");
			mapFileObjectBlueprint.Owner = Reader.GetAttribute("Owner");
			mapFileObjectBlueprint.Part = Reader.GetAttribute("Part");
			if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
			{
				return mapFileObjectBlueprint;
			}
			while (Reader.Read())
			{
				if (Reader.Name == "property")
				{
					MapFileObjectBlueprint mapFileObjectBlueprint2 = mapFileObjectBlueprint;
					if (mapFileObjectBlueprint2.Properties == null)
					{
						mapFileObjectBlueprint2.Properties = new Dictionary<string, string>();
					}
					mapFileObjectBlueprint.Properties[Reader.GetAttribute("Name")] = Reader.GetAttribute("Value");
					Reader.SkipToEnd();
				}
				else if (Reader.Name == "intproperty")
				{
					MapFileObjectBlueprint mapFileObjectBlueprint2 = mapFileObjectBlueprint;
					if (mapFileObjectBlueprint2.IntProperties == null)
					{
						mapFileObjectBlueprint2.IntProperties = new Dictionary<string, int>();
					}
					mapFileObjectBlueprint.IntProperties[Reader.GetAttribute("Name")] = int.Parse(Reader.GetAttribute("Value"));
					Reader.SkipToEnd();
				}
				else if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == "object" || Reader.Name == "cell"))
				{
					return mapFileObjectBlueprint;
				}
			}
			return null;
		}

		public void Save(string FileName)
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			XmlWriter xmlWriter = XmlWriter.Create(FileName, xmlWriterSettings);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("Map");
			xmlWriter.WriteAttributeString("Width", width.ToString());
			xmlWriter.WriteAttributeString("Height", height.ToString());
			foreach (MapFileCellReference item in Cells.AllCells())
			{
				xmlWriter.WriteStartElement("cell");
				int x = item.x;
				xmlWriter.WriteAttributeString("X", x.ToString());
				x = item.y;
				xmlWriter.WriteAttributeString("Y", x.ToString());
				if (item.cell.Clear)
				{
					xmlWriter.WriteAttributeString("Clear", "true");
				}
				foreach (MapFileObjectBlueprint @object in item.cell.Objects)
				{
					xmlWriter.WriteStartElement("object");
					xmlWriter.WriteAttributeString("Name", @object.Name);
					if (!@object.Owner.IsNullOrEmpty())
					{
						xmlWriter.WriteAttributeString("Owner", @object.Owner);
					}
					if (!@object.Part.IsNullOrEmpty())
					{
						xmlWriter.WriteAttributeString("Part", @object.Part);
					}
					if (!@object.Properties.IsNullOrEmpty())
					{
						foreach (KeyValuePair<string, string> property in @object.Properties)
						{
							xmlWriter.WriteStartElement("property");
							xmlWriter.WriteAttributeString("Name", property.Key);
							xmlWriter.WriteAttributeString("Value", property.Value);
							xmlWriter.WriteEndElement();
						}
					}
					if (!@object.IntProperties.IsNullOrEmpty())
					{
						foreach (KeyValuePair<string, int> intProperty in @object.IntProperties)
						{
							xmlWriter.WriteStartElement("intproperty");
							xmlWriter.WriteAttributeString("Name", intProperty.Key);
							xmlWriter.WriteAttributeString("Value", intProperty.Value.ToString());
							xmlWriter.WriteEndElement();
						}
					}
					xmlWriter.WriteFullEndElement();
				}
				xmlWriter.WriteFullEndElement();
			}
			xmlWriter.WriteFullEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
			xmlWriter.Close();
		}
	}
}
