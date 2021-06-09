using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public static class SplatmapImporter {

	public static Random random = new Random();

	public static byte[,] GetFixedSplatmap(string path, Color[] mappings, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ) {
		int width;
		int height;
		int depth;
		byte[] byteBuffer;
		using (FileStream stream = File.Open(path, FileMode.Open))
		{
			Bitmap splat = new Bitmap(stream);
			width = splat.Width;
			height = splat.Height;
			depth = Image.GetPixelFormatSize(splat.PixelFormat) / 8;
			var rect = new Rectangle(0, 0, width, height);
			var data = splat.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, splat.PixelFormat);
			byteBuffer = new byte[height * width * depth];
			Marshal.Copy(data.Scan0, byteBuffer, 0, byteBuffer.Length);
			splat.Dispose();
		}
		byte[,] map = new byte[sizeX, sizeZ];
		Parallel.For(0, sizeX, x =>
		{
			for (int y = 0; y < sizeZ; y++)
			{
				Color c = GetPixel(byteBuffer, offsetX + x, offsetZ + y, width, height, depth);
				byte mapping;
				if (ditherLimit > 1)
				{
					mapping = GetDitheredMapping(c, mappings, ditherLimit);
				}
				else
				{
					mapping = GetClosestMapping(c, mappings);
				}
				map[x, y] = mapping;
			}
		});
		return map;
	}

	static Color GetPixel(byte[] byteBuffer, int x, int y, int width, int height, int depth)
	{
		int by = height - y - 1;
		int pos = (by * width + x) * depth;

		var b = byteBuffer[pos + 0];
		var g = byteBuffer[pos + 1];
		var r = byteBuffer[pos + 2];
		var a = depth > 3 ? byteBuffer[pos + 3] : (byte)255;

		return Color.FromArgb(a, r, g, b);
	}

	static byte GetClosestMapping(Color c, Color[] mappings) {
		int[] deviations = new int[mappings.Length];
		for(int i = 0; i < mappings.Length; i++) {
			deviations[i] += Math.Abs(c.R - mappings[i].R);
			deviations[i] += Math.Abs(c.G - mappings[i].G);
			deviations[i] += Math.Abs(c.B - mappings[i].B);
		}
		byte index = 255;
		int closest = 999;
		for(byte i = 0; i < mappings.Length; i++) {
			if(deviations[i] < closest) {
				index = i;
				closest = deviations[i];
			}
		}
		return index;
	}

	static byte GetDitheredMapping(Color c, Color[] mappings, int ditherLimit) {
		float[] probs = new float[mappings.Length];
		for(int i = 0; i < mappings.Length; i++) {
			int deviation = 0;
			deviation += Math.Abs(c.R - mappings[i].R);
			deviation += Math.Abs(c.G - mappings[i].G);
			deviation += Math.Abs(c.B - mappings[i].B);
			if(deviation >= ditherLimit) {
				probs[i] = 0;
			} else {
				probs[i] = 1 - (deviation / (float)ditherLimit);
			}
		}
		float max = 0;
		foreach(float p in probs) max += p;
		double d = random.NextDouble() * max;
		double v = 0;
		for(byte i = 0; i < probs.Length; i++) {
			v += probs[i];
			if(d < v) return i;
		}
		return 255;
	}
}