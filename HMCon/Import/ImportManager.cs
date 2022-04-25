using HMCon.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HMCon.Import {
	public static class ImportManager {

		public static HeightData ImportFile(string path, params string[] args) {
			var format = FileFormat.GetFromFileName(path);
			if (format != null)
			{
				return format.Import(path, args);
			}
			else
			{
				throw new NotSupportedException($"Unknown or unsupported format: '{Path.GetExtension(path)}'");
			}
		}

		public static bool CanImport(string path)
		{
			var format = FileFormat.GetFromFileName(path);
			return format != null && format.HasImporter;
		}
	}
}
