using System.IO;

namespace XRL
{
	public class ModFile
	{
		public ModInfo Mod;

		public string Name;

		public string FullName;

		public string RelativeName;

		public string OriginalName;

		public string Extension;

		public long Size;

		public ModFileType Type;

		public ModFile(ModInfo Mod, FileInfo File)
		{
			this.Mod = Mod;
			OriginalName = File.FullName;
			RelativeName = Mod.RelativePath(OriginalName).ToLowerInvariant();
			FullName = OriginalName.ToLowerInvariant();
			Name = Path.GetFileNameWithoutExtension(FullName);
			Extension = Path.GetExtension(FullName);
			Size = File.Length;
			Type = Extension switch
			{
				".xml" => ModFileType.XML, 
				".cs" => ModFileType.CSharp, 
				".png" => ModFileType.Sprite, 
				".wav" => ModFileType.Audio, 
				".ogg" => ModFileType.Audio, 
				".aiff" => ModFileType.Audio, 
				".mp3" => ModFileType.Audio, 
				_ => ModFileType.Unknown, 
			};
		}
	}
}
