using HMCon;
using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;

namespace HMConImage {
	public class ImageCommandHandler : HMConCommandHandler {
		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("preview", "", "Previews the grid data in an image", HandlePreviewCmd));
			list.Add(new ConsoleCommand("preview-hm", "", "Previews the grid data in a heightmap", HandleHMPreviewCmd));
		}

		private bool HandlePreviewCmd(Job job, string[] args) {
			OpenPreview(job, false);
			return true;
		}

		private bool HandleHMPreviewCmd(Job job, string[] args) {
			OpenPreview(job, true);
			return true;
		}

		private void OpenPreview(Job job, bool heightmap) {
			ConsoleOutput.WriteLine("Opening preview...");
			Previewer.OpenDataPreview(job, heightmap);
		}
	}
}
