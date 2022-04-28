using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HMCon.Formats
{
	public class GeoMetadataFormat : FileFormat
	{
		public override string Identifier => "GEO_META";
		public override string ReadableName => "Geo Metadata";
		public override string CommandKey => "meta";
		public override string Description => ReadableName;
		public override string Extension => "txt";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportJob job)
		{
			var fileContents = new StringBuilder();
			fileContents.AppendLine("cells_x       " + job.data.GridWidth);
			fileContents.AppendLine("cells_y       " + job.data.GridHeight);
			fileContents.AppendLine("xll_corner    " + job.data.lowerCornerPos.X);
			fileContents.AppendLine("yll_corner    " + job.data.lowerCornerPos.Y);
			fileContents.AppendLine("cell_size     " + job.data.cellSize);
			fileContents.AppendLine("nodata_value  " + job.data.nodata_value);
			File.WriteAllText(path, fileContents.ToString());
			return true;
		}

		public override void ModifyFileName(ExportJob exportJob, FileNameBuilder nameBuilder)
		{
			nameBuilder.suffix = "geodata";
		}
	}
}
