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
		public const float DEFAULT_NODATA_VALUE = -9999f;

		public override string Identifier => "ASC";
		public override string ReadableName => "ESRI ASCII Grid";
		public override string Command => "asc";
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

			using(var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				using(var writer = new StreamWriter(fileStream))
				{
					writer.WriteLine($"ncols        {task.data.CellCountX}");
					writer.WriteLine($"nrows        {task.data.CellCountY}");
					writer.WriteLine($"xllcorner    {task.data.LowerCornerPosition.X}");
					writer.WriteLine($"yllcorner    {task.data.LowerCornerPosition.Y}");
					writer.WriteLine($"cellsize     {task.data.CellSize}");
					float nodataValue = task.settings.nodataValue ?? DEFAULT_NODATA_VALUE;
					writer.WriteLine($"NODATA_value {nodataValue}");

					var grid = task.data.GetDataGrid();
					string format = "";
					int mostZeros = Math.Max(Math.Abs((int)task.data.MaxElevation).ToString().Length, Math.Abs((int)task.data.MinElevation).ToString().Length);
					for(int i = 0; i < mostZeros; i++)
					{
						format += '0';
					}
					format += ".";
					for(int i = 0; i < decimals; i++)
					{
						format += '0';
					}
					format = $" {format};-{format}";

					for(int y = task.data.CellCountY - 1; y >= 0; y--)
					{
						for(int x = 0; x < task.data.CellCountX; x++)
						{
							if(x > 0) writer.Write(" ");
							writer.Write(grid[x, y].ToString(format));
						}
						writer.WriteLine();
					}
					return true;
				}
			}
		}
	}
}
