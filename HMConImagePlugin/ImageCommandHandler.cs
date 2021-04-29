using HMCon;
using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;

namespace HMConImage {
	public class ImageCommandHandler : HMConCommandHandler {
		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("preview", "", "Previews the grid data in an image", this));
			list.Add(new ConsoleCommand("preview-hm", "", "Previews the grid data in a heightmap", this));
		}

		public override void HandleCommand(string cmd, string[] args, ExportSettings options, HeightData data) {
			if(cmd == "preview-hm") {
				ConsoleOutput.WriteLine("Opening preview...");
				Previewer.OpenDataPreview(data, HMConManager.currentJob.exportSettings, true);
			} else if(cmd == "preview") {
				ConsoleOutput.WriteLine("Opening preview...");
				Previewer.OpenDataPreview(data, HMConManager.currentJob.exportSettings, false);
			}
		}
	}
}
