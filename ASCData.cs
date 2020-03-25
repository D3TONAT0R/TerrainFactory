using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using ASCReader.Export;
using ASCReader.Export.Exporters;
using Microsoft.Win32.SafeHandles;

namespace ASCReader {
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

		public ASCData(string filepath) {
			if(!File.Exists(filepath)) {
				Program.WriteError("File " + filepath + " does not exist!");
			}
			try {
				filename = Path.GetFileNameWithoutExtension(filepath);
				FileStream stream = File.OpenRead(filepath);
				ncols = int.Parse(ExtractValue(ReadHeaderLine(stream), "ncols"));
				nrows = int.Parse(ExtractValue(ReadHeaderLine(stream), "nrows"));
				Program.WriteLine("Dimensions: " + ncols + "x" + nrows);
				xllcorner = float.Parse(ExtractValue(ReadHeaderLine(stream), "xllcorner"));
				yllcorner = float.Parse(ExtractValue(ReadHeaderLine(stream), "yllcorner"));
				cellsize = float.Parse(ExtractValue(ReadHeaderLine(stream), "cellsize"));
				nodata_value = float.Parse(ExtractValue(ReadHeaderLine(stream), "NODATA_value"));
				//Read the actual data
				data = new float[ncols, nrows];
				for(int y = 0; y < nrows; y++) {
					string ln = ReadLine(stream);
					string[] split = ln.Split(' ');
					if(split.Length != ncols) throw new FormatException("Column count at row " + y + " does not match the required length! Required: " + ncols + " got: " + split.Length);
					for(int x = 0; x < ncols; x++) {
						float val = float.Parse(split[x]);
						if(val < lowestValue) lowestValue = val;
						if(val > highestValue) highestValue = val;
						data[x, nrows - y - 1] = val;
					}
				}
				isValid = true;
			} catch(Exception e) {
				Program.WriteError("Error occured while reading ASC file!");
				Program.WriteLine(e.ToString());
				Program.WriteLine("");
				isValid = false;
			}
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

		private string ReadLineRaw(FileStream stream) {
			StringBuilder str = new StringBuilder();
			int b = stream.ReadByte();
			if(b < 0) {
				Program.WriteWarning("WARNING: EOF reached!");
				return "";
			}
			if(!EndString(b)) str.Append((char)b);
			while(!EndString(b)) {
				b = stream.ReadByte();
				if(!EndString(b)) str.Append((char)b);
			}
			return str.ToString();
		}

		private string ReadLine(FileStream stream) {
			return GetCleanedString(ReadLineRaw(stream));
		}

		private string ReadHeaderLine(FileStream stream) {
			string str = ReadLineRaw(stream);
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

		private bool EndString(int b) {
			if(b < 0) return true;
			char c = (char)b;
			if(c == '\n') return true;
			return false;
		}

		private string ExtractValue(string input, string keyname) {
			input = input.Replace(keyname, "");
			input = input.Replace(" ", "");
			return input;
		}
	} 
}