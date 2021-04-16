

using HMCon;
using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;

namespace ASCReaderMC {
	public class MCExportHandler : ASCReaderExportHandler {
		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("MCA", "mca", "mca", "Minecraft Region File (1.16)", this));
			list.Add(new FileFormat("MCA-RAW", "mca-raw", "mca", "Minecraft Region File (stone heightmap only) (1.16)", this));
		}

		public override bool Export(ASCData data, FileFormat ff, string fullPath) {
			if(ff.IsFormat("MCA") || ff.IsFormat("MCA-RAW")) {
				return WriteFileMCA(data, fullPath, !ff.IsFormat("MCA-RAW"), CurrentExportJobInfo.exportSettings.useSplatmaps);
			}
			return false;
		}

		public override bool ValidateExportOptions(ExportSettings options, FileFormat format, ASCData data) {
			if(options.ContainsFormat("MCA")) {
				bool sourceIs512 = (data.nrows == 512 && data.ncols == 512) || (options.exportRange.NumCols == 512 && options.exportRange.NumRows == 512);
				if(options.fileSplitDims != 512 && !sourceIs512) {
					Program.WriteError("File splitting dimensions must be 512 when exporting to minecraft regions!");
					return false;
				}
			}
			return true;
		}

		public static bool WriteFileMCA(ASCData data, string fullPath, bool decorate, bool useSplatmaps) {
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
			IExporter exporter = new MCWorldExporter(grid, decorate, useSplatmaps);
			ExportUtility.WriteFile(exporter, fullPath, ExportUtility.GetFormatFromIdenfifier("MCA"));
			return true;
		}
	}
}