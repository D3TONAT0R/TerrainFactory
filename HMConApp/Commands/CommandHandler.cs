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
			//Remove illegal commands
			List<ConsoleCommand> rm = new List<ConsoleCommand>();
			foreach(var c in list) {
				if(c.commandHandler == null) {
					Program.WriteError($"CommandHandler for command '{c.command}' is null. The command has been removed.");
					rm.Add(c);
				}
			}
			foreach(var c in rm) list.Remove(c);
			return list;
		}
	}
}
