using HMCon.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HMCon.Export {
	public static class ExportUtility {

		public static List<ASCReaderExportHandler> exportHandlers = new List<ASCReaderExportHandler>();
		public static List<FileFormat> supportedFormats = new List<FileFormat>();

		public static void RegisterHandler(ASCReaderExportHandler e) {
			exportHandlers.Add(e);
			e.AddFormatsToList(supportedFormats);
		}

		public static bool CreateFilesForSection(ASCData source, string directory, string name) {
			int numX = CurrentExportJobInfo.exportNumX;
			int numY = CurrentExportJobInfo.exportNumZ;
			foreach(FileFormat ff in CurrentExportJobInfo.exportSettings.outputFormats) {
				FileNameProvider path = new FileNameProvider(directory, name, ff);
				path.gridNum = (numX, numY);

				//TODO: move to MC plugin
				if(CurrentExportJobInfo.exportSettings.ContainsFormat("MCA")) {
					path.gridNum = (numX + CurrentExportJobInfo.exportSettings.mcaOffsetX, numY + CurrentExportJobInfo.exportSettings.mcaOffsetZ);
					path.gridNumFormat = "r.{0}.{1}";
				}
				EditFilename(path, ff);
				string fullpath = path.GetFullPath();
				Program.WriteLine("Creating file " + fullpath + " ...");
				if(ExportFile(source, ff, fullpath)) {
					Program.WriteSuccess(ff.Identifier + " file created successfully!");
				} else {
					Program.WriteError("Failed to write " + ff.Identifier + " file!");
				}
			}
			return true;
		}

		public static bool ValidateExportOptions(ExportSettings exportOptions, ASCData data, FileFormat ff) {
			bool valid = true;
			foreach(var ex in exportHandlers) {
				valid &= ex.ValidateExportOptions(exportOptions, ff, data);
			}
			return valid;
		}

		public static bool ContainsExporterForFormat(string id) {
			return GetFormatFromIdenfifier(id) != null;
		}

		public static FileFormat GetFormatFromIdenfifier(string id) {
			foreach(var f in supportedFormats) {
				if(f.IsFormat(id)) return f;
			}
			return null;
		}

		public static FileFormat GetFormatFromInput(string key) {
			foreach(var f in supportedFormats) {
				if(f.inputKey == key) return f;
			}
			return null;
		}

		public static void EditFilename(FileNameProvider path, FileFormat ff) {
			((ASCReaderExportHandler)ff.handler).EditFileName(path, ff);
		}

		public static bool ExportFile(ASCData data, FileFormat ff, string fullPath) {
			if(ff != null && ff.handler != null) {
				return ((ASCReaderExportHandler)ff.handler).Export(data, ff, fullPath);
			} else {
				if(ff != null) {
					Program.WriteError("No exporter is defined for format '" + ff.Identifier + "'!");
				} else {
					Program.WriteError("FileFormat is null!");
				}
				return false;
			}
		}

		public static void WriteFile(IExporter ie, string path, FileFormat ff) {
			using(FileStream stream = new FileStream(path, FileMode.Create)) {
				ie.WriteFile(stream, ff);
			}
		}
	}
}
