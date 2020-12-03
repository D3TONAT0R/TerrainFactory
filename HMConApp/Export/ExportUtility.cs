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

		public static bool CreateFilesForSection(ASCData source, string sourceFilePath, string path, ExportOptions options, Bounds bounds) {
			int numX = CurrentExportJobInfo.exportNumX;
			int numY = CurrentExportJobInfo.exportNumX;
			foreach(FileFormat ff in options.outputFormats) {
				string subname;
				if(options.ContainsFormat("MCA")) {
					subname = "r." + (numX + options.mcaOffsetX) + "." + (numY + options.mcaOffsetZ);
				} else {
					subname = numX + "," + numY;
				}
				if(!string.IsNullOrEmpty(subname)) {
					string ext = Path.GetExtension(path);
					string p = path.Substring(0, path.Length - ext.Length);
					if(!path.EndsWith("\\")) {
						path = p + "_" + subname;
					} else {
						path = p + subname;
					}
				}
				string fullpath = path + GetSuffixWithExtension(ff);
				Program.WriteLine("Creating file " + fullpath + " ...");
				if(ExportFile(sourceFilePath, ff, source, fullpath, subname, options, bounds)) {
					Program.WriteSuccess(ff.Identifier + " file created successfully!");
				} else {
					Program.WriteError("Failed to write " + ff.Identifier + " file!");
				}
			}
			return true;
		}

		public static bool ValidateExportOptions(ExportOptions exportOptions, ASCData data, FileFormat ff) {
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

		public static string GetSuffixWithExtension(FileFormat ff) {
			return ((ASCReaderExportHandler)ff.handler).GetSuffixWithExtension(ff);
		}

		public static bool ExportFile(string importPath, FileFormat ff, ASCData data, string filename, string subName, ExportOptions exportOptions, Bounds bounds) {
			if(ff != null && ff.handler != null) {
				return ((ASCReaderExportHandler)ff.handler).Export(importPath, ff, data, filename, subName, exportOptions, bounds);
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
			FileStream stream = new FileStream(path, FileMode.Create);
			ie.WriteFile(stream, ff);
			stream.Close();
		}
	}
}
