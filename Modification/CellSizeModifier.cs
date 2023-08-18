using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modification {
	public class CellSizeModifier : Modifier {

		[DrawInInspector("New cell size")]
		public float newCellSize = 1;

		public CellSizeModifier()
		{

		}

		public CellSizeModifier(float newSize)
		{
			newCellSize = newSize;
		}

		protected override void ModifyData(HeightData data) {
			data.cellSize = newCellSize;
		}
	}
}
