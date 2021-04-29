using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {
	public static class CommandHandler {

		public static List<HMConCommandHandler> commandHandlers = new List<HMConCommandHandler>();

		public static List<ConsoleCommand> GetConsoleCommands() {
			List<ConsoleCommand> list = new List<ConsoleCommand>();
			foreach(var ex in commandHandlers) {
				ex.AddCommands(list);
			}
			//Remove illegal commands
			List<ConsoleCommand> rm = new List<ConsoleCommand>();
			foreach(var c in list) {
				if(c.commandHandler == null) {
					ConsoleOutput.WriteError($"CommandHandler for command '{c.command}' is null. The command has been removed.");
					rm.Add(c);
				}
			}
			foreach(var c in rm) list.Remove(c);
			return list;
		}
	}
}
