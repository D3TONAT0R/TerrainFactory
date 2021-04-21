using HMCon;
using HMCon.Export;
using HMCon.Import;
using System;

namespace HMCon3D {

	[PluginInfo("3DPlugin", "3D exporter v1.0")]
	public class HMCon3DPlugin : HMConPlugin {
		public override HMConCommandHandler GetCommandHandler() {
			return null;
		}

		public override HMConExportHandler GetExportHandler() {
			return new ModelExporter();
		}

		public override HMConImportHandler GetImportHandler() {
			return null;
		}
	}
}
