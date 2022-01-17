using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {
	public static class MathUtils {

		public static int Clamp(int v, int min, int max) {
			return Math.Max(Math.Min(v, max), min);
		}

		public static float Clamp(float v, float min, float max) {
			return Math.Max(Math.Min(v, max), min);
		}

		public static float Clamp01(float v) {
			return Clamp(v, 0, 1);
		}

		public static float Lerp(float a, float b, float t) {
			return a + (b - a) * t;
		}

		public static float InverseLerp(float a, float b, float v)
		{
			return (v - a) / (b - a);
		}

		public static float Remap(float value, float oldA, float oldB, float newA, float newB)
		{
			float t = InverseLerp(oldA, oldB, value);
			return Lerp(newA, newB, t);
		}
	}
}
