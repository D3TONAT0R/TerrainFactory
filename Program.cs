using System;
using System.Collections.Generic;
using System.IO;
using ASCReader.Export;
using ASCReader.Import;

namespace ASCReader {
	class Program {

		#if DEBUG
		private static bool autoInputEnabled = true;
		private static int autoInputNum = 0;
		private static string[] autoInputs = new string[]{
			"C:\\Users\\gdv\\Dropbox\\World Machine\\World Machine Documents\\heightmap.png",
			"format mca",
			"mcasplatmapper",
			"setrange 0 255",
			"split 512",
			"export",
			"C:\\Users\\gdv\\Dropbox\\World Machine\\World Machine Documents\\"
		};
		#endif

		public static bool debugLogging = false;
		public static int exported3dFiles = 0;
		
		static List<string> inputFileList;
		static ASCData data;
		static ExportOptions exportOptions;
		static ASCSummary targetValues;
		static void Main(string[] args)
		{
			#if DEBUG
			if(args.Length > 0 && args[0] == "auto") autoInputEnabled = true;
			#endif
			WriteLine("---------------------------------");
			WriteLine("ASCII-GRID FILE CONVERTER");
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

		static int GetInputFiles() {
			WriteLine("Enter path to the input file:");
			WriteLine("or type 'batch' and a path to perform batch operations");
			string input = GetInput();
			inputFileList = new List<string>();
			int result = 0;
			input = input.Replace("\"", "");
			if(input.ToLower().StartsWith("batch")) {
				input = input.Substring(6);
				if(IsDirectory(input)) {
					WriteLine("Starting batch in directory "+input+" ...");
					foreach(string f in Directory.GetFiles(input, "*.asc")) {
						inputFileList.Add(Path.GetFullPath(f));
					}
					foreach(string f in Directory.GetFiles(input, "*.mca")) {
						inputFileList.Add(Path.GetFullPath(f));
					}
					WriteLine(inputFileList.Count + " files have been added to the batch queue");
				}
				result = 1;
			} else if(input.ToLower().StartsWith("quit")) {
				return -1;
			} else {
				inputFileList.Add(input);
				WriteLine("Reading file "+input+" ...");
			}
			data = nextFile();
			return result;
		}

		static ASCData nextFile() {
			if(inputFileList.Count > 0) {
				string f = inputFileList[0];
				string ext = Path.GetExtension(f).ToLower();
				ASCData d;
				if(ext == ".asc") d = new ASCData(inputFileList[0]);
				else if(ext == ".png" || ext == ".jpeg"  || ext == ".jpg" || ext == ".bmp" || ext == ".tif" ) {
					d = HeightmapImporter.ImportHeightmap(f);
					WriteLineSpecial("Heightmap imported. Override cellsize and low/high values for the desired result.");
					WriteLineSpecial("Default cell size: 1.0     Default data range 0.0-1.0");
				} else if(ext == ".mca") {
					d = MinecraftRegionImporter.ImportHeightmap(f);
					WriteLineSpecial("Minecraft region heightmap imported.");
				} else {
					WriteError("Don't know how to read file with extension: "+ext);
					d = null;
				}
				CurrentExportJobInfo.importedFilePath = f;
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
			string s = ReplacePathVars(GetInput());
			while(!IsDirectory(s)) {
				WriteWarning("Directory not found!");
				s = GetInput();
			}
			return s;
		}

		static bool OutputFiles() {
			WriteLine("Enter path & name to write the file(s):");
			string path = ReplacePathVars(GetInput());
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
			while(!ValidateExportOptions()) {
				Console.WriteLine("Cannot export with the current settings / format!");
				if(!GetExportOptions(batch)) return false;
			}
			return true;
		}

		static bool GetExportOptions(bool batch) {
			if(batch) WriteLine("Note: The following export options will be applied to all files in the batch");
			WriteLine("* = Required setting");
			WriteLine("File Information:");
			WriteLine("    showheader              Shows the header of the loaded file");
			WriteLine("    preview                 Previews the grid data in an image");
			WriteLine("    preview-hm              Previews the grid data in a heightmap");
			WriteLine("Export options:");      
			WriteLine("*   format N..              Export to the specified format(s)");
			WriteLine("        asc                 ASCII-Grid (same as input)");
			WriteLine("        xyz                 ASCII-XYZ points");
			WriteLine("        3ds                 3d Mesh");
			WriteLine("        fbx                 3d Mesh");
			WriteLine("        png-hm              Heightmap");
			WriteLine("        png-nm              Normalmap");
			WriteLine("        png-hs              Hillshade");
			WriteLine("        mca                 Minecraft Region format (1.16)");
			WriteLine("    subsample N             Only export every N-th cell");
			WriteLine("    split N                 Split files every NxN cells (minimum 32)");
			WriteLine("    selection x1 y1 x2 y2   Export only the selected data range (use 'preview' to see the data grid)");
			WriteLine("    overridecellsize N      Override size per cell");
			WriteLine("    setrange N N            Change the height data range (min - max)");
			WriteLine("    mcaoffset X Z           Apply offset to region terrain, in regions (512) (MCA format only)");
			WriteLine("    mcasplatmapper          Use splatmap files to define the world's surface (MCA format only, file <name>.splat required)");
			if(batch) {
				WriteLineSpecial("Batch export options:");
				WriteLineSpecial("    join                Joins all files into one large file");
				WriteLineSpecial("    equalizeheightmaps  Equalizes all heightmaps with the same low and high values");
			}
			WriteLine("Type 'export' when ready to export");
			WriteLine("Type 'abort' to abort the export");
			string input;
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
				} else if(input.StartsWith("preview")) {
					WriteLine("Opening preview...");
					Previewer.OpenDataPreview(data, exportOptions, false);
				} else if(input.StartsWith("preview-hm")) {
					WriteLine("Opening preview...");
					Previewer.OpenDataPreview(data, exportOptions, true);
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
				} else if(input.StartsWith("selection")) {
					string[] split = input.Split(' ');
					if(split.Length > 4) {
						int[] nums = new int[4];
						bool b = true;
						for(int i = 0; i < 4; i++) {
							b &= int.TryParse(split[i+1], out nums[i]);
						}
						if(b) {
							if(exportOptions.SetExportRange(data,nums[0],nums[1],nums[2],nums[3])) {
								WriteLine("Selection set ("+exportOptions.ExportRangeCellCount+" cells total)");
							} else {
								WriteWarning("The specified input is invalid!");
							}
						} else {
							WriteWarning("Failed to parse to int");
						}
					} else {
						if(split.Length == 1) {
							WriteLine("Selection reset");
						} else {
							WriteWarning("Four integers are required!");
						}
					}
				} else if(input.StartsWith("setrange")) {
					string[] split = input.Split(' ');
					if(split.Length > 2) {
						bool b = true;
						float min;
						float max;
						b &= float.TryParse(split[1], out min) & float.TryParse(split[2], out max);
						if(b) {
							data.SetRange(min, max);
							Program.WriteLine("Height rescaled successfully");
						} else {
							WriteWarning("Failed to parse to float");
						}
					} else {
						WriteWarning("Two numbers are required!");
					}
				} else if(input.StartsWith("mcaoffset")) {
					string[] split = input.Split(' ');
					if(split.Length > 2) {
						bool b = true;
						int x, z;
						b &= int.TryParse(split[1], out x) & int.TryParse(split[2], out z);
						if(b) {
							exportOptions.mcaOffsetX = x;
							exportOptions.mcaOffsetZ = z;
							WriteLine("MCA terrain offset set to "+x+","+z+" ("+(x*512)+" blocks , "+z*512+" blocks)");
						} else {
							WriteWarning("Failed to parse to int");
						}
					} else {
						WriteWarning("Two integers are required!");
					}
				} else if(input.StartsWith("mcasplatmapper")) {
					exportOptions.useSplatmaps = !exportOptions.useSplatmaps;
					Program.WriteLine("MCA splatmapping "+ (exportOptions.useSplatmaps? "enabled" : "disabled"));
				} else if(batch) {
					if(input.StartsWith("equalizeheightmaps")) {
						targetValues = new ASCSummary();
						WriteLine("Fetching summary from files...");
						int i = 0;
						foreach(string path in inputFileList) {
							i++;
							var s = ASCData.GetSummary(path);
							WriteLine(i+"/"+inputFileList.Count);
							if(s.lowestValue < targetValues.lowestValue) targetValues.lowestValue = s.lowestValue;
							if(s.highestValue > targetValues.highestValue) targetValues.highestValue = s.highestValue;
							targetValues.averageValue += s.averageValue;
						}
						WriteLine("Success:");
						WriteLine("    lowest:   "+targetValues.lowestValue);
						WriteLine("    highest:  "+targetValues.highestValue);
						WriteLine("    average:  "+targetValues.averageValue);
						targetValues.averageValue /= i;
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
			if(exportOptions.outputFormats.Contains(FileFormat.MINECRAFT_REGION)) {
				if(exportOptions.fileSplitDims != 512 && (data.nrows != 512 || data.ncols != 512)) {
					WriteError("File splitting dimensions must be 512 when exporting to minecraft regions!");
					valid = false;
				}
			}
			return valid;
		}

		public static string GetInput() {
			string s;
			bool autoinput = false;
			#if DEBUG
			autoinput = autoInputEnabled && autoInputNum < autoInputs.Length;
			if(autoinput) {
				s = autoInputs[autoInputNum];
				autoInputNum++;
				WriteAutoTask("> " + s);
			} else {
			#endif
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("> ");
				s = Console.ReadLine();
				Console.ResetColor();
			#if DEBUG
			}
			#endif
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
			#if DEBUG
			autoInputs = null; //Stop any upcoming automated inputs
			#endif
		}

		private static int GetTotalExportCellsPerFile() {
			int cells = exportOptions.fileSplitDims >= 32 ? (int)Math.Pow(exportOptions.fileSplitDims, 2) : data.ncols*data.nrows;
			if(exportOptions.subsampling > 1) cells /= exportOptions.subsampling*exportOptions.subsampling;
			return cells;
		}
	}
}
