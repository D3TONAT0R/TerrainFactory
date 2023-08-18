using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modification {
	public class HeightRemapModifier : Modifier {

		[DrawInInspector("Old A")]
		public float oldA = 0;
		[DrawInInspector("New A")]
		public float newA = 0;
		[DrawInInspector("Old B")]
		public float oldB = 1;
		[DrawInInspector("New B")]
		public float newB = 1;

		public HeightRemapModifier()
		{

		}

		public HeightRemapModifier(float oldA, float newA, float oldB, float newB)
		{
			this.oldA = oldA;
			this.newA = newA;
			this.oldB = oldB;
			this.newB = newB;
		}

		protected override void ModifyData(HeightData data) {
			for(int y = 0; y < data.GridLengthY; y++) {
				for(int x = 0; x < data.GridLengthX; x++) {
					var value = data.GetHeight(x, y);
					value = MathUtils.Remap(value, oldA, oldB, newA, newB);
					data.SetHeight(x, y, value);
				}
			}
			data.lowPoint = MathUtils.Remap(data.lowPoint, oldA, oldB, newA, newB);
			data.highPoint = MathUtils.Remap(data.highPoint, oldA, oldB, newA, newB);
			data.RecalculateValues(false);
		}
	}
}
