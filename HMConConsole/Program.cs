using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using System;
using System.Collections.Generic;
using System.IO;
using static HMCon.ConsoleOutput;
using static HMCon.HMConManager;

namespace HMConConsole
{
	public class Program
	{

#if DEBUG
		internal static int autoInputNum = 0;
		internal static string[] autoInputs = GetAutoInputs();

		private static string[] GetAutoInputs()
		{
			try
			{
				List<string> list = new List<string>();
				foreach (var ln in Resources.autoinputs.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)) {
					if(!ln.StartsWith("//") && !ln.StartsWith("#"))
					{
						list.Add(ln.Trim());
					}
				}
				return list.ToArray();
			}
			catch
			{
				WriteWarning("No auto inputs were loaded");
				return null;
			}
		}
#endif

		public static void Main(string[] launchArgs)
		{

			bool loadPlugins = true;
			foreach (var a in launchArgs) if (a == "noplugins") loadPlugins = false;
			Initialize(loadPlugins ? AppContext.BaseDirectory : null);

#if DEBUG
			if (launchArgs.Length > 0 && launchArgs[0] == "auto") autoInputActive = true;
#endif
			WriteLine("---------------------------------");
			WriteLine("HEIGHTMAP CONVERTER V1.1");
			WriteLine("---------------------------------");
			while (true)
			{
				BeginNewJob();

				if (currentJob == null)
				{
					//quit the application by leaving the while loop
					break;
				}

				currentJob.NextFile();
				if (currentJob.CurrentData == null) continue;

				if (currentJob.CurrentData.isValid)
				{
					if (!GetExportSettings(currentJob.batchMode))
					{
						currentJob = null;
						continue;
					}

					currentJob.outputPath = GetExportPath(currentJob.batchMode);

					currentJob.ExportAll();

					WriteLine("---------------------------------");
					currentJob = null;
				}
			}
		}

		static void BeginNewJob()
		{
			int result = GetInputFiles(out var fileList, out var args);
			if (result < 0) return; //Do nothing, terminate the application
			currentJob = new Job()
			{
				importArgs = args,
				batchMode = result > 0
			};
			currentJob.AddInputFiles(fileList.ToArray());

			//Add console feedback
			currentJob.FileImported += (int i, string s) =>
			{

			};
			currentJob.FileImportFailed += (int i, string s, Exception e) =>
			{
				WriteError("IMPORT FAILED: " + s);
				WriteError(e.ToString());
			};
			currentJob.FileExported += (int i, string s) =>
			{
				if (!currentJob.batchMode)
				{
					WriteSuccess("EXPORT SUCCESSFUL");
				}
				else
				{
					WriteSuccess($"EXPORT {i + 1}/{currentJob.InputFileList.Count} SUCCESSFUL");
				}
			};
			currentJob.FileExportFailed += (int i, string s, Exception e) =>
			{
				if (!currentJob.batchMode)
				{
					WriteError("EXPORT FAILED: " + s);
				}
				else
				{
					WriteError($"EXPORT {i}/{currentJob.InputFileList.Count} FAILED:");
				}
				WriteError(e.ToString());
			};
			currentJob.ExportCompleted += () =>
			{
				if (currentJob.batchMode)
				{
					WriteSuccess("DONE!");
				}
			};
		}

