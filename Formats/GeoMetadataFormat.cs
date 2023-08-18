using TerrainFactory.Export;
using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TerrainFactory.Formats
{
	public class GeoMetadataFormat : FileFormat
	{
		public override string Identifier => "GEO_META";
		public override string ReadableName => "Geo Metadata";
		public override string CommandKey => "meta";
		public override string Description => ReadableName;
		public override string Extension => "txt";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportTask task)
		{
			var fileContents = new StringBuilder();
			fileContents.AppendLine("cells_x       " + task.data.GridLengthX);
			fileContents.AppendLine("cells_y       " + task.data.GridLengthY);
			fileContents.AppendLine("xll_corner    " + task.data.lowerCornerPos.X);
			fileContents.AppendLine("yll_corner    " + task.data.lowerCornerPos.Y);
			fileContents.AppendLine("cell_size     " + task.data.cellSize);
			fileContents.AppendLine("nodata_value  " + task.data.nodataValue);
			File.WriteAllText(path, fileContents.ToString());
			return true;
		}

		public override void ModifyFileName(ExportTask task, FileNameBuilder nameBuilder)
		{
			nameBuilder.suffix = "geodata";
		}
	}
}
