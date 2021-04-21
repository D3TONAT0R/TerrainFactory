using HMCon.Util;
using System.Collections.Generic;

namespace HMCon.Import {
	public abstract class HMConImportHandler {

		public abstract void AddFormatsToList(List<FileFormat> list);

		public abstract ASCData Import(string importPath, FileFormat ff);
	}
}
