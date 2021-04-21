using HMCon;
using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Text;

namespace HMConImage {
	public class ImageExporter : HMConExportHandler {
		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("IMG_PNG-HM", "png-hm", "png", "Heightmap", this));
			list.Add(new FileFormat("IMG_PNG-NM", "png-nm", "png", "Normalmap", this));
			list.Add(new FileFormat("IMG_PNG-HS", "png-hs", "png", "Hillshade", this));
		}

		public override bool Export(ASCData data, FileFormat ff, string fullPath) {
			if(CurrentExportJobInfo.exportSettings == null) {
				throw new NullReferenceException("exportSettings was null");
			}
			if(data == null) {
				throw new NullReferenceException("data was null");
			}
			return WriteFileImage(data, fullPath, CurrentExportJobInfo.exportSettings.subsampling, CurrentExportJobInfo.bounds ?? data.GetBounds(), ff);
		}

		public override void EditFileName(FileNameProvider path, FileFormat fileFormat) {
			if(fileFormat.IsFormat("IMG_PNG-HM")) path.suffix = "_height";
			else if(fileFormat.IsFormat("IMG_PNG-NM")) path.suffix = "_normal";
			else if(fileFormat.IsFormat("IMG_PNG-HS")) path.suffix = "_hillshade";
		}

		public override bool ValidateExportOptions(ExportSettings options, FileFormat format, ASCData data) {
			return true;
		}

		bool WriteFileImage(ASCData source, string filename, int subsampling, Bounds bounds, FileFormat ff) {
			if(subsampling < 1) subsampling = 1;
			float[,] grid = new float[bounds.NumCols / subsampling, bounds.NumRows / subsampling];
			for(int x = 0; x < grid.GetLength(0); x++) {
				for(int y = 0; y < grid.GetLength(1); y++) {
					grid[x, y] = source.data[bounds.xMin + x * subsampling, bounds.yMin + y * subsampling];
				}
			}
			IExporter exporter = new ImageGenerator(grid, source.cellsize, ff.GetImageType(), source.lowPoint, source.highPoint);
			ExportUtility.WriteFile(exporter, filename, ff);
			return true;
		}

		public static Bitmap GenerateCompositeMap(ASCData data, Bitmap baseMap, float heightmapIntensity, float hillshadeIntensity) {
			Bitmap result;
			if(baseMap == null) {
				result = new Bitmap(data.ncols, data.nrows);
				var graphics = Graphics.FromImage(result);
				graphics.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(0, 0, baseMap.Width, baseMap.Height));
			} else {
				result = baseMap;
			}
			if(heightmapIntensity > 0) {
				Bitmap hm = new ImageGenerator(data.data, data.cellsize, ImageType.Heightmap, data.lowPoint, data.highPoint).image;
				result = OverlayBlend(result, hm, heightmapIntensity);
			}
			if(hillshadeIntensity > 0) {
				Bitmap hs = new ImageGenerator(data.data, data.cellsize, ImageType.Hillshade, data.lowPoint, data.highPoint).image;
				result = OverlayBlend(result, hs, hillshadeIntensity);
			}
			return result;
		}

		private static Bitmap OverlayBlend(Bitmap a, Bitmap b, float strength) {
			Bitmap result = new Bitmap(a);
			for(int y = 0; y < a.Height; y++) {
				for(int x = 0; x < a.Width; x++) {
					var ca = a.GetPixel(x, y);
					var cb = b.GetPixel(x, y);
					result.SetPixel(x, y, OverlayBlend(ca, cb, strength));
				}
			}
			return result;
		}

		private static Color OverlayBlend(Color ca, Color cb, float strength) {
			float[] a = new float[] { ca.R / 255f, ca.G / 255f, ca.B / 255f };
			float[] b = new float[] { cb.R / 255f, cb.G / 255f, cb.B / 255f };
			float[] r = new float[3];
			for(int i = 0; i < 3; i++) {
				if(b[i] > 0.5f) {
					r[i] = a[i] + (b[i] - 0.5f) * strength * 2f;
				} else {
					r[i] = a[i] + (b[i] - 0.5f) * strength * 2f;
				}
				r[i] = Math.Max(0, Math.Min(1, r[i]));
			}
			return Color.FromArgb((byte)(r[0] * 255), (byte)(r[1] * 255), (byte)(r[2] * 255));
		}
	}
}
