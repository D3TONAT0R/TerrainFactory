using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modification {
	public class LowHighScaleModifier : Modifier {

		[DrawInInspector("New low")]
		public float newLow;
		[DrawInInspector("New high")]
		public float newHigh;

		public float? srcLow;
		public float? srcHigh;

		public LowHighScaleModifier()
		{

		}

		public LowHighScaleModifier(float? sourceLow, float? sourceHigh, float targetLow, float targetHigh)
		{
			srcLow = sourceLow;
			srcHigh = sourceHigh;
			newLow = targetLow;
			newHigh = targetHigh;
		}

		protected override void ModifyData(ElevationData data) {

			float lowPoint = srcLow ?? data.GrayscaleRange.Min;
			float highPoint = srcHigh ?? data.GrayscaleRange.Max;
			float oldRange = highPoint - lowPoint;
			float newRange = newHigh - newLow;

			for(int y = 0; y < data.CellCountY; y++) {
				for(int x = 0; x < data.CellCountX; x++) {
					var value = data.GetElevationAtCell(x, y);
					double h = (value - lowPoint) / oldRange;
					h *= newRange;
					h += newLow;
					value = (float)h;
					data.SetHeightAt(x, y, value);
				}
			}

			data.CustomBlackPoint = lowPoint;
			data.CustomWhitePoint = highPoint;
			data.RecalculateElevationRange(false);
		}
	}
}
