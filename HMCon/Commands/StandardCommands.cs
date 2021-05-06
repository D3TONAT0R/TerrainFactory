using HMCon.Export;
using HMCon.Modification;
using HMCon.Util;
using System;
using System.Collections.Generic;
using static HMCon.ConsoleOutput;
using static HMCon.Util.ConsoleCommand;

namespace HMCon {
	public class StandardCommands : HMConCommandHandler {

		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("split", "N", "Split files every NxN cells (minimum 32)", HandleSplitCmd));
			list.Add(new ConsoleCommand("clearmodifiers", "", "Removes all modifiers from the chain", HandleClearModifierCmd));
		}

		public override void AddModifiers(List<ModificationCommand> list) {
			list.Add(new ModificationCommand("subsample", "N", "Subsamples the data by factor N", HandleSubsampleMod, new SubsamplingModifier(2)));
			list.Add(new ModificationCommand("selection", "x1 y1 x2 y2", "Selects the specified area for export", HandleSelectionMod, new AreaSelectionModifier(null)));
			list.Add(new ModificationCommand("scale", "mul <pivot>", "Scales the height values with optional scaling pivot", HandleHeightScaleMod, new HeightScaleModifier(1)));
			list.Add(new ModificationCommand("heightrange", "min max", "Modifies the height range (low- and high points)", HandleHeightRangeMod, new LowHighScaleModifier(null, null, 0, 1)));
			list.Add(new ModificationCommand("resize", "sizeX", "Resizes the data grid to match the target width", HandleResizeMod, new ResizingModifier(0, true)));
			list.Add(new ModificationCommand("cellsize", "size", "Changes the data's cell size", HandleCellsizeMod, new CellSizeModifier(1)));
			list.Add(new ModificationCommand("lowhighpoints", "L H", "Changes the data's low and high points (for heightmap mapping)", HandleLowHighPointMod, new LowHighPointModifier(0, 1)));
			list.Add(new ModificationCommand("clip", "L H", "Clips height values below or above the thresholds", HandleClipMod, new ClippingModifier(0, 1)));
		}

		private bool HandleSplitCmd(Job job, string[] args) {
			int i = ParseArg<int>(args, 0);
			job.exportSettings.fileSplitDims = i;
			WriteLine("File splitting set to: " + i + "x" + i);
			return true;
		}

		private bool HandleClearModifierCmd(Job job, string[] args) {
			int l = job.exportSettings.modificationChain.Count;
			job.exportSettings.modificationChain.Clear();
			WriteLine($"Removed {l} modifiers from the chain");
			return true;
		}

		private Modifier HandleSubsampleMod(Job job, string[] args) {
			if(ParseArgOptional(args, 0, out int i)) {
				WriteLine("Subsampling set to: " + i);
				return new SubsamplingModifier(i);
			} else {
				WriteLine("Subsampling disabled");
				return new SubsamplingModifier(1);
			}
		}

		private Modifier HandleSelectionMod(Job job, string[] args) {
			if(args.Length == 0) {
				WriteLine("Selection reset");
				return new AreaSelectionModifier(null);
			}
			int x1 = ParseArg<int>(args, 0);
			int y1 = ParseArg<int>(args, 1);
			int x2 = ParseArg<int>(args, 2);
			int y2 = ParseArg<int>(args, 3);
			Bounds bounds = new Bounds(x1, y1, x2, y2);
			if(bounds.IsValid(job.CurrentData)) {
				WriteLine($"Selection set ({bounds.CellCount} cells total)");
				return new AreaSelectionModifier(bounds);
			} else {
				WriteWarning("The specified input is invalid");
			}
			return null;
		}

		private Modifier HandleHeightScaleMod(Job job, string[] args) {
			float scale = ParseArg<float>(args, 0);
			if(ParseArgOptional(args, 1, out float pivot)) {
				WriteLine($"Height rescaled successfully with pivot at {pivot}");
				return new HeightScaleModifier(pivot, scale);
			} else {
				WriteLine("Height rescaled successfully");
				return new HeightScaleModifier(scale);
			}
		}

		private Modifier HandleHeightRangeMod(Job job, string[] args) {
			float min = ParseArg<float>(args, 0);
			float max = ParseArg<float>(args, 1);
			WriteLine("Height rescaled successfully");
			return new LowHighScaleModifier(null, null, min, max);
		}

		private Modifier HandleResizeMod(Job job, string[] args) {
			int w = ParseArg<int>(args, 0);
			WriteLine($"Resizing from {job.CurrentData.GridWidth} to {w} ({Math.Round(w/(float)job.CurrentData.GridWidth*100)}%)");
			float f = ParseArg<float>(args, 0);
			WriteLine("Cellsize changed from {0} to {1}", job.CurrentData.cellSize, f);
			return null;
		}

		private Modifier HandleCellsizeMod(Job job, string[] args) {
			float f = ParseArg<float>(args, 0);
			WriteLine("Cellsize changed from {0} to {1}", job.CurrentData.cellSize, f);
			return null;
		}

		private Modifier HandleLowHighPointMod(Job job, string[] args) {
			if(args.Length >= 2) {
				float low = ParseArg<float>(args, 0);
				float high = ParseArg<float>(args, 1);
				WriteLine($"Setting low and high points to {low} and {high}");
				return new LowHighPointModifier(low, high);
			} else if(args.Length == 0) {
				WriteLine("Recalculating low and high values from data");
				return new LowHighPointModifier(0, 0);
			} else {
				throw new ArgumentException("Incorrect number of arguments");
			}
		}

		private Modifier HandleClipMod(Job job, string[] args) {
			float min = ParseArg<float>(args, 0);
			float max = ParseArg<float>(args, 1);
			WriteLine($"Clipping height data to {min} and {max}");
			return new ClippingModifier(min, max);
		}
	}
}
