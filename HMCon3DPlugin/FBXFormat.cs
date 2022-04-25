using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon3D;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon3D
{
	public class FBXFormat : FileFormat
	{
		public override string Identifier => "FBX";
		public override string ReadableName => "FBX 3D Model";
		public override string CommandKey => "fbx";
		public override string Description => ReadableName;
		public override string Extension => "fbx";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportJob job)
		{
			var model = ModelData.Create(job.data);
			using (var stream = BeginWriteStream(path)) {
				//TODO: use single library, perhaps look for a better alternative?
				if (HMCon3DPlugin.exported3dFiles < 50)
				{
					new Aspose3DExporter(model).WriteFile(stream, this);
				}
				else
				{
					ConsoleOutput.WriteWarning("Aspose3D's export limit was reached! Attempting to export using Assimp, which may throw an error.");
					new Assimp3DExporter(model).WriteFile(stream, this);
				}
				HMCon3DPlugin.exported3dFiles++;
			}
			return true;
		}
	}
}
