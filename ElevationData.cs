using System;
using System.Numerics;
using TerrainFactory.Util;

namespace TerrainFactory
{

	public class ElevationData
	{
		public const float NODATA_VALUE = float.NaN;

		/// <summary>
		/// The path to the file this data originated from. Will be null if this data was created from scratch.
		/// </summary>
		public string SourceFileName { get; set; }

		/// <summary>
		/// The number of cells in the horizontal (X) direction.
		/// </summary>
		public int CellCountX => data.GetLength(0);
		/// <summary>
		/// The number of cells in the vertical (Y) direction.
		/// </summary>
		public int CellCountY => data.GetLength(1);
		/// <summary>
		/// The aspect ratio of the data grid (X/Y).
		/// </summary>
		public float CellAspectRatio => CellCountX / (float)CellCountY;
		/// <summary>
		/// The total number of cells in the data grid.
		/// </summary>
		public int TotalCellCount => CellCountX * CellCountY;

		/// <summary>
		/// The size of each cell in the data grid, in world units.
		/// </summary>
		public float CellSize { get; set; } = 1f;

		/// <summary>
		/// The world position of the lower left corner of the data grid.
		/// </summary>
		public Vector2 LowerCornerPosition { get; set; }

		/// <summary>
		/// The world position of the upper right corner of the data grid.
		/// </summary>
		public Vector2 UpperCornerPosition => LowerCornerPosition + Dimensions;

		/// <summary>
		/// The world position of the center of the data grid.
		/// </summary>
		public Vector2 CenterPosition => LowerCornerPosition + Dimensions * 0.5f;

		/// <summary>
		/// The size of the data grid in world units.
		/// </summary>
		public Vector2 Dimensions => new Vector2(CellCountX * CellSize, CellCountY * CellSize);

		/// <summary>
		/// The total square area of the data grid in world units.
		/// </summary>
		public float TotalArea => Dimensions.X * Dimensions.Y;

		private float[,] data;

		//TODO: Refactor for better clarity
		public (int x, int y) offsetFromSource = (0, 0);

		/// <summary>
		/// The lowest elevation value present in the data grid.
		/// </summary>
		public float MinElevation { get; private set; } = float.PositiveInfinity;
		/// <summary>
		/// The highest elevation value present in the data grid.
		/// </summary>
		public float MaxElevation { get; private set; } = float.NegativeInfinity;

		//Used for scaling operations. In data created from an image, these values represent the black and white values of the source image (default 0 and 1 respectively)
		//In data created from ASC data itself, these are equal to lowestValue and highestValue unless overridden for heightmap export.

		/// <summary>
		/// Custom elevation value to represent black in a grayscale heightmap. If null, the lowest elevation value in the data grid will be used.
		/// </summary>
		public float? CustomBlackPoint { get; set; }
		/// <summary>
		/// Custom elevation value to represent white in a grayscale heightmap. If null, the highest elevation value in the data grid will be used.
		/// </summary>
		public float? CustomWhitePoint { get; set; }

		/// <summary>
		/// The elevations that should be used to represent black and white in a grayscale heightmap.
		/// </summary>
		public Range GrayscaleRange => new Range(CustomBlackPoint ?? MinElevation, CustomWhitePoint ?? MaxElevation);

		public bool WasModified { get; private set; } = false;

		public ElevationData()
		{

		}

		public ElevationData(float[,] data, float cellSize)
		{
			this.data = data;
			CellSize = cellSize;
		}

		public ElevationData(int cellCountX, int cellCountY, string sourceFileName = null)
		{
			SourceFileName = sourceFileName;
			data = new float[cellCountX, cellCountY];
		}

		public ElevationData(ElevationData original)
		{
			data = (float[,])original.data.Clone();
			original.CopyAllPropertiesTo(this);
		}

		public static bool IsNoData(float value)
		{
			return float.IsNaN(value);
		}

		public void RecalculateElevationRange(bool clearLowHighPoints)
		{
			MinElevation = float.MaxValue;
			MaxElevation = float.MinValue;
			foreach(float e in data)
			{
				if(!float.IsNaN(e))
				{
					if(e < MinElevation) MinElevation = e;
					if(e > MaxElevation) MaxElevation = e;
				}
			}
			if(clearLowHighPoints)
			{
				CustomBlackPoint = null;
				CustomWhitePoint = null;
			}
		}

		public void CopyAllPropertiesTo(ElevationData other)
		{
			other.CellSize = CellSize;
			other.LowerCornerPosition = LowerCornerPosition;
			other.CustomBlackPoint = CustomBlackPoint;
			other.CustomWhitePoint = CustomWhitePoint;
			other.WasModified = true;
			other.offsetFromSource = offsetFromSource;
			RecalculateElevationRange(false);
		}

		public Bounds GetBounds()
		{
			return new Bounds(0, 0, CellCountX - 1, CellCountY - 1);
		}

		#region modification

		public void SetHeightAt(int x, int y, float value)
		{
			data[x, y] = value;
			WasModified = true;
		}

		public void AddHeightAt(int x, int y, float add)
		{
			data[x, y] += add;
			WasModified = true;
		}

		public void Add(ElevationData other)
		{
			Modify((x, y, rx, ry, v) => v + other.GetElevationAtNormalizedPos(rx, ry));
		}

