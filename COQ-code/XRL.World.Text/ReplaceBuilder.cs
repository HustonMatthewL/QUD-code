using System;
using System.Collections.Generic;
using System.Text;
using XRL.Collections;
using XRL.Language;
using XRL.World.Text.Delegates;

namespace XRL.World.Text
{
	public class ReplaceBuilder
	{
		private const int FLAG_STRIP_COLORS = 1;

		private const int FLAG_THIRD_PERSON = 2;

		private static Stack<ReplaceBuilder> Builders = new Stack<ReplaceBuilder>(2);

		private StringBuilder Target;

		private StringMap<ReplacerEntry> Replacers = new StringMap<ReplacerEntry>();

		private StringMap<int> Aliases = new StringMap<int>();

		private List<TextArgument> Arguments = new List<TextArgument>();

		private int DefaultArgument = -1;

		private int Flags;

		private bool Valid = true;

		private static Replacer DefaultReplacer = (DelegateContext Context) => Context.Default;

		public static ReplaceBuilder Get()
		{
			if (!Builders.TryPop(out var result))
			{
				result = new ReplaceBuilder();
			}
			result.Valid = true;
			return result;
		}

		public static void Return(ReplaceBuilder Builder)
		{
			if (!Builders.Contains(Builder))
			{
				Builder.Valid = false;
				Builder.Flags = 0;
				Builder.Target = null;
				Builder.Replacers.Clear();
				Builder.Aliases.Clear();
				Builder.Arguments.Clear();
				Builder.DefaultArgument = -1;
				Builders.Push(Builder);
			}
		}

		public ReplaceBuilder()
		{
		}

		public ReplaceBuilder(StringBuilder Target)
		{
			this.Target = Target;
		}

		private void AssertValid()
		{
			if (!Valid)
			{
				throw new InvalidOperationException(Builders.Contains(this) ? "Builder is finished" : "Builder is invalid");
			}
		}

		public ReplaceBuilder Start(StringBuilder Target)
		{
			AssertValid();
			this.Target = Target;
			Replacers.Clear();
			Aliases.Clear();
			Arguments.Clear();
			DefaultArgument = -1;
			return this;
		}

		public ReplaceBuilder SetDefaultArgument(int Index)
		{
			AssertValid();
			DefaultArgument = Index;
			return this;
		}

		public ReplaceBuilder SetDefaultArgument(GameObject Object)
		{
			AssertValid();
			int count = Arguments.Count;
			for (int i = 0; i < count; i++)
			{
				if (Arguments[i].Object == Object)
				{
					DefaultArgument = i;
					return this;
				}
			}
			throw new KeyNotFoundException("Object was not present in list of arguments.");
		}

		public ReplaceBuilder SetDefaultArgument(string Explicit)
		{
			AssertValid();
			int count = Arguments.Count;
			for (int i = 0; i < count; i++)
			{
				if (Arguments[i].Explicit == Explicit)
				{
					DefaultArgument = i;
					return this;
				}
			}
			throw new KeyNotFoundException("Explicit string was not present in list of arguments.");
		}

		public ReplaceBuilder AddObject(GameObject Object, string Alias = null)
		{
			AssertValid();
			if (Object != null)
			{
				Arguments.Add(new TextArgument(Object));
				if (Alias != null)
				{
					Aliases[Alias] = Arguments.Count - 1;
				}
				if (DefaultArgument == -1)
				{
					DefaultArgument = 0;
				}
			}
			return this;
		}

		public ReplaceBuilder AddExplicit(string Name, string Alias = null, IPronounProvider Pronouns = null)
		{
			AssertValid();
			if (!Name.IsNullOrEmpty())
			{
				Arguments.Add(new TextArgument(Name, Pronouns));
				if (Alias != null)
				{
					Aliases[Alias] = Arguments.Count - 1;
				}
				if (DefaultArgument == -1)
				{
					DefaultArgument = 0;
				}
			}
			return this;
		}

		public ReplaceBuilder AddExplicit(string Name, bool Plural)
		{
			AssertValid();
			if (!Name.IsNullOrEmpty())
			{
				Arguments.Add(new TextArgument(Name, Plural));
				if (DefaultArgument == -1)
				{
					DefaultArgument = 0;
				}
			}
			return this;
		}

		public ReplaceBuilder AddAlias(string Alias, int Index)
		{
			AssertValid();
			Aliases[Alias] = Index;
			return this;
		}

		public ReplaceBuilder AddAlias(string Alias, GameObject Object)
		{
			AssertValid();
			int count = Arguments.Count;
			for (int i = 0; i < count; i++)
			{
				if (Arguments[i].Object == Object)
				{
					Aliases[Alias] = i;
					return this;
				}
			}
			throw new KeyNotFoundException("Object was not present in list of arguments.");
		}

		public ReplaceBuilder AddReplacer(string Key, Replacer Delegate, string Default = null, int Flags = 0, bool Capitalization = false)
		{
			AssertValid();
			if (!Capitalization)
			{
				Replacers.Add(Key, new ReplacerEntry(Delegate, Default, Flags));
			}
			else
			{
				Replacers.Add(Grammar.InitLower(Key), new ReplacerEntry(Delegate, (Default != null) ? Grammar.InitLower(Default) : null, Flags));
				Replacers.Add(Grammar.InitCap(Key), new ReplacerEntry(Delegate, (Default != null) ? Grammar.InitCap(Default) : null, Flags, Capitalize: true));
			}
			return this;
		}

		public ReplaceBuilder AddReplacer(string Key, string Value)
		{
			AssertValid();
			Replacers.Add(Key, new ReplacerEntry(DefaultReplacer, Value));
			return this;
		}

		public ReplaceBuilder StripColors()
		{
			AssertValid();
			Flags.SetBit(1, value: true);
			return this;
		}

		public ReplaceBuilder ForceThirdPerson()
		{
			AssertValid();
			Flags.SetBit(2, value: true);
			return this;
		}

		public void Execute()
		{
			Process();
			Return(this);
		}

		public override string ToString()
		{
			Process();
			string result = Target.ToString();
			Return(this);
			return result;
		}

		private void Process()
		{
			AssertValid();
			bool flag = Flags.HasBit(2) && Grammar.AllowSecondPerson;
			try
			{
				if (flag)
				{
					Grammar.AllowSecondPerson = false;
				}
				GameText.Process(Target, Replacers, Aliases, Arguments, DefaultArgument, Flags.HasBit(1));
			}
			finally
			{
				if (flag)
				{
					Grammar.AllowSecondPerson = true;
				}
			}
		}
	}
}
