using TerrainFactory.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TerrainFactory.Import {
	public static class ImportManager {

		public static ElevationData ImportFile(string path, params string[] args) {
			var format = FileFormat.GetFromFileName(path);
			if (format != null)
			{
				var data = format.Import(path, args);
				data.RecalculateElevationRange(false);
				return data;
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
