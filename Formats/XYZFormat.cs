﻿using TerrainFactory.Export;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TerrainFactory.Formats
{
	public class XYZFormat : FileFormat
	{
		public override string Identifier => "XYZ";
		public override string ReadableName => "Text-based XYZ point array";
		public override string CommandKey => "xyz";
		public override string Description => ReadableName;
		public override string Extension => "xyz";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportTask task)
		{
			StringBuilder contents = new StringBuilder();
			var grid = task.data.GetDataGrid();
			var cs = task.data.CellSize;
			for (int y = 0; y < task.data.CellCountY; y++)
			{
				for (int x = 0; x < task.data.CellCountX; x++)
				{
					float z = grid[x, y];
					if (z != task.data.NoDataValue)
					{
						contents.AppendLine($"{x * cs} {y * cs} {z}");
					}
				}
			}
			File.WriteAllText(path, contents.ToString());
			return true;
		}
	}
}
