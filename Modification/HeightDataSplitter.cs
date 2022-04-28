using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Modification {
	public class HeightDataSplitter {

		private readonly HeightData sourceData;
		private readonly int intervalX;
		private readonly int intervalY;

		public int NumDataX => (int)Math.Ceiling(sourceData.GridWidth / (float)intervalX);
		public int NumDataY => (int)Math.Ceiling(sourceData.GridWidth / (float)intervalY);

		public int NumChunks => NumDataX * NumDataY;

		public HeightDataSplitter(HeightData data, int splitIntervalX, int splitIntervalY) {
			intervalX = splitIntervalX;
			intervalY = splitIntervalY;
			if(intervalX < 2) intervalX = int.MaxValue;
			if(intervalY < 2) intervalY = int.MaxValue;
			sourceData = data;
		}

		public HeightDataSplitter(HeightData data, int splitInterval) : this(data, splitInterval, splitInterval) {

		}

		public HeightData GetDataChunk(int indexX, int indexY) {
			int sx = Math.Min(sourceData.GridWidth - indexX * intervalX, intervalX);
			int sy = Math.Min(sourceData.GridHeight - indexY * intervalY, intervalY);
			HeightData data = new HeightData(sourceData, new float[sx, sy]);
			for(int x = 0; x < sx; x++) {
				for(int y = 0; y < sy; y++) {
					data.SetHeight(x, y, sourceData.GetHeight(indexX * intervalX + x, indexY * intervalY + y));
				}
			}
			data.offsetFromSource.x += indexX * intervalX;
			data.offsetFromSource.y += indexY * intervalY;
			data.lowerCornerPos += new System.Numerics.Vector2(indexX * intervalX, indexY * intervalY) * data.cellSize;
			data.RecalculateValues(false);
			return data;
		}
	}
}
