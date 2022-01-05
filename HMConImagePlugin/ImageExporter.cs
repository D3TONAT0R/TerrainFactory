using HMCon;
using HMCon.Export;
using HMCon.Util;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Text;

namespace HMConImage
{
	public class ImageExporter : HMConExportHandler
	{

		const string FormatPrefix = "IMG_";
		const string PNGPrefix = "PNG_";

		public const string HeightmapPNG8Bit = FormatPrefix + PNGPrefix + "HM";
		public const string HeightmapPNG16Bit = FormatPrefix + PNGPrefix + "HM-16";
		public const string NormalsPNG = FormatPrefix + PNGPrefix + "NM";
		public const string HillshadePNG = FormatPrefix + PNGPrefix + "HS";

		public override void AddFormatsToList(List<FileFormat> list)
		{
			list.Add(new FileFormat(HeightmapPNG8Bit, "png-hm", "png", "Heightmap", this));
			list.Add(new FileFormat(HeightmapPNG16Bit, "png-hm-16", "png", "Heightmap (16 Bit)", this));
			list.Add(new FileFormat(NormalsPNG, "png-nm", "png", "Normalmap", this));
			list.Add(new FileFormat(HillshadePNG, "png-hs", "png", "Hillshade", this));
		}

		public override bool Export(ExportJob job)
		{
			return WriteFileImage(job.data, job.FilePath, job.format);
		}

		public override void EditFileName(ExportJob job, FileNameBuilder nameBuilder)
		{
			if (job.format.IsFormat(HeightmapPNG8Bit)) nameBuilder.suffix = "height";
			if (job.format.IsFormat(HeightmapPNG16Bit)) nameBuilder.suffix = "height16";
			else if (job.format.IsFormat(NormalsPNG)) nameBuilder.suffix = "normal";
			else if (job.format.IsFormat(HillshadePNG)) nameBuilder.suffix = "hillshade";
		}

		public override bool AreExportSettingsValid(ExportSettings options, FileFormat format, HeightData data)
		{
			return true;
		}

		bool WriteFileImage(HeightData source, string filename, FileFormat ff)
		{
			IExporter exporter;
			exporter = new ImageGeneratorMagick(source, ff.GetImageType(), source.lowPoint, source.highPoint);
			ExportUtility.WriteFile(exporter, filename, ff);
			return true;
		}

		public static Bitmap GenerateCompositeMap(HeightData data, Bitmap baseMap, float heightmapIntensity, float hillshadeIntensity)
		{
			Bitmap result;
			if (baseMap == null)
			{
				result = new Bitmap(data.GridWidth, data.GridHeight);
				var graphics = Graphics.FromImage(result);
				graphics.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(0, 0, baseMap.Width, baseMap.Height));
			}
			else
			{
				result = baseMap;
			}
			if (heightmapIntensity > 0)
			{
				var hm = new ImageGeneratorMagick(data, ImageType.Heightmap8, data.lowPoint, data.highPoint).GetImageAsBitmap();
				result = OverlayBlend(result, hm, heightmapIntensity);
			}
			if (hillshadeIntensity > 0)
			{
				var hs = new ImageGeneratorMagick(data, ImageType.Hillshade, data.lowPoint, data.highPoint).GetImageAsBitmap();
				result = OverlayBlend(result, hs, hillshadeIntensity);
			}
			return result;
		}

		private static Bitmap OverlayBlend(Bitmap a, Bitmap b, float strength)
		{
			Bitmap result = new Bitmap(a.Width, a.Height);
			for (int y = 0; y < a.Height; y++)
			{
				for (int x = 0; x < a.Width; x++)
				{
					var ca = a.GetPixel(x, y);
					var cb = b.GetPixel(x, y);
					result.SetPixel(x, y, OverlayBlend(ca, cb, strength));
				}
			}
			return result;
		}

		private static Color OverlayBlend(Color ca, Color cb, float strength)
		{
			float[] a = new float[] { ca.R / 255f, ca.G / 255f, ca.B / 255f };
			float[] b = new float[] { cb.R / 255f, cb.G / 255f, cb.B / 255f };
			float[] r = new float[3];
			for (int i = 0; i < 3; i++)
			{
				if (b[i] > 0.5f)
				{
					r[i] = a[i] + (b[i] - 0.5f) * strength * 2f;
				}
				else
				{
					r[i] = a[i] + (b[i] - 0.5f) * strength * 2f;
				}
				r[i] = Math.Max(0, Math.Min(1, r[i]));
			}
			return Color.FromArgb((byte)(r[0] * 255), (byte)(r[1] * 255), (byte)(r[2] * 255));
		}
	}
}
