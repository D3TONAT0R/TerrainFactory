using System;
using System.Drawing;
using System.IO;

namespace ASCReader.Import {
	public static class HeightmapImporter {

		public static ASCData ImportHeightmap(string filepath) {
			FileStream stream = File.Open(filepath, FileMode.Open);
			var image = new Bitmap(stream);
			ASCData asc = new ASCData(image.Width, image.Height, filepath);
			Program.WriteLine(image.Width + "x" + image.Height);
			asc.cellsize = 1;
			asc.nodata_value = -9999;
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					Color c = image.GetPixel(x, y);
					asc.data[x, y] = c.GetBrightness();
				}
			}
			asc.RecalculateValues(false);
			asc.lowPoint = 0;
			asc.highPoint = 1;
			image.Dispose();
			stream.Close();
			asc.isValid = true;
			return asc;
		}

		public static byte[,] ImportHeightmapRaw(string filepath, int offsetX, int offsetY, int width, int height) {
			FileStream stream = File.Open(filepath, FileMode.Open);
			var image = new Bitmap(stream);
			byte[,] arr = new byte[width, height];
			for(int x = 0; x < width; x++) {
				for(int y = 0; y < height; y++) {
					Color c = image.GetPixel(offsetX + x, offsetY + y);
					arr[x, y] = (byte)Math.Round(c.GetBrightness() * 255);
				}
			}
			return arr;
		}
	}
}