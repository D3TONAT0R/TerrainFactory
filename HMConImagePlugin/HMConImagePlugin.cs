using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using HMConImage.Formats;
using System;
using System.Collections.Generic;

namespace HMConImage {

	[PluginInfo("ImagePlugin", "Image importer/exporter v1.0")]
	public class HMConImagePlugin : HMConPlugin {

		public override HMConCommandHandler GetCommandHandler() {
			return new ImageCommandHandler();
		}

		public override void RegisterFormats(List<FileFormat> registry)
		{
			registry.Add(new HeightmapPNG8Format());
			registry.Add(new HeightmapPNG16Format());
			registry.Add(new HeightmapJPGFormat());
			registry.Add(new HeightmapTIFFormat());
			registry.Add(new NormalPNGFormat());
			registry.Add(new HillshadePNGFormat());
		}
	}
}
