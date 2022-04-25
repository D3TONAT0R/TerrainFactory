using HMCon;
using HMCon.Export;
using HMCon.Formats;

namespace HMConImage.Formats
{
	public class HeightmapPNG8Format : AbstractHeightmapFormat
	{
		public override string Identifier => "PNG";
		public override string ReadableName => "PNG Heightmap (8 Bit)";
		public override string CommandKey => "png";
		public override string Description => ReadableName;
		public override string Extension => "png";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		protected override bool ExportFile(string path, ExportJob job)
		{
			var gen = new ImageGeneratorMagick(job.data, ImageType.Heightmap8, job.data.lowPoint, job.data.highPoint);
			gen.WriteFile(path, ImageMagick.MagickFormat.Png24);
			return true;
		}
	}
}
