using ASCReader.Export;
using ASCReader.Import;
using ASCReader.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace ASCReader {
	public class Program {

#if DEBUG
		private static bool autoInputEnabled = false;
		private static int autoInputNum = 0;
		private static string[] autoInputs = new string[]{

		};
#endif

		public static bool debugLogging = false;
		public static int exported3dFiles = 0;

		static string progressString = null;

		static List<string> inputFileList;
		static ASCData data;
		public static ExportOptions exportOptions;
		static ASCSummary targetValues;

		static void Main(string[] args) {
			ImportManager.RegisterHandler(new StandardImporter());
			ExportUtility.RegisterHandler(new StandardExporter());
			CommandHandler.commandHandlers.Add(new StandardCommands());

			bool loadPlugins = true;
			foreach(var a in args) if(a == "noplugins") loadPlugins = false;
			if(loadPlugins) {
				PluginLoader.LoadPlugins();
			} else {
				WriteLine("INFO: Plugins are disabled via launch arguments.");
			}

#if DEBUG
			if(args.Length > 0 && args[0] == "auto") autoInputEnabled = true;
#endif
			WriteLine("---------------------------------");
			WriteLine("HEIGHTMAP CONVERTER V1.0");
			WriteLine("---------------------------------");
			while(data == null || !data.isValid) {
				int result = GetInputFiles();
				bool batchMode = result == 1;
				if(result == -1) {
					break; //quit the application by leaving the while loop
				}
				if(data != null && data.isValid) {
					if(!GetValidExportOptions(batchMode)) {
						data = null;
						continue;
					}
					if(batchMode) {
						string path = GetBatchExportPath();
						int i = 0;
						int total = inputFileList.Count + 1;
						while(data != null) {
							if(WriteFilesForData(path + "\\" + data.filename)) {
								i++;
								WriteSuccess(string.Format("EXPORT {0}/{1} SUCCESSFUL", i, total));
								data = NextFile();
							} else {
								data = null;
								WriteError("EXPORT FAILED");
							}
						}
					} else {
						if(OutputFiles()) {
							WriteSuccess("EXPORT SUCCESSFUL");
						} else {
							WriteError("EXPORT FAILED");
						}
					}
					WriteLine("---------------------------------");
					data = null;
				}
			}
		}

		static int GetInputFiles() {
			WriteLine("Enter path to the input file:");
			WriteLine("or type 'batch' and a path to perform batch operations");
			string input = GetInput();
			inputFileList = new List<string>();
			int result = 0;
			input = input.Replace("\"", "");
			if(input.ToLower().StartsWith("batch")) {
				if(input.Length > 6) {
					input = input.Substring(6);
					if(IsDirectory(input)) {
						WriteLine("Starting batch in directory " + input + " ...");
						foreach(string f in Directory.GetFiles(input, "*.asc")) {
							inputFileList.Add(Path.GetFullPath(f));
						}
						foreach(string f in Directory.GetFiles(input, "*.mca")) {
							inputFileList.Add(Path.GetFullPath(f));
						}
						WriteLine(inputFileList.Count + " files have been added to the batch queue");
					}
					result = 1;
				} else {
					inputFileList.Add("");
				}
			} else if(input.ToLower().StartsWith("quit")) {
				return -1;
			} else {
				inputFileList.Add(input);
				WriteLine("Reading file " + input + " ...");
			}
			data = NextFile();
			return result;
		}

		static ASCData NextFile() {
			if(inputFileList.Count > 0) {
				string f = inputFileList[0];
				string ext = Path.GetExtension(f).ToLower().Replace(".", "");
				ASCData d;
				d = ImportManager.ImportFile(inputFileList[0], ext);
				inputFileList.RemoveAt(0);
				if(d != null) {
					CurrentExportJobInfo.importedFilePath = f;
					return d;
				} else {
					WriteError("Don't know how to read file with extension: " + ext);
					return null;
				}
				/*
				else if(ext == ".png" || ext == ".jpeg" || ext == ".jpg" || ext == ".bmp" || ext == ".tif") {
					d = HeightmapImporter.ImportHeightmap(f);
					WriteLineSpecial("Heightmap imported. Override cellsize and low/high values for the desired result.");
					WriteLineSpecial("Default cell size: 1.0     Default data range 0.0 (black) - 1.0 (white)");
				} else if(ext == ".mca") {
					d = MinecraftRegionImporter.ImportHeightmap(f);
					WriteLineSpecial("Minecraft region heightmap imported.");
				} else {
					
					d = null;
				}
				CurrentExportJobInfo.importedFilePath = f;
				inputFileList.RemoveAt(0);
				return d;
				*/
			} else {
				return null;
			}
		}

		static bool IsDirectory(string p) {
			try {
				return File.GetAttributes(p).HasFlag(FileAttributes.Directory);
			} catch {
				return false;
			}
		}

		static string GetBatchExportPath() {
			WriteLine("Enter path to write the files:");
			string s = ReplacePathVars(GetInput());
			while(!IsDirectory(s)) {
				WriteWarning("Directory not found!");
				s = GetInput();
			}
			return s;
		}

		static bool OutputFiles() {
			WriteLine("Enter path & name to write the file(s):");
			string path = ReplacePathVars(GetInput().Replace("\"", ""));
			return WriteFilesForData(path);
		}

		static bool WriteFilesForData(string path) {
			try {
				return data.WriteAllFiles(path, exportOptions);
			} catch(Exception e) {
				WriteError("Error encountered while trying to output files!");
				WriteLine(e.ToString());
				return false;
			}
		}

		static string ReplacePathVars(string path) {
			path = path.Replace("{datetime}", System.DateTime.Now.ToString("yy-MM-dd_HH-mm-ss"));
			return path;
		}

		static bool GetValidExportOptions(bool batch) {
			if(!GetExportOptions(batch)) return false;
			foreach(var ff in exportOptions.outputFormats) {
				while(!ExportUtility.ValidateExportOptions(exportOptions, data, ff)) {
					Console.WriteLine("Cannot export with the current settings / format!");
					if(!GetExportOptions(batch)) return false;
				}
			}
			return true;
		}

		static void WriteListEntry(string cmd, string desc, int indentLevel, bool required) {
			string s = required ? "*" : "";
			s = s.PadRight((indentLevel + 1) * 4);
			s += cmd;
			s = s.PadRight(24);
			s += desc;
			WriteLine(s);
		}

		static bool GetExportOptions(bool batch) {
			WriteLine("--------------------");
			if(batch) WriteLine("Note: The following export options will be applied to all files in the batch");
			WriteLine("* = Required setting");
			WriteLine("File Information:");
			WriteListEntry("showheader", "Shows the header of the loaded file", 0, false);
			WriteLine("Export options:");
			WriteListEntry("format N..", "Export to the specified format(s)", 0, true);
			foreach(var f in ExportUtility.supportedFormats) {
				WriteListEntry(f.inputKey, f.description, 1, false);
			}
			foreach(var c in CommandHandler.GetConsoleCommands()) {
				WriteListEntry(c.command, c.description, 0, false);
			}
			if(batch) {
				WriteLineSpecial("Batch export options:");
				WriteLineSpecial("    join                Joins all files into one large file");
				WriteLineSpecial("    equalizeheightmaps  Equalizes all heightmaps with the same low and high values");
			}
			WriteLine("");
			WriteLine("Type 'export' when ready to export");
			WriteLine("Type 'abort' to abort the export");
			WriteLine("--------------------");
			string input;
			exportOptions = new ExportOptions();
			while(true) {
				input = GetInput();
				while(input.Contains("  ")) input = input.Replace("  ", " "); //Remove all double spaces
				var split = input.Split(' ');
				string cmd = split[0].ToLower();
				string[] args = new string[split.Length - 1];
				for(int i = 0; i < split.Length - 1; i++) {
					args[i] = split[i + 1];
				}
				var r = HandleCommand(cmd, args, batch);
				if(r != null) {
					return (bool)r;
				}
			}
		}

		static bool? HandleCommand(string cmd, string[] args, bool batch) {
			if(cmd == "export") {
				return true;
			} else if(cmd == "abort") {
				WriteWarning("Export aborted");
				return false;
			} else if(cmd == "showheader") {
				WriteLine(data.fileHeader);
				return null;
			} else if(cmd == "format") {
				if(args.Length > 0) {
					exportOptions.SetOutputFormats(args, false);
					string str = "";
					foreach(FileFormat ff in exportOptions.outputFormats) {
						str += " " + ff.Identifier;
					}
					if(str == "") str = " <NONE>";
					WriteLine("Exporting to the following format(s):" + str);
				} else {
					WriteWarning("A list of formats is required!");
				}
				return null;
			}
			foreach(var c in CommandHandler.GetConsoleCommands()) {
				if(c.command == cmd) {
					c.commandHandler.HandleCommand(c.command, args, exportOptions, data);
					return null;
				}
			}
			if(batch) {
				if(cmd == "join") {
					WriteWarning("to do"); //TODO
				} else if(cmd == "equalizeheightmaps") {
					targetValues = new ASCSummary();
					WriteLine("Fetching summary from files...");
					int i = 0;
					foreach(string path in inputFileList) {
						i++;
						var s = ASCData.GetSummary(path);
						WriteLine(i + "/" + inputFileList.Count);
						if(s.lowestValue < targetValues.lowestValue) targetValues.lowestValue = s.lowestValue;
						if(s.highestValue > targetValues.highestValue) targetValues.highestValue = s.highestValue;
						targetValues.averageValue += s.averageValue;
					}
					WriteLine("Success:");
					WriteLine("    lowest:   " + targetValues.lowestValue);
					WriteLine("    highest:  " + targetValues.highestValue);
					WriteLine("    average:  " + targetValues.averageValue);
					targetValues.averageValue /= i;
					return null;
				}
			} else {
				WriteWarning("Unknown option :" + cmd);
			}
			return null;
		}

		public static string GetInput() {
			Console.CursorVisible = true;
			string s;
			bool autoinput = false;
#if DEBUG
			autoinput = autoInputs != null && autoInputEnabled && autoInputNum < autoInputs.Length;
			if(autoinput) {
				s = autoInputs[autoInputNum];
				autoInputNum++;
				WriteAutoTask("> " + s);
			} else {
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

		static void WriteConsoleLine(string str) {
			Console.CursorVisible = false;
			if(progressString != null) {
				WriteProgress("", -1);
				progressString = null;
				Console.SetCursorPosition(0, Console.CursorTop);
			}
			Console.WriteLine(str);
			Console.ResetColor();
		}

		public static void WriteLine(string str) {
			WriteConsoleLine(str);
		}

		public static void WriteSuccess(string str) {
			Console.ForegroundColor = ConsoleColor.Green;
			WriteConsoleLine(str);
		}

		public static void WriteLineSpecial(string str) {
			Console.ForegroundColor = ConsoleColor.Cyan;
			WriteConsoleLine(str);
		}

		public static void WriteAutoTask(string str) {
			Console.BackgroundColor = ConsoleColor.DarkBlue;
			WriteConsoleLine(str);
		}

		public static void WriteLine(string str, params Object[] args) {
			Console.WriteLine(str, args);
		}

		public static void WriteWarning(string str) {
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			WriteConsoleLine(str);
		}

		public static void WriteError(string str) {
			Console.ForegroundColor = ConsoleColor.DarkRed;
			WriteConsoleLine(str);
#if DEBUG
			autoInputs = null; //Stop any upcoming automated inputs
#endif
		}

		public static void WriteProgress(string str, float progressbar) {
			Console.CursorVisible = false;
			int lastLength = 0;
			if(progressString != null) {
				lastLength = progressString.Length;
				Console.SetCursorPosition(0, Console.CursorTop);
			}
			progressString = str;
			if(progressbar >= 0) progressString += " " + GetProgressBar(progressbar) + " " + (int)Math.Round(progressbar * 100) + "%";
			if(lastLength > 0) progressString = progressString.PadRight(lastLength, ' ');
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write(progressString);
			Console.ResetColor();
		}

		static string GetProgressBar(float prog) {
			string s = "";
			for(int i = 0; i < 20; i++) {
				s += (prog >= (i + 1) / 20f) ? "█" : "░";
			}
			s += "";
			return s;
		}

		public static int GetTotalExportCellsPerFile() {
			int cells = exportOptions.fileSplitDims >= 32 ? (int)Math.Pow(exportOptions.fileSplitDims, 2) : data.ncols * data.nrows;
			if(exportOptions.subsampling > 1) cells /= exportOptions.subsampling * exportOptions.subsampling;
			return cells;
		}
	}
}
