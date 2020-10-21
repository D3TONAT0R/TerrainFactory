using ASCReader.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ASCReader.Export {
	public static class ExportUtility {

		public static List<ASCReaderExportHandler> exportHandlers = new List<ASCReaderExportHandler>();
		public static List<FileFormat> supportedFormats = new List<FileFormat>();

		public static void LoadExportHandlers() {
			string pluginPath = AppContext.BaseDirectory;
			foreach(var path in Directory.GetFiles(pluginPath, "*.dll")) {
				var assembly = Assembly.LoadFrom(path);
				foreach(var t in assembly.GetTypes()) {
					if(t.IsSubclassOf(typeof(ASCReaderExportHandler)) && !t.IsAbstract) {
						ASCReaderExportHandler handler;
						if(t == typeof(ExportHandler)) {
							handler = new ExportHandler();
						} else {
							handler = (ASCReaderExportHandler)Activator.CreateInstance(t);
							var attribute = t.GetCustomAttribute<PluginInfoAttribute>();
							if(attribute != null) {
								Program.WriteLine("Loaded Plugin '" + attribute.Name + "'");
							} else {
								Program.WriteWarning("Plugin with class '" + t.FullName + "' does not specify a Plugin name!");
							}
						}
						exportHandlers.Add(handler);
						handler.AddFormatsToList(supportedFormats);
					}
				}
			}
		}

		public static List<ConsoleCommand> GetConsoleCommands() {
			List<ConsoleCommand> list = new List<ConsoleCommand>();
			foreach(var ex in exportHandlers) {
				ex.AddCommands(list);
			}
			return list;
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
				ex.ValidateExportOptions(exportOptions, ff, ref valid, data);
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
			return ff.handler.GetSuffixWithExtension(ff);
		}

		public static bool ExportFile(string importPath, FileFormat ff, ASCData data, string filename, string subName, ExportOptions exportOptions, Bounds bounds) {
			if(ff != null && ff.handler != null) return ff.handler.Export(importPath, ff, data, filename, subName, exportOptions, bounds);
			Program.WriteError("No exporter is defined for format '" + ff.Identifier + "'!");
			return false;
		}

		public static void WriteFile(IExporter ie, string path, FileFormat ff) {
			FileStream stream = new FileStream(path, FileMode.Create);
			ie.WriteFile(stream, ff);
			stream.Close();
		}
	}
}
