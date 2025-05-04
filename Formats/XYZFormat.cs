using TerrainFactory.Export;
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
		public override string Command => "xyz";
		public override string Description => ReadableName;
		public override string Extension => "xyz";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportTask task)
		{
			var cs = task.data.CellSize;
			using(var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				using(var writer = new StreamWriter(stream))
				{
					for(int y = 0; y < task.data.CellCountY; y++)
					{
						for(int x = 0; x < task.data.CellCountX; x++)
						{
							float z = task.data.GetElevationAtCellUnchecked(x, y);
							if(ElevationData.IsNoData(z)) continue;
							writer.WriteLine($"{x * cs} {y * cs} {z}");
						}
					}
				}
			}
			return true;
		}
	}
}
