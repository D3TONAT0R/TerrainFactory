using System;
using System.Collections.Generic;
using System.IO;
using ASCReader.Export;

namespace ASCReader {
	class Program
	{

		#if DEBUG
		private static bool autoInputEnabled = false;
		private static int autoInputNum = 0;
		private static string[] autoInputs = new string[]{
			"C:\\Users\\gdv\\Desktop\\ascrtest\\DOM_26920_12490.asc",
			"format asc xyz 3ds fbx png-hm png-nm png-hs",
			"split 1000",
			"subsample 2",
			"export",
			"C:\\Users\\gdv\\Desktop\\ascrtest\\out\\testexport"
		};
		#endif

		public static bool debugLogging = false;
		public static int exported3dFiles = 0;
		
		static List<string> inputFileList;
		static ASCData data;
		static ExportOptions exportOptions;
		static void Main(string[] args)
		{
			WriteLine("---------------------------------");
			WriteLine("ASCII-GRID FILE CONVERTER");
			WriteLine("---------------------------------");
			while(data == null || !data.isValid) {
				bool batchMode = GetInputFiles();
				if(data != null && data.isValid) {
					if(!GetValidExportOptions(batchMode)) {
						data = null;
						continue;
					}
					if(batchMode) {
						string path = GetBatchExportPath();
						int i = 0;
						int total = inputFileList.Count+1;
						while(data != null) {
							if(WriteFilesForData(path+"\\"+data.filename)) {
								i++;
								WriteSuccess(string.Format("EXPORT {0}/{1} SUCCESSFUL", i, total));
								data = nextFile();
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

		static bool GetInputFiles() {
			WriteLine("Enter path to .asc file:");
			WriteLine("or type 'batch' and a path to perform batch operations");
			string input = GetInput();
			inputFileList = new List<string>();
			bool doBatch = false;
			input = input.Replace("\"", "");
			if(input.ToLower().StartsWith("batch")) {
				input = input.Substring(6);
				if(IsDirectory(input)) {
					WriteLine("Starting batch in directory "+input+" ...");
					foreach(string f in Directory.GetFiles(input, "*.asc")) {
						inputFileList.Add(Path.GetFullPath(f));
					}
					WriteLine(inputFileList.Count + " files have been added to the batch queue");
				}
				doBatch = true;
			} else {
				inputFileList.Add(input);
				WriteLine("Reading file "+input+" ...");
			}
			data = nextFile();
			return doBatch;
		}

		static ASCData nextFile() {
			if(inputFileList.Count > 0) {
				var d = new ASCData(inputFileList[0]);
				inputFileList.RemoveAt(0);
				return d;
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
			string s = GetInput();
			while(!IsDirectory(s)) {
				WriteWarning("Directory not found!");
				s = GetInput();
			}
			return s;
		}

		static bool OutputFiles() {
			WriteLine("Enter path & name to write the file(s):");
			string path = GetInput();
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

		static bool GetValidExportOptions(bool batch) {
			if(!GetExportOptions(batch)) return false;
			while(!ValidateExportOptions()) {
				Console.WriteLine("Cannot export with the current settings / format!");
				if(!GetExportOptions(batch)) return false;
			}
			return true;
		}

		static bool GetExportOptions(bool batch) {
			if(batch) WriteLine("Note: The following export options will be applied to all files in the batch");
			WriteLine("File Information:");
			WriteLine("    showheader          Shows the header of the loaded file");
			WriteLine("Export options:");      
			WriteLine("    format N..          Export to the specified format(s)");
			WriteLine("        asc             ASCII-Grid (same as input)");
			WriteLine("        xyz             ASCII-XYZ points");
			WriteLine("        3ds             3d Mesh");
			WriteLine("        fbx             3d Mesh");
			WriteLine("        png-hm          Heightmap");
			WriteLine("        png-nm          Normalmap");
			WriteLine("        png-hs          Hillshade");
			WriteLine("    subsample N         Only export every N-th cell");
			WriteLine("    split N             Split files every NxN cells (minimum 32)");
			WriteLine("    overridecellsize N  Override size per cell");
			if(batch) {
				WriteLineSpecial("Batch export options:");
				WriteLineSpecial("    join                Joins all files into one large file");
				WriteLineSpecial("    equalizeheightmaps  Equalizes all heightmaps with the same low and high values");
			}
			WriteLine("Type 'export' when ready to export");
			WriteLine("Type 'abort' to abort the export");
			String input;
			exportOptions = new ExportOptions(); 
			while(true) {
				input = GetInput();
				input = input.ToLower();
				if(input == "export") {
					return true;
				} else if(input == "abort") {
					WriteWarning("Export aborted");
					return false;
				} else if(input.StartsWith("showheader")) {
					WriteLine(data.fileHeader);
				} else if(input.StartsWith("subsample")) {
					string[] split = input.Split(' ');
					if(split.Length > 1) {
						int i;
						if(int.TryParse(split[1], out i)) {
							exportOptions.subsampling = i;
							WriteLine("Subsampling set to: "+i);
						} else {
							WriteWarning("Can't parse to int: "+split[1]);
						}
					} else {
						WriteWarning("An integer is required!");
					}
				} else if(input.StartsWith("split")) {
					string[] split = input.Split(' ');
					if(split.Length > 1) {
						int i;
						if(int.TryParse(split[1], out i)) {
							exportOptions.fileSplitDims = i;
							WriteLine("File splitting set to: "+i+"x"+i);
						} else {
							WriteWarning("Can't parse to int: "+split[1]);
						}
					} else {
						WriteWarning("An integer is required!");
					}
				} else if(input.StartsWith("overridecellsize")) {
					string[] split = input.Split(' ');
					if(split.Length > 1) {
						float f;
						if(float.TryParse(split[1], out f)) {
							WriteLine("Cellsize changed from {0} to {1}", data.cellsize, f);
							data.cellsize = f;
						} else {
							WriteWarning("Can't parse to int: " + split[1]);
						}
					} else {
						WriteWarning("A number is required!");
					}
				} else if(input.StartsWith("format")) {
					string[] split = input.Split(' ');
					if(split.Length > 1) {
						split[0] = null;
						exportOptions.SetOutputFormats(split, false);
						string str = "";
						foreach(FileFormat ff in exportOptions.outputFormats) {
							str += " "+ff;
						}
						if(str == "") str = " <NONE>";
						WriteLine("Exporting to the following format(s):"+str);
					} else {
						WriteWarning("A list of formats is required!");
					}
				} else {
					WriteWarning("Unknown option :"+input);
				}
			}
		}

		static bool ValidateExportOptions() {
			bool valid = true;
			int cellsPerFile = GetTotalExportCellsPerFile();
			if(exportOptions.outputFormats.Count == 0) {
				WriteWarning("No export format is defined! choose at least one format for export!");
				return false;
			}
			if(exportOptions.outputFormats.Contains(FileFormat.MDL_3ds)) {
				/*if(cellsPerFile >= 65535) {
					Console.WriteLine("ERROR: Cannot export more than 65535 cells in a single 3ds file! Current amount: "+cellsPerFile);
					Console.WriteLine("       Reduce splitting interval or increase subsampling to allow for exporting 3ds Files");
					valid = false;
				}*/
			}
			return valid;
		}

		public static string GetInput() {
			string s;
			bool autoinput = false;
			#if DEBUG
			autoinput = autoInputEnabled && autoInputNum < autoInputs.Length;
			#endif
			if(autoinput) {
				s = autoInputs[autoInputNum];
				autoInputNum++;
				WriteAutoTask("> " + s);
			} else {
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("> ");
				s = Console.ReadLine();
				Console.ResetColor();
			}
			return s;
		}

		public static void WriteLine(string str) {
			Console.WriteLine(str);
		}

		public static void WriteSuccess(string str) {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write(str);
			Console.ResetColor();
			Console.WriteLine();
		}

		public static void WriteLineSpecial(string str) {
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write(str);
			Console.ResetColor();
			Console.WriteLine();
		}

		public static void WriteAutoTask(string str) {
			Console.BackgroundColor = ConsoleColor.DarkBlue;
			Console.Write(str);
			Console.ResetColor();
			Console.WriteLine();
		}

		public static void WriteLine(string str, params Object[] args) {
			Console.WriteLine(str, args);
		}

		public static void WriteWarning(string str) {
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Write(str);
			Console.ResetColor();
			Console.WriteLine();
		}

		public static void WriteError(string str) {
			Console.BackgroundColor = ConsoleColor.DarkRed;
			Console.Write(str);
			Console.ResetColor();
			Console.WriteLine();
			autoInputs = null; //Stop any upcoming automated inputs
		}

		private static int GetTotalExportCellsPerFile() {
			int cells = exportOptions.fileSplitDims >= 32 ? (int)Math.Pow(exportOptions.fileSplitDims, 2) : data.ncols*data.nrows;
			if(exportOptions.subsampling > 1) cells /= exportOptions.subsampling*exportOptions.subsampling;
			return cells;
		}
	}
}
