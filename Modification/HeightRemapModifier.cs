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

		protected override void ModifyData(ElevationData data) {
			for(int y = 0; y < data.CellCountY; y++) {
				for(int x = 0; x < data.CellCountX; x++) {
					var value = data.GetElevationAtCell(x, y);
					value = MathUtils.Remap(value, oldA, oldB, newA, newB);
					data.SetHeightAt(x, y, value);
				}
			}
			if(data.CustomBlackPoint.HasValue) data.CustomBlackPoint = MathUtils.Remap(data.CustomBlackPoint.Value, oldA, oldB, newA, newB);
			if(data.CustomWhitePoint.HasValue) data.CustomWhitePoint = MathUtils.Remap(data.CustomWhitePoint.Value, oldA, oldB, newA, newB);
			data.RecalculateElevationRange(false);
		}
	}
}
