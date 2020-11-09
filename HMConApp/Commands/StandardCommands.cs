using ASCReader.Export;
using ASCReader.Util;
using System;
using System.Collections.Generic;
using static ASCReader.Program;

namespace ASCReader {
	public class StandardCommands : ASCReaderCommandHandler {

		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("subsample", "N", "Only export every N-th cell", null));
			list.Add(new ConsoleCommand("split", "N", "Split files every NxN cells (minimum 32)", null));
			list.Add(new ConsoleCommand("selection", "x1 y1 x2 y2", "Export only the selected data range(use 'preview' to see the data grid)", null));
			list.Add(new ConsoleCommand("overridecellsize", "N", "Override size per cell", null));
			list.Add(new ConsoleCommand("setrange", "N N", "Change the height data range (min - max)", null));
		}


		public override void HandleCommand(string cmd, string[] args, ExportOptions exportOptions, ASCData data) {
			if(cmd == "subsample") {
				if(args.Length > 0) {
					if(int.TryParse(args[0], out int i)) {
						exportOptions.subsampling = i;
						WriteLine("Subsampling set to: " + i);
					} else {
						WriteWarning("Can't parse to int: " + args[0]);
					}
				} else {
					WriteWarning("An integer is required!");
				}
			} else if(cmd == "split") {
				if(args.Length > 0) {
					if(int.TryParse(args[0], out int i)) {
						exportOptions.fileSplitDims = i;
						WriteLine("File splitting set to: " + i + "x" + i);
					} else {
						WriteWarning("Can't parse to int: " + args[0]);
					}
				} else {
					WriteWarning("An integer is required!");
				}
			} else if(cmd == "overridecellsize") {
				if(args.Length > 0) {
					if(float.TryParse(args[0], out float f)) {
						WriteLine("Cellsize changed from {0} to {1}", data.cellsize, f);
						data.cellsize = f;
					} else {
						WriteWarning("Can't parse to float: " + args[0]);
					}
				} else {
					WriteWarning("A number is required!");
				}
			} else if(cmd == "selection") {
				if(args.Length > 3) {
					int[] nums = new int[4];
					bool b = true;
					for(int i = 0; i < 4; i++) {
						b &= int.TryParse(args[i], out nums[i]);
					}
					if(b) {
						if(exportOptions.SetExportRange(data, nums[0], nums[1], nums[2], nums[3])) {
							WriteLine("Selection set (" + exportOptions.ExportRangeCellCount + " cells total)");
						} else {
							WriteWarning("The specified input is invalid!");
						}
					} else {
						WriteWarning("Failed to parse to int");
					}
				} else {
					if(args.Length == 0) {
						WriteLine("Selection reset");
					} else {
						WriteWarning("Four integers are required!");
					}
				}
			} else if(cmd == "setrange") {
				if(args.Length > 1) {
					bool b = true;
					b &= float.TryParse(args[0], out float min) & float.TryParse(args[1], out float max);
					if(b) {
						data.SetRange(min, max);
						WriteLine("Height rescaled successfully");
					} else {
						WriteWarning("Failed to parse to float");
					}
				} else {
					WriteWarning("Two numbers are required!");
				}
			}
		}

	}
}
