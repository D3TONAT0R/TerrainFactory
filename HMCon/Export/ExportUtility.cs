using HMCon.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HMCon.Export {
	public static class ExportUtility {

		public static List<HMConExportHandler> exportHandlers = new List<HMConExportHandler>();
		public static List<FileFormat> supportedFormats = new List<FileFormat>();

		public static void RegisterHandler(HMConExportHandler e) {
			exportHandlers.Add(e);
			e.AddFormatsToList(supportedFormats);
		}

		public static bool CreateFilesForSection(HeightData source, string directory, string name) {
			int numX = CurrentExportJobInfo.exportNumX;
			int numY = CurrentExportJobInfo.exportNumZ;
			foreach(FileFormat ff in CurrentExportJobInfo.exportSettings.outputFormats) {
				FileNameProvider path = new FileNameProvider(directory, name, ff) {
					gridNum = (numX, numY)
				};
				EditFilename(path, ff);
				string fullpath = path.GetFullPath();
				ConsoleOutput.WriteLine("Creating file " + fullpath + " ...");
				if(ExportFile(source, ff, fullpath)) {
					ConsoleOutput.WriteSuccess(ff.Identifier + " file created successfully!");
				} else {
					ConsoleOutput.WriteError("Failed to write " + ff.Identifier + " file!");
				}
			}
			return true;
		}

		public static bool ValidateExportSettings(ExportSettings settings, HeightData data) {
			bool valid = true;
			foreach(var ff in settings.outputFormats) {
				valid &= ValidateExportSettings(settings, data, ff);
			}
			return valid;
		}

		public static bool ValidateExportSettings(ExportSettings settings, HeightData data, FileFormat ff) {
			bool valid = true;
			foreach(var ex in exportHandlers) {
				valid &= ex.AreExportSettingsValid(settings, ff, data);
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
			((HMConExportHandler)ff.handler).EditFileName(path, ff);
		}

		public static bool ExportFile(HeightData data, FileFormat ff, string fullPath) {
			if(ff != null && ff.handler != null) {
				return ((HMConExportHandler)ff.handler).Export(data, ff, fullPath);
			} else {
				if(ff != null) {
					ConsoleOutput.WriteError("No exporter is defined for format '" + ff.Identifier + "'!");
				} else {
					ConsoleOutput.WriteError("FileFormat is null!");
				}
				return false;
			}
		}

		public static void WriteFile(IExporter ie, string path, FileFormat ff) {
			FileStream stream = null;
			if(ie.NeedsFileStream(ff)) {
				//Only create a file stream if the Exporter requires it
				stream = new FileStream(path, FileMode.Create);
			}
			try {
				ie.WriteFile(stream, path, ff);
			} finally {
				if(stream != null) {
					stream.Close();
					stream.Dispose();
				}
			}
		}
	}
}
