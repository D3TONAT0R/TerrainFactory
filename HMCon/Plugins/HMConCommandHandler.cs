using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {
	public abstract class HMConCommandHandler {

		public abstract void AddCommands(List<ConsoleCommand> list);

		public abstract void HandleCommand(string cmd, string[] args, ExportSettings options, ASCData data);
	}
}
