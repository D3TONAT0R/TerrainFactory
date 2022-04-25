using HMCon;
using HMCon.Export;
using HMCon.Formats;

namespace HMConImage.Formats
{
	public class HeightmapPNG16Format : AbstractHeightmapFormat
	{
		public override string Identifier => "PNG16";
		public override string ReadableName => "PNG Heightmap (16 Bit)";
		public override string CommandKey => "png16";
		public override string Description => ReadableName;
		public override string Extension => "png";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		protected override bool ExportFile(string path, ExportJob job)
		{
			var gen = new ImageGeneratorMagick(job.data, ImageType.Heightmap16, job.data.lowPoint, job.data.highPoint);
			gen.WriteFile(path, ImageMagick.MagickFormat.Png48);
			return true;
		}
	}
}
