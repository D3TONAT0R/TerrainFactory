using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ASCReader.Export;
using ASCReader.Export.Exporters;

namespace ASCReader {
	public class Previewer {

		public static readonly (int size, Color col)[] allGrids = new (int, Color)[] {
			(10,Color.Black),
			(50,Color.DarkCyan),
			(100,Color.DarkBlue),
			(500,Color.DarkGreen),
			(1000,Color.Yellow),
		};

		public static void OpenDataPreview(ASCData data, ExportOptions options, bool heightmap) {
			float[,] d;
			if(options.useExportRange) {
				d = data.GetDataRange(options.exportRange);
			} else {
				d = data.data;
			}
			ImageExporter exporter = new ImageExporter(d, data.cellsize, heightmap ? ImageType.Heightmap : ImageType.Hillshade, data.lowPoint, data.highPoint);
			MakeGrid(exporter.image, options);
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

		private static void MakeGrid(Bitmap img, ExportOptions options) {
			int dim = MinDim(img);
			if(dim < 50) return;
			Queue<(int size, Color col)> grids = new Queue<(int size, Color col)>();
			foreach(var g in allGrids) {
				if(Range(dim,g.size*2,g.size*20)) grids.Enqueue(g);
			}
			int i = 0;
			while(grids.Count > 0) {
				float opacity = (float)Math.Pow(1f/grids.Count, 2);
				var grid = grids.Dequeue();
				DrawGrid(img, grid.size, grid.col, opacity, i == 0, options);
				DrawGridLegend(img, grid.size, grid.col, i);
				i++;
			}
		}

		private static bool Range(int i, int min, int max) {
			return i >= min && i < max;
		}

		private static void DrawGrid(Bitmap img, int size, Color color, float opacity, bool drawCoords, ExportOptions range) {
			var graphics = Graphics.FromImage(img);
			var font = SystemFonts.MessageBoxFont;
			var brush = new SolidBrush(Color.Black);
			int shiftX = range.exportRange.xMin;
			int shiftY = range.exportRange.yMin;
			//vertical lines
			for(int x = 0; x < img.Width; x++) {
				if((x-shiftX) % size == 0) {
					for(int y = 0; y < img.Height; y++) {
						SetPixel(img,x,y,color,opacity);
					}
					if(drawCoords) graphics.DrawString((x+shiftX).ToString(), font, brush, new PointF(x,0));
				}
			}
			//horizontal lines
			for(int y = 0; y < img.Height; y++) {
				if((y-shiftY) % size == 0) {
					for(int x = 0; x < img.Width; x++) {
						SetPixel(img,x,y,color,opacity);
					}
					if(drawCoords) graphics.DrawString((y+shiftY).ToString(), font, brush, new PointF(0,img.Height-y-1));
				}
			}
		}

		private static void DrawGridLegend(Bitmap img, int size, Color color, int index) {
			//Draw info text in the corner
			Graphics g = Graphics.FromImage(img);
			Font f = SystemFonts.MessageBoxFont;
			g.DrawString(size.ToString(), SystemFonts.MessageBoxFont, new SolidBrush(color), new PointF(2,2+index*(f.Size+2)));
		}

		private static int MinDim(Image img) {
			if(img.Width < img.Height) return img.Width;
			else return img.Height;
		}

		private static Color Lerp(Color ca, Color cb, float t) {
			byte r = (byte)(cb.R*t + ca.R*(1-t));
			byte g = (byte)(cb.G*t + ca.G*(1-t));
			byte b = (byte)(cb.B*t + ca.B*(1-t));
			return Color.FromArgb(255,r,g,b);
		}

		private static void SetPixel(Bitmap img, int x, int y, Color color, float opacity) {
			y = img.Height-y-1;
			Color src = img.GetPixel(x,y);
			img.SetPixel(x,y,Lerp(src,color,opacity));
		}
	}
}