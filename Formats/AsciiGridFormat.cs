using TerrainFactory.Export;
using TerrainFactory.Import;
using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TerrainFactory.Formats
{
	public class AsciiGridFormat : FileFormat
	{
		public override string Identifier => "ASC";
		public override string ReadableName => "ESRI ASCII Grid";
		public override string CommandKey => "asc";
		public override string Description => ReadableName;
		public override string Extension => "asc";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		protected override ElevationData ImportFile(string filepath, params string[] args)
		{
			return ASCImporter.Import(filepath, args);
		}

		protected override bool ExportFile(string path, ExportTask task)
		{
			int decimals = task.settings.GetCustomSetting("decimals", 2);

			StringBuilder fileContents = new StringBuilder();
			fileContents.AppendLine($"ncols        {task.data.CellCountX}");
			fileContents.AppendLine($"nrows        {task.data.CellCountY}");
			fileContents.AppendLine($"xllcorner    {task.data.LowerCornerPosition.X}");
			fileContents.AppendLine($"yllcorner    {task.data.LowerCornerPosition.Y}");
			fileContents.AppendLine($"cellsize     {task.data.CellSize}");
			fileContents.AppendLine($"NODATA_value {task.data.NoDataValue}");
			var grid = task.data.GetDataGrid();

			string format = "";
			int mostZeros = Math.Max(Math.Abs((int)task.data.MaxElevation).ToString().Length, Math.Abs((int)task.data.MinElevation).ToString().Length);
			for (int i = 0; i < mostZeros; i++)
			{
				format += '0';
			}
			format += ".";
			for (int i = 0; i < decimals; i++)
			{
				format += '0';
			}

			format = " " + format + ";" + "-" + format;

			for (int y = task.data.CellCountY - 1; y >= 0; y--)
			{
				for(int x = 0; x < task.data.CellCountX; x++)
				{
					if (x > 0) fileContents.Append(" ");
					fileContents.Append(grid[x, y].ToString(format));
				}
				fileContents.AppendLine();
			}

			File.WriteAllText(path, fileContents.ToString());
			return true;
		}
	}
}
