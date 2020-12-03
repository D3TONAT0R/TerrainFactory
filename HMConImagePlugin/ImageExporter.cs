using HMCon;
using HMCon.Export;
using HMCon.Util;
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
			list.Add(new FileFormat("IMG_PNG-HM", "png-hm", "png", "Heightmap", this));
			list.Add(new FileFormat("IMG_PNG-NM", "png-nm", "png", "Normalmap", this));
			list.Add(new FileFormat("IMG_PNG-HS", "png-hs", "png", "Hillshade", this));
		}

		public override bool Export(string importPath, FileFormat ff, ASCData data, string filename, string fileSubName, ExportOptions exportOptions, Bounds bounds) {
			string path = Path.ChangeExtension(filename, null);
			filename = path + fileSubName + ".png";
			return WriteFileImage(data, filename, exportOptions.subsampling, bounds, ff);
		}

		public override string GetSuffixWithExtension(FileFormat ff) {
			string str = "";
			if(ff.IsFormat("IMG_PNG-HM")) str = "_height";
			else if(ff.IsFormat("IMG_PNG-NM")) str = "_normal";
			else if(ff.IsFormat("IMG_PNG-HS")) str = "_hillshade";
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
				IExporter exporter = new ImageGenerator(grid, source.cellsize, ff.GetImageType(), source.lowPoint, source.highPoint);
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
