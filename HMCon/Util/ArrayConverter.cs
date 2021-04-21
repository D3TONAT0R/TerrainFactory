using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Util {
	public static class ArrayConverter {

		public static float[,] ToFloatMap(short[,] arr) {
			float[,] result = new float[arr.GetLength(0), arr.GetLength(1)];
			for(int y = 0; y < arr.GetLength(1); y++) {
				for(int x = 0; x < arr.GetLength(0); x++) {
					result[x, y] = arr[x, y];
				}
			}
			return result;
		}

		/*public static void Flip(short[,] arr) {
			int ly = arr.GetLength(1);
			for(int x = 0; x < arr.GetLength(0); x++) {
				for(int y = 0; y < arr.GetLength(1) / 2; y++) {
					short v1 = arr[x, ly - 1 - y];
					arr[x, ly - 1 - y] = arr[x, y];
					arr[x, y] = v1;
				}
			}
		}*/

		public static short[,] Flip(short[,] arr) {
			short[,] flip = new short[arr.GetLength(0), arr.GetLength(1)];
			for(int x = 0; x < arr.GetLength(0); x++) {
				for(int y = 0; y < arr.GetLength(1); y++) {
					flip[x, y] = arr[x, arr.GetLength(1) - y - 1];
				}
			}
			return flip;
		}
	}
}
