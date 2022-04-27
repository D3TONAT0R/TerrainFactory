using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMConImage.Formats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace HMConImage {
	public class Previewer {

		public static readonly (int size, Color col)[] allGrids = new (int, Color)[] {
			(10,Color.Black),
			(50,Color.DarkCyan),
			(100,Color.DarkBlue),
			(500,Color.DarkGreen),
			(1000,Color.Yellow),
		};

		public static void OpenDataPreview(Job job, bool heightmap) {

			var data = job.ApplyModificationChain(job.CurrentData);

			var exporter = new ImageGeneratorMagick(data, heightmap ? ImageType.Heightmap8 : ImageType.Hillshade, data.lowPoint, data.highPoint);
			//TODO: Make grid on magick image
			//MakeGrid(exporter.GetImageAsBitmap(), data.offsetFromSource);
			string path = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
			exporter.WriteFile(path, ImageMagick.MagickFormat.Png24);
			var p = new Process {
				StartInfo = new ProcessStartInfo(path) {
					UseShellExecute = true
				}
			};
			p.Start();
		}

		private static void MakeGrid(Bitmap img, (int x, int y) offsetFromSource) {
			int dim = MinDim(img);
			if(dim < 50) return;
			Queue<(int size, Color col)> grids = new Queue<(int size, Color col)>();
			foreach(var g in allGrids) {
				if(Range(dim, g.size * 2, g.size * 20)) grids.Enqueue(g);
			}
			int i = 0;
			while(grids.Count > 0) {
				float opacity = (float)Math.Pow(1f / grids.Count, 2);
				var (size, col) = grids.Dequeue();
				DrawGrid(img, size, col, opacity, i == 0, offsetFromSource);
				DrawGridLegend(img, size, col, i);
				i++;
			}
		}

		private static bool Range(int i, int min, int max) {
			return i >= min && i < max;
		}

		private static void DrawGrid(Bitmap img, int size, Color color, float opacity, bool drawCoords, (int x, int y) offsetFromSource) {
			//vertical lines
			for(int x = 0; x < img.Width; x++) {
				if((x - offsetFromSource.x) % size == 0) {
					for(int y = 0; y < img.Height; y++) {
						SetPixel(img, x, y, color, opacity);
					}
					if(drawCoords && x > 20) {
						int tx = x;
						int ty = 0;
						DrawString(img, (x + offsetFromSource.x).ToString(), color, ref tx, ref ty);
					}
				}
			}
			//horizontal lines
			for(int y = 0; y < img.Height; y++) {
				if((y - offsetFromSource.y) % size == 0) {
					for(int x = 0; x < img.Width; x++) {
						SetPixel(img, x, y, color, opacity);
					}
					if(drawCoords && y > 20) {
						int tx = 0;
						int ty = img.Height - y - 1;
						DrawString(img, (y + offsetFromSource.y).ToString(), color, ref tx, ref ty);
					}
				}
			}
		}

		private static void DrawGridLegend(Bitmap img, int size, Color color, int index) {
			//Draw info text in the corner
			int x = 2;
			int y = (int)(2 + index * (SystemFonts.MessageBoxFont.Size + 2));
			DrawString(img, size.ToString(), color, ref x, ref y);
		}

		private static void DrawString(Bitmap img, string str, Color color, ref int x, ref int y) {
			if(MinDim(img) > 200) {
				Graphics g = Graphics.FromImage(img);
				g.DrawString(str, SystemFonts.MessageBoxFont, new SolidBrush(color), new PointF(x, y));
			} else {
				PixelFont.DrawString(img, str, ref x, ref y, color, 1);
			}
		}

		private static int MinDim(Image img) {
			if(img.Width < img.Height) return img.Width;
			else return img.Height;
		}

		private static Color Lerp(Color ca, Color cb, float t) {
			byte r = (byte)(cb.R * t + ca.R * (1 - t));
			byte g = (byte)(cb.G * t + ca.G * (1 - t));
			byte b = (byte)(cb.B * t + ca.B * (1 - t));
			return Color.FromArgb(255, r, g, b);
		}

		public static void SetPixel(Bitmap img, int x, int y, Color color, float opacity) {
			y = img.Height - y - 1;
			if(x < 0 || x >= img.Width || y < 0 || y >= img.Height) return;
			Color src = img.GetPixel(x, y);
			img.SetPixel(x, y, Lerp(src, color, opacity));
		}
	}
}