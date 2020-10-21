using ASCReader.Export;

namespace ASCReader {

	public class FileFormat {
		public string Identifier {
			get { return identifier; }
			set { identifier = value.ToUpper(); }
		}

		private string identifier;
		public string inputKey;
		public string description;
		public string extension;
		public ASCReaderExportHandler handler;

		public FileFormat(string id, string input, string ext, string desc, ASCReaderExportHandler handler) {
			Identifier = id;
			inputKey = input.ToLower();
			description = desc;
			extension = ext;
			this.handler = handler;
		}

		public bool IsFormat(string id) {
			return id.ToUpper() == identifier;
		}
	}
}