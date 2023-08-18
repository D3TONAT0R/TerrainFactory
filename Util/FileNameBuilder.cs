using TerrainFactory.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TerrainFactory.Util {
	public class FileNameBuilder {

		public string directoryPath;

		public string prefix;
		public string filename;
		public (int x, int y)? tileIndex = null;
		public string suffix;

		public FileFormat extension;

		public string fullNameFormat = "{0}{1}{2}{3}.{4}";

		public string prefixFormat = "{0}_";
		public string gridNumFormat = "_{0},{1}";
		public string suffixFormat = "_{0}";

		public FileNameBuilder(string path, string name, FileFormat ext) {
			directoryPath = path;
			filename = name;
			extension = ext;
		}

		public string GetFullPath() {
			if(extension == null) {
				throw new NullReferenceException("Extension was null");
			}
			return Path.Combine(directoryPath, GetFileName());
		}

		public string GetFileName() {
			StringBuilder sb = new StringBuilder();
			if(!string.IsNullOrEmpty(prefix)) sb.Append(string.Format(prefixFormat, prefix));
			sb.Append(filename);
			if(tileIndex != null) sb.Append(string.Format(gridNumFormat, tileIndex.Value.x, tileIndex.Value.y));
			if(!string.IsNullOrEmpty(suffix)) sb.Append(string.Format(suffixFormat, suffix));
			sb.Append("." + extension.Extension);
			return sb.ToString();
		}
	}
}
