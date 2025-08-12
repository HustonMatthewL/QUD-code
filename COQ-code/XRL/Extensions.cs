using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using XRL.Collections;
using XRL.Language;
using XRL.Rules;
using XRL.World;

namespace XRL
{
	public static class Extensions
	{
		public static RectTransform CopyFrom(this RectTransform self, RectTransform other)
		{
			self.anchorMin = new Vector2(other.anchorMin.x, other.anchorMin.y);
			self.anchorMax = new Vector2(other.anchorMax.x, other.anchorMax.y);
			self.anchoredPosition = new Vector2(other.anchoredPosition.x, other.anchoredPosition.y);
			self.sizeDelta = new Vector2(other.sizeDelta.x, other.sizeDelta.y);
			self.pivot = new Vector2(other.pivot.x, other.pivot.y);
			return self;
		}

		public static string Things(this int num, string what, string whatPlural = null)
		{
			if (num == 1)
			{
				return "1 " + what;
			}
			return num + " " + (whatPlural ?? Grammar.Pluralize(what));
		}

		public static string Things(this float num, string what, string whatPlural = null)
		{
			if (num == 1f)
			{
				return "1 " + what;
			}
			return num + " " + (whatPlural ?? Grammar.Pluralize(what));
		}

		public static string Things(this double num, string what, string whatPlural = null)
		{
			if (num == 1.0)
			{
				return "1 " + what;
			}
			return num + " " + (whatPlural ?? Grammar.Pluralize(what));
		}

		public static StringBuilder DumpStringBuilder(this List<XRL.World.GameObject> list, StringBuilder SB = null)
		{
			if (SB == null)
			{
				SB = new StringBuilder();
			}
			for (int i = 0; i < list.Count; i++)
			{
				SB.Append(i).Append(": ").Append(list[i].DebugName)
					.Append('\n');
			}
			return SB;
		}

		public static StringBuilder AppendColored(this StringBuilder SB, string color, string text)
		{
			return SB.Append("{{").Append(color).Append("|")
				.Append(text)
				.Append("}}");
		}

		public static StringBuilder AppendRules(this StringBuilder SB, string text)
		{
			if (!text.IsNullOrEmpty())
			{
				SB.Append("\n{{rules|").Append(text).Append("}}");
			}
			return SB;
		}

		public static StringBuilder AppendRules(this StringBuilder SB, string text, Action<StringBuilder> proc)
		{
			if (!text.IsNullOrEmpty())
			{
				SB.Append("\n{{rules|").Append(text);
				proc?.Invoke(SB);
				SB.Append("}}");
			}
			return SB;
		}

		public static StringBuilder AppendRules(this StringBuilder SB, Action<StringBuilder> appender)
		{
			SB.Append("\n{{rules|");
			appender(SB);
			SB.Append("}}");
			return SB;
		}

		public static StringBuilder AppendRules(this StringBuilder SB, Action<StringBuilder> appender, Action<StringBuilder> proc)
		{
			SB.Append("\n{{rules|");
			appender(SB);
			proc(SB);
			SB.Append("}}");
			return SB;
		}

		public static string Dump(this List<XRL.World.GameObject> list)
		{
			return list.DumpStringBuilder().ToString();
		}

		public static int Roll(this string Dice)
		{
			return Stat.Roll(Dice);
		}

		public static int RollMin(this string Dice)
		{
			return Stat.RollMin(Dice);
		}

		public static int RollMax(this string Dice)
		{
			return Stat.RollMax(Dice);
		}

		public static int RollCached(this string Dice)
		{
			return Stat.RollCached(Dice);
		}

		public static int RollMinCached(this string Dice)
		{
			return Stat.RollMinCached(Dice);
		}

		public static int RollMaxCached(this string Dice)
		{
			return Stat.RollMaxCached(Dice);
		}

		public static double RollAverageCached(this string Dice)
		{
			return Stat.RollAverageCached(Dice);
		}

		public static DieRoll GetCachedDieRoll(this string Dice)
		{
			return Stat.GetCachedDieRoll(Dice);
		}

		public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext context)
		{
			return new SynchronizationContextAwaiter(context);
		}

		public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}
			if (list is List<T> list2)
			{
				list2.AddRange(items);
				return;
			}
			if (list is PooledList<T> pooledList)
			{
				pooledList.AddRange(items);
				return;
			}
			if (list is Rack<T> rack)
			{
				rack.AddRange(items);
				return;
			}
			foreach (T item in items)
			{
				list.Add(item);
			}
		}

		public static void Sort<T>(this IList<T> list)
		{
			if (list is List<T> list2)
			{
				list2.Sort();
				return;
			}
			List<T> list3 = new List<T>(list);
			list3.Sort();
			Copy(list3, 0, list, 0, list.Count);
		}

		public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
		{
			if (list is List<T> list2)
			{
				list2.Sort(comparison);
				return;
			}
			List<T> list3 = new List<T>(list);
			list3.Sort(comparison);
			Copy(list3, 0, list, 0, list.Count);
		}

		public static void Sort<T>(this IList<T> list, IComparer<T> comparer)
		{
			if (list is List<T> list2)
			{
				list2.Sort(comparer);
				return;
			}
			List<T> list3 = new List<T>(list);
			list3.Sort(comparer);
			Copy(list3, 0, list, 0, list.Count);
		}

		public static void Sort<T>(this IList<T> list, int index, int count, IComparer<T> comparer)
		{
			if (list is List<T> list2)
			{
				list2.Sort(index, count, comparer);
				return;
			}
			List<T> list3 = new List<T>(count);
			for (int i = 0; i < count; i++)
			{
				list3.Add(list[index + i]);
			}
			list3.Sort(comparer);
			Copy(list3, 0, list, index, count);
		}

		private static void Copy<T>(IList<T> sourceList, int sourceIndex, IList<T> destinationList, int destinationIndex, int count)
		{
			for (int i = 0; i < count; i++)
			{
				destinationList[destinationIndex + i] = sourceList[sourceIndex + i];
			}
		}

		public static ScopeDisposedList<T> GetScopeDisposedCopy<T>(this IEnumerable<T> sourceList)
		{
			return ScopeDisposedList<T>.GetFromPoolFilledWith(sourceList);
		}

		public static ScopeDisposedList<T> GetScopeDisposedCopy<T>(this IReadOnlyCollection<T> sourceList)
		{
			return ScopeDisposedList<T>.GetFromPoolFilledWith(sourceList);
		}

		public static ScopeDisposedList<T> GetScopeDisposedCopy<T>(this IReadOnlyList<T> sourceList)
		{
			return ScopeDisposedList<T>.GetFromPoolFilledWith(sourceList);
		}
	}
}
