using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modification {
	public class HeightScaleModifier : Modifier {

		[DrawInInspector("Scale")]
		public float scaleMultiplier = 1;
		[DrawInInspector("Scaling pivot")]
		public float scalePivot = 0;

		public HeightScaleModifier()
		{

		}

		public HeightScaleModifier(float scale) : this(0, scale) {

		}

		public HeightScaleModifier(float pivot, float scale)
		{
			scalePivot = pivot;
			scaleMultiplier = scale;
		}

		protected override void ModifyData(ElevationData data) {
			for(int y = 0; y < data.CellCountY; y++) {
				for(int x = 0; x < data.CellCountX; x++) {
					var value = data.GetElevationAtCell(x, y);
					value = (value - scalePivot) * scaleMultiplier + scalePivot;
					data.SetHeightAt(x, y, value);
				}
			}
			data.RecalculateElevationRange(true);
		}
	}
}
