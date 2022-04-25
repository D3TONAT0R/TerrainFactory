using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HMConImage
{
	public static class HeightmapImporter
	{

		const string progString = "Importing heightmap";

		public enum ColorChannel
		{
			Red,
			Green,
			Blue,
			Alpha,
			CombinedBrightness
		}

		public static HeightData Import(string importPath, params string[] args)
		{
			ColorChannel? channel = null;
			if (args.TryGetArgument("channel", out string v))
			{
				v = v.ToUpper();
				if (v == "R") channel = ColorChannel.Red;
				else if (v == "G") channel = ColorChannel.Green;
				else if (v == "B") channel = ColorChannel.Blue;
				else if (v == "A") channel = ColorChannel.Alpha;
				else if (v == "C") channel = ColorChannel.CombinedBrightness;
			}
			if (args.TryGetArgument("bytes"))
			{
				return ImportHeightmap256(importPath, channel ?? ColorChannel.Red);
			}
			else
			{
				return ImportHeightmap(importPath, 0, 1, channel ?? ColorChannel.CombinedBrightness);
			}
		}

		public static HeightData ImportHeightmap(string filepath, float low, float high, ColorChannel channel = ColorChannel.CombinedBrightness)
		{
			return ImportHeightmap(filepath,
				(HeightData d, int x, int y, Color c) =>
				{
					d.SetHeight(x, y, GetValue(c, channel));
				},
				(HeightData d) =>
				{
					d.RecalculateValues(false);
					d.lowPoint = low;
					d.highPoint = high;
				}
			);
		}

		public static HeightData ImportHeightmap256(string filepath, ColorChannel channel = ColorChannel.CombinedBrightness)
		{
			return ImportHeightmap(filepath,
				(HeightData d, int x, int y, Color c) =>
				{
					d.SetHeight(x, y, GetValueRaw(c, channel));
				},
				(HeightData d) =>
				{
					d.RecalculateValues(false);
					d.lowPoint = 0;
					d.highPoint = 255;
				}
			);
		}

		private static HeightData ImportHeightmap(string filepath, Action<HeightData, int, int, Color> iterator, Action<HeightData> finalizer)
		{
			ConsoleOutput.UpdateProgressBar(progString, 0);
			FileStream stream = File.Open(filepath, FileMode.Open);
			var image = new Bitmap(stream);
			stream.Dispose();
			ConsoleOutput.UpdateProgressBar(progString, 0.5f);
			HeightData heightData = new HeightData(image.Width, image.Height, filepath);
			heightData.cellSize = 1;
			heightData.nodata_value = -9999;

			int width = image.Width;
			int height = image.Height;
			int depth = 4;
			var rect = new Rectangle(0, 0, width, height);
			var data = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			var byteBuffer = new byte[height * width * depth];
			Marshal.Copy(data.Scan0, byteBuffer, 0, byteBuffer.Length);
			image.Dispose();

			int progress = 0;

			Parallel.For(0, width, (x) =>
			{
				for (int y = 0; y < height; y++)
				{
					Color c = GetPixel(byteBuffer, x, y, width, height, depth);
					//Color c = image.GetPixel(x, image.Height - y - 1);
					iterator(heightData, x, y, c);
				}
				progress++;
				ConsoleOutput.UpdateProgressBar(progString, 0.5f + progress / (float)width * 0.5f);
			}
			);
			image.Dispose();
			stream.Close();

			finalizer(heightData);

			heightData.isValid = true;
			return heightData;
		}

		static Color GetPixel(byte[] byteBuffer, int x, int y, int width, int height, int depth)
		{
			int by = height - y - 1;
			int pos = (by * width + x) * depth;

			var b = byteBuffer[pos + 0];
			var g = byteBuffer[pos + 1];
			var r = byteBuffer[pos + 2];
			var a = depth > 3 ? byteBuffer[pos + 3] : (byte)255;

			return Color.FromArgb(a, r, g, b);
		}

		public static byte[,] ImportHeightmapRaw(string filepath, int offsetX, int offsetY, int width, int height, ColorChannel channel = ColorChannel.Red)
		{
			FileStream stream = File.Open(filepath, FileMode.Open);
			var image = new Bitmap(stream);
			byte[,] arr = new byte[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Color c = image.GetPixel(offsetX + x, offsetY + y);
					arr[x, height - 1 - y] = GetValueRaw(c, channel);
				}
			}
			return arr;
		}

		public static byte GetValueRaw(Color c, ColorChannel channel)
		{
			if (channel == ColorChannel.Red)
			{
				return c.R;
			}
			else if (channel == ColorChannel.Green)
			{
				return c.G;
			}
			else if (channel == ColorChannel.Blue)
			{
				return c.B;
			}
			else if (channel == ColorChannel.Alpha)
			{
				return c.A;
			}
			else
			{
				return (byte)Math.Round(c.GetBrightness() * 255);
			}
		}

		public static float GetValue(Color c, ColorChannel channel)
		{
			if (channel == ColorChannel.Red)
			{
				return c.R / 255f;
			}
			else if (channel == ColorChannel.Green)
			{
				return c.G / 255f;
			}
			else if (channel == ColorChannel.Blue)
			{
				return c.B / 255f;
			}
			else if (channel == ColorChannel.Alpha)
			{
				return c.A / 255f;
			}
			else
			{
				return c.GetBrightness();
			}
		}
	}
}
