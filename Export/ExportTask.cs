﻿using TerrainFactory.Formats;
using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TerrainFactory.Export
{
	public class ExportTask
	{

		public bool useGridNumbering = false;
		public int exportNumX;
		public int exportNumZ;

		public ElevationData data;
		public ExportSettings settings;
		public FileFormat format;

		public FileNameBuilder filenameBuilder;
		public string FilePath => filenameBuilder.GetFullPath();

		public bool allowOverwrite = true;

		public ExportTask(ElevationData heightData, FileFormat fileFormat, ExportSettings exportSettings, string targetDirectory, string filename)
		{
			data = heightData;
			settings = exportSettings;
			format = fileFormat;
			if (Path.GetExtension(filename).Equals("." + fileFormat.Extension, StringComparison.OrdinalIgnoreCase))
			{
				filename = filename.Substring(0, filename.Length - 1 - fileFormat.Extension.Length);
			}
			filenameBuilder = new FileNameBuilder(targetDirectory, filename, fileFormat);
		}

		public ExportTask(ElevationData heightData, FileFormat fileFormat, ExportSettings exportSettings, string targetFilePathAndName)
		 : this(heightData, fileFormat, exportSettings, Path.GetDirectoryName(targetFilePathAndName), Path.GetFileName(targetFilePathAndName))
		{

		}

		public bool Export()
		{
			return format.Export(FilePath, this);
		}
	}
}