using ASCReader.Util;
using System.Collections.Generic;

namespace ASCReader.Import {
	public abstract class ASCReaderImportHandler {

		public abstract void AddFormatsToList(List<FileFormat> list);

		public abstract ASCData Import(string importPath, FileFormat ff);
	}
}
