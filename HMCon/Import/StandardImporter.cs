using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Import {
	class StandardImporter : HMConImportHandler {
		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("ASC", "ASC", "asc", "ESRI ASCII grid", this));
		}

		public override ASCData Import(string importPath, FileFormat ff) {
			if(ff.IsFormat("ASC")) {
				return new ASCData(importPath);
			}
			return null;
		}
	}
}
