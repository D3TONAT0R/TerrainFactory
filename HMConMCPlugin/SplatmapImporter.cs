using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace HMConMC
{
	public enum ColorChannel
	{
		Red,
		Green,
		Blue,
		Alpha
	}

	public static class SplatmapImporter
	{

		public static Random random = new Random();

		public static byte[,] GetFixedSplatmap(string path, Color[] mappings, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var byteBuffer = GetBitmapBytes(path, out int width, out int height, out int depth);
			byte[,] map = new byte[sizeX, sizeZ];
			Parallel.For(0, sizeX, x =>
			{
				for (int y = 0; y < sizeZ; y++)
				{
					Color c = GetPixel(byteBuffer, offsetX + x, offsetZ + y, width, height, depth);
					byte mapping;
					if (ditherLimit > 1)
					{
						mapping = GetDitheredMapping(c, mappings, ditherLimit);
					}
					else
					{
						mapping = GetClosestMapping(c, mappings);
					}
					map[x, y] = mapping;
				}
			});
			return map;
		}

		public static bool[,] GetBitMask(float[,] mask, float threshold)
		{
			bool[,] bitMask = new bool[mask.GetLength(0), mask.GetLength(1)];
			for (int x = 0; x < mask.GetLength(0); x++)
			{
				for (int y = 0; y < mask.GetLength(1); y++)
				{
					bitMask[x, y] = mask[x, y] >= threshold;
				}
			}
			return bitMask;
		}

		public static float[][,] GetSplatMask(string path, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			float[][,] masks = new float[4][,];
			var byteBuffer = GetBitmapBytes(path, out int w, out int h, out int d);
			masks[0] = GetMask(byteBuffer, w, h, d, ColorChannel.Red, offsetX, offsetZ, sizeX, sizeZ);
			masks[1] = GetMask(byteBuffer, w, h, d, ColorChannel.Green, offsetX, offsetZ, sizeX, sizeZ);
			masks[2] = GetMask(byteBuffer, w, h, d, ColorChannel.Blue, offsetX, offsetZ, sizeX, sizeZ);
			masks[3] = GetMask(byteBuffer, w, h, d, ColorChannel.Alpha, offsetX, offsetZ, sizeX, sizeZ);
			return masks;
		}

		public static float[,] GetMask(string path, ColorChannel channel, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var byteBuffer = GetBitmapBytes(path, out int width, out int height, out int depth);
			return GetMask(byteBuffer, width, height, depth, channel, offsetX, offsetZ, sizeX, sizeZ);
		}

		private static float[,] GetMask(byte[] byteBuffer, int width, int height, int depth, ColorChannel channel, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			float[,] mask = new float[sizeX, sizeZ];
			Parallel.For(0, sizeX, x =>
			{
				for (int y = 0; y < sizeZ; y++)
				{
					Color c = GetPixel(byteBuffer, offsetX + x, offsetZ + y, width, height, depth);
					byte v = 0;
					if (channel == ColorChannel.Red)
					{
						v = c.R;
					}
					else if (channel == ColorChannel.Green)
					{
						v = c.G;
					}
					else if (channel == ColorChannel.Blue)
					{
						v = c.B;
					}
					else if (channel == ColorChannel.Alpha)
					{
						v = c.A;
					}
					mask[x, y] = (v / 255f);
				}
			});
			return mask;
		}

		static byte[] GetBitmapBytes(string bitmapPath, out int width, out int height, out int depth)
		{
			using (FileStream stream = File.Open(bitmapPath, FileMode.Open))
			{
				using (var bmp = new Bitmap(stream))
				{
					Bitmap splat = new Bitmap(stream);
					return GetBitmapBytes(splat, out width, out height, out depth);
				}
			}
		}

		static byte[] GetBitmapBytes(Bitmap bmp, out int width, out int height, out int depth)
		{
			byte[] byteBuffer;
			width = bmp.Width;
			height = bmp.Height;
			depth = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
			var rect = new Rectangle(0, 0, width, height);
			var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
			byteBuffer = new byte[height * width * depth];
			Marshal.Copy(data.Scan0, byteBuffer, 0, byteBuffer.Length);
			return byteBuffer;
		}

		static Color GetPixel(byte[] byteBuffer, int x, int y, int width, int height, int depth)
		{
			int pos = (y * width + x) * depth;

			var b = byteBuffer[pos + 0];
			var g = byteBuffer[pos + 1];
			var r = byteBuffer[pos + 2];
			var a = depth > 3 ? byteBuffer[pos + 3] : (byte)255;

			return Color.FromArgb(a, r, g, b);
		}

		static byte GetClosestMapping(Color c, Color[] mappings)
		{
			int[] deviations = new int[mappings.Length];
			for (int i = 0; i < mappings.Length; i++)
			{
				deviations[i] += Math.Abs(c.R - mappings[i].R);
				deviations[i] += Math.Abs(c.G - mappings[i].G);
				deviations[i] += Math.Abs(c.B - mappings[i].B);
			}
			byte index = 255;
			int closest = 999;
			for (byte i = 0; i < mappings.Length; i++)
			{
				if (deviations[i] < closest)
				{
					index = i;
					closest = deviations[i];
				}
			}
			return index;
		}

		static byte GetDitheredMapping(Color c, Color[] mappings, int ditherLimit)
		{
			float[] probs = new float[mappings.Length];
			for (int i = 0; i < mappings.Length; i++)
			{
				int deviation = 0;
				deviation += Math.Abs(c.R - mappings[i].R);
				deviation += Math.Abs(c.G - mappings[i].G);
				deviation += Math.Abs(c.B - mappings[i].B);
				if (deviation >= ditherLimit)
				{
					probs[i] = 0;
				}
				else
				{
					probs[i] = 1 - (deviation / (float)ditherLimit);
				}
			}
			float max = 0;
			foreach (float p in probs) max += p;
			double d = random.NextDouble() * max;
			double v = 0;
			for (byte i = 0; i < probs.Length; i++)
			{
				v += probs[i];
				if (d < v) return i;
			}
			return 255;
		}
	}
}