namespace HMCon {
	public static class FileFormatUtil {

		public static bool IsPointFormat(this FileFormat format) {
			return format.IsFormat("ASC") || format.Identifier.StartsWith("PTS");
		}
	}
}