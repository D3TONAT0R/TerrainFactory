using TerrainFactory.Modification;
using System;
using System.Collections.Generic;
using static TerrainFactory.ConsoleOutput;
using static TerrainFactory.Commands.CommandParser;
using TerrainFactory.Formats;

namespace TerrainFactory.Commands
{
	public static class StandardCommands
	{

		[Command("info", "", "Prints general info about the imported height data")]
		public static bool PrintInfoCmd(Project sheet, string[] args)
		{
			sheet.InputData.LoadIfRequired();
			var d = sheet.InputData.Current;
			Console.WriteLine($"Grid Size: {d.CellCountX} x {d.CellCountY}");
			Console.WriteLine($"Cell Size: {d.CellSize}");
			Console.WriteLine($"Dimensions: {d.CellCountX * d.CellSize} x {d.CellCountY * d.CellSize}");
			Console.WriteLine($"Lower Corner Pos: {d.LowerCornerPosition.X}, {d.LowerCornerPosition.Y}");
			Console.WriteLine($"Lowest/Highest: {d.MinElevation} / {d.MaxElevation}");
			Console.WriteLine($"Nodata Value: {d.NoDataValue}");
			return true;
		}

		[Command("split", "N", "Split files every NxN cells (minimum 32)")]
		public static bool RunSplitCmd(Project sheet, string[] args)
		{
			int i = ParseArg<int>(args, 0);
			sheet.exportSettings.splitInterval = i;
			WriteLine("File splitting set to: " + i + "x" + i);
			return true;
		}

		[Command("clearmods", "", "Removes all added modifiers")]
		public static bool RunClearModifierCmd(Project sheet, string[] args)
		{
			int l = sheet.modificationChain.chain.Count;
			sheet.modificationChain.chain.Clear();
			WriteLine($"Removed {l} modifiers");
			return true;
		}

		[Command("format", "F ..", "Sets the given formats for export", hidden = true)]
		public static bool RunFormatCmd(Project sheet, string[] args)
		{
			if(args.Length > 0)
			{
				sheet.outputFormats.SetFormats(args, false);
				string str = "";
				foreach(FileFormat ff in sheet.outputFormats)
				{
					str += " " + ff.Identifier;
				}
				if(str == "") str = " <NONE>";
				WriteLine("Exporting to the following format(s):" + str);
			}
			else
			{
				WriteWarning("A list of formats is required.");
			}
			return true;
		}

		//------------------
		//Universal commands

		[Command("exec", "path", "Executes a list of commands defined in the given file", context = CommandAttribute.ContextFlags.Global)]
		public static bool RunExecCmd(Project sheet, string[] args)
		{
			if(args.Length > 0)
			{
				CommandHandler.AddCommandsToQueue(args[0]);
			}
			else
			{
				WriteError("Path to file is required.");
			}
			return true;
		}

		//---------------
		//Hidden commands

		[Command("alias", "key value", "Define a variable with the given name", hidden = true)]
		public static bool RunAliasCmd(Project sheet, string[] args)
		{
			if(args.Length >= 2)
			{
				sheet.Wildcards.Add(args[0], args[1]);
			}
			else
			{
				WriteError("Not enough arguments.");
			}
			return true;
		}

		[Command("aliasp", "key (prompt)", "Prompt user to define a variable with the given name", hidden = true)]
		public static bool RunAliasPromptCmd(Project sheet, string[] args)
		{
			if(args.Length >= 1)
			{
				string prompt;
				if(args.Length >= 2)
				{
					prompt = args[1];
				}
				else
				{
					prompt = $"Enter value for variable '{args[0]}'";
				}
				WriteLine(prompt);
				sheet.Wildcards.Add(args[0], CommandHandler.GetInput(sheet, prompt));
			}
			else
			{
				WriteError("Not enough arguments.");
			}
			return true;
		}
	}
}
