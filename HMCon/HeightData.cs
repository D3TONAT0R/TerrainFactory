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

		public int GridWidth => dataGrid?.GetLength(0) ?? 0;
		public int GridHeight => dataGrid?.GetLength(1) ?? 0;

		public Vector2 lowerCornerPos;
		public (int x, int y) offsetFromSource = (0, 0);

		public float cellSize;

		public float nodata_value = float.MinValue;

		//public string fileHeader = "";

		private float[,] dataGrid;
		public bool HasHeightData => dataGrid != null;

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
			dataGrid = data;
			this.cellSize = cellSize;
		}

		public HeightData(int ncols, int nrows, string sourceFile) {
			filename = sourceFile;
			dataGrid = new float[ncols, nrows];
		}

		public HeightData(HeightData original, float[,] newGrid) {
			filename = original.filename;
			lowerCornerPos = original.lowerCornerPos;
			offsetFromSource = original.offsetFromSource;
			cellSize = original.cellSize;
			nodata_value = original.nodata_value;
			dataGrid = newGrid;
			RecalculateValues(false);
			lowPoint = original.lowPoint;
			highPoint = original.highPoint;
		}

		public HeightData(HeightData original) : this(original, (float[,])original.dataGrid.Clone()) {

		}

		public void RecalculateValues(bool updateLowHighPoints) {
			foreach(float f in dataGrid) {
				if(Math.Abs(f - nodata_value) > 0.1f) {
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
			return new Bounds(0, 0, GridWidth - 1, GridHeight - 1);
		}

		#region modification

		public void SetHeight(int x, int y, float value) {
			dataGrid[x, y] = value;
		}

		public void AddHeight(int x, int y, float add) {
			dataGrid[x, y] += add;
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
			for(int y = 0; y < GridHeight; y++) {
				for(int x = 0; x < GridWidth; x++) {
					float rx = x / (float)(GridWidth - 1);
					float ry = y / (float)(GridHeight - 1);
					dataGrid[x, y] = modificator(x, y, rx, ry, dataGrid[x, y]);
				}
			}
			RecalculateValues(false);
		}

		public void Resize(int newDimX, bool scaleHeight) {
			int dimX = newDimX;
			float ratio = GridWidth / (float)GridHeight;
			int dimY = (int)(dimX / ratio);
			dataGrid = GetResizedData(dimX, dimY);
			float resizeRatio = GridWidth / (float)GridWidth;
			if(scaleHeight) {
				for(int x = 0; x < GridWidth; x++) {
					for(int y = 0; y < GridHeight; y++) {
						dataGrid[x, y] *= resizeRatio;
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
					newData[x, y] = GetHeightInterpolated(nx * GridWidth, ny * GridHeight);
				}
			}
			return newData;
		}

		#endregion
		#region getter functions

		public float[,] GetDataGrid() {
			return dataGrid;
		}

		public float[,] GetDataGridFlipped() {
			float[,] grid = new float[GridWidth, GridHeight];
			var zLength = grid.GetLength(1);
			for(int x = 0; x < grid.GetLength(0); x++) {
				for(int z = 0; z < grid.GetLength(1); z++) {
					//Z starts from top
					grid[x, zLength - z - 1] = GetHeight(x, z);
				}
			}
			return grid;
		}

		public void SetDataGrid(float[,] newGrid) {
			dataGrid = newGrid;
		}

		public float GetHeight(int x, int y) {
			if(x < 0 || y < 0 || x >= GridWidth || y >= GridHeight) {
				return nodata_value;
			} else {
				return dataGrid[x, y];
			}
		}

		public float GetHeightInterpolated(float x, float y) {
			int x1 = (int)x;
			int y1 = (int)y;
			int x2 = x1 + 1;
			int y2 = y1 + 1;
			x1 = MathUtils.Clamp(x1, 0, GridWidth - 1);
			x2 = MathUtils.Clamp(x2, 0, GridWidth - 1);
			y1 = MathUtils.Clamp(y1, 0, GridHeight - 1);
			y2 = MathUtils.Clamp(y2, 0, GridHeight - 1);
			float wx = x - x1;
			float wy = y - y1;
			float vx1 = MathUtils.Lerp(GetHeight(x1, y1), GetHeight(x2, y1), wx);
			float vx2 = MathUtils.Lerp(GetHeight(x1, y2), GetHeight(x2, y2), wx);
			return MathUtils.Lerp(vx1, vx2, wy);
		}

		public float GetHeightRelative(float rx, float ry) {
			return GetHeightInterpolated(rx * (GridWidth - 1), ry * (GridHeight - 1));
		}

		public float[,] GetDataRange(Bounds bounds) {
			float[,] newdata = new float[bounds.NumCols, bounds.NumRows];
			for(int x = 0; x < bounds.NumCols; x++) {
				for(int y = 0; y < bounds.NumRows; y++) {
					newdata[x, y] = dataGrid[bounds.xMin + x, bounds.yMin + y];
				}
			}
			return newdata;
		}

		#endregion
	}
}