using HMCon.Export;
using HMCon.Modification;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Util {
	public class ModificationCommand : ConsoleCommand {

		public delegate Modifier HandleModCommandDelegate(Job job, string[] args);

		private HandleModCommandDelegate handler;

		private Modifier template;

		public ModificationCommand(string cmd, string argHint, string desc, HandleModCommandDelegate del, Modifier templateModifier)
			: base(cmd, argHint, desc, null) {
			handler = del;
			template = templateModifier;
		}

		public new Modifier ExecuteCommand(Job job, string[] args) {
			
			return handler(job, args);
		}

		public Modifier CreateModifier()
		{
			return (Modifier)template.Clone();
		}
	}
}
