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
			list.Add(new ConsoleCommand("mcpostprocess", "", "[MCA] Run various world generators defined in a separate XML file", HandlePostProcessingCmd));
		}

		private bool HandleOffsetCmd(Job job, string[] args) {
			int x = ConsoleCommand.ParseArg<int>(args, 0);
			int z = ConsoleCommand.ParseArg<int>(args, 1);
			job.exportSettings.SetCustomSetting("mcaOffsetX", x);
			job.exportSettings.SetCustomSetting("mcaOffsetZ", z);
			ConsoleOutput.WriteLine("MCA terrain offset set to " + x + "," + z + " (" + (x * 512) + " blocks , " + z * 512 + " blocks)");
			return true;
		}

		private bool HandlePostProcessingCmd(Job job, string[] args) {

			if(args.Length > 0)
			{
				bool b = job.exportSettings.GetCustomSetting("mcpostprocess", false);
				if (!b) job.exportSettings.SetCustomSetting("mcpostprocess", true);
				string file = args[0];
				ConsoleOutput.WriteLine($"MC World Post Processing enabled (using '{file}.xml').");
			}
			bool b2 = job.exportSettings.ToggleCustomBoolSetting("mcpostprocess");
			ConsoleOutput.WriteLine("MC World Post Processing " + (b2 ? "enabled" : "disabled"));
			return true;
		}
	}
}
