using HMCon;
using HMCon.Export;
using HMCon.Import;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace HMConImage {
	public class HeightmapImporter : HMConImportHandler {

		public enum ColorChannel {
			Red,
			Green,
			Blue,
			Alpha,
			CombinedBrightness
		}

		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("IMG-PNG", "PNG", "png", "PNG Image", this));
			list.Add(new FileFormat("IMG-JPG", "JPG", "jpg", "JPG Image", this));
			list.Add(new FileFormat("IMG-JPG", "JPG", "jpeg", "JPG Image", this));
			list.Add(new FileFormat("IMG-TIF", "TIF", "tif", "TIF Image", this));
			list.Add(new FileFormat("IMG-BMP", "BMP", "bmp", "BMP Image", this));
		}

		public override HeightData Import(string importPath, FileFormat ff, params string[] args) {
			ColorChannel? channel = null;
			if(args.TryGetArgument("channel", out string v)) {
				v = v.ToUpper();
				if(v == "R") channel = ColorChannel.Red;
				else if(v == "G") channel = ColorChannel.Green;
				else if(v == "B") channel = ColorChannel.Blue;
				else if(v == "A") channel = ColorChannel.Alpha;
				else if(v == "C") channel = ColorChannel.CombinedBrightness;
			}
			if(args.TryGetArgument("bytes")) {
				return ImportHeightmap256(importPath, channel ?? ColorChannel.Red);
			} else {
				return ImportHeightmap(importPath, 0, 1, channel ?? ColorChannel.CombinedBrightness);
			}
		}

		public HeightData ImportHeightmap(string filepath, float low, float high, ColorChannel channel = ColorChannel.CombinedBrightness) {
			FileStream stream = File.Open(filepath, FileMode.Open);
			var image = new Bitmap(stream);
			HeightData data = new HeightData(image.Width, image.Height, filepath);
			ConsoleOutput.WriteLine(image.Width + "x" + image.Height);
			data.cellSize = 1;
			data.nodata_value = -9999;
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					Color c = image.GetPixel(x, image.Height - y - 1);
					data.SetHeight(x, y, GetValue(c, channel));
				}
			}
			data.RecalculateValues(false);
			data.lowPoint = low;
			data.highPoint = high;
			image.Dispose();
			stream.Close();
			data.isValid = true;
			return data;
		}

		public HeightData ImportHeightmap256(string filepath, ColorChannel channel = ColorChannel.CombinedBrightness) {
			FileStream stream = File.Open(filepath, FileMode.Open);
			var image = new Bitmap(stream);
			HeightData data = new HeightData(image.Width, image.Height, filepath);
			ConsoleOutput.WriteLine(image.Width + "x" + image.Height);
			data.cellSize = 1;
			data.nodata_value = -9999;
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					Color c = image.GetPixel(x, image.Height - y - 1);
					data.SetHeight(x, y, GetValueRaw(c, channel));
				}
			}
			data.RecalculateValues(false);
			data.lowPoint = 0;
			data.highPoint = 255;
			image.Dispose();
			stream.Close();
			data.isValid = true;
			return data;
		}

		public static byte[,] ImportHeightmapRaw(string filepath, int offsetX, int offsetY, int width, int height, ColorChannel channel = ColorChannel.Red) {
			FileStream stream = File.Open(filepath, FileMode.Open);
			var image = new Bitmap(stream);
			byte[,] arr = new byte[width, height];
			for(int x = 0; x < width; x++) {
				for(int y = 0; y < height; y++) {
					Color c = image.GetPixel(offsetX + x, offsetY + y);
					arr[x, height - 1 - y] = GetValueRaw(c, channel);
				}
			}
			return arr;
		}

		public static byte GetValueRaw(Color c, ColorChannel channel) {
			if(channel == ColorChannel.Red) {
				return c.R;
			} else if(channel == ColorChannel.Green) {
				return c.G;
			} else if(channel == ColorChannel.Blue) {
				return c.B;
			} else if(channel == ColorChannel.Alpha) {
				return c.A;
			} else {
				return (byte)Math.Round(c.GetBrightness() * 255);
			}
		}

		public static float GetValue(Color c, ColorChannel channel) {
			if(channel == ColorChannel.Red) {
				return c.R / 255f;
			} else if(channel == ColorChannel.Green) {
				return c.G / 255f;
			} else if(channel == ColorChannel.Blue) {
				return c.B / 255f;
			} else if(channel == ColorChannel.Alpha) {
				return c.A / 255f;
			} else {
				return c.GetBrightness();
			}
		}
	}
}
