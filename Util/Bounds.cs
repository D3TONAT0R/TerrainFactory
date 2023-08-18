namespace TerrainFactory.Util {
	public struct Bounds {

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

		public int CellCount {
			get {
				return NumCols * NumRows;
			}
		}

		public bool IsValid(HeightData d) {
			if(xMin < 0 || xMin >= d.GridLengthX) return false;
			if(yMin < 0 || yMin >= d.GridLengthY) return false;
			if(xMax < 0 || xMax >= d.GridLengthX) return false;
			if(yMax < 0 || yMax >= d.GridLengthY) return false;
			if(xMin > xMax) return false;
			if(yMin > yMax) return false;
			return true;
		}
	}
}
