using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using ASCReader.Export;
using ASCReader.Export.Exporters;
using Microsoft.Win32.SafeHandles;

namespace ASCReader {

	public class ASCSummary {
		public float lowestValue = float.PositiveInfinity;
		public float highestValue = float.NegativeInfinity;
		public float averageValue = 0;
	}

	public class ASCData {

		public string filename;

		public int ncols;
		public int nrows;
		public float xllcorner;
		public float yllcorner;
		public float cellsize;
		public float nodata_value;

		public string fileHeader = "";

		public float[,] data;
		public float lowestValue = float.PositiveInfinity;
		public float highestValue = float.NegativeInfinity;

		public bool isValid;

		public ASCData() {

		}

		public ASCData(string filepath) {
			if(!File.Exists(filepath)) {
				Program.WriteError("File " + filepath + " does not exist!");
			}
			try {
				filename = Path.GetFileNameWithoutExtension(filepath);
				FileStream stream = File.OpenRead(filepath);
				ReadHeader(stream);
				//Read the actual data
				data = new float[ncols, nrows];
				ReadGridData(stream);
				isValid = true;
			} catch(Exception e) {
				Program.WriteError("Error occured while reading ASC file!");
				Program.WriteLine(e.ToString());
				Program.WriteLine("");
				isValid = false;
			}
		}

		public static ASCSummary GetSummary(string filepath) {
			try {
				FileStream stream = File.OpenRead(filepath);
				ASCData asc = new ASCData();
				asc.ReadHeader(stream);
				//Read the actual data
				//The cells will not be saved as long as grid is null
				asc.ReadGridData(stream);
				ASCSummary summary = new ASCSummary();
				double sum = 0;
				for(int i = 0; i < asc.ncols*asc.nrows; i++) {
					float value;
					if(!asc.NextGridValue(stream, out value)) break;
					sum += value;
				}
				summary.lowestValue = asc.lowestValue;
				summary.highestValue = asc.highestValue;
				summary.averageValue = (float)(sum/(asc.ncols*asc.nrows));
				return summary;
			} catch(Exception e) {
				Program.WriteError("Error occured while getting summary for ASC file!");
				Program.WriteLine(e.ToString());
				Program.WriteLine("");
				return null;
			}
		}

		public void ReadHeader(FileStream stream) {
			ncols = ExtractInt(ReadHeaderLine(stream), "ncols");
			nrows = ExtractInt(ReadHeaderLine(stream), "nrows");
			Program.WriteLine("Dimensions: " + ncols + "x" + nrows);
			xllcorner = ExtractFloat(ReadHeaderLine(stream), "xllcorner");
			yllcorner = ExtractFloat(ReadHeaderLine(stream), "yllcorner");
			cellsize = ExtractFloat(ReadHeaderLine(stream), "cellsize");
			nodata_value = ExtractFloat(ReadHeaderLine(stream), "NODATA_value");
		}

		public void ReadGridData(FileStream stream) {
			int length = ncols*nrows;
			for(int i = 0; i < length; i++) {
				float value;
				if(!NextGridValue(stream, out value)) break;
				if(Math.Abs(value-nodata_value) > 0.1f) {
					if(value < lowestValue) lowestValue = value;
					if(value > highestValue) highestValue = value;
				}
				int y = (int)Math.Floor(i / (double)ncols);
				int x = i % ncols;
				if(data != null) data[x, nrows - y - 1] = value;
			}
		}

		public bool NextGridValue(FileStream stream, out float value) {
			var d = NextValue(stream);
			if(d == "") {
				//Premature EOF
				Program.WriteError("Premature EOF reached!");
				value = 0;
				return false;
			}
			value = float.Parse(d);
			return true;
		}

		private string NextValue(FileStream stream) {
			return ReadDataRaw(stream, true);
		}

		public bool WriteAllFiles(string path, ExportOptions options) {
			string dir = Path.GetDirectoryName(path);
			if(Directory.Exists(dir)) {
				if(options.fileSplitDims < 32) {
					ExportUtility.CreateFilesForSection(this, path, null, options, 0, 0, ncols, nrows);
				} else {
					int dims = options.fileSplitDims;
					int yMin = 0;
					int fileY = 0;
					while(yMin + dims <= nrows) {
						int xMin = 0;
						int fileX = 0;
						int yMax = Math.Min(yMin + dims, nrows);
						while(xMin + dims <= ncols) {
							int xMax = Math.Min(xMin + dims, ncols);
							bool success = ExportUtility.CreateFilesForSection(this, path, fileX + "," + fileY, options, xMin, yMin, xMax, yMax);
							if(!success) throw new IOException("Failed to write file " + fileX + "," + fileY);
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
				Program.WriteError("Directory " + dir + " does not exist!");
				return false;
			}
		}

		public float GetData(int x, int y) {
			if(x < 0 || y < 0 || x >= ncols || y >= nrows) {
				return nodata_value;
			} else {
				return data[x, y];
			}
		}

		private string ReadDataRaw(FileStream stream, bool valueOnly) {
			StringBuilder str = new StringBuilder();
			int b = stream.ReadByte();
			if(b < 0) {
				Program.WriteWarning("WARNING: EOF reached!");
				return "";
			}
			while(str.Length == 0 && EndString(b, valueOnly)) b = stream.ReadByte();
			if(!EndString(b, valueOnly)) str.Append((char)b);
			while(!EndString(b, valueOnly)) {
				b = stream.ReadByte();
				if(!EndString(b, valueOnly)) str.Append((char)b);
			}
			return str.ToString();
		}

		private string ReadLine(FileStream stream) {
			return GetCleanedString(ReadDataRaw(stream, false));
		}

		private string ReadHeaderLine(FileStream stream) {
			string str = ReadDataRaw(stream, false);
			fileHeader += str + "\n";
			return GetCleanedString(str);
		}

		private string GetCleanedString(string line) {
			while(line.StartsWith(' ')) line = line.Substring(1);
			line = line.Replace("  ", " ");
			line = line.Replace("\r", "");
			while(line.EndsWith(' ')) line = line.Substring(0, line.Length - 2);
			return line;
		}

		private bool EndString(int b, bool spaces) {
			if(b < 0) return true;
			char c = (char)b;
			if(spaces && c == ' ') return true;
			if(c == '\n') return true;
			return false;
		}

		private string ExtractString(string input, string keyname) {
			input = input.ToLower();
			input = input.Replace(keyname.ToLower(), "");
			input = input.Replace(" ", "");
			return input;
		}

		private int ExtractInt(string input, string keyname) {
			try {
				return int.Parse(ExtractString(input, keyname));
			} catch(Exception e) {
				Program.WriteError("Failed to parse to int: "+ExtractString(input, keyname));
				throw e;
			}
		}

		private float ExtractFloat(string input, string keyname) {
			try {
				return float.Parse(ExtractString(input, keyname));
			} catch(Exception e) {
				Program.WriteError("Failed to parse to float: "+ExtractString(input, keyname));
				throw e;
			}
		}
	} 
}