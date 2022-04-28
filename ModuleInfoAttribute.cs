using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {
	public class ModuleInfoAttribute : Attribute {

		public string Name {
			get; private set;
		}
		public string ID {
			get; private set;
		}

		public ModuleInfoAttribute(string ID, string moduleName) {
			this.ID = ID;
			Name = moduleName;
		}
	}
}
