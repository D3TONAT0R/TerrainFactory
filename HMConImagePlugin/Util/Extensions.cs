using HMCon;
using static HMConImage.ImageExporter;

namespace HMConImage {
	static class Extensions {
		public static bool IsImage(this FileFormat format) {
			return format.Identifier.StartsWith("IMG");
		}

		public static ImageType GetImageType(this FileFormat ff) {
			if(ff.IsFormat(HeightmapPNG8Bit)) return ImageType.Heightmap8;
			else if(ff.IsFormat(HeightmapPNG16Bit)) return ImageType.Heightmap16;
			else if(ff.IsFormat(NormalsPNG)) return ImageType.Normalmap;
			else if(ff.IsFormat(HillshadePNG)) return ImageType.Hillshade;
			else return ImageType.Heightmap8;
		}
	}
}