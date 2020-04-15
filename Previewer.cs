using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ASCReader.Export;
using ASCReader.Export.Exporters;

namespace ASCReader {
	public class Previewer {

		public static void OpenDataPreview(ASCData data, ExportOptions options, bool heightmap) {
			float[,] d;
			if(options.useExportRange) {
				d = data.GetDataRange(options.exportRange);
			} else {
				d = data.data;
			}
			ImageExporter exporter = new ImageExporter(d, data.cellsize, heightmap ? ImageType.Heightmap : ImageType.Hillshade, data.lowPoint, data.highPoint);
			MakeGrid(exporter.image);
			var format = heightmap ? FileFormat.IMG_PNG_Height : FileFormat.IMG_PNG_Hillshade;
			string path = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
			FileStream stream = File.OpenWrite(path);
			exporter.WriteFile(stream, format);
			stream.Close();
			var p = new Process();
			p.StartInfo = new ProcessStartInfo(path) {
    			UseShellExecute = true
			};
			p.Start();
		}

		private static void MakeGrid(Bitmap img) {
			int dim = MinDim(img);
			if(dim < 50) return;
			if(Range(dim,50,250)) DrawGrid(img, 10, Color.Red);
			if(Range(dim,200,2000)) DrawGrid(img, 50, Color.Green);
			if(Range(dim,500,10000)) DrawGrid(img, 100, Color.Blue);
			if(dim <= 5000) DrawGrid(img, 500, Color.Yellow);
		}

		private static bool Range(int i, int min, int max) {
			return i >= min && i < max;
		}

		private static void DrawGrid(Bitmap img, int size, Color color) {
			//vertical lines
			for(int x = size; x < img.Width; x += size) {
				for(int y = 0; y < img.Height; y++) {
					img.SetPixel(x,y,color);
				}
			}
			//horizontal lines
			for(int y = size; y < img.Height; y += size) {
				for(int x = 0; x < img.Width; x++) {
					img.SetPixel(x,y,color);
				}
			}
		}

		private static int MinDim(Image img) {
			if(img.Width < img.Height) return img.Width;
			else return img.Height;
		}
	}
}