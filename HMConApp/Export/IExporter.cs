using System;
using System.IO;

namespace ASCReader.Export {

	public interface IExporter {
		void WriteFile(FileStream stream, FileFormat filetype);
	}
}
