using ASCReader.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASCReader {
	static class CommandHandler {

		public static List<ASCReaderCommandHandler> commandHandlers = new List<ASCReaderCommandHandler>();

		public static List<ConsoleCommand> GetConsoleCommands() {
			List<ConsoleCommand> list = new List<ConsoleCommand>();
			foreach(var ex in commandHandlers) {
				ex.AddCommands(list);
			}
			return list;
		}
	}
}
