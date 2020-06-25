using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ASCReader.Export {
	public interface IExporter {
		public void WriteFile(FileStream stream, FileFormat filetype);
	}
}
