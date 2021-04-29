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
	}
}
