﻿using HMCon.Export;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HMCon.Formats
{
	public class CommandStackFormat : FileFormat
	{
		public override string Identifier => "HMC_CMDS";
		public override string ReadableName => "HMCon command stack";
		public override string CommandKey => "cmds";
		public override string Description => ReadableName;
		public override string Extension => "cmds";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportJob job)
		{
			throw new NotImplementedException();
		}
	}
}