using ASCReader.Export;

namespace ASCReader.Util {
	public class ConsoleCommand {

		public string command;
		public string argsHint;
		public string description;
		public ASCReaderExportHandler commandHandler;

		public ConsoleCommand(string cmd, string argHint, string desc, ASCReaderExportHandler handler) {
			command = cmd;
			argsHint = argHint;
			description = desc;
			commandHandler = handler;
		}
	}
}
