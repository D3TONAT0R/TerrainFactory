using HMCon.Export;
using HMCon.Import;

namespace HMCon {

	public class FileFormat {
		public string Identifier {
			get { return identifier; }
			set { identifier = value.ToUpper(); }
		}

		private string identifier;
		public string InputKey { get; private set; }
		public string Description { get; private set; }
		public string Extension { get; private set; }
		public HMConImportHandler importHandler;
		public HMConExportHandler exportHandler;

		private FileFormat(string id, string input, string ext, string desc) {
			Identifier = id.ToUpper();
			InputKey = input.ToLower();
			Description = desc;
			Extension = ext;
		}

		public FileFormat(string id, string input, string ext, string desc, HMConExportHandler handler) : this(id, input, ext, desc) {
			exportHandler = handler;
		}

		public FileFormat(string id, string input, string ext, string desc, HMConImportHandler handler) : this(id, input, ext, desc) {
			importHandler = handler;
		}

		public bool IsFormat(params string[] ids) {
			foreach (var id in ids)
			{
				if (id.ToUpper() == identifier) return true;
			}
			return false;
		}
	}
}