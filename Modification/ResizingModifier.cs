using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modification {
	public class ResizingModifier : Modifier {

		[DrawInInspector("New width")]
		public int newWidth = 100;
		[DrawInInspector("Adjust height")]
		public bool adjustHeight = false;

		public ResizingModifier()
		{

		}

		public ResizingModifier(int newDimX, bool scaleHeight)
		{
			newWidth = newDimX;
			adjustHeight = scaleHeight;
		}

		protected override void ModifyData(ElevationData data) {
			if(newWidth > 0) data.Resample(newWidth, adjustHeight);
		}
	}
}
