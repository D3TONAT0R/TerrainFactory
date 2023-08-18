using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Commands
{
	public class CommandAttribute : Attribute
	{
		[Flags]
		public enum ContextFlags
		{
			BeforeImport = 0b01,
			AfterImport = 0b10,
			Global = 0b11
		}

		public string commandName;
		public string args;
		public string desc;
		public ContextFlags context;

		public bool hidden = false;

		public CommandAttribute(string commandName, string args, string desc, ContextFlags context)
		{
			this.commandName = commandName;
			this.args = args;
			this.desc = desc;
			this.context = context;
		}

		public CommandAttribute(string commandName, string args, string desc) : this(commandName, args, desc, ContextFlags.AfterImport)
		{

		}
	}
}
