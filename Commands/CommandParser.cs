using TerrainFactory.Export;
using TerrainFactory.Modification;
using TerrainFactory.Util;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TerrainFactory.Commands {
	public static class CommandParser {

		public static void ParseCommandInput(string input, out string cmd, out string[] args)
		{
			while(input.Contains("  ")) input = input.Replace("  ", " "); //Remove all double spaces

			cmd = input.Split(' ')[0].ToLower();
			string argsString = "";
			if(input.Length > cmd.Length + 1)
			{
				argsString = input.Substring(cmd.Length + 1);
			}

			args = Regex.Matches(argsString, @"[\""].+?[\""]|[^ ]+")
			.Cast<Match>()
			.Select(x => x.Value.Trim('"'))
			.ToArray();
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
