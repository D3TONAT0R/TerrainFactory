using HMCon;
using HMCon.Export;
using HMCon.Import;
using HMCon.Util;
using System;
using System.Collections.Generic;

namespace ASCReaderMC {

	[PluginInfo("Minecraft Region Exporter v0.9.2")]
	public class ASCReaderMCPlugin : ASCReaderPlugin {

		public override ASCReaderExportHandler GetExportHandler() {
			return new MCExportHandler();
		}

		public override ASCReaderImportHandler GetImportHandler() {
			return new MinecraftRegionImporter();
		}

		public override ASCReaderCommandHandler GetCommandHandler() {
			return new MCCommandHandler();
		}
	}
}
