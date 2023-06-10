using HMCon.Export;
using HMCon.Modification;
using System;

namespace HMCon.Util {
	public class ConsoleCommand {

		public string command;
		public string argsHint;
		public string description;
		private HandleCommandDelegate commandHandler;

		public delegate bool HandleCommandDelegate(Worksheet job, string[] args);

		public ConsoleCommand(string cmd, string argHint, string desc, HandleCommandDelegate handler) {
			command = cmd;
			argsHint = argHint;
			description = desc;
			commandHandler = handler;
		}

		public bool ExecuteCommand(Worksheet job, string[] args) {
			return commandHandler(job, args);
		}

		public static T ParseArg<T>(string[] args, int i) {
			if(i >= args.Length) {
				throw new ArgumentException("Not enough arguments for command");
			}
			try {
				if(typeof(T) == typeof(Coordinate))
				{
					return (T)Convert.ChangeType(Coordinate.Parse(args[i]), typeof(T));
				}
				return (T)Convert.ChangeType(args[i], typeof(T));
			}
			catch(Exception e) {
				throw new ArgumentException($"Failed to parse argument {i} to {typeof(T).Name}", e);
			}
		}

		public static bool ParseArgOptional<T>(string[] args, int i, out T result) {
			if(i >= args.Length) {
				result = default;
				return false;
			}
			try {
				result = ParseArg<T>(args, i);
				return true;
			}
			catch {
				result = default;
				return false;
			}
		}
	}
}
