using ASCReader.Export;
using ASCReader.Import;
using ASCReader.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASCReader {
	public abstract class ASCReaderPlugin {

		public abstract ASCReaderExportHandler GetExportHandler();

		public abstract ASCReaderImportHandler GetImportHandler();

		public abstract ASCReaderCommandHandler GetCommandHandler();
	}
}
