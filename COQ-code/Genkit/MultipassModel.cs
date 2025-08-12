using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genkit
{
	public class MultipassModel : WaveCollapseModelBase
	{
		private int[][][][] propagator;

		private int N;

		private int TStart;

		private byte[][] patterns;

		private Color32[] colors;

		private int colorCount;

		private int ground;

		public void ClearColors(string clearColors, string clearstyle = "border1")
		{
			List<Color32> list = new List<Color32>();
			for (int i = 0; i < clearColors.Length; i++)
			{
				if (clearColors[i] == 'W')
				{
					list.Add(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				}
				if (clearColors[i] == 'w')
				{
					list.Add(new Color32(128, 128, 128, byte.MaxValue));
				}
				if (clearColors[i] == 'R')
				{
					list.Add(new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
				}
				if (clearColors[i] == 'K')
				{
					list.Add(new Color32(0, 0, 0, byte.MaxValue));
				}
				if (clearColors[i] == 'B')
				{
					list.Add(new Color32(0, 0, byte.MaxValue, byte.MaxValue));
				}
				if (clearColors[i] == 'G')
				{
					list.Add(new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				}
				if (clearColors[i] == 'M')
				{
					list.Add(new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue));
				}
				if (clearColors[i] == 'Y')
				{
					list.Add(new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
				}
			}
			for (int j = 0; j < FMY; j++)
			{
				int num = ((j >= FMY - N + 1) ? (N - 1) : 0);
				for (int k = 0; k < FMX; k++)
				{
					int num2 = ((k >= FMX - N + 1) ? (N - 1) : 0);
					Color32 color = colors[patterns[observed[k - num2][j - num]][num2 + num * N]];
					if (clearstyle == "border1")
					{
						if (list.Contains(color) && (k == 0 || WaveCollapseTools.equals(colors[patterns[observed[k - 1][j]][num2 + num * N]], color)) && (k == FMX - 1 || WaveCollapseTools.equals(colors[patterns[observed[k + 1][j]][num2 + num * N]], color)) && (j == 0 || WaveCollapseTools.equals(colors[patterns[observed[k][j - 1]][num2 + num * N]], color)) && (j == FMY - 1 || WaveCollapseTools.equals(colors[patterns[observed[k][j + 1]][num2 + num * N]], color)))
						{
							wave[k][j] = null;
						}
					}
					else if (clearstyle == "border1.5")
					{
						if (list.Contains(color) && (k == 0 || WaveCollapseTools.equals(colors[patterns[observed[k - 1][j]][num2 + num * N]], color)) && (k == FMX - 1 || WaveCollapseTools.equals(colors[patterns[observed[k + 1][j]][num2 + num * N]], color)) && (j == 0 || WaveCollapseTools.equals(colors[patterns[observed[k][j - 1]][num2 + num * N]], color)) && (j == FMY - 1 || WaveCollapseTools.equals(colors[patterns[observed[k][j + 1]][num2 + num * N]], color)) && (k <= 0 || j <= 0 || WaveCollapseTools.equals(colors[patterns[observed[k - 1][j - 1]][num2 + num * N]], color)) && (k <= 0 || j >= FMY - 1 || WaveCollapseTools.equals(colors[patterns[observed[k - 1][j + 1]][num2 + num * N]], color)) && (k >= FMX - 1 || j <= 0 || WaveCollapseTools.equals(colors[patterns[observed[k + 1][j - 1]][num2 + num * N]], color)) && (k >= FMX - 1 || j >= FMY - 1 || WaveCollapseTools.equals(colors[patterns[observed[k + 1][j + 1]][num2 + num * N]], color)))
						{
							wave[k][j] = null;
						}
					}
					else if (list.Contains(color))
					{
						wave[k][j] = null;
					}
				}
			}
		}

		public void UpdateSample(string entry, int N, bool periodicInput, bool periodicOutput, int symmetry, int ground)
		{
			if (!WaveCollapseTools.waveTemplates.ContainsKey(entry.ToLower()))
			{
				Debug.LogError("Unknown wave template: " + entry.ToLower());
			}
			else
			{
				UpdateSample(WaveCollapseTools.waveTemplates[entry.ToLower()], N, periodicInput, periodicOutput, symmetry, ground);
			}
		}

		public void UpdateSample(WaveTemplateEntry entry, int N, bool periodicInput, bool periodicOutput, int symmetry, int ground)
		{
			this.N = N;
			int SMX = entry.width;
			int SMY = entry.height;
			byte[,] sample = new byte[SMX, SMY];
			bClear = colors == null;
			if (colors == null)
			{
				colors = new Color32[256];
				colorCount = 0;
			}
			for (int i = 0; i < SMY; i++)
			{
				for (int j = 0; j < SMX; j++)
				{
					Color32 color = entry.pixels[j + (SMY - 1 - i) * SMX];
					byte b = 0;
					for (int k = 0; k < colorCount && !WaveCollapseTools.equals(colors[k], color); k++)
					{
						b++;
					}
					if (b >= colorCount)
					{
						colors[b] = color;
						colorCount++;
					}
					sample[j, i] = b;
				}
			}
			int C = colorCount;
			long W = WaveCollapseTools.Power(C, N * N);
			Func<Func<int, int, byte>, byte[]> pattern = delegate(Func<int, int, byte> f)
			{
				byte[] array = new byte[N * N];
				for (int l = 0; l < N; l++)
				{
					for (int m = 0; m < N; m++)
					{
						array[m + l * N] = f(m, l);
					}
				}
				return array;
			};
			Func<int, int, byte[]> func = (int x, int y) => pattern((int dx, int dy) => sample[(x + dx) % SMX, (y + dy) % SMY]);
			Func<byte[], byte[]> func2 = (byte[] p) => pattern((int x, int y) => p[N - 1 - y + x * N]);
			Func<byte[], byte[]> func3 = (byte[] p) => pattern((int x, int y) => p[N - 1 - x + y * N]);
			Func<byte[], long> func4 = delegate(byte[] p)
			{
				long num = 0L;
				long num2 = 1L;
				for (int n = 0; n < p.Length; n++)
				{
					num += p[p.Length - 1 - n] * num2;
					num2 *= C;
				}
				return num;
			};
			Func<long, byte[]> func5 = delegate(long ind)
			{
				long num3 = ind;
				long num4 = W;
				byte[] array2 = new byte[N * N];
				for (int num5 = 0; num5 < array2.Length; num5++)
				{
					num4 /= C;
					int num6 = 0;
					while (num3 >= num4)
					{
						num3 -= num4;
						num6++;
					}
					array2[num5] = (byte)num6;
				}
				return array2;
			};
			Dictionary<long, int> dictionary = new Dictionary<long, int>();
			List<long> list = new List<long>();
			for (int num7 = 0; num7 < (periodicInput ? SMY : (SMY - N + 1)); num7++)
			{
				for (int num8 = 0; num8 < (periodicInput ? SMX : (SMX - N + 1)); num8++)
				{
					byte[][] array3 = new byte[8][];
					array3[0] = func(num8, num7);
					array3[1] = func3(array3[0]);
					array3[2] = func2(array3[0]);
					array3[3] = func3(array3[2]);
					array3[4] = func2(array3[2]);
					array3[5] = func3(array3[4]);
					array3[6] = func2(array3[4]);
					array3[7] = func3(array3[6]);
					for (int num9 = 0; num9 < symmetry; num9++)
					{
						long num10 = func4(array3[num9]);
						if (dictionary.ContainsKey(num10))
						{
							dictionary[num10]++;
							continue;
						}
						dictionary.Add(num10, 1);
						list.Add(num10);
					}
				}
			}
			TStart = T;
			T += dictionary.Count;
			this.ground = (ground + T) % T;
			byte[][] array4 = patterns;
			patterns = new byte[T][];
			for (int num11 = 0; num11 < TStart; num11++)
			{
				patterns[num11] = array4[num11];
			}
			double[] array5 = stationary;
			stationary = new double[T];
			for (int num12 = 0; num12 < TStart; num12++)
			{
				stationary[num12] = array5[num12];
			}
			propagator = new int[2 * N - 1][][][];
			int num13 = T - dictionary.Count;
			foreach (long item in list)
			{
				patterns[num13] = func5(item);
				stationary[num13] = dictionary[item];
				num13++;
			}
			for (int num14 = 0; num14 < FMX; num14++)
			{
				for (int num15 = 0; num15 < FMY; num15++)
				{
					if (wave[num14][num15] == null)
					{
						wave[num14][num15] = new bool[T];
						for (int num16 = 0; num16 < T; num16++)
						{
							wave[num14][num15][num16] = num16 > TStart;
						}
					}
					else
					{
						if (wave[num14][num15].GetUpperBound(0) >= T)
						{
							continue;
						}
						int upperBound = wave[num14][num15].GetUpperBound(0);
						bool[] array6 = new bool[T];
						for (int num17 = 0; num17 < T; num17++)
						{
							if (num17 <= upperBound)
							{
								array6[num17] = wave[num14][num15][num17];
							}
							else
							{
								array6[num17] = false;
							}
						}
						wave[num14][num15] = array6;
					}
				}
			}
			Func<byte[], byte[], int, int, bool> func6 = delegate(byte[] p1, byte[] p2, int dx, int dy)
			{
				int num18 = ((dx >= 0) ? dx : 0);
				int num19 = ((dx < 0) ? (dx + N) : N);
				int num20 = ((dy >= 0) ? dy : 0);
				int num21 = ((dy < 0) ? (dy + N) : N);
				for (int num22 = num20; num22 < num21; num22++)
				{
					for (int num23 = num18; num23 < num19; num23++)
					{
						if (p1[num23 + N * num22] != p2[num23 - dx + N * (num22 - dy)])
						{
							return false;
						}
					}
				}
				return true;
			};
			for (int num24 = 0; num24 < 2 * N - 1; num24++)
			{
				propagator[num24] = new int[2 * N - 1][][];
				for (int num25 = 0; num25 < 2 * N - 1; num25++)
				{
					propagator[num24][num25] = new int[T][];
					for (int num26 = 0; num26 < T; num26++)
					{
						List<int> list2 = new List<int>();
						for (int num27 = 0; num27 < T; num27++)
						{
							if (func6(patterns[num26], patterns[num27], num24 - N + 1, num25 - N + 1))
							{
								list2.Add(num27);
							}
						}
						propagator[num24][num25][num26] = new int[list2.Count];
						for (int num28 = 0; num28 < list2.Count; num28++)
						{
							propagator[num24][num25][num26][num28] = list2[num28];
						}
					}
				}
			}
		}

		public MultipassModel(string entry, int N, int width, int height, bool periodicInput, bool periodicOutput, int symmetry, int ground)
			: base(width, height)
		{
			periodic = periodicOutput;
			if (!WaveCollapseTools.waveTemplates.ContainsKey(entry))
			{
				Debug.LogError("Unknown WFC template:" + entry);
			}
			UpdateSample(WaveCollapseTools.waveTemplates[entry], N, periodicInput, periodicOutput, symmetry, ground);
		}

		public MultipassModel(WaveTemplateEntry entry, int N, int width, int height, bool periodicInput, bool periodicOutput, int symmetry, int ground)
			: base(width, height)
		{
			periodic = periodicOutput;
			UpdateSample(entry, N, periodicInput, periodicOutput, symmetry, ground);
		}

		protected override bool OnBoundary(int x, int y)
		{
			if (!periodic)
			{
				if (x + N <= FMX)
				{
					return y + N > FMY;
				}
				return true;
			}
			return false;
		}

		protected override bool Propagate()
		{
			bool result = false;
			for (int i = 0; i < FMX; i++)
			{
				for (int j = 0; j < FMY; j++)
				{
					if (!changes[i][j])
					{
						continue;
					}
					changes[i][j] = false;
					for (int k = -N + 1; k < N; k++)
					{
						for (int l = -N + 1; l < N; l++)
						{
							int num = i + k;
							if (num < 0)
							{
								num += FMX;
							}
							else if (num >= FMX)
							{
								num -= FMX;
							}
							int num2 = j + l;
							if (num2 < 0)
							{
								num2 += FMY;
							}
							else if (num2 >= FMY)
							{
								num2 -= FMY;
							}
							if (!periodic && (num + N > FMX || num2 + N > FMY))
							{
								continue;
							}
							bool[] array = wave[i][j];
							bool[] array2 = wave[num][num2];
							int[][] array3 = propagator[N - 1 - k][N - 1 - l];
							for (int m = TStart; m < T; m++)
							{
								if (!array2[m])
								{
									continue;
								}
								bool flag = false;
								int[] array4 = array3[m];
								for (int n = 0; n < array4.Length; n++)
								{
									if (flag)
									{
										break;
									}
									flag = array[array4[n]];
								}
								if (!flag)
								{
									changes[num][num2] = true;
									result = true;
									array2[m] = false;
								}
							}
						}
					}
				}
			}
			return result;
		}

		public override Color32[] GetResult()
		{
			Color32[] array = new Color32[FMX * FMY];
			if (observed != null)
			{
				for (int i = 0; i < FMY; i++)
				{
					int num = ((i >= FMY - N + 1) ? (N - 1) : 0);
					for (int j = 0; j < FMX; j++)
					{
						int num2 = ((j >= FMX - N + 1) ? (N - 1) : 0);
						Color32 color = colors[patterns[observed[j - num2][i - num]][num2 + num * N]];
						if (wave[j][i] == null)
						{
							array[j + i * FMX] = new Color32(byte.MaxValue, 0, byte.MaxValue, 128);
						}
						else
						{
							array[j + i * FMX] = new Color32(color.r, color.g, color.b, byte.MaxValue);
						}
					}
				}
			}
			else
			{
				for (int k = 0; k < FMY; k++)
				{
					for (int l = 0; l < FMX; l++)
					{
						int num3 = 0;
						int num4 = 0;
						int num5 = 0;
						int num6 = 0;
						for (int m = 0; m < N; m++)
						{
							for (int n = 0; n < N; n++)
							{
								int num7 = l - n;
								if (num7 < 0)
								{
									num7 += FMX;
								}
								int num8 = k - m;
								if (num8 < 0)
								{
									num8 += FMY;
								}
								if (OnBoundary(num7, num8))
								{
									continue;
								}
								for (int num9 = 0; num9 < T; num9++)
								{
									if (wave[num7][num8][num9])
									{
										num3++;
										Color32 color2 = colors[patterns[num9][n + m * N]];
										num4 += color2.r;
										num5 += color2.g;
										num6 += color2.b;
									}
								}
							}
						}
						array[l + k * FMX] = new Color32((byte)(num4 / num3 * 255), (byte)(num5 / num3 * 255), (byte)(num6 / num3 * 255), byte.MaxValue);
					}
				}
			}
			return array;
		}

		protected override void Clear()
		{
			base.Clear();
			if (ground == 0)
			{
				return;
			}
			for (int i = 0; i < FMX; i++)
			{
				if (bClear)
				{
					for (int j = 0; j < T; j++)
					{
						if (j != ground)
						{
							wave[i][FMY - 1][j] = false;
						}
					}
				}
				if (bClear)
				{
					changes[i][FMY - 1] = true;
				}
				for (int k = 0; k < FMY - 1; k++)
				{
					wave[i][k][ground] = false;
					if (bClear)
					{
						changes[i][k] = true;
					}
				}
			}
			if (bClear)
			{
				while (Propagate())
				{
				}
			}
		}
	}
}
