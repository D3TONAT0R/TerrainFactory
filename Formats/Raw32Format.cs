using TerrainFactory.Export;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TerrainFactory.Formats
{
	public class Raw32Format : Raw16Format
	{
		public override string Identifier => "R32";
		public override string ReadableName => "32 Bit Raw Data";
		public override string CommandKey => "r32";
		public override string Description => ReadableName;
		public override string Extension => "r32";

		protected override bool Is32BitFormat => true;
	}
}
