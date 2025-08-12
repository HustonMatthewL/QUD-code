using System;
using Newtonsoft.Json;

namespace XRL
{
	[Serializable]
	[JsonConverter(typeof(Converter))]
	public struct Version : IComparable<Version>, IEquatable<Version>
	{
		public class Converter : JsonConverter
		{
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				if (value == null)
				{
					writer.WriteNull();
					return;
				}
				if (value is Version version)
				{
					writer.WriteValue(version.ToString(4));
					return;
				}
				throw new JsonSerializationException("Expected XRL.Version type value");
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				if (reader.TokenType == JsonToken.String)
				{
					return new Version((string)reader.Value);
				}
				return null;
			}

			public override bool CanConvert(Type objectType)
			{
				return (object)objectType == typeof(Version);
			}
		}

		public static readonly Version Zero = new Version(0);

		public int Major;

		public int Minor;

		public int Build;

		public int Revision;

		public Version(int Major, int Minor = 0, int Build = 0, int Revision = 0)
		{
			this.Major = Major;
			this.Minor = Minor;
			this.Build = Build;
			this.Revision = Revision;
		}

		public Version(string Version)
		{
			if (!TryParse(Version, out Major, out Minor, out Build, out Revision))
			{
				throw new FormatException("'" + Version + "' is not a valid version string.");
			}
		}

		public static bool TryParse(ReadOnlySpan<char> Text, out Version Version)
		{
			Version = default(Version);
			return TryParse(Text, out Version.Major, out Version.Minor, out Version.Build, out Version.Revision);
		}

		public static bool TryParse(ReadOnlySpan<char> Text, out int Major, out int Minor, out int Build, out int Revision)
		{
			Major = (Minor = (Build = (Revision = 0)));
			if (Text.Length == 0)
			{
				return false;
			}
			int num = Text.IndexOf('.');
			if (num == -1)
			{
				return int.TryParse(Text, out Major);
			}
			if (!int.TryParse(Text.Slice(0, num), out Major))
			{
				return false;
			}
			Text = Text.Slice(num + 1);
			int num2 = Text.IndexOf('.');
			if (num2 == -1)
			{
				return int.TryParse(Text, out Minor);
			}
			if (!int.TryParse(Text.Slice(0, num2), out Minor))
			{
				return false;
			}
			Text = Text.Slice(num2 + 1);
			int num3 = Text.IndexOf('.');
			if (num3 == -1)
			{
				return int.TryParse(Text, out Build);
			}
			if (!int.TryParse(Text.Slice(0, num3), out Build))
			{
				return false;
			}
			Text = Text.Slice(num3 + 1);
			int num4 = Text.IndexOf('.');
			return int.TryParse((num4 == -1) ? Text : Text.Slice(0, num4), out Revision);
		}

		public int CompareTo(Version Other)
		{
			if (Major != Other.Major)
			{
				if (Major > Other.Major)
				{
					return 1;
				}
				return -1;
			}
			if (Minor != Other.Minor)
			{
				if (Minor > Other.Minor)
				{
					return 1;
				}
				return -1;
			}
			if (Build != Other.Build)
			{
				if (Build > Other.Build)
				{
					return 1;
				}
				return -1;
			}
			if (Revision != Other.Revision)
			{
				if (Revision > Other.Revision)
				{
					return 1;
				}
				return -1;
			}
			return 0;
		}

		public override int GetHashCode()
		{
			return (Major, Minor, Build, Revision).GetHashCode();
		}

		public override bool Equals(object other)
		{
			if (other is Version other2)
			{
				return Equals(other2);
			}
			return false;
		}

		public bool Equals(Version Other)
		{
			if (Major == Other.Major && Minor == Other.Minor && Build == Other.Build)
			{
				return Revision == Other.Revision;
			}
			return false;
		}

		public static bool operator ==(Version First, Version Second)
		{
			return First.Equals(Second);
		}

		public static bool operator !=(Version First, Version Second)
		{
			return !First.Equals(Second);
		}

		public static bool operator <(Version First, Version Second)
		{
			return First.CompareTo(Second) < 0;
		}

		public static bool operator <=(Version First, Version Second)
		{
			return First.CompareTo(Second) <= 0;
		}

		public static bool operator >(Version First, Version Second)
		{
			return First.CompareTo(Second) > 0;
		}

		public static bool operator >=(Version First, Version Second)
		{
			return First.CompareTo(Second) >= 0;
		}

		public bool IsZero()
		{
			return this == Zero;
		}

		public override string ToString()
		{
			return ToString(3);
		}

		public string ToString(int Fields)
		{
			switch (Fields)
			{
			case 0:
				return string.Empty;
			case 1:
				return Major.ToString();
			default:
			{
				int num = Major.CountDigits() + Minor.CountDigits() + 1;
				int Index = 0;
				if (Fields >= 3)
				{
					num += Build.CountDigits() + 1;
				}
				if (Fields >= 4)
				{
					num += Revision.CountDigits() + 1;
				}
				Span<char> Text = stackalloc char[num];
				Text.Insert(ref Index, Major);
				Text[Index++] = '.';
				Text.Insert(ref Index, Minor);
				if (Fields >= 3)
				{
					Text[Index++] = '.';
					Text.Insert(ref Index, Build);
				}
				if (Fields >= 4)
				{
					Text[Index++] = '.';
					Text.Insert(ref Index, Revision);
				}
				return new string(Text);
			}
			}
		}
	}
}
