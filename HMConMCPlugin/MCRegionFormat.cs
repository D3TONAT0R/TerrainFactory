using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConMC
{
	public class MCRegionFormat : FileFormat
	{
		public override string Identifier => "MCR";
		public override string ReadableName => "Minecraft Region";
		public override string CommandKey => "mcr";
		public override string Description => ReadableName;
		public override string Extension => "mca";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		protected override HeightData ImportFile(string importPath, params string[] args)
		{
			//TODO: control heightmap type with args
			return MinecraftRegionImporter.ImportHeightmap(importPath, MCUtils.HeightmapType.TerrainBlocksNoLiquid);
		}

		public override void ModifyFileName(ExportJob job, FileNameBuilder nameBuilder)
		{
			nameBuilder.gridNum = (job.exportNumX + job.settings.GetCustomSetting("mcaOffsetX", 0), job.exportNumZ + job.settings.GetCustomSetting("mcaOffsetZ", 0));
			nameBuilder.gridNumFormat = "r.{0}.{1}";
		}

		public override bool ValidateSettings(ExportSettings settings, HeightData data)
		{
			bool sourceIs512 = (data.GridHeight == 512 && data.GridWidth == 512);
			if (settings.fileSplitDims != 512 && !sourceIs512)
			{
				ConsoleOutput.WriteError("File splitting dimensions must be 512 when exporting to minecraft regions!");
				return false;
			}
			return true;
		}
	}
}
