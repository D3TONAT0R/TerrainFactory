using HMCon.Formats;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HMCon.Export {
	public static class ExportUtility {

		public static bool ValidateExportSettings(ExportSettings settings, HeightData data) {
			//TODO: modification chain not taken into account (checks the raw (unprocessed) height data)
			if(settings.outputFormats.Count == 0) {
				ConsoleOutput.WriteError("No export formats have been set.");
				return false;
			}
			bool valid = true;
			foreach(var ff in settings.outputFormats) {
				valid &= ff.ValidateSettings(settings, data);
			}
			return valid;
		}
	}
}
