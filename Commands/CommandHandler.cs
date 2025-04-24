using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static TerrainFactory.Commands.CommandAttribute;

namespace TerrainFactory.Commands
{
	public static class CommandHandler {

		public class CommandDefinition<A> where A : Attribute
		{
			public MethodInfo method;
			public A attribute;

			public CommandDefinition(MethodInfo method, A attribute)
			{
				this.method = method;
				this.attribute = attribute ?? throw new NullReferenceException("Attribute was null.");
			}
		}

		public static List<CommandDefinition<CommandAttribute>> Commands { get; private set; }
		public static List<CommandDefinition<ModifierCommandAttribute>> ModifierCommands { get; private set; }

		public static List<string> CommandQueue { get; private set; } = new List<string>();

		public static void Initialize() {
			Commands = new List<CommandDefinition<CommandAttribute>>();
			ModifierCommands = new List<CommandDefinition<ModifierCommandAttribute>>();
			AddCommandMethodsFromTypes(Commands, typeof(StandardCommands));
			AddCommandMethodsFromTypes(ModifierCommands, typeof(StandardModifierCommands));
			foreach(var m in ModuleLoader.loadedModules)
			{
				try
				{
					var definingTypes = m.Value.CommandDefiningTypes.ToArray();
					AddCommandMethodsFromTypes(Commands, definingTypes);
					AddCommandMethodsFromTypes(ModifierCommands, definingTypes);
				}
				catch(Exception e)
				{
					ConsoleOutput.WriteError($"Failed to initialize commands from module '{m.Key}': {e.Message}");
				}
			}
		}

		public static IEnumerable<CommandDefinition<CommandAttribute>> ListValidCommands(CommandAttribute.ContextFlags context)
		{
			foreach(var cmd in Commands)
			{
				if(cmd.attribute.context.HasFlag(context))
				{
					yield return cmd;
				}
			}
		}

		public static string GetInput(Project project, string prompt = null, bool allowQueued = true)
		{
			if(ConsoleOutput.consoleHandler != null)
			{
				return ConsoleOutput.consoleHandler.GetInput(prompt, allowQueued ? CommandQueue : null);
			}
			else
			{
				Console.CursorVisible = true;
				string input = null;

				var lastBackgroundColor = Console.BackgroundColor;
				if(CommandQueue != null && CommandQueue.Count > 0)
				{
					input = CommandQueue[0];
					CommandQueue.RemoveAt(0);
					Console.BackgroundColor = ConsoleColor.DarkBlue;
				}

				if(!string.IsNullOrEmpty(prompt))
				{
					Console.Write(prompt + ": ");
				}
				else
				{
					Console.Write("> ");
				}

				if(input == null)
				{
					input = Console.ReadLine();
				}
				else
				{
					Console.WriteLine(input);
				}
				Console.BackgroundColor = lastBackgroundColor;

				//Parse variables
				if(project != null)
				{
					input = project.ResolveWildcards(input, null);
				}

				return input;
			}
		}

		public static CommandResult ExecuteCommand(Project project, string input, ContextFlags context)
		{
			CommandParser.ParseCommandInput(input, out string cmd, out string[] args);
			if(context.HasFlag(ContextFlags.AfterImport))
			{
				if(cmd == "abort")
				{
					return CommandResult.RequestsQuit;
				}
			}
			if(context.HasFlag(ContextFlags.BeforeImport))
			{
				if(cmd == "exit")
				{
					return CommandResult.RequestsQuit;
				}
			}

			foreach(var c in ListValidCommands(context))
			{
				if(cmd == c.attribute.commandName)
				{
					bool result = (bool)c.method.Invoke(null, new object[] { project, args });
					return result ? CommandResult.Success : CommandResult.Failed;
				}
			}

			//No command was executed
			return CommandResult.None;
		}

		public static bool AddCommandsToQueue(string path)
		{
			path = path.Replace("\"", "");
			try
			{
				var lines = File.ReadAllLines(path);
				if(CommandQueue.Count + lines.Length > 100)
				{
					ConsoleOutput.WriteError("Command queue overflow (> 100). Commands not added.");
					return false;
				}
				else
				{
					CommandQueue.InsertRange(0, lines);
					return true;
				}
			}
			catch(Exception e)
			{
				ConsoleOutput.WriteError($"Failed to add commands from file to queue ({path}): {e.Message}");
				return false;
			}
		}

		public static void ClearCommandQueue()
		{
			CommandQueue.Clear();
		}

		private static void AddCommandMethodsFromTypes<A>(List<CommandDefinition<A>> list, params Type[] types) where A : Attribute
		{
			list.AddRange(types
				.SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				.Where(m => m.GetCustomAttribute<A>() != null)
				.Select(m => new CommandDefinition<A>(m, m.GetCustomAttribute<A>())));
		}
	}
}
