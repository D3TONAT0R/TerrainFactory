﻿using HMCon.Export;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HMCon.Formats
{
	public class Raw32Format : Raw16Format
	{
		public override string Identifier => "R32";
		public override string ReadableName => "32 Bit Raw Data";
		public override string CommandKey => "r32";
		public override string Description => ReadableName;
		public override string Extension => "r32";

		protected override bool ExportFile(string path, ExportJob job)
		{
			using (var stream = BeginWriteStream(path))
			{
				WriteBytes(stream, job.data, 4);
			}
			return true;
		}
	}
}