using HMCon;
using HMCon.Export;
using HMCon.Formats;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Text;

namespace HMConImage {
	class ImageGeneratorMagick {

		private MagickImage image;

		ImageType imageType;
		HeightData data;
		float lowValue;
		float highValue;

		public ImageGeneratorMagick(HeightData heightData, ImageType type, float blackValue, float whiteValue) {
			data = heightData;
			imageType = type;
			lowValue = blackValue;
			highValue = whiteValue;
			if (type == ImageType.Heightmap8) MakeHeightmap(false);
			else if (type == ImageType.Heightmap16) MakeHeightmap(true);
			else if (type == ImageType.Normalmap) MakeNormalmap(false);
			else if (type == ImageType.Hillshade) MakeHillshademap();
			else throw new NotImplementedException();
		}

		public MagickImage GetImage()
		{
			return image;
		}

		public Bitmap GetImageAsBitmap()
		{
			return image.ToBitmap(ImageFormat.Png);
		}

		public void WriteFile(string filename, MagickFormat format) {
			image.Write(filename, format);
		}

		private void MakeHeightmap(bool is16bit) {
			image = CreateImage(0, is16bit ? MagickFormat.Png48 : MagickFormat.Png24);
			var pixels = image.GetPixels();
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					float v = MathUtils.Clamp01(MathUtils.InverseLerp(lowValue, highValue, data.GetHeightUnchecked(x,y)));
					pixels.SetPixel(x, image.Height - y - 1, CreateColorGrayscale(v));
				}
			}
		}

		private void MakeNormalmap(bool sharp)
		{
			if (sharp)
			{
				image = CreateImage(-1);
			}
			else
			{
				image = CreateImage(0);
			}
			var pixels = image.GetPixels();
			var normals = NormalMapper.CalculateNormals(data, sharp);
			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					Vector3 nrm = normals[x, y];
					float r = 0.5f + nrm.X / 2f;
					float g = 0.5f + nrm.Y / 2f;
					float b = 0.5f + nrm.Z / 2f;
					pixels.SetPixel(x, image.Height - y - 1, CreateColor(r, g, b));
				}
			}
		}

		private void MakeHillshademap()
		{
			var normals = NormalMapper.CalculateNormals(data, true);
			image = CreateImage();
			var pixels = image.GetPixels();
			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					Vector3 nrm = normals[x, y];
					float strength = 1.5f;
					float light = 0.5f + nrm.X / 2f * 0.8f * strength;
					light += 0.5f + nrm.Y / 2f * 1.2f * strength;
					light /= 1.4f;
					pixels.SetPixel(x, image.Height - y - 1, CreateColorGrayscale(light));
				}
			}
		}

		private MagickImage CreateImage(int width, int height, MagickFormat format = MagickFormat.Png24)
		{
			var img = new MagickImage("xc:black", width, height);
			img.Format = format;
			return img;
		}

		private MagickImage CreateImage(int sizeOffset = 0, MagickFormat format = MagickFormat.Png24)
		{
			return CreateImage(data.GridWidth + sizeOffset, data.GridHeight + sizeOffset, format);
		}

		private ushort[] CreateColorGrayscale(float v) {
			ushort u = (ushort)(v * ushort.MaxValue);
			return new ushort[] { u, u, u, ushort.MaxValue };
		}

		private ushort[] CreateColor(float r, float g, float b)
		{
			ushort ur = (ushort)(r * ushort.MaxValue);
			ushort ug = (ushort)(g * ushort.MaxValue);
			ushort ub = (ushort)(b * ushort.MaxValue);
			return new ushort[] { ur, ug, ub, ushort.MaxValue };
		}
	}
}
