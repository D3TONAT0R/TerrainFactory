using HMCon;
using HMConImage;
using System.Drawing;

namespace HMConImage {
	public static class PixelFont {

		public static void DrawString(Bitmap img, string str, ref int x, ref int y, Color color, float opacity) {
			x++;
			y++;
			foreach(char c in str) {
				DrawChar(img, c, ref x, ref y, color, opacity);
			}
		}

		public static void DrawChar(Bitmap img, char c, ref int x, ref int y, Color color, float opacity) {
			bool[,] map = GetCharPixels(c);
			for(int i = 0; i < map.GetLength(0); i++) {
				for(int j = 0; j < map.GetLength(1); j++) {
					if(map[i, j]) {
						Previewer.SetPixel(img, x + i, y + j, color, opacity);
					}
				}
			}
			x += map.GetLength(0) + 1;
		}


		private static bool[,] GetCharPixels(char c) {
			int[,] map;
			switch(c) {
				case '0':
					map = new int[,] {
				{ 1,1,1 },
				{ 1,0,1 },
				{ 1,0,1 },
				{ 1,0,1 },
				{ 1,1,1 }};
					break;
				case '1':
					map = new int[,] {
				{ 1,1,0 },
				{ 0,1,0 },
				{ 0,1,0 },
				{ 0,1,0 },
				{ 1,1,1 }};
					break;
				case '2':
					map = new int[,] {
				{ 1,1,1 },
				{ 0,0,1 },
				{ 1,1,1 },
				{ 1,0,0 },
				{ 1,1,1 }};
					break;
				case '3':
					map = new int[,] {
				{ 1,1,1 },
				{ 0,0,1 },
				{ 1,1,1 },
				{ 0,0,1 },
				{ 1,1,1 }};
					break;
				case '4':
					map = new int[,] {
				{ 1,0,1 },
				{ 1,0,1 },
				{ 1,1,1 },
				{ 0,0,1 },
				{ 0,0,1 }};
					break;
				case '5':
					map = new int[,] {
				{ 1,1,1 },
				{ 1,0,0 },
				{ 1,1,1 },
				{ 0,0,1 },
				{ 1,1,1 }};
					break;
				case '6':
					map = new int[,] {
				{ 1,1,1 },
				{ 1,0,0 },
				{ 1,1,1 },
				{ 1,0,1 },
				{ 1,1,1 }};
					break;
				case '7':
					map = new int[,] {
				{ 1,1,1 },
				{ 0,0,1 },
				{ 0,0,1 },
				{ 0,0,1 },
				{ 0,0,1 }};
					break;
				case '8':
					map = new int[,] {
				{ 1,1,1 },
				{ 1,0,1 },
				{ 1,1,1 },
				{ 1,0,1 },
				{ 1,1,1 }};
					break;
				case '9':
					map = new int[,] {
				{ 1,1,1 },
				{ 1,0,1 },
				{ 1,1,1 },
				{ 0,0,1 },
				{ 1,1,1 }};
					break;
				case ' ':
					map = new int[,] {
				{ 0,0,0 },
				{ 0,0,0 },
				{ 0,0,0 },
				{ 0,0,0 },
				{ 0,0,0 }};
					break;
				default:
					map = new int[,] {
				{ 1,0,1 },
				{ 0,1,0 },
				{ 1,0,1 },
				{ 0,1,0 },
				{ 1,0,1 }};
					break;
			}
			return ConvertToBools(map);
		}

		private static bool[,] ConvertToBools(int[,] pixels) {
			bool[,] ret = new bool[pixels.GetLength(0), pixels.GetLength(1)];
			for(int x = 0; x < ret.GetLength(0); x++) {
				for(int y = 0; y < ret.GetLength(1); y++) {
					ret[x, y] = pixels[x, y] > 0;
				}
			}
			return ret;
		}
	} 
}