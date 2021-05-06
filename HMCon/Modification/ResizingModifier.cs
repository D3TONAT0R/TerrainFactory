using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Modification {
	public class ResizingModifier : Modifier {

		[DrawInInspector("New width")]
		public int newWidth;
		[DrawInInspector("Adjust height")]
		public bool adjustHeight;

		public ResizingModifier(int newDimX, bool scaleHeight) {
			newWidth = newDimX;
			adjustHeight = scaleHeight;
		}

		protected override void ModifyData(HeightData data) {
			if(newWidth > 0) data.Resize(newWidth, adjustHeight);
		}
	}
}
