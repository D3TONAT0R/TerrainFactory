using HMCon.Formats;


namespace HMConImage.Formats
{
	public class HeightmapTIFFormat : AbstractHeightmapFormat
	{
		public override string Identifier => "TIF";
		public override string ReadableName => "TIF Heightmap";
		public override string CommandKey => "tif";
		public override string Description => ReadableName;
		public override string Extension => "tif";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Import;
	}
}
