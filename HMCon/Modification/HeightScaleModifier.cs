using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Modification {
	public class HeightScaleModifier : Modifier {

		[DrawInInspector("Scale")]
		public float scaleMultiplier;
		[DrawInInspector("Scaling pivot")]
		public float scalePivot;

		public HeightScaleModifier(float scale) : this(0, scale) {

		}

		public HeightScaleModifier(float pivot, float scale)
		{
			scalePivot = pivot;
			scaleMultiplier = scale;
		}

		protected override void ModifyData(HeightData data) {
			for(int y = 0; y < data.GridHeight; y++) {
				for(int x = 0; x < data.GridWidth; x++) {
					var value = data.GetHeight(x, y);
					value = (value - scalePivot) * scaleMultiplier + scalePivot;
					data.SetHeight(x, y, value);
				}
			}
			data.RecalculateValues(true);
		}
	}
}
