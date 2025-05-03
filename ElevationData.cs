using System;
using System.Numerics;
using TerrainFactory.Util;

namespace TerrainFactory
{

	public class ElevationData
	{

		public string SourceFileName { get; set; }

		private float[,] Data { get; set; }
		public bool HasElevationData => Data != null;

		public int CellCountX => Data.GetLength(0);
		public int CellCountY => Data.GetLength(1);
		public float CellAspectRatio => CellCountX / (float)CellCountY;

		public int TotalCellCount => CellCountX * CellCountY;

		public float CellSize { get; set; }
		public float NoDataValue { get; set; } = float.MinValue;


		public Vector2 LowerCornerPosition { get; set; }

		public Vector2 UpperCornerPosition => LowerCornerPosition + Size;

		public Vector2 CenterPosition => LowerCornerPosition + Size * 0.5f;

		public Vector2 Size => new Vector2(CellCountX * CellSize, CellCountY * CellSize);

		public float TotalArea => Size.X * Size.Y;


		//TODO: Refactor for better clarity
		public (int x, int y) offsetFromSource = (0, 0);


		public float MinElevation { get; private set; } = float.PositiveInfinity;
		public float MaxElevation { get; private set; } = float.NegativeInfinity;

		//Used for scaling operations. In data created from an image, these values represent the black and white values of the source image (default 0 and 1 respectively)
		//In data created from ASC data itself, these are equal to lowestValue and highestValue unless overridden for heightmap export.
		public float? OverrideLowPoint { get; set; }
		public float? OverrideHighPoint { get; set; }

		public float LowPoint => OverrideLowPoint ?? MinElevation;

		public float HighPoint => OverrideHighPoint ?? MaxElevation;

		public Range GrayscaleRange => new Range(LowPoint, HighPoint);

		public bool WasModified { get; private set; } = false;

		public ElevationData()
		{

		}

		public ElevationData(float[,] data, float cellSize)
		{
			Data = data;
			CellSize = cellSize;
		}

		public ElevationData(int cellCountX, int cellCountY, string sourceFileName = null)
		{
			SourceFileName = sourceFileName;
			Data = new float[cellCountX, cellCountY];
		}

		public ElevationData(ElevationData original)
		{
			Data = (float[,])original.Data.Clone();
			original.CopyAllPropertiesTo(this);
		}

		public void RecalculateElevationRange(bool clearLowHighPoints)
		{
			MinElevation = float.MaxValue;
			MaxElevation = float.MinValue;
			foreach(float f in Data)
			{
				if(Math.Abs(f - NoDataValue) > 0.1f)
				{
					if(f < MinElevation) MinElevation = f;
					if(f > MaxElevation) MaxElevation = f;
				}
			}
			if(clearLowHighPoints)
			{
				OverrideLowPoint = null;
				OverrideHighPoint = null;
			}
		}

		public void CopyAllPropertiesTo(ElevationData other)
		{
			other.CellSize = CellSize;
			other.NoDataValue = NoDataValue;
			other.LowerCornerPosition = LowerCornerPosition;
			other.OverrideLowPoint = OverrideLowPoint;
			other.OverrideHighPoint = OverrideHighPoint;
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
			Data[x, y] = value;
			WasModified = true;
		}

		public void AddHeightAt(int x, int y, float add)
		{
			Data[x, y] += add;
			WasModified = true;
		}

		public void Add(ElevationData other)
		{
			Modify((x, y, rx, ry, v) =>
			{
				return v + other.GetElevationAtNormalizedPos(rx, ry);
			});
		}

		public void Multiply(ElevationData other)
		{
			Modify((x, y, rx, ry, v) =>
			{
				return v * other.GetElevationAtNormalizedPos(rx, ry);
			});

		}

		public void RemapHeights(float fromMin, float fromMax, float toMin, float toMax)
		{
			Modify((x, y, rx, ry, height) =>
			{
				return MathUtils.Remap(height, fromMin, fromMax, toMin, toMax);
			});
			if(OverrideLowPoint.HasValue) OverrideLowPoint = MathUtils.Remap(OverrideLowPoint.Value, fromMin, fromMax, toMin, toMax);
			if(OverrideHighPoint.HasValue) OverrideHighPoint = MathUtils.Remap(OverrideHighPoint.Value, fromMin, fromMax, toMin, toMax);
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
					Data[x, y] = modificator(x, y, rx, ry, Data[x, y]);
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
			Data = newData;
			WasModified = true;
		}

		public void ClearModifiedFlag() => WasModified = false;

		#endregion

		#region getter functions

		public float[,] GetDataGrid()
		{
			return Data;
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
				return NoDataValue;
			}
			else
			{
				return Data[x, y];
			}
		}

		public float GetElevationAtCellUnchecked(int x, int y)
		{
			return Data[x, y];
		}

		public float GetElevationAtCellClamped(int x, int y)
		{
			x = MathUtils.Clamp(x, 0, CellCountX - 1);
			y = MathUtils.Clamp(y, 0, CellCountY - 1);
			return Data[x, y];
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
			float vx1 = MathUtils.Lerp(GetElevationAtCell(x1, y1), GetElevationAtCell(x2, y1), wx);
			float vx2 = MathUtils.Lerp(GetElevationAtCell(x1, y2), GetElevationAtCell(x2, y2), wx);
			return MathUtils.Lerp(vx1, vx2, wy);
		}

		public float GetElevationAtNormalizedPos(float rx, float ry)
		{
			return GetElevationAtCellInterpolated(rx * (CellCountX - 1), ry * (CellCountY - 1));
		}

		public float[,] GetCellRange(Bounds bounds)
		{
			float[,] newdata = new float[bounds.NumCols, bounds.NumRows];
			for(int x = 0; x < bounds.NumCols; x++)
			{
				for(int y = 0; y < bounds.NumRows; y++)
				{
					newdata[x, y] = Data[bounds.xMin + x, bounds.yMin + y];
				}
			}
			return newdata;
		}

		#endregion
	}
}