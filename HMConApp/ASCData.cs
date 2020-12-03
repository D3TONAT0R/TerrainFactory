using HMCon.Export;
using HMCon.Util;
using System;
using System.IO;
using System.Text;

namespace HMCon {

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
		public float cellsize_x;
		public float cellsize_y;
		public float cellsize {
			get {
				return cellsize_x;
			}
			set {
				cellsize_x = value;
				cellsize_y = value;
			}
		}
		public float nodata_value;

		public string fileHeader = "";

		public float[,] data;
		public float lowestValue = float.PositiveInfinity;
		public float highestValue = float.NegativeInfinity;

		//Used for scaling operations. In data created from an image, these values represent the black and white values of the source image (default 0 and 1 respectively)
		//In data created from ASC data itself, these are equal to lowestValue and highestValue unless overridden for heightmap export.
		public float lowPoint;
		public float highPoint;

		public bool isValid;

		public ASCData() {

		}

		public ASCData(int ncols, int nrows, string sourceFile) {
			filename = sourceFile;
			data = new float[ncols, nrows];
			this.ncols = ncols;
			this.nrows = nrows;
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
				stream.Close();
				isValid = true;
			} catch(Exception e) {
				Program.WriteError("Error occured while reading ASC file!");
				Program.WriteLine(e.ToString());
				Program.WriteLine("");
				isValid = false;
			}
		}

		public ASCData(ASCData original) {
			filename = original.filename;
			ncols = original.ncols;
			nrows = original.nrows;
			xllcorner = original.xllcorner;
			yllcorner = original.yllcorner;
			cellsize = original.cellsize;
			nodata_value = original.nodata_value;
			fileHeader = original.fileHeader;
			data = (float[,])original.data.Clone();
			RecalculateValues(false);
			lowPoint = original.lowPoint;
			highPoint = original.highPoint;
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
				for(int i = 0; i < asc.ncols * asc.nrows; i++) {
					float value;
					if(!asc.NextGridValue(stream, out value)) break;
					sum += value;
				}
				summary.lowestValue = asc.lowestValue;
				summary.highestValue = asc.highestValue;
				summary.averageValue = (float)(sum / (asc.ncols * asc.nrows));
				stream.Close();
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
			int length = ncols * nrows;
			for(int i = 0; i < length; i++) {
				float value;
				if(!NextGridValue(stream, out value)) break;
				if(Math.Abs(value - nodata_value) > 0.1f) {
					if(value < lowestValue) lowestValue = value;
					if(value > highestValue) highestValue = value;
				}
				int y = (int)Math.Floor(i / (double)ncols);
				int x = i % ncols;
				lowPoint = lowestValue;
				highPoint = highestValue;
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

		public void SetRange(float low, float high) {
			float dataRange = high - low;
			for(int x = 0; x < ncols; x++) {
				for(int y = 0; y < nrows; y++) {
					double h = (data[x, y] - lowPoint) / (highPoint - lowPoint);
					h *= dataRange;
					h += low;
					data[x, y] = (float)h;
				}
			}
			RecalculateValues(false);
			lowPoint = low;
			highPoint = high;
		}

		public void RecalculateValues(bool updateLowHighPoints) {
			foreach(float f in data) {
				if(Math.Abs(f - nodata_value) > 0.1f) {
					if(f < lowestValue) lowestValue = f;
					if(f > highestValue) highestValue = f;
				}
			}
			if(updateLowHighPoints) {
				lowPoint = lowestValue;
				highPoint = highestValue;
			}
		}

		public bool WriteAllFiles(string path, ExportOptions options) {
			int rangeMinX = 0;
			int rangeMinY = 0;
			int rangeMaxX = ncols - 1;
			int rangeMaxY = nrows - 1;
			if(options.useExportRange) {
				rangeMinX = options.exportRange.xMin;
				rangeMinY = options.exportRange.yMin;
				rangeMaxX = options.exportRange.xMax;
				rangeMaxY = options.exportRange.yMax;
			}
			string dir = Path.GetDirectoryName(path);
			CurrentExportJobInfo.mcaGlobalPosX = options.mcaOffsetX;
			CurrentExportJobInfo.mcaGlobalPosZ = options.mcaOffsetZ;
			if(Directory.Exists(dir)) {
				if(options.fileSplitDims < 32) {
					ExportUtility.CreateFilesForSection(this, filename, path, options, new Bounds(rangeMinX, rangeMinY, rangeMaxX, rangeMaxY));
				} else {
					int dims = options.fileSplitDims;
					int yMin = rangeMinY;
					int fileY = 0;
					while(yMin + dims <= rangeMaxY) {
						int xMin = rangeMinX;
						int fileX = 0;
						int yMax = Math.Min(yMin + dims, nrows);
						while(xMin + dims <= rangeMaxX) {
							int xMax = Math.Min(xMin + dims, ncols);
							CurrentExportJobInfo.exportNumX = fileX;
							CurrentExportJobInfo.exportNumZ = fileY;
							bool success = ExportUtility.CreateFilesForSection(this, filename, path, options, new Bounds(xMin, yMin, xMax, yMax));
							if(!success) throw new IOException("Failed to write file!");
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

		public float GetDataInterpolated(float x, float y) {
			int x1 = (int)x;
			int y1 = (int)y;
			int x2 = x1 + 1;
			int y2 = y1 + 1;
			x1 = Clamp(x1, 0, ncols - 1);
			x2 = Clamp(x2, 0, ncols - 1);
			y1 = Clamp(y1, 0, nrows - 1);
			y2 = Clamp(y2, 0, nrows - 1);
			float wx = x - x1;
			float wy = y - y1;
			float vx1 = Lerp(GetData(x1, y1), GetData(x2, y1), wx);
			float vx2 = Lerp(GetData(x1, y2), GetData(x2, y2), wx);
			return Lerp(vx1, vx2, wy);
		}

		int Clamp(int v, int min, int max) {
			return Math.Max(Math.Min(v, max), min);
		}

		float Lerp(float a, float b, float t) {
			return a + (b - a) * t;
		}

		public ASCData Resize(int newDimX, bool scaleHeight) {
			ASCData newData = new ASCData(this);
			int dimX = newDimX;
			float ratio = ncols / (float)nrows;
			int dimY = (int)(dimX / ratio);
			newData.data = GetResizedData(dimX, dimY);
			newData.ncols = dimX;
			newData.nrows = dimY;
			float resizeRatio = newData.ncols / (float)ncols;
			if(scaleHeight) {
				for(int x = 0; x < newData.ncols; x++) {
					for(int y = 0; y < newData.nrows; y++) {
						newData.data[x, y] *= resizeRatio;
					}
				}
			}
			newData.cellsize = resizeRatio;
			return newData;
		}

		public float[,] GetResizedData(int dimX, int dimY) {
			float[,] newData = new float[dimX, dimY];
			for(int x = 0; x < dimX; x++) {
				for(int y = 0; y < dimY; y++) {
					float nx = x / (float)dimX;
					float ny = y / (float)dimY;
					newData[x, y] = GetDataInterpolated(nx * ncols, ny * nrows);
				}
			}
			return newData;
		}

		public float[,] GetDataRange(Bounds bounds) {
			float[,] newdata = new float[bounds.NumCols, bounds.NumRows];
			for(int x = 0; x < bounds.NumCols; x++) {
				for(int y = 0; y < bounds.NumRows; y++) {
					newdata[x, y] = data[bounds.xMin + x, bounds.yMin + y];
				}
			}
			return newdata;
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
			while(line.StartsWith(" ")) line = line.Substring(1);
			line = line.Replace("  ", " ");
			line = line.Replace("\r", "");
			while(line.EndsWith(" ")) line = line.Substring(0, line.Length - 2);
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
				Program.WriteError("Failed to parse to int: " + ExtractString(input, keyname));
				throw e;
			}
		}

		private float ExtractFloat(string input, string keyname) {
			try {
				return float.Parse(ExtractString(input, keyname));
			} catch(Exception e) {
				Program.WriteError("Failed to parse to float: " + ExtractString(input, keyname));
				throw e;
			}
		}
	}
}