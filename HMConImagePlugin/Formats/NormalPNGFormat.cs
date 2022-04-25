using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Util;

namespace HMConImage.Formats
{
	public class NormalPNGFormat : FileFormat
	{
		public override string Identifier => "PNG_NM";
		public override string ReadableName => "PNG Normal Map";
		public override string CommandKey => "png-nm";
		public override string Description => ReadableName;
		public override string Extension => "png";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportJob job)
		{
			var gen = new ImageGeneratorMagick(job.data, ImageType.Normalmap, job.data.lowPoint, job.data.highPoint);
			gen.WriteFile(path, ImageMagick.MagickFormat.Png24);
			return true;
		}

		public override void ModifyFileName(ExportJob exportJob, FileNameBuilder nameBuilder)
		{
			nameBuilder.suffix = "normal";
		}
	}
}
