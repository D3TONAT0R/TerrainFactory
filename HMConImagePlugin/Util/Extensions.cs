using ASCReader;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASCReaderImagePlugin {
	static class Extensions {
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
