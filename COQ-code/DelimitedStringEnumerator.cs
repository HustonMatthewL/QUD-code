using System;

public ref struct DelimitedStringEnumerator
{
	private string Value;

	private char Separator;

	private int Start;

	private int End;

	private int Length;

	public ReadOnlySpan<char> Current => Value.AsSpan(Start, End - Start);

	public DelimitedStringEnumerator(string Value, char Separator)
	{
		this.Value = Value;
		this.Separator = Separator;
		End = -1;
		Start = 0;
		Length = Value?.Length ?? 0;
	}

	public DelimitedStringEnumerator GetEnumerator()
	{
		return this;
	}

	public bool MoveNext()
	{
		Start = End + 1;
		if (Start >= Length)
		{
			return false;
		}
		End = Value.IndexOf(Separator, Start);
		if (End == -1)
		{
			End = Length;
		}
		return true;
	}
}
