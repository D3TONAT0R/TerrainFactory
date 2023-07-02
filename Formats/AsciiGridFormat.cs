using HMCon.Export;
using HMCon.Import;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HMCon.Formats
{
	public class AsciiGridFormat : FileFormat
	{
		public override string Identifier => "ASC";
		public override string ReadableName => "ESRI ASCII Grid";
		public override string CommandKey => "asc";
		public override string Description => ReadableName;
		public override string Extension => "asc";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		protected override HeightData ImportFile(string filepath, params string[] args)
		{
			return ASCImporter.Import(filepath, args);
		}

		protected override bool ExportFile(string path, ExportTask task)
		{
			int decimals = task.settings.GetCustomSetting("decimals", 2);

			StringBuilder fileContents = new StringBuilder();
			fileContents.AppendLine($"ncols        {task.data.GridLengthX}");
			fileContents.AppendLine($"nrows        {task.data.GridLengthY}");
			fileContents.AppendLine($"xllcorner    {task.data.lowerCornerPos.X}");
			fileContents.AppendLine($"yllcorner    {task.data.lowerCornerPos.Y}");
			fileContents.AppendLine($"cellsize     {task.data.cellSize}");
			fileContents.AppendLine($"NODATA_value {task.data.nodataValue}");
			var grid = task.data.GetDataGrid();

			string format = "";
			int mostZeros = Math.Max(Math.Abs((int)task.data.highestValue).ToString().Length, Math.Abs((int)task.data.lowestValue).ToString().Length);
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

			for (int y = task.data.GridLengthY - 1; y >= 0; y--)
			{
				for(int x = 0; x < task.data.GridLengthX; x++)
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
