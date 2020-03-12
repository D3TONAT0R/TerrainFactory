using System;
using System.IO;
using System.Text;

public class ASCData {
	
	private FileStream stream;

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

	public bool WriteToXYZ(string path, ExportOptions options) {
		string dir = Path.GetDirectoryName(path);
		if(Directory.Exists(dir)) {
			if(options.fileSplitDims < 32) {
				CreateFile(path, null, options, 0, 0, ncols, nrows);
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
						bool success = CreateFile(path, fileX+","+fileY, options, xMin, yMin, xMax, yMax);
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

	public bool CreateFile(string path, string subname, ExportOptions options, int xMin, int yMin, int xMax, int yMax) {
		if(!string.IsNullOrEmpty(subname)) {
			string ext = Path.GetExtension(path);
			string p = path.Substring(0, path.Length-ext.Length);
			path = p+"_"+subname+ext;
		}
		Console.WriteLine("Creating file "+path+" ...");
		FileStream stream = new FileStream(path, FileMode.CreateNew);
			for(int y = yMin; y < yMax; y++) {
				for(int x = xMin; x < xMax; x++) {
					if(x % options.subsampling == 0 && y % options.subsampling == 0) {
						float f = data[x,y];
						if(f != nodata_value) {
							stream.Write(Encoding.ASCII.GetBytes(x*cellsize+" "+y*cellsize+" "+f+"\n"));
						}
					}
				}
			}
			stream.Close();
			Console.WriteLine("XYZ File "+path+" created successfully!");
			return true;
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