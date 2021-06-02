using HMCon;
using HMCon.Export;
using HMCon.Import;
using HMCon.Util;
using System;
using System.Collections.Generic;

namespace HMConMC {

	[PluginInfo("MinecraftPlugin", "Minecraft Region Exporter v0.9.5")]
	public class HMConMCPlugin : HMConPlugin {

		public override HMConExportHandler GetExportHandler() {
			return new MCExportHandler();
		}

		public override HMConImportHandler GetImportHandler() {
			return new MinecraftRegionImporter();
		}

		public override HMConCommandHandler GetCommandHandler() {
			return new MCCommandHandler();
		}
	}
}
