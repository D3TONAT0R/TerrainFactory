using System;
using System.Collections.Generic;

public class ExportOptions {

    public List<FileFormat> outputFormats = new List<FileFormat>();
    public int subsampling = 1;
    public int fileSplitDims = -1;

    public void SetOutputFormats(string[] inputs, bool append) {
        if(!append) outputFormats.Clear();
        foreach(string s in inputs) {
            if(string.IsNullOrEmpty(s)) continue;
            var ff = GetFormatFromString(s);
            if(ff != FileFormat.UNKNOWN) {
                outputFormats.Add(ff);
            } else {
                Console.WriteLine("Unknown format: "+s);
            }
        }
    }

    public FileFormat GetFormatFromString(string str) {
        str = str.ToLower();
        if(str == "asc") return FileFormat.ASC;
        else if(str == "xyz") return FileFormat.PTS_XYZ;
        else if(str == "3ds") return FileFormat.MDL_3ds;
        else if(str == "fbx") return FileFormat.MDL_FBX;
        else if(str == "png") return FileFormat.IMG_PNG;
        else return FileFormat.UNKNOWN;
    }

    public string GetExtension(FileFormat ff) {
        if(ff == FileFormat.ASC) return ".asc";
        else if(ff == FileFormat.PTS_XYZ) return ".xyz";
        else if(ff == FileFormat.MDL_3ds) return ".3ds";
        else if(ff == FileFormat.MDL_FBX) return ".fbx";
        else if(ff == FileFormat.IMG_PNG) return ".png";
        else return "";
    }
}