using ASCReader;
using ASCReader.Export;
using ASCReader.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Text;

namespace ASCReaderImagePlugin {
	public class ImageExporter : ASCReaderExportHandler {
		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("IMG_PNG-HEIGHT", "png-hm", "png", "Heightmap", this));
			list.Add(new FileFormat("IMG_PNG-NORMAL", "png-nm", "png", "Normalmap", this));
			list.Add(new FileFormat("IMG_PNG-HILLSHADE", "png-hs", "png", "Hillshade", this));
		}

		public override bool Export(string importPath, FileFormat ff, ASCData data, string filename, string fileSubName, ExportOptions exportOptions, Bounds bounds) {
			return WriteFileImage(data, filename, exportOptions.subsampling, bounds, ff);
		}

		public override string GetSuffixWithExtension(FileFormat ff) {
			string str = "";
			if(ff.IsFormat("IMG_PNG-HEIGHT")) str = "_height";
			else if(ff.IsFormat("IMG_PNG-NORMAL")) str = "_normal";
			else if(ff.IsFormat("IMG_PNG-HILLSHADE")) str = "_hillshade";
			string ext = ff.extension;
			if(!string.IsNullOrEmpty(ext)) {
				return str + "." + ext;
			} else {
				return str;
			}
		}

		public override bool ValidateExportOptions(ExportOptions options, FileFormat format, ASCData data) {
			return true;
		}

		bool WriteFileImage(ASCData source, string filename, int subsampling, Bounds bounds, FileFormat ff) {
			if(subsampling < 1) subsampling = 1;
			float[,] grid = new float[bounds.NumCols / subsampling, bounds.NumRows / subsampling];
			for(int x = 0; x < grid.GetLength(0); x++) {
				for(int y = 0; y < grid.GetLength(1); y++) {
					grid[x, y] = source.data[bounds.xMin + x * subsampling, bounds.yMin + y * subsampling];
				}
			}
			try {
				IExporter exporter = new ImageGenerator(grid, source.cellsize, ff.GetImageType(), source.lowestValue, source.highestValue);
				ExportUtility.WriteFile(exporter, filename, ff);
				return true;
			} catch(Exception e) {
				Program.WriteError("Failed to create Image file!");
				Program.WriteLine(e.ToString());
				return false;
			}
		}
	}
}
