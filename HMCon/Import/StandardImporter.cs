using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Import {
	class StandardImporter : HMConImportHandler {
		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("ASC", "ASC", "asc", "ESRI ASCII grid", this));
		}

		public override HeightData Import(string importPath, FileFormat ff, params string[] args) {
			if(ff.IsFormat("ASC")) {
				return ASCImporter.Import(importPath);
			}
			return null;
		}
	}
}
