using System;
using System.IO;

namespace HMCon.Export {

	public interface IExporter {
		void WriteFile(FileStream stream, string path, FileFormat filetype);

		bool NeedsFileStream(FileFormat format);
	}
}
