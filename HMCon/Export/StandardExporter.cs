using HMCon.Export.Exporters;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using static HMCon.Program;

namespace HMCon.Export {

	class StandardExporter : ASCReaderExportHandler {

		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("ASC", "asc", "asc", "ESRI ASCII grid (same as input)", this));
			list.Add(new FileFormat("PTS_XYZ", "xyz", "xyz", "ASCII-XYZ points", this));
		}

		public override bool Export(ASCData data, FileFormat ff, string fullPath) {
			if(ff.IsPointFormat()) {
				return WriteFilePointData(data, ff, fullPath);
			} else {
				return false;
			}
		}


		public static bool WriteFilePointData(ASCData source, FileFormat ff, string fullPath) {
			try {
				if(ff.IsFormat("ASC") || ff.IsFormat("PTS_XYZ")) {
					IExporter exporter;
					exporter = new PointDataExporter(source, CurrentExportJobInfo.exportSettings.subsampling, CurrentExportJobInfo.bounds ?? source.GetBounds());
					ExportUtility.WriteFile(exporter, fullPath, ff);
					return true;
				} else {
					WriteError("Don't know how to export " + ff.ToString());
					return false;
				}
			} catch(Exception e) {
				WriteError("Failed to create Point data file!");
				WriteLine(e.ToString());
				return false;
			}
		}

		public override bool ValidateExportOptions(ExportSettings options, FileFormat format, ASCData data) {
			if(options.outputFormats.Count == 0) {
				WriteWarning("No export format is defined! choose at least one format for export!");
				return false;
			}
			return true;
		}
	}
}
