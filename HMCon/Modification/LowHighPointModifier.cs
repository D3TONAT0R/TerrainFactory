using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Modification {
	public class LowHighPointModifier : Modifier {

		[DrawInInspector("New low")]
		public float newLow;
		[DrawInInspector("New high")]
		public float newHigh;

		public LowHighPointModifier(float low, float high) {
			newLow = low;
			newHigh = high;
		}

		protected override void ModifyData(HeightData data) {
			if(newLow == 0 && newHigh == 0) {
				data.RecalculateValues(true);
			} else {
				data.lowPoint = newLow;
				data.highPoint = newHigh;
				data.RecalculateValues(false);
			}
		}
	}
}
