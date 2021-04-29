using HMCon.Export;
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

		public HeightData(HeightData original) {
			filename = original.filename;
			lowerCornerPos = original.lowerCornerPos;
			cellSize = original.cellSize;
			nodata_value = original.nodata_value;
			//fileHeader = original.fileHeader;
			dataGrid = (float[,])original.dataGrid.Clone();
			RecalculateValues(false);
			lowPoint = original.lowPoint;
			highPoint = original.highPoint;
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

		public bool WriteAllFiles(string path, ExportSettings options) {
			int rangeMinX = 0;
			int rangeMinY = 0;
			int rangeMaxX = GridWidth - 1;
			int rangeMaxY = GridHeight - 1;
			if(options.UseExportRange) {
				rangeMinX = options.exportRange.xMin;
				rangeMinY = options.exportRange.yMin;
				rangeMaxX = options.exportRange.xMax;
				rangeMaxY = options.exportRange.yMax;
			}
			string dir = Path.GetDirectoryName(path);
			CurrentExportJobInfo.mcaGlobalPosX = options.mcaOffsetX;
			CurrentExportJobInfo.mcaGlobalPosZ = options.mcaOffsetZ;
			if(Directory.Exists(dir)) {
				if(options.fileSplitDims < 32) {
					CurrentExportJobInfo.bounds = new Bounds(rangeMinX, rangeMinY, rangeMaxX, rangeMaxY);
					CurrentExportJobInfo.exportSettings = options;
					ExportUtility.CreateFilesForSection(this, dir, path);
				} else {
					int dims = options.fileSplitDims;
					int yMin = rangeMinY;
					int fileY = 0;
					while(yMin + dims <= rangeMaxY + 1) {
						int xMin = rangeMinX;
						int fileX = 0;
						int yMax = Math.Min(yMin + dims, GridHeight);
						while(xMin + dims <= rangeMaxX + 1) {
							int xMax = Math.Min(xMin + dims, GridWidth);
							CurrentExportJobInfo.exportNumX = fileX;
							CurrentExportJobInfo.exportNumZ = fileY;
							CurrentExportJobInfo.bounds = new Bounds(xMin, yMin, xMax - 1, yMax - 1);
							CurrentExportJobInfo.exportSettings = options;
							bool success = ExportUtility.CreateFilesForSection(this, filename, path);
							if(!success) throw new IOException("Failed to write file!");
							xMin += dims;
							xMin = Math.Min(xMin, GridWidth);
							fileX++;
						}
						yMin += dims;
						yMin = Math.Min(yMin, GridHeight);
						fileY++;
					}
				}
				return true;
			} else {
				ConsoleOutput.WriteError("Directory " + dir + " does not exist!");
				return false;
			}
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

		public HeightData Resize(int newDimX, bool scaleHeight) {
			HeightData newData = new HeightData(this);
			int dimX = newDimX;
			float ratio = GridWidth / (float)GridHeight;
			int dimY = (int)(dimX / ratio);
			newData.dataGrid = GetResizedData(dimX, dimY);
			float resizeRatio = newData.GridWidth / (float)GridWidth;
			if(scaleHeight) {
				for(int x = 0; x < newData.GridWidth; x++) {
					for(int y = 0; y < newData.GridHeight; y++) {
						newData.dataGrid[x, y] *= resizeRatio;
					}
				}
			}
			newData.cellSize = resizeRatio;
			return newData;
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