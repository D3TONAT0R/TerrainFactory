using HMCon.Export;
using HMCon.Import;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {
	public abstract class ASCReaderPlugin {

		public abstract ASCReaderExportHandler GetExportHandler();

		public abstract ASCReaderImportHandler GetImportHandler();

		public abstract ASCReaderCommandHandler GetCommandHandler();
	}
}
