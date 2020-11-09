using ASCReader.Export.Exporters;
using ASCReader.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using static ASCReader.Program;

namespace ASCReader.Export {

	class StandardExporter : ASCReaderExportHandler {

		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("ASC", "asc", "asc", "ESRI ASCII grid (same as input)", this));
			list.Add(new FileFormat("PTS_XYZ", "xyz", "xyz", "ASCII-XYZ points", this));
		}

		public override bool Export(string sourceFilePath, FileFormat ff, ASCData source, string fullpath, string fileSubName, ExportOptions options, Bounds bounds) {
			if(ff.IsPointFormat()) {
				return WriteFilePointData(source, fullpath, options.subsampling, bounds, ff);
			} else {
				return false;
			}
		}


		public static bool WriteFilePointData(ASCData source, string filename, int subsampling, Bounds bounds, FileFormat ff) {
			try {
				if(ff.IsFormat("ASC") || ff.IsFormat("PTS_XYZ")) {
					IExporter exporter;
					exporter = new PointDataExporter(source, subsampling, bounds);
					ExportUtility.WriteFile(exporter, filename, ff);
					return true;
				} else {
					Program.WriteError("Don't know how to export " + ff.ToString());
					return false;
				}
			} catch(Exception e) {
				Program.WriteError("Failed to create Point data file!");
				Program.WriteLine(e.ToString());
				return false;
			}
		}

		public override bool ValidateExportOptions(ExportOptions options, FileFormat format, ASCData data) {
			if(options.outputFormats.Count == 0) {
				Program.WriteWarning("No export format is defined! choose at least one format for export!");
				return false;
			}
			return true;
		}
	}
}
