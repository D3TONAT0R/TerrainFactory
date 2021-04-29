using HMCon;
using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConMC {
	public class MCCommandHandler : HMConCommandHandler {

		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("mcaoffset", "X Z", "[MCA] Apply offset to region terrain, in regions (512)", this));
			list.Add(new ConsoleCommand("mcasplatmapper", "", "[MCA] Use splatmap files to define the world's surface (file <name>.splat required)", this));
		}

		public override void HandleCommand(string cmd, string[] args, ExportSettings options, HeightData data) {
			if(cmd == "mcaoffset") {
				if(args.Length > 1) {
					bool b = true;
					b &= int.TryParse(args[0], out int x) & int.TryParse(args[1], out int z);
					if(b) {
						options.mcaOffsetX = x;
						options.mcaOffsetZ = z;
						ConsoleOutput.WriteLine("MCA terrain offset set to " + x + "," + z + " (" + (x * 512) + " blocks , " + z * 512 + " blocks)");
					} else {
						ConsoleOutput.WriteWarning("Failed to parse to int");
					}
				} else {
					ConsoleOutput.WriteWarning("Two integers are required!");
				}
			} else if(cmd == "mcasplatmapper") {
				options.useSplatmaps = !options.useSplatmaps;
				ConsoleOutput.WriteLine("MCA splatmapping " + (options.useSplatmaps ? "enabled" : "disabled"));
			}
		}
	}
}
