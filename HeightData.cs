using HMCon.Export;
using HMCon.Modification;
using HMCon.Util;
using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace HMCon {

	public class HeightData {

		public string filename;

		public int GridLengthX { get; private set; }
		public int GridLengthY { get; private set; }

		public int GridCellCount => GridLengthX * GridLengthY;

		public Vector2 lowerCornerPos;
		public (int x, int y) offsetFromSource = (0, 0);

		public float cellSize;

		public float nodataValue = float.MinValue;

		private float[,] DataGrid
		{
			get => grid;
			set
			{
				grid = value;
				GridLengthX = grid.GetLength(0);
				GridLengthY = grid.GetLength(1);
			}
		}
		private float[,] grid;

		public bool HasHeightData => DataGrid != null;

		public float lowestValue = float.PositiveInfinity;
		public float highestValue = float.NegativeInfinity;

		//Used for scaling operations. In data created from an image, these values represent the black and white values of the source image (default 0 and 1 respectively)
		//In data created from ASC data itself, these are equal to lowestValue and highestValue unless overridden for heightmap export.
		public float lowPoint;
		public float highPoint;

		public bool isValid;

		public bool wasModified = false;

		public HeightData() {

		}

		public HeightData(float[,] data, float cellSize) {
			DataGrid = data;
			this.cellSize = cellSize;
		}

		public HeightData(int ncols, int nrows, string sourceFile) {
			filename = sourceFile;
			DataGrid = new float[ncols, nrows];
		}

		public HeightData(HeightData original, float[,] newGrid) {
			filename = original.filename;
			lowerCornerPos = original.lowerCornerPos;
			offsetFromSource = original.offsetFromSource;
			cellSize = original.cellSize;
			nodataValue = original.nodataValue;
			DataGrid = newGrid;
			RecalculateValues(false);
			lowPoint = original.lowPoint;
			highPoint = original.highPoint;
		}

		public HeightData(HeightData original) : this(original, (float[,])original.DataGrid.Clone()) {

		}

		public void RecalculateValues(bool updateLowHighPoints) {
			lowestValue = float.MaxValue;
			highestValue = float.MinValue;
			foreach(float f in DataGrid) {
				if(Math.Abs(f - nodataValue) > 0.1f) {
					if(f < lowestValue) lowestValue = f;
					if(f > highestValue) highestValue = f;
				}
			}
			if(updateLowHighPoints) {
				lowPoint = lowestValue;
				highPoint = highestValue;
			}
		}

		public Bounds GetBounds() {
			return new Bounds(0, 0, GridLengthX - 1, GridLengthY - 1);
		}

		#region modification

		public void SetHeight(int x, int y, float value) {
			DataGrid[x, y] = value;
		}

		public void AddHeight(int x, int y, float add) {
			DataGrid[x, y] += add;
		}

		public void Add(HeightData other) {
			Modify((x, y, rx, ry, v) => {
				return v + other.GetHeightRelative(rx, ry);
			});
		}

		public void Multiply(HeightData other) {
			Modify((x, y, rx, ry, v) => {
				return v * other.GetHeightRelative(rx, ry);
			});
		}

		public void Rescale(float low, float high) {
			float dataRange = high - low;
			Modify((x, y, rx, ry, v) => {
				double h = (v - lowPoint) / (highPoint - lowPoint);
				h *= dataRange;
				h += low;
				return (float)h;
			});
			lowPoint = low;
			highPoint = high;
		}

		public delegate float ModificationFunc(int x, int y, float rx, float ry, float value);
		public void Modify(ModificationFunc modificator) {
			for(int y = 0; y < GridLengthY; y++) {
				for(int x = 0; x < GridLengthX; x++) {
					float rx = x / (float)(GridLengthX - 1);
					float ry = y / (float)(GridLengthY - 1);
					DataGrid[x, y] = modificator(x, y, rx, ry, DataGrid[x, y]);
				}
			}
			RecalculateValues(false);
		}

		public void Resize(int newDimX, bool scaleHeight) {
			int dimX = newDimX;
			float ratio = GridLengthX / (float)GridLengthY;
			int dimY = (int)(dimX / ratio);
			DataGrid = GetResizedData(dimX, dimY);
			float resizeRatio = GridLengthX / (float)GridLengthX;
			if(scaleHeight) {
				for(int x = 0; x < GridLengthX; x++) {
					for(int y = 0; y < GridLengthY; y++) {
						DataGrid[x, y] *= resizeRatio;
					}
				}
			}
			cellSize = resizeRatio;
		}

		public float[,] GetResizedData(int dimX, int dimY) {
			float[,] newData = new float[dimX, dimY];
			for(int x = 0; x < dimX; x++) {
				for(int y = 0; y < dimY; y++) {
					float nx = x / (float)dimX;
					float ny = y / (float)dimY;
					newData[x, y] = GetHeightInterpolated(nx * GridLengthX, ny * GridLengthY);
				}
			}
			return newData;
		}

		#endregion
		#region getter functions

		public float[,] GetDataGrid() {
			return DataGrid;
		}

		public float[,] GetDataGridFlipped() {
			float[,] grid = new float[GridLengthX, GridLengthY];
			var zLength = grid.GetLength(1);
			for(int x = 0; x < grid.GetLength(0); x++) {
				for(int y = 0; y < grid.GetLength(1); y++) {
					//Y starts from top
					grid[x, zLength - y - 1] = GetHeight(x, y);
				}
			}
			return grid;
		}

		public void SetDataGrid(float[,] newGrid) {
			DataGrid = newGrid;
		}

		public float GetHeight(int x, int y) {
			if(x < 0 || y < 0 || x >= GridLengthX || y >= GridLengthY) {
				return nodataValue;
			} else {
				return DataGrid[x, y];
			}
		}

		public float GetHeightUnchecked(int x, int y)
		{
			return DataGrid[x, y];
		}

		public float GetHeightBounded(int x, int y)
		{
			x = MathUtils.Clamp(x, 0, GridLengthX - 1);
			y = MathUtils.Clamp(y, 0, GridLengthY - 1);
			return DataGrid[x, y];
		}

		public float GetHeightInterpolated(float x, float y) {
			int x1 = (int)x;
			int y1 = (int)y;
			int x2 = x1 + 1;
			int y2 = y1 + 1;
			x1 = MathUtils.Clamp(x1, 0, GridLengthX - 1);
			x2 = MathUtils.Clamp(x2, 0, GridLengthX - 1);
			y1 = MathUtils.Clamp(y1, 0, GridLengthY - 1);
			y2 = MathUtils.Clamp(y2, 0, GridLengthY - 1);
			float wx = x - x1;
			float wy = y - y1;
			float vx1 = MathUtils.Lerp(GetHeight(x1, y1), GetHeight(x2, y1), wx);
			float vx2 = MathUtils.Lerp(GetHeight(x1, y2), GetHeight(x2, y2), wx);
			return MathUtils.Lerp(vx1, vx2, wy);
		}

		public float GetHeightRelative(float rx, float ry) {
			return GetHeightInterpolated(rx * (GridLengthX - 1), ry * (GridLengthY - 1));
		}

		public float[,] GetDataRange(Bounds bounds) {
			float[,] newdata = new float[bounds.NumCols, bounds.NumRows];
			for(int x = 0; x < bounds.NumCols; x++) {
				for(int y = 0; y < bounds.NumRows; y++) {
					newdata[x, y] = DataGrid[bounds.xMin + x, bounds.yMin + y];
				}
			}
			return newdata;
		}

		#endregion
	}
}