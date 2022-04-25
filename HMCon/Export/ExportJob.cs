using HMCon.Formats;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HMCon.Export
{
	public class ExportJob
	{

		public bool useGridNumbering = false;
		public int exportNumX;
		public int exportNumZ;

		public HeightData data;
		public ExportSettings settings;
		public FileFormat format;

		public FileNameBuilder nameBuilder;
		public string FilePath => nameBuilder.GetFullPath();

		public bool allowOverwrite = true;

		public ExportJob(HeightData heightData, FileFormat fileFormat, ExportSettings exportSettings, string targetDirectory, string filename)
		{
			data = heightData;
			settings = exportSettings;
			format = fileFormat;
			if (Path.GetExtension(filename).Equals("." + fileFormat.Extension, StringComparison.OrdinalIgnoreCase))
			{
				filename = filename.Substring(0, filename.Length - 1 - fileFormat.Extension.Length);
			}
			nameBuilder = new FileNameBuilder(targetDirectory, filename, fileFormat);
		}

		public void Export()
		{
			format.Export(FilePath, this);
		}
	}
}