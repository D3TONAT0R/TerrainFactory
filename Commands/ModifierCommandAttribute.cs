using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Commands
{
	public class ModifierCommandAttribute : Attribute
	{
		public string commandName;
		public string args;
		public string desc;

		public ModifierCommandAttribute(string commandName, string args, string desc)
		{
			this.commandName = commandName;
			this.args = args;
			this.desc = desc;
		}
	}
}
