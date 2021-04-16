using HMCon.Util;
using System;

namespace HMCon.Export {
	public static class CurrentExportJobInfo {

		public static string importedFilePath;

		public static int exportNumX;
		public static int exportNumZ;

		public static ExportSettings exportSettings;
		public static Bounds bounds;

		public static int mcaGlobalPosX;
		public static int mcaGlobalPosZ;

		public static void Reset() {
			importedFilePath = null;
			exportNumX = 0;
			exportNumZ = 0;
			exportSettings = new ExportSettings();
			bounds = null;
			mcaGlobalPosX = 0;
			mcaGlobalPosZ = 0;
		}
	}
}
