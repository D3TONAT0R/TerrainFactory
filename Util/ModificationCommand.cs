using HMCon.Export;
using HMCon.Modification;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Util {
	public class ModificationCommand : ConsoleCommand {

		public delegate Modifier HandleModCommandDelegate(Worksheet sheet, string[] args);

		private HandleModCommandDelegate handler;

		private Modifier template;

		public ModificationCommand(string cmd, string argHint, string desc, HandleModCommandDelegate del, Modifier templateModifier)
			: base(cmd, argHint, desc, null) {
			handler = del;
			template = templateModifier;
		}

		public new Modifier ExecuteCommand(Worksheet sheet, string[] args) {
			var mod = handler(sheet, args);
			//TODO: needs major refactoring for this to work
			/*
			string cmdString = mod.sourceCommand.command;
			foreach(var arg in args)
			{
				cmdString += " " + arg;
			}
			mod.sourceCommandString = cmdString;
			*/
			return mod;
		}

		public Modifier CreateModifier()
		{
			return (Modifier)template.Clone();
		}
	}
}
