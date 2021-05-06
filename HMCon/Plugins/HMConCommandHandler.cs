using HMCon.Export;
using HMCon.Modification;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {
	public abstract class HMConCommandHandler {

		public virtual void AddCommands(List<ConsoleCommand> list) {

		}

		/*public virtual void HandleCommand(string cmd, string[] args, ExportSettings settings, HeightData data) {

		}*/

		public virtual void AddModifiers(List<ModificationCommand> list) {

		}

		/*public virtual Modifier HandleModifierCommand(string name, string[] args, HeightData data) {
			return null;
		}*/
	}
}
