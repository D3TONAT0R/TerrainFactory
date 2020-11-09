using System;
using System.Collections.Generic;
using System.Text;

namespace ASCReader.Import {
	public static class ImportManager {

		public static List<ASCReaderImportHandler> importHandlers = new List<ASCReaderImportHandler>();
		public static List<FileFormat> supportedFormats = new List<FileFormat>();

		public static void RegisterHandler(ASCReaderImportHandler e) {
			importHandlers.Add(e);
			e.AddFormatsToList(supportedFormats);
		}

		public static ASCData ImportFile(string path, string ext) {
			ext = ext.ToLower();
			foreach(var ff in supportedFormats) {
				if(ff.extension.ToLower() == ext) {
					return ((ASCReaderImportHandler)ff.handler).Import(path, ff);
				}
			}
			return null;
		}
	}
}
