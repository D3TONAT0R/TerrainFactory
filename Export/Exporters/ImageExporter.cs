using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Text;

namespace ASCReader.Export.Exporters {
	public class ImageExporter : IExporter {

		public Bitmap image;

		ImageType imageType;
		float[,] grid;
		float gridSpacing;
		float lowValue;
		float highValue;

		Vector3[,] normals;

		public ImageExporter(float[,] cells, float cellsize, ImageType type, float blackValue, float whiteValue) {
			grid = cells;
			gridSpacing = cellsize;
			imageType = type;
			lowValue = blackValue;
			highValue = whiteValue;
			image = new Bitmap(grid.GetLength(0), grid.GetLength(1));
			if(type == ImageType.Heightmap) MakeHeightmap();
			else if(type == ImageType.Normalmap) MakeNormalmap();
			else if(type == ImageType.Hillshade) MakeReliefmap();
		}

		private void MakeHeightmap() {
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					float v = (grid[x, y] - lowValue) / (highValue - lowValue);
					image.SetPixel(x, image.Height-y-1, CreateColorGrayscale(v));
				}
			}
		}

		private void CalculateNormals() {
			normals = new Vector3[grid.GetLength(0), grid.GetLength(1)];
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					float m = GetValueAt(x, y);
					float r = GetSlope(GetValueAt(x + 1, y), m);
					float l = GetSlope(m, GetValueAt(x - 1, y));
					float u = GetSlope(GetValueAt(x, y + 1), m);
					float d = GetSlope(m, GetValueAt(x, y - 1));
					float rl = (r + l) / 2f;
					float ud = (u + d) / 2f;
					float power = Math.Abs(rl) + Math.Abs(ud);
					if(power > 1) {
						rl /= power;
						ud /= power;
					}
					float vert = 1f - power;
					normals[x, y] = new Vector3(rl, ud, vert);
				}
			}
		}

		private void MakeNormalmap() {
			CalculateNormals();
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					Vector3 nrm = normals[x, y];
					float r = 0.5f + nrm.X / 2f;
					float g = 0.5f + nrm.Y / 2f;
					float b = 0.5f + nrm.Z / 2f;
					image.SetPixel(x, image.Height-y-1, CreateColor(r, g, b, 1));
				}
			}
		}

		private void MakeReliefmap() {
			CalculateNormals();
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					Vector3 nrm = normals[x, y];
					float light = 0.5f + nrm.X / 2f;
					light += 0.5f + nrm.Y / 2f;
					light /= 1.4f;
					image.SetPixel(x, image.Height - y - 1, CreateColorGrayscale(light));
				}
			}
		}

		private float GetValueAt(int x, int y) {
			x = Math.Clamp(x, 0, image.Width - 1);
			y = Math.Clamp(y, 0, image.Height - 1);
			return grid[x, y];
		}

		private float GetSlope(float from, float to) {
			float hdiff = to - from;
			return (float)(Rad2Deg(Math.Atan(hdiff / gridSpacing )) / 90f);
		}

		private Color CreateColor(float r, float g, float b, float a) {
			return Color.FromArgb(ToColorByte(a), ToColorByte(r), ToColorByte(g), ToColorByte(b));
		}

		private int ToColorByte(float f) {
			return (int)Math.Round(Math.Clamp(f, 0f, 1f) * 255);
		}

		private Color CreateColorGrayscale(float b) {
			return CreateColor(b,b,b,1);
		}

		public void WriteFile(FileStream stream, FileFormat filetype) {
			image.Save(stream, ImageFormat.Png);
		}

		private double Rad2Deg(double rad) {
			return rad * 180f / Math.PI;
		}
	}
}
