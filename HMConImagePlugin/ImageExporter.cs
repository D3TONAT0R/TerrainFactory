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
			list.Add(new FileFormat("IMG_PNG-HM-S", "png-hm-s", "png", "Heightmap", this));
			list.Add(new FileFormat("IMG_PNG-NM", "png-nm", "png", "Normalmap", this));
			list.Add(new FileFormat("IMG_PNG-HS", "png-hs", "png", "Hillshade", this));
		}

		public override bool Export(ExportJob job) {
			return WriteFileImage(job.data, job.FilePath, job.format);
		}

		public override void EditFileName(ExportJob job, FileNameBuilder nameBuilder) {
			if(job.format.IsFormat("IMG_PNG-HM")) nameBuilder.suffix = "_height";
			if(job.format.IsFormat("IMG_PNG-HM-S")) nameBuilder.suffix = "_height_s";
			else if(job.format.IsFormat("IMG_PNG-NM")) nameBuilder.suffix = "_normal";
			else if(job.format.IsFormat("IMG_PNG-HS")) nameBuilder.suffix = "_hillshade";
		}

		public override bool AreExportSettingsValid(ExportSettings options, FileFormat format, HeightData data) {
			return true;
		}

		bool WriteFileImage(HeightData source, string filename, FileFormat ff) {
			float[,] grid = source.GetDataGrid();
			IExporter exporter = new ImageGenerator(grid, source.cellSize, ff.GetImageType(), source.lowPoint, source.highPoint);
			ExportUtility.WriteFile(exporter, filename, ff);
			return true;
		}

		public static Bitmap GenerateCompositeMap(HeightData data, Bitmap baseMap, float heightmapIntensity, float hillshadeIntensity) {
			Bitmap result;
			if(baseMap == null) {
				result = new Bitmap(data.GridWidth, data.GridHeight);
				var graphics = Graphics.FromImage(result);
				graphics.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(0, 0, baseMap.Width, baseMap.Height));
			} else {
				result = baseMap;
			}
			if(heightmapIntensity > 0) {
				Bitmap hm = new ImageGenerator(data.GetDataGrid(), data.cellSize, ImageType.Heightmap, data.lowPoint, data.highPoint).image;
				result = OverlayBlend(result, hm, heightmapIntensity);
			}
			if(hillshadeIntensity > 0) {
				Bitmap hs = new ImageGenerator(data.GetDataGrid(), data.cellSize, ImageType.Hillshade, data.lowPoint, data.highPoint).image;
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