		static int GetInputFiles(out List<string> files, out string[] args)
		{
			WriteLine("Enter path to the input file:");
			WriteLine("or type 'batch' and a path to perform batch operations");
			string inputPath = GetInput();
			files = new List<string>();
			inputPath = inputPath.Replace("\"", "");
			inputPath = ExtractArgs(inputPath, out args);
			if (inputPath.ToLower().StartsWith("quit"))
			{
				return -1;
			}
			else if (inputPath.ToLower().StartsWith("batch"))
			{
				if (inputPath.Length > 6)
				{
					inputPath = inputPath.Substring(6);
					if (Directory.Exists(inputPath))
					{
						WriteLine("Starting batch in directory " + inputPath + " ...");
						foreach (string f in Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories))
						{
							if (ImportManager.CanImport(f))
							{
								files.Add(f);
							}
							else
							{
								WriteWarning($"Skipping file 'f', unknown or unsupported file type.");
							}
						}
						WriteLine(files.Count + " files have been added to the batch queue");
					}
					return 1;
				}
				else
				{
					files.Add("");
					return 0;
				}
			}
			else
			{
				files.Add(inputPath);
				WriteLine("Reading file " + inputPath + " ...");
				return 0;
			}
		}

		static string GetExportPath(bool mustBeDirectory)
		{
			if (mustBeDirectory)
			{
				WriteLine("Enter destination path:");
			}
			else
			{
				WriteLine("Enter path and filename to write the file(s):");
			}
			var path = GetInputPath();
			if (mustBeDirectory)
			{
				while (!Directory.Exists(path))
				{
					WriteWarning("Directory not found!");
					path = GetInputPath();
				}
			}
			else
			{
				while (!Directory.Exists(Path.GetDirectoryName(path)))
				{
					WriteWarning("Directory not found!");
					path = GetInputPath();
				}
			}
			return path;
		}

		static bool GetExportSettings(bool batch)
		{
			if (!GetExportOptions(batch)) return false;
			while (!ExportUtility.ValidateExportSettings(currentJob.exportSettings, currentJob.CurrentData))
			{
				WriteError("Cannot export with the current settings / format!");
				if (!GetExportOptions(batch)) return false;
			}
			return true;
		}

		static void WriteListEntry(string cmd, string desc, int indentLevel, bool required)
		{
			string s = required ? "*" : "";
			s = s.PadRight((indentLevel + 1) * 4);
			s += cmd;
			s = s.PadRight(24);
			s += desc;
			WriteLine(s);
		}

		static bool GetExportOptions(bool batch)
		{
			WriteLine("--------------------");
			if (batch) WriteLine("Note: The following export options will be applied to all files in the batch");
			WriteLine("* = Required setting");
			WriteLine("Export options:");
			WriteListEntry("format N..", "Export to the specified format(s)", 0, true);
			foreach (var f in FileFormatManager.GetSupportedFormats())
			{
				WriteListEntry(f.CommandKey, f.Description, 1, false);
			}
			foreach (var c in CommandHandler.ConsoleCommands)
			{
				WriteListEntry(c.command, c.description, 0, false);
			}
			WriteListEntry("modify X..", "Modification commands", 0, false);
			foreach (var m in CommandHandler.ModificationCommands)
			{
				WriteListEntry(m.command, m.description, 1, false);
			}
			if (batch)
			{
				WriteLineSpecial("Batch export options:");
				WriteLineSpecial("    join                Joins all files into one large file");
				WriteLineSpecial("    equalizeheightmaps  Equalizes all heightmaps with the same low and high values");
			}
			WriteLine("");
			WriteLine("Type 'export' when ready to export");
			WriteLine("Type 'cancel' to abort");
			WriteLine("--------------------");
			string input;
			while (true)
			{
				input = GetInput();
				while (input.Contains("  ")) input = input.Replace("  ", " "); //Remove all double spaces
				var split = input.Split(' ');
				string cmd = split[0].ToLower();
				string[] args = new string[split.Length - 1];
				for (int i = 0; i < split.Length - 1; i++)
				{
					args[i] = split[i + 1];
				}
				var r = HandleCommand(cmd, args, batch);
				if (r != null)
				{
					return (bool)r;
				}
			}
		}

		static bool? HandleCommand(string cmd, string[] args, bool batch)
		{
			if (cmd == "export")
			{
				return true;
			}
			else if (cmd == "abort")
			{
				WriteWarning("Export aborted");
				return false;
			}
			else if (cmd == "format")
			{
				if (args.Length > 0)
				{
					currentJob.exportSettings.SetOutputFormats(args, false);
					string str = "";
					foreach (FileFormat ff in currentJob.exportSettings.outputFormats)
					{
						str += " " + ff.Identifier;
					}
					if (str == "") str = " <NONE>";
					WriteLine("Exporting to the following format(s):" + str);
				}
				else
				{
					WriteWarning("A list of formats is required!");
				}
				return null;
			}
			else if (cmd == "modify")
			{
				if (args.Length > 0)
				{
					List<string> argList = new List<string>(args);
					cmd = argList[0];
					argList.RemoveAt(0);
					args = argList.ToArray();
					foreach (var c in CommandHandler.ModificationCommands)
					{
						if (c.command == cmd)
						{
							try
							{
								var mod = c.ExecuteCommand(currentJob, args);
								if (mod != null)
								{
									currentJob.exportSettings.AddModifierToChain(mod);
								}
							}
							catch (Exception e)
							{
								WriteWarning(e.Message);
								WriteWarning($"Usage: {c.command} {c.argsHint}");
							}
							return null;
						}
					}
					WriteWarning("Unknown modifier: " + cmd);
				}
				return null;
			}
			foreach (var c in CommandHandler.ConsoleCommands)
			{
				if (c.command == cmd)
				{
					c.ExecuteCommand(currentJob, args);
					return null;
				}
			}
			if (batch)
			{
				if (cmd == "join")
				{
					WriteWarning("to do"); //TODO
				}
				else if (cmd == "equalizeheightmaps")
				{
					float low = float.MaxValue;
					float high = float.MinValue;
					float avg = 0;
					int i = 0;
					foreach (string path in currentJob.InputFileList)
					{
						if (Path.GetExtension(path).ToLower() == ".asc")
						{
							i++;
							ASCImporter.GetDataInfo(path, out float ascLow, out float ascHigh, out float ascAvg);
							WriteLine(i + "/" + currentJob.InputFileList.Count);
							low = Math.Min(low, ascLow);
							high = Math.Max(high, ascHigh);
							avg += ascAvg;
						}
						else
						{
							WriteError(path + " is not a ASC file!");
						}
					}
					avg /= i;
					WriteLine("Success:");
					WriteLine("    lowest:   " + low);
					WriteLine("    highest:  " + high);
					WriteLine("    average:  " + avg);
					return null;
				}
			}
			else
			{
				WriteWarning("Unknown option: " + cmd);
			}
			return null;
		}

		public static string GetInput()
		{
			Console.CursorVisible = true;
			string s;
			bool autoinput = false;
#if DEBUG
			autoinput = autoInputs != null && HMConManager.autoInputActive && autoInputNum < autoInputs.Length;
			if (autoinput)
			{
				s = autoInputs[autoInputNum];
				autoInputNum++;
				WriteAutoTask("> " + s);
			}
			else
			{
#endif
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.Write("> ");
				s = Console.ReadLine();
				Console.ResetColor();
#if DEBUG
			}
#endif
			return s;
		}

		public static string GetInputPath()
		{
			return Job.ReplacePathVars(GetInput().Replace("\"", ""));
		}

		static string ExtractArgs(string input, out string[] args)
		{
			var split = input.Split(new string[] { " -" }, StringSplitOptions.RemoveEmptyEntries);
			List<string> argList = new List<string>();
			input = split[0];
			for (int i = 1; i < split.Length; i++)
			{
				argList.Add(split[i].Trim());
			}
			args = argList.ToArray();
			return input;
		}
	}
}
