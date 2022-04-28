using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {
	public class PluginInfoAttribute : Attribute {

		public string Name {
			get; private set;
		}
		public string ID {
			get; private set;
		}

		public PluginInfoAttribute(string ID, string pluginName) {
			this.ID = ID;
			Name = pluginName;
		}
	}
}
