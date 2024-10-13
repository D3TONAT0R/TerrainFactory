using System;

namespace TerrainFactory.Export
{
	public class ExportTileInfo
	{
		public int xIndex;
		public int yIndex;

		public ElevationData data;

		public bool HasMultiple => xIndex >= 0 || yIndex >= 0;

		private ExportTileInfo(int xIndex, int yIndex, ElevationData data)
		{
			this.xIndex = xIndex;
			this.yIndex = yIndex;
			this.data = data;
		}

		public static ExportTileInfo CreateFullTile(ElevationData data)
		{
			return new ExportTileInfo(-1, -1, data);
		}

		public static ExportTileInfo GetTile(ElevationData source, int splitInterval, int xIndex, int yIndex)
		{
			int sx = Math.Min(source.CellCountX - xIndex * splitInterval, splitInterval);
			int sy = Math.Min(source.CellCountY - yIndex * splitInterval, splitInterval);
			ElevationData data = new ElevationData(sx, sy);
			source.CopyAllPropertiesTo(data);
			for(int x = 0; x < sx; x++)
			{
				for(int y = 0; y < sy; y++)
				{
					data.SetHeightAt(x, y, source.GetElevationAtCell(xIndex * splitInterval + x, yIndex * splitInterval + y));
				}
			}
			data.offsetFromSource.x += xIndex * splitInterval;
			data.offsetFromSource.y += yIndex * splitInterval;
			data.LowerCornerPosition += new System.Numerics.Vector2(xIndex * splitInterval, yIndex * splitInterval) * data.CellSize;
			data.RecalculateElevationRange(false);
			return new ExportTileInfo(xIndex, yIndex, data);
		}

		public static void CalcTileCount(ElevationData data, int splitInterval, out int xCount, out int yCount)
		{
			if(splitInterval > 2)
			{
				xCount = (int)Math.Ceiling(data.CellCountX / (float)splitInterval);
				yCount = (int)Math.Ceiling(data.CellCountY / (float)splitInterval);
			}
			else
			{
				xCount = 1;
				yCount = 1;
			}
		}
	}
}
