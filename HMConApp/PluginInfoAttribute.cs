using System;
using System.Collections.Generic;
using System.Text;

namespace ASCReader {
	public class PluginInfoAttribute : Attribute {

		public string Name {
			get; private set;
		}

		public PluginInfoAttribute(string pluginName) {
			Name = pluginName;
		}
	}
}
