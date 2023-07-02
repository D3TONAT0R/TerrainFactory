using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HMCon.Formats
{
	public static class FileFormatManager
	{

		private static List<FileFormat> registeredFormats = new List<FileFormat>();

		internal static void RegisterStandardFormats()
		{
			RegisterFormat(new CommandStackFormat());
			RegisterFormat(new GeoMetadataFormat());
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
			registeredFormats.Add(f);
		}

		public static FileFormat GetFormatFromIdentifier(string identifier)
		{
			return registeredFormats.FirstOrDefault(f => f.Identifier == identifier);
		}

		public static FileFormat GetFormatFromCommandKey(string key)
		{
			key = key.ToLower();
			return registeredFormats.FirstOrDefault(f => f.CommandKey == key);
		}

		public static FileFormat GetFormatByFileName(string filename)
		{
			var ext = Path.GetExtension(filename);
			if (ext.Length > 0)
			{
				ext = ext.Substring(1);
				return GetFormatByExtension(ext);
			}
			else
			{
				throw new InvalidOperationException("Unable to determine file format: Filename does not have an extension.");
			}
		}

		public static FileFormat GetFormatByExtension(string ext)
		{
			return registeredFormats.FirstOrDefault(f => f.Extension == ext);
		}

		public static FileFormat GetFormatFromType(Type type)
		{
			return registeredFormats.FirstOrDefault(f => f.GetType() == type);
		}

		public static FileFormat[] GetSupportedFormats(FileFormat.FileSupportFlags supportType)
		{
			List<FileFormat> formats = new List<FileFormat>();
			foreach(var f in registeredFormats)
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
