using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Modification {
	public abstract class Modifier : ICloneable {

		public Modifier()
		{

		}

		protected abstract void ModifyData(HeightData data);

		public HeightData Modify(HeightData inputData, bool keepOriginal) {
			HeightData data = keepOriginal ? new HeightData(inputData) : inputData;
			ModifyData(data);
			return data;
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}
	}
}
