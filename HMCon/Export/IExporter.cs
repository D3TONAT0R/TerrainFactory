using System;
using System.IO;

namespace HMCon.Export {

	public interface IExporter {
		void WriteFile(FileStream stream, FileFormat filetype);
	}
}
