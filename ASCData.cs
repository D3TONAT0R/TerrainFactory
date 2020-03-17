using System;
using System.IO;
using System.Text;
using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Utilities;

public class ASCData {
	
	private FileStream stream;

	private static readonly Vector4 nullVector4 = new Vector4(0,0,0,0);
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
			stream = File.OpenRead(filepath);
			ncols = int.Parse(ExtractValue(ReadLine(), "ncols"));
			nrows = int.Parse(ExtractValue(ReadLine(), "nrows"));
			Console.WriteLine("Dimensions: "+ncols+"x"+nrows);
			//Ignore x,y corner coordinates
			ReadLine();
			ReadLine();
			cellsize = float.Parse(ExtractValue(ReadLine(), "cellsize"));
			nodata_value = float.Parse(ExtractValue(ReadLine(), "NODATA_value"));
			//Read the actual data
			data = new float[ncols,nrows];
			for(int y = 0; y < nrows; y++) {
				string ln = ReadLine();
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
				while(yMin+dims < nrows) {
					int xMin = 0;
					int fileX = 0;
					int yMax = Math.Min(yMin+dims, nrows);
					while(xMin+dims < ncols) {
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
			} else if(ff == FileFormat.PTS_XYZ) {
				if(!WriteFile3ds(fullpath, options.subsampling, xMin, yMin, xMax, yMax)) return false;
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

	private bool WriteFile3ds(string filename, int subsampling, int xMin, int yMin, int xMax, int yMax) {
		Mesh m = new Mesh();
		//Increase boundaries for lossless tiling
		if(xMax < ncols) xMax++;
		if(yMax < nrows) yMax++;
		Vector4[,] points = new Vector4[xMax-xMin,yMax-yMin];
		for(int i = 0; i < xMax-xMin; i++) for(int j = 0; j < yMax-yMin; j++) points[i,j] = nullVector4;
		for(int y = yMin; y < yMax; y++) {
			for(int x = xMin; y < xMax; x++) {
				if(x % subsampling == 0 && y % subsampling == 0) {
					float f = data[x,y];
					if(f != nodata_value) {
						Vector4 vec = new Vector4(x*cellsize, data[x,y], y*cellsize);
						points[x-xMin,y-yMin] = vec;
						m.ControlPoints.Add(vec);
					}
				}
			}
		}
		for(int y = yMin; y < yMax-1; y++) {
			for(int x = xMin; x < xMax-1; x++) {
				if(x % subsampling == 0 && y % subsampling == 0) {
					Vector4[] pts = GetPointsForFace(points, x, y, subsampling);
					if(pts != null) { //if the list is null, then a nodata-value was found
						int i0 = m.ControlPoints.IndexOf(pts[0]);
						int i1 = m.ControlPoints.IndexOf(pts[1]);
						int i2 = m.ControlPoints.IndexOf(pts[2]);
						int i3 = m.ControlPoints.IndexOf(pts[3]);
						m.CreatePolygon(i0, i1, i3);
						m.CreatePolygon(i0, i3, i2);
					}
				}
			}
		}
		
		Scene scene = new Scene();
		FileStream stream = new FileStream(filename, FileMode.CreateNew);
		scene.Save(stream, Aspose.ThreeD.FileFormat.Discreet3DS);
		stream.Close();

		Console.WriteLine("3ds File "+filename+" created successfully!");
		return true;
	}

	private Vector4[] GetPointsForFace(Vector4[,] points, int x, int y, int subsample) {
		Vector4[] pts = new Vector4[4];
		int i = subsample > 1 ? subsample : 1;
		int x1 = x;
		int y1 = y;
		int x2 = Math.Min(x1+i, points.GetLength(0));
		int y2 = Math.Min(y1+i, points.GetLength(1));
		pts[0] = points[x1,y1];
		pts[1] = points[x2,y1];
		pts[2] = points[x1,y2];
		pts[3] = points[x2,y2];
		foreach(Vector4 pt in pts) if(pt.Equals(nullVector4)) return null;
		return pts;
	}

	private string ReadLine() {
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