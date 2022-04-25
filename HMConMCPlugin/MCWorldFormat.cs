using HMCon.Export;
using HMCon.Formats;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConMC
{
	public class MCWorldFormat : FileFormat
	{
		public override string Identifier => "MCW";
		public override string ReadableName => "Minecraft World";
		public override string CommandKey => "mcw";
		public override string Description => ReadableName;
		public override string Extension => "";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportJob job)
		{
			new MCWorldExporter(job).WriteFile(path, null, this);
			return true;
		}
	}
}
