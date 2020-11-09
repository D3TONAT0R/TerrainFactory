using ASCReader;
using ASCReader.Export;
using ASCReader.Import;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace ASCReaderImagePlugin {
	public class HeightmapImporter : ASCReaderImportHandler {
		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("IMG-PNG", "PNG", "png", "PNG Image", this));
			list.Add(new FileFormat("IMG-JPG", "JPG", "jpg", "JPG Image", this));
			list.Add(new FileFormat("IMG-JPG", "JPG", "jpeg", "JPG Image", this));
			list.Add(new FileFormat("IMG-TIF", "TIF", "tif", "TIF Image", this));
			list.Add(new FileFormat("IMG-BMP", "BMP", "bmp", "BMP Image", this));
		}

		public override ASCData Import(string importPath, FileFormat ff) {
			return ImportHeightmap(importPath);
		}

		public ASCData ImportHeightmap(string filepath) {
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
