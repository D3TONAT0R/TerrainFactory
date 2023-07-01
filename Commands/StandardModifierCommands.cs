using HMCon.Modification;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;
using static HMCon.Commands.CommandParser;
using static HMCon.ConsoleOutput;
using HMCon.Export;

namespace HMCon.Commands
{
	public static class StandardModifierCommands
	{
		[ModifierCommand("subsample", "N", "Subsamples the data by factor N")]
		public static Modifier HandleSubsampleMod(Worksheet sheet, string[] args)
		{
			if(ParseArgOptional(args, 0, out int i))
			{
				WriteLine("Subsampling set to: " + i);
				return new SubsamplingModifier(i);
			}
			else
			{
				WriteLine("Subsampling disabled");
				return new SubsamplingModifier(1);
			}
		}

		[ModifierCommand("areaselect", "x1 y1 x2 y2", "Selects an area defined by lower and upper bounds")]
		public static Modifier HandleAreaSelectionMod(Worksheet sheet, string[] args)
		{
			if(args.Length == 0)
			{
				WriteLine("Selection reset");
				return new BoundedAreaSelectionModifier(null);
			}
			int x1 = ParseArg<int>(args, 0);
			int y1 = ParseArg<int>(args, 1);
			int x2 = ParseArg<int>(args, 2);
			int y2 = ParseArg<int>(args, 3);
			Bounds bounds = new Bounds(x1, y1, x2, y2);
			if(bounds.IsValid(sheet.CurrentData))
			{
				WriteLine($"Selection set ({bounds.CellCount} cells total)");
				return new BoundedAreaSelectionModifier(bounds);
			}
			else
			{
				WriteWarning("The specified input is invalid");
			}
			return null;
		}

		[ModifierCommand("radselect", "$cx $cy size", "Selects an area defined by a center point and area size")]
		public static Modifier HandleCenteredSelectionMod(Worksheet sheet, string[] args)
		{
			if(args.Length == 0)
			{
				WriteLine("Selection reset");
				return new CenteredAreaSelectionModifier();
			}
			var cx = ParseArg<Coordinate>(args, 0);
			var cy = ParseArg<Coordinate>(args, 1);
			var size = ParseArg<float>(args, 2);
			WriteLine($"Selection set (center: ({cx},{cy}) size: {size})");
			return new CenteredAreaSelectionModifier(cx, cy, size);
		}

		[ModifierCommand("scale", "mul <pivot>", "Scales the height values with optional scaling pivot")]
		public static Modifier HandleHeightScaleMod(Worksheet sheet, string[] args)
		{
			float scale = ParseArg<float>(args, 0);
			if(ParseArgOptional(args, 1, out float pivot))
			{
				WriteLine($"Height rescaled successfully with pivot at {pivot}");
				return new HeightScaleModifier(pivot, scale);
			}
			else
			{
				WriteLine("Height rescaled successfully");
				return new HeightScaleModifier(scale);
			}
		}

		[ModifierCommand("remap", "old-H1 new-H1 old-H2 new-H2", "Remaps the given heights to match the new heights")]
		public static Modifier HandleRemapMod(Worksheet sheet, string[] args)
		{
			return new HeightRemapModifier(ParseArg<float>(args, 0), ParseArg<float>(args, 1), ParseArg<float>(args, 2), ParseArg<float>(args, 3));
		}

		[ModifierCommand("heightrange", "min max", "Modifies the height range (low- and high points)")]
		public static Modifier HandleHeightRangeMod(Worksheet sheet, string[] args)
		{
			float min = ParseArg<float>(args, 0);
			float max = ParseArg<float>(args, 1);
			WriteLine("Height rescaled successfully");
			return new LowHighScaleModifier(null, null, min, max);
		}

		[ModifierCommand("resize", "sizeX", "Resizes the data grid to match the target width")]
		public static Modifier HandleResizeMod(Worksheet sheet, string[] args)
		{
			int w = ParseArg<int>(args, 0);
			WriteLine($"Resizing from {sheet.CurrentData.GridLengthX} to {w} ({Math.Round(w / (float)sheet.CurrentData.GridLengthX * 100)}%)");
			return new ResizingModifier(w, false);
		}

		[ModifierCommand("cellsize", "size", "Changes the data's cell size")]
		public static Modifier HandleCellsizeMod(Worksheet sheet, string[] args)
		{
			float f = ParseArg<float>(args, 0);
			WriteLine($"Cellsize changed from {sheet.CurrentData.cellSize} to {f}");
			return new CellSizeModifier(f);
		}

		[ModifierCommand("lowhighpoints", "L H", "Changes the data's low and high points (for heightmap mapping)")]
		public static Modifier HandleLowHighPointMod(Worksheet sheet, string[] args)
		{
			if(args.Length >= 2)
			{
				float low = ParseArg<float>(args, 0);
				float high = ParseArg<float>(args, 1);
				WriteLine($"Setting low and high points to {low} and {high}");
				return new LowHighPointModifier(low, high);
			}
			else if(args.Length == 0)
			{
				WriteLine("Recalculating low and high values from data");
				return new LowHighPointModifier(0, 0);
			}
			else
			{
				throw new ArgumentException("Incorrect number of arguments");
			}
		}

		[ModifierCommand("clip", "L H", "Clips height values below or above the thresholds")]
		public static Modifier HandleClipMod(Worksheet sheet, string[] args)
		{
			float min = ParseArg<float>(args, 0);
			float max = ParseArg<float>(args, 1);
			WriteLine($"Clipping height data to {min} and {max}");
			return new ClippingModifier(min, max);
		}
	}
}
