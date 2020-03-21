using System.Drawing.Imaging;

namespace ASCReader {
	public static class FileFormatUtil {

		public static bool IsPointFormat(this FileFormat format) {
			if(format == FileFormat.ASC || format == FileFormat.PTS_XYZ) {
				return true;
			} else {
				return false;
			}
		}

		public static bool Is3DFormat(this FileFormat format) {
			if(format == FileFormat.MDL_3ds || format == FileFormat.MDL_FBX) {
				return true;
			} else {
				return false;
			}
		}

		public static bool IsImage(this FileFormat format) {
			if(format == FileFormat.IMG_PNG_Height || format == FileFormat.IMG_PNG_Normal || format == FileFormat.IMG_PNG_Hillshade) {
				return true;
			} else {
				return false;
			}
		}

		public static string GetFiletypeString(this FileFormat ff) {
			if(ff == FileFormat.ASC) return "asc";
			else if(ff == FileFormat.PTS_XYZ) return "xyz";
			else if(ff == FileFormat.MDL_3ds) return "3ds";
			else if(ff == FileFormat.MDL_FBX) return "fbx";
			else if(ff == FileFormat.IMG_PNG_Height) return "png";
			else if(ff == FileFormat.IMG_PNG_Normal) return "png";
			else if(ff == FileFormat.IMG_PNG_Hillshade) return "png";
			else return "";
		}

		public static FileFormat GetFileFormat(this string str) {
			str = str.ToLower();
			if(str == "asc") return FileFormat.ASC;
			else if(str == "xyz") return FileFormat.PTS_XYZ;
			else if(str == "3ds") return FileFormat.MDL_3ds;
			else if(str == "fbx") return FileFormat.MDL_FBX;
			else if(str == "png") return FileFormat.IMG_PNG_Height;
			else if(str == "png-hm") return FileFormat.IMG_PNG_Height;
			else if(str == "png-nm") return FileFormat.IMG_PNG_Normal;
			else if(str == "png-hs") return FileFormat.IMG_PNG_Hillshade;
			else return FileFormat.UNKNOWN;
		}

		public static ImageType GetImageType(this FileFormat ff) {
			if(ff == FileFormat.IMG_PNG_Height) return ImageType.Heightmap;
			else if(ff == FileFormat.IMG_PNG_Normal) return ImageType.Normalmap;
			else if(ff == FileFormat.IMG_PNG_Hillshade) return ImageType.Hillshade;
			else return ImageType.Heightmap;
		}

		public static string GetSuffixWithExtension(this FileFormat ff) {
			string str = "";
			if(ff == FileFormat.IMG_PNG_Height) str = "_height";
			else if(ff == FileFormat.IMG_PNG_Normal) str = "_normal";
			else if(ff == FileFormat.IMG_PNG_Hillshade) str = "_relief";
			string ext = GetFiletypeString(ff);
			if(!string.IsNullOrEmpty(ext)) {
				return str + "." + ext;
			} else {
				return str;
			}
		}
	} 
}