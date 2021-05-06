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

		public static bool ValidateExportSettings(ExportSettings settings, HeightData data) {
			if(settings.outputFormats.Count == 0) {
				ConsoleOutput.WriteError("You must specify at least one output format");
				return false;
			}
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
				if(f.InputKey == key) return f;
			}
			return null;
		}

		public static void WriteFile(IExporter ie, string path, FileFormat ff) {
			FileStream stream = null;
			if(ie.NeedsFileStream(ff)) {
				//Only create a file stream if the exporter needs one
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
