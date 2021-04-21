using HMCon;
using HMCon.Export;
using HMCon.Import;
using System;

namespace HMConImage {

	[PluginInfo("ImagePlugin", "Image importer/exporter v1.0")]
	public class HMConImagePlugin : HMConPlugin {
		public override HMConExportHandler GetExportHandler() {
			return new ImageExporter();
		}

		public override HMConImportHandler GetImportHandler() {
			return new HeightmapImporter();
		}

		public override HMConCommandHandler GetCommandHandler() {
			return new ImageCommandHandler();
		}
	}
}
