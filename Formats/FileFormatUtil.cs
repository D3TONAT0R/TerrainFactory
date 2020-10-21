namespace ASCReader {
	public static class FileFormatUtil {

		public static bool IsPointFormat(this FileFormat format) {
			return format.IsFormat("ASC") || format.Identifier.StartsWith("PTS");
		}

		public static bool Is3DFormat(this FileFormat format) {
			return format.Identifier.StartsWith("MDL");
		}

		public static bool IsImage(this FileFormat format) {
			return format.Identifier.StartsWith("IMG");
		}

		public static ImageType GetImageType(this FileFormat ff) {
			if(ff.IsFormat("IMG_PNG-HEIGHT")) return ImageType.Heightmap;
			else if(ff.IsFormat("IMG_PNG-NORMAL")) return ImageType.Normalmap;
			else if(ff.IsFormat("IMG_PNG-HILLSHADE")) return ImageType.Hillshade;
			else return ImageType.Heightmap;
		}
	}
}