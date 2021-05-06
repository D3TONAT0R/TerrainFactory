using HMCon;
using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConMC {
	public class MCCommandHandler : HMConCommandHandler {

		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("mcaoffset", "X Z", "[MCA] Apply offset to region terrain, in regions (512)", HandleOffsetCmd));
			list.Add(new ConsoleCommand("mcasplatmapper", "", "[MCA] Use splatmap files to define the world's surface (file <name>.splat required)", HandleSplatmapperCmd));
		}

		private bool HandleOffsetCmd(Job job, string[] args) {
			int x = ConsoleCommand.ParseArg<int>(args, 0);
			int z = ConsoleCommand.ParseArg<int>(args, 1);
			job.exportSettings.SetCustomSetting("mcaOffsetX", x);
			job.exportSettings.SetCustomSetting("mcaOffsetZ", z);
			ConsoleOutput.WriteLine("MCA terrain offset set to " + x + "," + z + " (" + (x * 512) + " blocks , " + z * 512 + " blocks)");
			return true;
		}

		private bool HandleSplatmapperCmd(Job job, string[] args) {
			bool b = job.exportSettings.ToggleCustomBoolSetting("mcaUseSplatmaps");
			ConsoleOutput.WriteLine("MCA splatmapping " + (b ? "enabled" : "disabled"));
			return true;
		}
	}
}
