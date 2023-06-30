using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Modification {
	public abstract class Modifier : ICloneable {

		public Modifier()
		{
			
		}

		public string sourceCommandString;

		public virtual string Name => GetType().Name;

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

		public string VerboseOutput()
		{
			StringBuilder sb = new StringBuilder();
			foreach(var t in GetType().GetFields())
			{
				var attr = t.GetCustomAttributes(typeof(DrawInInspectorAttribute), true);
				if (attr.Length > 0)
				{
					var a = (DrawInInspectorAttribute)attr[0];
					if (sb.Length > 0) sb.Append(", ");
					sb.Append(a.label+"="+t.GetValue(this));
				}
			}
			return sb.ToString();
		}
	}
}
