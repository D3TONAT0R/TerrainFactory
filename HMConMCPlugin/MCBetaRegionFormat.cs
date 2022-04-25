using HMCon;
using HMCon.Formats;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConMC
{
	public class MCBetaRegionFormat : MCRegionFormat
	{
		public override string Identifier => "MCR_B";
		public override string ReadableName => "Minecraft Region (Beta)";
		public override string CommandKey => "mcr_beta";
		public override string Description => ReadableName;
		public override string Extension => "mcr";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Import;
	}
}
