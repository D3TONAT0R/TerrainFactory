

using HMConImage;
using HMCon;
using HMCon.Export;
using HMCon.Util;
using HMConMCPlugin;
using System;
using System.Collections.Generic;

namespace HMConMC {
	public class MCExportHandler : HMConExportHandler {
		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("MCR", "mca", "mca", "Minecraft Region File (1.16)", this));
			list.Add(new FileFormat("MCR-RAW", "mca-raw", "mca", "Minecraft Region File (stone heightmap only) (1.16)", this));
			if(HMCon.PluginLoader.IsPluginLoaded("ImagePlugin")) {
				list.Add(new FileFormat("IMG_MCR", "mca-map", "png", "Region overview map (MC)", this));
			}
			list.Add(new FileFormat("MCW", "mcw", "", "Minecraft World Save (1.16)", this));
		}

		public override bool Export(ASCData data, FileFormat ff, string fullPath) {
			if(ff.IsFormat("MCR") || ff.IsFormat("MCR-RAW")) {
				return WriteFileMCA(data, fullPath, !ff.IsFormat("MCR-RAW"), CurrentExportJobInfo.exportSettings.useSplatmaps);
			} else if(ff.IsFormat("IMG_MCR")) {
				ExportUtility.WriteFile(new OverviewmapExporter(CurrentExportJobInfo.importedFilePath), fullPath, ff);
			} else if(ff.IsFormat("MCW")) {
				return WriteWorldSave(data, fullPath, true, CurrentExportJobInfo.exportSettings.useSplatmaps);
			}
			return false;
		}

		public override bool ValidateExportOptions(ExportSettings options, FileFormat format, ASCData data) {
			if(options.ContainsFormat("MCR", "MCR-RAW")) {
				bool sourceIs512 = (data.nrows == 512 && data.ncols == 512) || (options.exportRange.NumCols == 512 && options.exportRange.NumRows == 512);
				if(options.fileSplitDims != 512 && !sourceIs512) {
					Program.WriteError("File splitting dimensions must be 512 when exporting to minecraft regions!");
					return false;
				}
			}
			return true;
		}

		public override void EditFileName(FileNameProvider path, FileFormat fileFormat) {
			if(CurrentExportJobInfo.exportSettings.ContainsFormat("MCR")) {
				path.gridNum = (CurrentExportJobInfo.exportNumX + CurrentExportJobInfo.exportSettings.mcaOffsetX, CurrentExportJobInfo.exportNumZ + CurrentExportJobInfo.exportSettings.mcaOffsetZ);
				path.gridNumFormat = "r.{0}.{1}";
			}
			if(fileFormat.Identifier == "MCW") {
				path.gridNumFormat = "";
			}
			if(fileFormat.IsFormat("IMG_MCR")) path.suffix = "overview";
		}

		public static bool WriteFileMCA(ASCData data, string fullPath, bool decorate, bool useSplatmaps) {
			var grid = GetGrid(data);
			IExporter exporter = new MCWorldExporter(grid, decorate, useSplatmaps);
			ExportUtility.WriteFile(exporter, fullPath, ExportUtility.GetFormatFromIdenfifier("MCR"));
			return true;
		}

		public static bool WriteWorldSave(ASCData data, string fullPath, bool decorate, bool useSplatmaps) {
			var grid = GetGrid(data);
			IExporter exporter = new MCWorldExporter(grid, decorate, useSplatmaps);
			ExportUtility.WriteFile(exporter, fullPath, ExportUtility.GetFormatFromIdenfifier("MCW"));
			return true;
		}

		private static float[,] GetGrid(ASCData data) {
			int subsampling = CurrentExportJobInfo.exportSettings.subsampling;
			if(subsampling < 1) subsampling = 1;
			var bounds = CurrentExportJobInfo.bounds ?? data.GetBounds();
			float[,] grid = new float[bounds.NumCols / subsampling, bounds.NumRows / subsampling];
			var zLength = grid.GetLength(1);
			for(int x = 0; x < grid.GetLength(0); x++) {
				for(int z = 0; z < grid.GetLength(1); z++) {
					//Note: Minecraft's Z coordinate is upside-down, Z starts from top
					grid[x, zLength - z - 1] = data.data[bounds.xMin + x * subsampling, bounds.yMin + z * subsampling];
				}
			}
			return grid;
		}
	}
}