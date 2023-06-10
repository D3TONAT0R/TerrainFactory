using HMCon.Formats;
using System;

namespace HMCon.Export
{
	public static class ExportManager
	{

		public static bool ValidateExportSettings(FileFormatList formats, ExportSettings settings, HeightData data)
		{
			//TODO: modification chain not taken into account (checks the raw (unprocessed) height data)
			if (formats.Count == 0)
			{
				ConsoleOutput.WriteError("No export formats have been set.");
				return false;
			}
			bool valid = true;
			foreach (var format in formats)
			{
				valid &= format.ValidateSettings(settings, data);
			}
			return valid;
		}

		public static bool ExportAs(HeightData heightData, ExportSettings settings, string filename, params FileFormat[] formats)
		{
			if (formats.Length == 0)
			{
				ConsoleOutput.WriteWarning("Warning: No target format(s) have been specified.");
				return false;
			}
			bool success = true;
			foreach (var format in formats)
			{
				var export = new ExportTask(heightData, format, settings, filename);
				success &= export.Export();
			}
			return success;
		}

		public static bool ExportAs(HeightData heightData, ExportSettings settings, string filename, params string[] formatIDs)
		{
			FileFormat[] formats = new FileFormat[formatIDs.Length];
			for (int i = 0; i < formats.Length; i++)
			{
				formats[i] = FileFormat.GetFromCommandInput(formatIDs[i]);
			}
			return ExportAs(heightData, settings, filename, formats);
		}

		public static bool ExportAs(HeightData heightData, ExportSettings settings, string filename, params Type[] formatTypes)
		{
			FileFormat[] ffs = new FileFormat[formatTypes.Length];
			for (int i = 0; i < ffs.Length; i++)
			{
				ffs[i] = FileFormat.GetFromType(formatTypes[i]);
			}
			return ExportAs(heightData, settings, filename, ffs);
		}

		public static bool ExportAs(HeightData heightData, string filename, params string[] formatIDs)
		{
			return ExportAs(heightData, new ExportSettings(), filename, formatIDs);
		}

		public static bool ExportAs(HeightData heightData, string filename, params Type[] formatTypes)
		{
			return ExportAs(heightData, new ExportSettings(), filename, formatTypes);
		}
	}
}
