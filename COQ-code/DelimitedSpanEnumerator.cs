using System;

public ref struct DelimitedSpanEnumerator
{
	private ReadOnlySpan<char> Value;

	private char Separator;

	private int Start;

	private int End;

	private int Length;

	public ReadOnlySpan<char> Current => Value.Slice(Start, End - Start);

	public DelimitedSpanEnumerator(ref ReadOnlySpan<char> Value, char Separator)
	{
		this.Value = Value;
		this.Separator = Separator;
		End = -1;
		Start = 0;
		Length = Value.Length;
	}

	public DelimitedSpanEnumerator GetEnumerator()
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
		End = -1;
		for (int i = Start; i < Length; i++)
		{
			if (Value[i] == Separator)
			{
				End = i;
				break;
			}
		}
		if (End == -1)
		{
			End = Length;
		}
		return true;
	}
}