		public void Multiply(ElevationData other)
		{
			Modify((x, y, rx, ry, v) => v * other.GetElevationAtNormalizedPos(rx, ry));
		}

		public void RemapHeights(float fromMin, float fromMax, float toMin, float toMax)
		{
			Modify((x, y, rx, ry, height) => MathUtils.Remap(height, fromMin, fromMax, toMin, toMax));
			if(CustomBlackPoint.HasValue) CustomBlackPoint = MathUtils.Remap(CustomBlackPoint.Value, fromMin, fromMax, toMin, toMax);
			if(CustomWhitePoint.HasValue) CustomWhitePoint = MathUtils.Remap(CustomWhitePoint.Value, fromMin, fromMax, toMin, toMax);
		}

		public delegate float ModificationFunc(int x, int y, float rx, float ry, float value);

		public void Modify(ModificationFunc modificator)
		{
			for(int y = 0; y < CellCountY; y++)
			{
				for(int x = 0; x < CellCountX; x++)
				{
					float rx = x / (float)(CellCountX - 1);
					float ry = y / (float)(CellCountY - 1);
					data[x, y] = modificator(x, y, rx, ry, data[x, y]);
				}
			}
			RecalculateElevationRange(false);
			WasModified = true;
		}

		public void Resample(int newCellCountX, bool scaleHeight)
		{
			float resampleRatio = newCellCountX / (float)CellCountX;
			int newCellCountY = (int)Math.Round(CellCountY * resampleRatio, 0, MidpointRounding.AwayFromZero);

			float[,] newData = new float[newCellCountX, newCellCountY];
			for(int x = 0; x < newCellCountX; x++)
			{
				for(int y = 0; y < newCellCountY; y++)
				{
					newData[x, y] = GetElevationAtCellInterpolated(x / resampleRatio, y / resampleRatio);
				}
			}

			ReplaceData(newData);
			CellSize /= resampleRatio;
			RecalculateElevationRange(false);
			WasModified = true;
		}

		public void ScaleHeight(float scale)
		{
			Modify((x,y,rx,ry,h) => h *= scale);
		}

		public void ReplaceData(float[,] newData)
		{
			data = newData;
			WasModified = true;
		}

		public void ClearModifiedFlag() => WasModified = false;

		#endregion

		#region getter functions

		public float[,] GetDataGrid()
		{
			return data;
		}

		public float[,] GetDataGridYFlipped()
		{
			float[,] grid = new float[CellCountX, CellCountY];
			var zLength = grid.GetLength(1);
			for(int x = 0; x < grid.GetLength(0); x++)
			{
				for(int y = 0; y < grid.GetLength(1); y++)
				{
					//Y starts from top
					grid[x, zLength - y - 1] = GetElevationAtCell(x, y);
				}
			}
			return grid;
		}

		public float GetElevationAtCell(int x, int y)
		{
			if(x < 0 || y < 0 || x >= CellCountX || y >= CellCountY)
			{
				return NODATA_VALUE;
			}
			else
			{
				return data[x, y];
			}
		}

		public float GetElevationAtCellUnchecked(int x, int y)
		{
			return data[x, y];
		}

		public float GetElevationAtCellClamped(int x, int y)
		{
			x = MathUtils.Clamp(x, 0, CellCountX - 1);
			y = MathUtils.Clamp(y, 0, CellCountY - 1);
			return data[x, y];
		}

		public float GetElevationAtCellInterpolated(float x, float y)
		{
			int x1 = (int)x;
			int y1 = (int)y;
			int x2 = x1 + 1;
			int y2 = y1 + 1;
			x1 = MathUtils.Clamp(x1, 0, CellCountX - 1);
			x2 = MathUtils.Clamp(x2, 0, CellCountX - 1);
			y1 = MathUtils.Clamp(y1, 0, CellCountY - 1);
			y2 = MathUtils.Clamp(y2, 0, CellCountY - 1);
			float wx = x - x1;
			float wy = y - y1;
			float vx1 = LerpElevation(GetElevationAtCell(x1, y1), GetElevationAtCell(x2, y1), wx);
			float vx2 = LerpElevation(GetElevationAtCell(x1, y2), GetElevationAtCell(x2, y2), wx);
			return LerpElevation(vx1, vx2, wy);
		}

		public float GetElevationAtNormalizedPos(float rx, float ry)
		{
			return GetElevationAtCellInterpolated(rx * (CellCountX - 1), ry * (CellCountY - 1));
		}

		public bool IsNodataAtCell(int x, int y)
		{
			return IsNoData(GetElevationAtCell(x, y));
		}

		public float[,] GetCellRange(Bounds bounds)
		{
			float[,] newdata = new float[bounds.NumCols, bounds.NumRows];
			for(int x = 0; x < bounds.NumCols; x++)
			{
				for(int y = 0; y < bounds.NumRows; y++)
				{
					newdata[x, y] = data[bounds.xMin + x, bounds.yMin + y];
				}
			}
			return newdata;
		}

		private static float LerpElevation(float a, float b, float t)
		{
			bool aIsNaN = float.IsNaN(a);
			bool bIsNaN = float.IsNaN(b);
			if(aIsNaN && bIsNaN) return NODATA_VALUE;
			if(bIsNaN && t <= 0.5f) return a;
			if(aIsNaN && t > 0.5f) return b;
			return MathUtils.Lerp(a, b, t);
		}

		#endregion
	}
}