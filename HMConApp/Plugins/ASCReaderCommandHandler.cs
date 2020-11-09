using ASCReader.Export;
using ASCReader.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASCReader {
	public abstract class ASCReaderCommandHandler {

		public abstract void AddCommands(List<ConsoleCommand> list);

		public abstract void HandleCommand(string cmd, string[] args, ExportOptions options, ASCData data);
	}
}
