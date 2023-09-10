using TerrainFactory.Export;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TerrainFactory.Formats
{
	public class Raw16Format : RawFormat
	{
		public override string Identifier => "R16";
		public override string ReadableName => "16 Bit Raw Data";
		public override string CommandKey => "r16";
		public override string Description => ReadableName;
		public override string Extension => "r16";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool Is32BitFormat => false;
	}
}
