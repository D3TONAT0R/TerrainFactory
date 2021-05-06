using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HMCon.Import {
	public static class ImportManager {

		public static List<HMConImportHandler> importHandlers = new List<HMConImportHandler>();
		public static List<FileFormat> supportedFormats = new List<FileFormat>();

		public static void RegisterHandler(HMConImportHandler e) {
			importHandlers.Add(e);
			e.AddFormatsToList(supportedFormats);
		}

		public static HeightData ImportFile(string path, params string[] args) {
			string ext = Path.GetExtension(path).Replace(".", "").ToLower();
			foreach(var ff in supportedFormats) {
				if(ff.Extension.ToLower() == ext) {
					return ff.importHandler.Import(path, ff, args);
				}
			}
			throw new NotSupportedException($"Unable to import file of type '{ext}'.");
		}

		public static bool SupportsFileType(string path) {
			string ext = Path.GetExtension(path).Replace(".", "").ToLower();
			foreach(var ff in supportedFormats) {
				if(ff.Extension.ToLower() == ext) return true;
			}
			return false;
		}
	}
}
