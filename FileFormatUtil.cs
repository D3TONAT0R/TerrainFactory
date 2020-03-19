public static class FileFormatUtil {

    public static bool Is3DFormat(this FileFormat format) {
        if(format == FileFormat.MDL_3ds || format == FileFormat.MDL_FBX) {
            return true;
        } else {
            return false;
        }
    }
}