using HMCon.Export;
using HMCon.Import;
using System;
using static HMCon.ConsoleOutput;

namespace HMCon {
	public static class HMConManager {

		public static Job currentJob;

#if DEBUG
		public static bool autoInputActive = false;
#endif

		public static void Initialize(string pluginPath) {
			ImportManager.RegisterHandler(new StandardImporter());
			ExportUtility.RegisterHandler(new StandardExporter());
			CommandHandler.commandHandlers.Add(new StandardCommands());
			if(!string.IsNullOrEmpty(pluginPath)) {
				PluginLoader.LoadPlugins(pluginPath);
			} else {
				WriteLine("INFO: Plugins are disabled via launch arguments.");
			}
		}

		//TODO: Move somewhere else
		public static int GetTotalExportCellsPerFile() {
			int cells = currentJob.exportSettings.fileSplitDims >= 32 ? (int)Math.Pow(currentJob.exportSettings.fileSplitDims, 2) : currentJob.CurrentData.GridWidth * currentJob.CurrentData.GridHeight;
			if(currentJob.exportSettings.Subsampling > 1) cells /= currentJob.exportSettings.Subsampling * currentJob.exportSettings.Subsampling;
			return cells;
		}
	}
}
