namespace HMCon.Util {
	public class Bounds {

		public int xMin;
		public int yMin;
		public int xMax;
		public int yMax;

		public Bounds(int x1, int y1, int x2, int y2) {
			xMin = x1;
			yMin = y1;
			xMax = x2;
			yMax = y2;
		}

		public int NumCols {
			get {
				return xMax - xMin + 1;
			}
		}

		public int NumRows {
			get {
				return yMax - yMin + 1;
			}
		}
	}
}
