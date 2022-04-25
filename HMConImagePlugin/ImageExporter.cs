using HMCon;
using HMCon.Export;
using HMCon.Formats;
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
	public static class ImageExporter
	{

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
