using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Modification {
	public class LowHighScaleModifier : Modifier {

		[DrawInInspector("New low")]
		public float newLow;
		[DrawInInspector("New high")]
		public float newHigh;

		public float? srcLow;
		public float? srcHigh;

		public LowHighScaleModifier(float? sourceLow, float? sourceHigh, float targetLow, float targetHigh)
		{
			srcLow = sourceLow;
			srcHigh = sourceHigh;
			newLow = targetLow;
			newHigh = targetHigh;
		}

		protected override void ModifyData(HeightData data) {

			float lowPoint = srcLow ?? data.lowPoint;
			float highPoint = srcHigh ?? data.highPoint;
			float oldRange = highPoint - lowPoint;
			float newRange = newHigh - newLow;

			for(int y = 0; y < data.GridHeight; y++) {
				for(int x = 0; x < data.GridWidth; x++) {
					var value = data.GetHeight(x, y);
					double h = (value - lowPoint) / oldRange;
					h *= newRange;
					h += newLow;
					value = (float)h;
					data.SetHeight(x, y, value);
				}
			}

			data.lowPoint = lowPoint;
			data.highPoint = highPoint;
			data.RecalculateValues(false);
		}
	}
}
