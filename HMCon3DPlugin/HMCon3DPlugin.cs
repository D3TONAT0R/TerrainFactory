using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using System;
using System.Collections.Generic;

namespace HMCon3D {

	[PluginInfo("3DPlugin", "3D exporter v1.0")]
	public class HMCon3DPlugin : HMConPlugin {

		public static int exported3dFiles = 0;

		public override HMConCommandHandler GetCommandHandler() {
			return null;
		}

		public override void RegisterFormats(List<FileFormat> registry)
		{
			registry.Add(new Autodesk3DSFormat());
			registry.Add(new FBXFormat());
		}
	}
}
