using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Util
{
	public class DrawInInspectorAttribute : Attribute
	{
		public string label;
		public bool editable = true;

		public DrawInInspectorAttribute(string label, bool editable = true)
		{
			this.label = label;
			this.editable = editable;
		}
	}
}
