using ASCReader;
using ASCReader.Export;
using ASCReader.Import;
using System;

namespace ASCReader3DPlugin {

	[PluginInfo("3D exporter v1.0")]
	public class ASCReader3DPlugin : ASCReaderPlugin {
		public override ASCReaderCommandHandler GetCommandHandler() {
			return null;
		}

		public override ASCReaderExportHandler GetExportHandler() {
			return new ModelExporter();
		}

		public override ASCReaderImportHandler GetImportHandler() {
			return null;
		}
	}
}
