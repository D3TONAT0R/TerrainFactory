using ASCReader;
using ASCReader.Export;
using ASCReader.Import;
using ASCReader.Util;
using System;
using System.Collections.Generic;

namespace ASCReaderMC {

	[PluginInfo("Minecraft Region Exporter v0.9.1")]
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
