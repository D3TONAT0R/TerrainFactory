using HMCon.Util;
using System.Collections.Generic;
using System.IO;

namespace HMCon.Export {
	public abstract class HMConExportHandler {

		public abstract bool AreExportSettingsValid(ExportSettings options, FileFormat format, HeightData data);

		public virtual void HandleFileName(ExportSettings options, FileFormat format, ref string filename) {

		}

		public abstract bool Export(HeightData data, FileFormat ff, string fullPath);

		public virtual void EditFileName(FileNameProvider path, FileFormat fileFormat) {
			
		}

		public abstract void AddFormatsToList(List<FileFormat> list);

		//public abstract void WriteFile(FileStream stream, FileFormat filetype);
	}
}
