using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace XRL.CharacterBuilds.Qud
{
	public class QudMutationsModuleData : AbstractEmbarkBuilderModuleData
	{
		public int mp = -1;

		public List<QudMutationModuleDataRow> selections = new List<QudMutationModuleDataRow>();

		[JsonIgnore]
		public static List<MutationCategory> categories => MutationFactory.GetCategories();

		public static MutationEntry getMutationEntryByName(string name)
		{
			return MutationFactory.GetMutationEntryByName(name);
		}

		public QudMutationsModuleData()
		{
			version = "1.1.0";
		}

		[OnDeserialized]
		private void LegacySupport(StreamingContext Context)
		{
			if (!Version.TryParse(version, out var Version) || !(Version < new Version(1, 1)))
			{
				return;
			}
			foreach (QudMutationModuleDataRow selection in selections)
			{
				selection.Upgrade(Version);
			}
		}
	}
}
