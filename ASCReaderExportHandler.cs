using ASCReader.Util;
using System.Collections.Generic;

namespace ASCReader.Export {
	public abstract class ASCReaderExportHandler {

		public abstract void ValidateExportOptions(ExportOptions options, FileFormat format, ref bool valid, ASCData data);

		public virtual void AddCommands(List<ConsoleCommand> list) {

		}

		public virtual void HandleCommand(string cmd, string[] args, ExportOptions options, ASCData data) {

		}

		public virtual void HandleFileName(ExportOptions options, FileFormat format, ref string filename) {

		}

		public abstract bool Export(string importPath, FileFormat ff, ASCData data, string filename, string fileSubName, ExportOptions exportOptions, Bounds bounds);

		public abstract string GetSuffixWithExtension(FileFormat fileFormat);

		public abstract void AddFormatsToList(List<FileFormat> list);
	}
}
