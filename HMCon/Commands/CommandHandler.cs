using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {
	public static class CommandHandler {

		public static List<HMConCommandHandler> commandHandlers = new List<HMConCommandHandler>();

		public static List<ConsoleCommand> ConsoleCommands { get; private set; }
		public static List<ModificationCommand> ModificationCommands { get; private set; }

		public static void Initialize() {
			ConsoleCommands = new List<ConsoleCommand>();
			foreach(var ex in commandHandlers) {
				ex.AddCommands(ConsoleCommands);
			}
			ModificationCommands = new List<ModificationCommand>();
			foreach(var ex in commandHandlers) {
				ex.AddModifiers(ModificationCommands);
			}
		}
	}
}
