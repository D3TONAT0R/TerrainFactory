using HMCon.Export;
using HMCon.Import;
using System;
using static HMCon.ConsoleOutput;

namespace HMCon {
	public static class HMConManager {

		public static Job currentJob;

		private static bool isInitialized = false;

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
				WriteLine("Plugin loading has been disabled");
			}
			CommandHandler.Initialize();
			isInitialized = true;
		}

		public static bool ConvertFile(string inputPath, string outputPath, ExportSettings settings, params string[] importArgs) {
			if(!isInitialized) {
				throw new ArgumentException("The application must be initialized before it can be used");
			}
			HeightData data = ImportManager.ImportFile(inputPath, importArgs);
			return true;
		}

		//TODO: Move somewhere else
		/*public static int GetTotalExportCellsPerFile() {
			int cells = currentJob.exportSettings.fileSplitDims >= 32 ? (int)Math.Pow(currentJob.exportSettings.fileSplitDims, 2) : currentJob.CurrentData.GridWidth * currentJob.CurrentData.GridHeight;
			if(currentJob.exportSettings.Subsampling > 1) cells /= currentJob.exportSettings.Subsampling * currentJob.exportSettings.Subsampling;
			return cells;
		}*/
	}
}
