using System;
using System.Collections.Generic;
using Genkit;
using XRL.EditorFormats.Map;
using XRL.World.Quests;

namespace XRL.World.ZoneParts
{
	[Serializable]
	public class ZoneRepair : IZonePart
	{
		public string FileName;

		public string RequiresObjectWithTagOrProperty;

		public string RequiresObjectOfFaction;

		public long TurnsPerObject = 50L;

		public long BuildCounter;

		public long LastTurn = long.MinValue;

		[NonSerialized]
		private MapFile Map;

		[NonSerialized]
		private List<(Location2D, string)> ToBuild;

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == ZoneActivatedEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(ZoneActivatedEvent E)
		{
			CheckZoneRepair();
			return base.HandleEvent(E);
		}

		public void CheckZoneRepair()
		{
			if (LastTurn == long.MinValue && The.Game != null)
			{
				LastTurn = The.Game.TimeTicks;
			}
			BuildCounter += The.Game.TimeTicks - LastTurn;
			LastTurn = The.Game.TimeTicks;
			if (BuildCounter < TurnsPerObject || (!RequiresObjectWithTagOrProperty.IsNullOrEmpty() && !ParentZone.HasObjectWithTagOrProperty(RequiresObjectWithTagOrProperty)) || (!RequiresObjectOfFaction.IsNullOrEmpty() && !ParentZone.HasObject((GameObject o) => o.IsFactionMember(RequiresObjectOfFaction))) || FileName.IsNullOrEmpty())
			{
				return;
			}
			long num = Math.Max(1L, BuildCounter / TurnsPerObject);
			BuildCounter = 0L;
			if (Map == null)
			{
				Map = MapFile.LoadWithMods(FileName);
			}
			if (Map == null)
			{
				return;
			}
			if (ToBuild == null)
			{
				ToBuild = new List<(Location2D, string)>();
				for (int i = 0; i < ParentZone.Height; i++)
				{
					for (int j = 0; j < ParentZone.Width; j++)
					{
						foreach (MapFileObjectBlueprint @object in Map.Cells[j, i].Objects)
						{
							GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(@object.Name);
							if (blueprint != null && !blueprint.IsWall() && !blueprint.HasPart("Brain") && !blueprint.HasTag("Non") && !ParentZone.GetCell(j, i).HasObject(@object.Name))
							{
								ToBuild.Add((Location2D.Get(j, i), @object.Name));
							}
						}
					}
				}
				ToBuild.ShuffleInPlace();
			}
			bool flag = false;
			for (long num2 = 0L; num2 < num; num2++)
			{
				if (ToBuild.Count <= 0)
				{
					break;
				}
				(Location2D, string) tuple = ToBuild[0];
				ToBuild.RemoveAt(0);
				ParentZone.GetCell(tuple.Item1).AddObject(tuple.Item2);
				flag = true;
			}
			GritGateScripts.OpenRank2Doors();
			if (ToBuild.Count == 0)
			{
				ParentZone.RemovePart(this);
			}
			if (flag)
			{
				PrimePowerSystemsEvent.Send(ParentZone);
			}
		}
	}
}
