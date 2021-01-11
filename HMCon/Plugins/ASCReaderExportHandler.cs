using HMCon.Util;
using System.Collections.Generic;
using System.IO;

namespace HMCon.Export {
	public abstract class ASCReaderExportHandler {

		public abstract bool ValidateExportOptions(ExportOptions options, FileFormat format, ASCData data);

		public virtual void HandleFileName(ExportOptions options, FileFormat format, ref string filename) {

		}

		public abstract bool Export(string importPath, FileFormat ff, ASCData data, string filename, string fileSubName, ExportOptions exportOptions, Bounds bounds);

		public virtual string GetSuffixWithExtension(FileFormat fileFormat) {
			string ext = fileFormat.extension;
			if(!string.IsNullOrEmpty(ext)) {
				return "." + ext;
			} else {
				return "";
			}
		}

		public abstract void AddFormatsToList(List<FileFormat> list);

		//public abstract void WriteFile(FileStream stream, FileFormat filetype);
	}
}
