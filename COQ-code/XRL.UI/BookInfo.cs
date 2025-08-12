using System.Collections.Generic;

namespace XRL.UI
{
	public class BookInfo
	{
		public string ID;

		public string Title;

		public string Format;

		public int LeftMargin = 2;

		public int RightMargin = 2;

		public int TopMargin = 2;

		public int BottomMargin = 2;

		public bool Dynamic;

		public List<string> Texts;

		private List<BookPage> _Pages;

		public BookPage this[int Index] => Pages[Index];

		public int Count => Pages.Count;

		public List<BookPage> Pages
		{
			get
			{
				if (_Pages.IsNullOrEmpty() && !Texts.IsNullOrEmpty())
				{
					if (_Pages == null)
					{
						_Pages = new List<BookPage>(Texts.Count);
					}
					foreach (string text2 in Texts)
					{
						if (!Format.IsNullOrEmpty())
						{
							_Pages.AddRange(BookUI.AutoformatPages(Title, text2, Format, TopMargin: TopMargin, RightMargin: RightMargin, BottomMargin: BottomMargin, LeftMargin: LeftMargin));
						}
						else
						{
							_Pages.Add(new BookPage(Title, text2));
						}
					}
				}
				return _Pages;
			}
			set
			{
				_Pages = value;
			}
		}

		public void Reset()
		{
			_Pages?.Clear();
		}

		public List<BookPage>.Enumerator GetEnumerator()
		{
			return Pages.GetEnumerator();
		}
	}
}
