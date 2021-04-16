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

		public override bool Export(ASCData data, FileFormat ff, string fullPath) {
			if(CurrentExportJobInfo.exportSettings == null) {
				throw new NullReferenceException("exportSettings was null");
			}
			if(data == null) {
				throw new NullReferenceException("data was null");
			}
			return WriteFileImage(data, fullPath, CurrentExportJobInfo.exportSettings.subsampling, CurrentExportJobInfo.bounds ?? data.GetBounds(), ff);
		}

		public override void EditFileName(FileNameProvider path, FileFormat fileFormat) {
			if(fileFormat.IsFormat("IMG_PNG-HM")) path.suffix = "_height";
			else if(fileFormat.IsFormat("IMG_PNG-NM")) path.suffix = "_normal";
			else if(fileFormat.IsFormat("IMG_PNG-HS")) path.suffix = "_hillshade";
		}

		public override bool ValidateExportOptions(ExportSettings options, FileFormat format, ASCData data) {
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
			IExporter exporter = new ImageGenerator(grid, source.cellsize, ff.GetImageType(), source.lowPoint, source.highPoint);
			ExportUtility.WriteFile(exporter, filename, ff);
			return true;
		}
	}
}
