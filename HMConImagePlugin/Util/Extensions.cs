using HMCon;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConImage {
	static class Extensions {
		public static bool IsImage(this FileFormat format) {
			return format.Identifier.StartsWith("IMG");
		}

		public static ImageType GetImageType(this FileFormat ff) {
			if(ff.IsFormat("IMG_PNG-HM")) return ImageType.Heightmap;
			else if(ff.IsFormat("IMG_PNG-NM")) return ImageType.Normalmap;
			else if(ff.IsFormat("IMG_PNG-HS")) return ImageType.Hillshade;
			else if(ff.IsFormat("IMG_PNG-HM-S")) return ImageType.Heightmap_Banded;
			else return ImageType.Heightmap;
		}
	}
}