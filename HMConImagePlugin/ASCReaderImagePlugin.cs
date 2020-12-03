using HMCon;
using HMCon.Export;
using HMCon.Import;
using System;

namespace ASCReaderImagePlugin {

	[PluginInfo("Image importer/exporter v1.0")]
	public class ASCReaderImagePlugin : ASCReaderPlugin {
		public override ASCReaderExportHandler GetExportHandler() {
			return new ImageExporter();
		}

		public override ASCReaderImportHandler GetImportHandler() {
			return new HeightmapImporter();
		}

		public override ASCReaderCommandHandler GetCommandHandler() {
			return new ImageCommandHandler();
		}
	}
}
