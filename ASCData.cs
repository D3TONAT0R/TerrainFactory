using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using ASCReader;
using Assimp;
using Assimp.Unmanaged;

public class ASCData {

	private static readonly Vector3 nullVector3 = new Vector3(0,0,0);
	public int ncols;
	public int nrows;
	public float cellsize;
	public float nodata_value;

	public float[,] data;

	public bool isValid;

	public ASCData (string filepath) {
		if(!File.Exists(filepath)) {
			Console.WriteLine("File "+filepath+" does not exist!");
		}
		try {
			FileStream stream = File.OpenRead(filepath);
			ncols = int.Parse(ExtractValue(ReadLine(stream), "ncols"));
			nrows = int.Parse(ExtractValue(ReadLine(stream), "nrows"));
			Console.WriteLine("Dimensions: "+ncols+"x"+nrows);
			//Ignore x,y corner coordinates
			ReadLine(stream);
			ReadLine(stream);
			cellsize = float.Parse(ExtractValue(ReadLine(stream), "cellsize"));
			nodata_value = float.Parse(ExtractValue(ReadLine(stream), "NODATA_value"));
			//Read the actual data
			data = new float[ncols,nrows];
			for(int y = 0; y < nrows; y++) {
				string ln = ReadLine(stream);
				string[] split = ln.Split(' ');
				if(split.Length != ncols) throw new FormatException("Column count at row "+y+" does not match the required length");
				for(int x = 0; x < ncols; x++) {
					data[x,nrows-y-1] = float.Parse(split[x]);
				}
			}
			isValid = true;
		}
		catch(Exception e) {
			Console.WriteLine("Error occured while reading ASC file!");
			Console.Write(e);
			Console.WriteLine("");
			isValid = false;
		}
	}

	public bool WriteAllFiles(string path, ExportOptions options) {
		string dir = Path.GetDirectoryName(path);
		if(Directory.Exists(dir)) {
			if(options.fileSplitDims < 32) {
				CreateFiles(path, null, options, 0, 0, ncols, nrows);
			} else {
				int dims = options.fileSplitDims;
				int yMin = 0;
				int fileY = 0;
				while(yMin+dims <= nrows) {
					int xMin = 0;
					int fileX = 0;
					int yMax = Math.Min(yMin+dims, nrows);
					while(xMin+dims <= ncols) {
						int xMax = Math.Min(xMin+dims, ncols);
						bool success = CreateFiles(path, fileX+","+fileY, options, xMin, yMin, xMax, yMax);
						if(!success) throw new IOException("Failed to write file "+fileX+","+fileY);
						xMin += dims;
						xMin = Math.Min(xMin, ncols);
						fileX++;
					}
					yMin += dims;
					yMin = Math.Min(yMin, nrows);
					fileY++;
				}
			}
			return true;
		} else {
			Console.WriteLine("Directory "+dir+" does not exist!");
			return false;
		}
	}

	public bool CreateFiles(string path, string subname, ExportOptions options, int xMin, int yMin, int xMax, int yMax) {
		if(!string.IsNullOrEmpty(subname)) {
			string ext = Path.GetExtension(path);
			string p = path.Substring(0, path.Length-ext.Length);
			path = p+"_"+subname;
		}
		foreach(FileFormat ff in options.outputFormats) {
			string fullpath = path+options.GetExtension(ff);
			Console.WriteLine("Creating file "+fullpath+" ...");
			if(ff == FileFormat.PTS_XYZ) {
				if(!WriteFileXYZ(fullpath, options.subsampling, xMin, yMin, xMax, yMax)) return false;
			} else if(ff.Is3DFormat()) {
				if(!WriteFile3D(fullpath, options.subsampling, xMin, yMin, xMax, yMax, ff)) return false;
			}
		}
		return true;
	}

	private bool WriteFileXYZ(string filename, int subsampling, int xMin, int yMin, int xMax, int yMax) {
		FileStream stream = new FileStream(filename, FileMode.CreateNew);
		for(int y = yMin; y < yMax; y++) {
			for(int x = xMin; x < xMax; x++) {
				if(x % subsampling == 0 && y % subsampling == 0) {
					float f = data[x,y];
					if(f != nodata_value) {
						stream.Write(Encoding.ASCII.GetBytes(x*cellsize+" "+y*cellsize+" "+f+"\n"));
					}
				}
			}
		}
		stream.Close();
		Console.WriteLine("XYZ File "+filename+" created successfully!");
		return true;
	}

