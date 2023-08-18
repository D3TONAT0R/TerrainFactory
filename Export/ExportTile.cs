using System;

namespace TerrainFactory.Export
{
	public class ExportTile
	{
		public int xIndex;
		public int yIndex;

		public HeightData data;

		public bool HasMultiple => xIndex >= 0 || yIndex >= 0;

		private ExportTile(int xIndex, int yIndex, HeightData data)
		{
			this.xIndex = xIndex;
			this.yIndex = yIndex;
			this.data = data;
		}

		public static ExportTile CreateFullTile(HeightData data)
		{
			return new ExportTile(-1, -1, data);
		}

		public static ExportTile GetTile(HeightData source, int splitInterval, int xIndex, int yIndex)
		{
			int sx = Math.Min(source.GridLengthX - xIndex * splitInterval, splitInterval);
			int sy = Math.Min(source.GridLengthY - yIndex * splitInterval, splitInterval);
			HeightData data = new HeightData(source, new float[sx, sy]);
			for(int x = 0; x < sx; x++)
			{
				for(int y = 0; y < sy; y++)
				{
					data.SetHeight(x, y, source.GetHeight(xIndex * splitInterval + x, yIndex * splitInterval + y));
				}
			}
			data.offsetFromSource.x += xIndex * splitInterval;
			data.offsetFromSource.y += yIndex * splitInterval;
			data.lowerCornerPos += new System.Numerics.Vector2(xIndex * splitInterval, yIndex * splitInterval) * data.cellSize;
			data.RecalculateValues(false);
			return new ExportTile(xIndex, yIndex, data);
		}

		public static void CalcTileCount(HeightData data, int splitInterval, out int xCount, out int yCount)
		{
			if(splitInterval > 2)
			{
				xCount = (int)Math.Ceiling(data.GridLengthX / (float)splitInterval);
				yCount = (int)Math.Ceiling(data.GridLengthY / (float)splitInterval);
			}
			else
			{
				xCount = 1;
				yCount = 1;
			}
		}
	}
}
