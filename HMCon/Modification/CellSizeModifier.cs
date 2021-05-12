using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Modification {
	public class CellSizeModifier : Modifier {

		[DrawInInspector("New cell size")]
		public float newCellSize;

		public CellSizeModifier(float newSize)
		{
			newCellSize = newSize;
		}

		protected override void ModifyData(HeightData data) {
			data.cellSize = newCellSize;
		}
	}
}