	private bool WriteFile3D(string filename, int subsampling, int xMin, int yMin, int xMax, int yMax, FileFormat ff) {
		//Increase boundaries for lossless tiling
		if(xMax < ncols) xMax++;
		if(yMax < nrows) yMax++;
		Vector3[,] points = new Vector3[xMax-xMin,yMax-yMin];
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		for(int i = 0; i < xMax-xMin; i++) for(int j = 0; j < yMax-yMin; j++) points[i,j] = nullVector3;
		for(int y = yMin; y < yMax; y++) {
			for(int x = xMin; x < xMax; x++) {
				if(x % subsampling == 0 && y % subsampling == 0) {
					float f = data[x,y];
					if(f != nodata_value) {
						Vector3 vec = new Vector3(-x*cellsize, data[x,y], y*cellsize);
						points[x-xMin,y-yMin] = vec;
						if(!verts.Contains(vec)) verts.Add(vec);
					}
				}
			}
		}
		int nodatas = 0;
		for(int y = yMin; y < yMax-1; y++) {
			for(int x = xMin; x < xMax-1; x++) {
				if(x % subsampling == 0 && y % subsampling == 0) {
					Vector3[] pts = GetPointsForFace(points, x-xMin, y-yMin, subsampling);
					if(pts != null) { //if the list is null, then a nodata-value was found
						int i0 = verts.IndexOf(pts[0]);
						int i1 = verts.IndexOf(pts[1]);
						int i2 = verts.IndexOf(pts[2]);
						int i3 = verts.IndexOf(pts[3]);
						//Lower-Right triangle
						tris.Add(i0);
						tris.Add(i1);
						tris.Add(i3);
						//Upper-Left triangle
						tris.Add(i0);
						tris.Add(i3);
						tris.Add(i2);
					} else {
						nodatas++;
						if(Program.debugLogging) Console.WriteLine("[#"+nodatas+"] NODATA_VALUE or missing data at point "+x+" "+y);
					}
				}
			}
		}
		string filetype = "fbx";
		if(ff == FileFormat.MDL_3ds) {
			filetype = "3ds";
		}
		if(Program.exported3dFiles < 50) {
			Aspose3DExporter.ExportModel(verts, tris, filename, filetype);
		} else {
			Assimp3DExporter.ExportModel(verts, tris, filename, filetype);
		}
		Program.exported3dFiles++;
		Console.WriteLine("3ds File "+filename+" created successfully!");
		return true;
	}

	private Vector3[] GetPointsForFace(Vector3[,] points, int x, int y, int subsample) {
		Vector3[] pts = new Vector3[4];
		int i = subsample > 1 ? subsample : 1;
		int x1 = x;
		int y1 = y;
		int x2 = Math.Min(x1+i, points.GetLength(0)-1);
		int y2 = Math.Min(y1+i, points.GetLength(1)-1);
		pts[0] = points[x1,y1];
		pts[1] = points[x2,y1];
		pts[2] = points[x1,y2];
		pts[3] = points[x2,y2];
		foreach(Vector3 pt in pts) if(pt.Equals(nullVector3)) return null;
		return pts;
	}

	private string ReadLine(FileStream stream) {
		StringBuilder str = new StringBuilder();
		int b = stream.ReadByte();
		if(b < 0) {
			Console.WriteLine("WARNING: EOF reached!");
			return "";
		}
		if(!EndString(b)) str.Append((char)b);
		while(!EndString(b)) {
			b = stream.ReadByte();
			if(!EndString(b)) str.Append((char)b);
		}
		string output = str.ToString();
		while(output.StartsWith(' ')) output = output.Substring(1);
		return output;
	}

	private bool EndString(int b) {
		if(b < 0) return true;
		char c = (char)b;
		if(c == '\n') return true;
		return false;
	}

	private string ExtractValue(string input, string keyname) {
		input = input.Replace(keyname,"");
		input = input.Replace(" ", "");
		return input;
	}
}