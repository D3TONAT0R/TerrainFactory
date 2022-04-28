using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Modification {
	public class ClippingModifier : Modifier {

		[DrawInInspector("Minimum")]
		public float minimum;
		[DrawInInspector("Maximum")]
		public float maximum;

		public ClippingModifier(float min, float max)
		{
			minimum = min;
			maximum = max;
		}

		protected override void ModifyData(HeightData data) {
			data.Modify((x, y, rx, ry, v) => {
				return MathUtils.Clamp(v, minimum, maximum);
			});
		}
	}
}
