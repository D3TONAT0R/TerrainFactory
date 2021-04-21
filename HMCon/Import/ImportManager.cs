using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Import {
	public static class ImportManager {

		public static List<HMConImportHandler> importHandlers = new List<HMConImportHandler>();
		public static List<FileFormat> supportedFormats = new List<FileFormat>();

		public static void RegisterHandler(HMConImportHandler e) {
			importHandlers.Add(e);
			e.AddFormatsToList(supportedFormats);
		}

		public static ASCData ImportFile(string path, string ext) {
			ext = ext.ToLower();
			foreach(var ff in supportedFormats) {
				if(ff.extension.ToLower() == ext) {
					return ((HMConImportHandler)ff.handler).Import(path, ff);
				}
			}
			throw new NotSupportedException($"Unable to import file of type '{ext}'.");
		}
	}
}
