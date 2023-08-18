using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TerrainFactory.Util
{
	public static class DumpFileUtility
	{
		public static void DumpStreamToFile(Stream stream, string path)
		{
			using (var fileStream = File.Create(path))
			{
				var pos = stream.Position;
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(fileStream);
				stream.Seek(pos, SeekOrigin.Begin);
			}
		}

		public static void DumpStreamToDesktop(Stream stream, string filename)
		{
			DumpStreamToFile(stream, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename));
		}
	}
}
