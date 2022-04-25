using HMCon;
using HMCon.Formats;

namespace HMConImage.Formats
{
	public class HeightmapJPGFormat : AbstractHeightmapFormat
	{
		public override string Identifier => "JPG";
		public override string ReadableName => "JPG Heightmap";
		public override string CommandKey => "jpg";
		public override string Description => ReadableName;
		public override string Extension => "jpg";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Import;
	}
}
