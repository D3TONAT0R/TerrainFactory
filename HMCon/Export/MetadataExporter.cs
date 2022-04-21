using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HMCon.Export.Exporters
{
	public class MetadataExporter : IExporter
	{
		ExportJob job;

		public MetadataExporter(ExportJob job)
		{
			this.job = job;
		}

		public bool NeedsFileStream(FileFormat format)
		{
			return false;
		}

		public void WriteFile(FileStream stream, string path, FileFormat filetype)
		{
			StringBuilder file = new StringBuilder();
			if (filetype.IsFormat("HMC_CMDS"))
			{
				foreach (var mod in job.settings.modificationChain)
				{
					file.AppendLine("modify " + mod.sourceCommandString);
				}
			}
			else if (filetype.IsFormat("GEO_META"))
			{
				file.AppendLine("cells_x       " + job.data.GridWidth);
				file.AppendLine("cells_y       " + job.data.GridHeight);
				file.AppendLine("xll_corner    " + job.data.lowerCornerPos.X);
				file.AppendLine("yll_corner    " + job.data.lowerCornerPos.Y);
				file.AppendLine("cell_size     " + job.data.cellSize);
				file.AppendLine("nodata_value  " + job.data.nodata_value);
			}
			File.WriteAllText(path, file.ToString());
		}
	}
}
