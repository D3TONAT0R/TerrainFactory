using HMCon.Export;

namespace HMCon.Util {
	public class ConsoleCommand {

		public string command;
		public string argsHint;
		public string description;
		public HMConCommandHandler commandHandler;

		public ConsoleCommand(string cmd, string argHint, string desc, HMConCommandHandler handler) {
			command = cmd;
			argsHint = argHint;
			description = desc;
			commandHandler = handler;
		}
	}
}
