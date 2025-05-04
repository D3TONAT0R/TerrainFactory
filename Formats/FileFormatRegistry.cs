using System;
using System.Collections.Generic;

namespace TerrainFactory.Formats
{
	public static class FileFormatRegistry
	{

		internal static List<FileFormat> availableFormats = new List<FileFormat>();

		internal static void RegisterStandardFormats()
		{
			RegisterFormat(new ElevationDataFormat());
			RegisterFormat(new ElevationMetadataFormat());
			RegisterFormat(new AsciiGridFormat());
			RegisterFormat(new XYZFormat());
			RegisterFormat(new Raw16Format());
			RegisterFormat(new Raw32Format());
			RegisterFormat(new DXFFormat());
			RegisterFormat(new DXF3DFormat());
		}

		internal static void RegisterFormat(FileFormat f)
		{
			if (f == null) throw new NullReferenceException();
			availableFormats.Add(f);
		}

		public static FileFormat[] GetSupportedFormats(FileFormat.FileSupportFlags supportType)
		{
			List<FileFormat> formats = new List<FileFormat>();
			foreach(var f in availableFormats)
			{
				if(f.SupportedActions.HasFlag(supportType))
				{
					formats.Add(f);
				}
			}
			return formats.ToArray();
		}
	}
}
