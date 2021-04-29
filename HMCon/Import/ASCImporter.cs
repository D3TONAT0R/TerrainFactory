using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using static HMCon.ConsoleOutput;

namespace HMCon.Import {
	public static class ASCImporter {

		private static HeightData current;

		public static HeightData Import(string filepath) {
			if(!File.Exists(filepath)) {
				ConsoleOutput.WriteError("File " + filepath + " does not exist!");
			}
			HeightData data;
			try {
				string filename = Path.GetFileNameWithoutExtension(filepath);
				using(FileStream stream = File.OpenRead(filepath)) {
					current = CreateBaseData(stream, filepath, out int ncols, out int nrows);
					//Read the actual data
					ReadGridData(stream, ncols, nrows);
				}
				current.isValid = true;
				
			} catch(Exception e) {
				throw new IOException("ASC import failed", e);
			} finally {
				data = current;
				current = null;
			}
			return data;
		}

		public static void GetDataInfo(string filepath, out float lowest, out float highest, out float average) {
			try {
				using(FileStream stream = File.OpenRead(filepath)) {
					var asc = CreateBaseData(stream, filepath, out int ncols, out int nrows);
					//Read the actual data
					//The cells will not be saved as long as grid is null
					ReadGridData(stream, ncols, nrows);
					double sum = 0;
					for(int i = 0; i < asc.GridWidth * asc.GridHeight; i++) {
						float value;
						if(!NextGridValue(stream, out value)) break;
						sum += value;
					}
					lowest = asc.lowestValue;
					highest = asc.highestValue;
					average = (float)(sum / (asc.GridWidth * asc.GridHeight));
				}
			} catch(Exception e) {
				WriteError("Error occured while getting summary for ASC file!");
				WriteLine(e.ToString());
				WriteLine("");
				lowest = 0;
				highest = 0;
				average = 0;
			}
		}


		static HeightData CreateBaseData(FileStream stream, string filename, out int ncols, out int nrows) {
			ncols = ExtractInt(ReadHeaderLine(stream), "ncols");
			nrows = ExtractInt(ReadHeaderLine(stream), "nrows");
			WriteLine("Dimensions: " + ncols + "x" + nrows);
			HeightData d = new HeightData(ncols, nrows, filename);
			var xllcorner = ExtractFloat(ReadHeaderLine(stream), "xllcorner");
			var yllcorner = ExtractFloat(ReadHeaderLine(stream), "yllcorner");
			d.lowerCornerPos = new Vector2(xllcorner, yllcorner);
			d.cellSize = ExtractFloat(ReadHeaderLine(stream), "cellsize");
			d.nodata_value = ExtractFloat(ReadHeaderLine(stream), "NODATA_value");
			return d;
		}

		static void ReadGridData(FileStream stream, int ncols, int nrows) {
			int length = ncols * nrows;
			for(int i = 0; i < length; i++) {
				float value;
				if(!NextGridValue(stream, out value)) break;
				if(Math.Abs(value - current.nodata_value) > 0.1f) {
					if(value < current.lowestValue) current.lowestValue = value;
					if(value > current.highestValue) current.highestValue = value;
				}
				int y = (int)Math.Floor(i / (double)ncols);
				int x = i % ncols;
				current.lowPoint = current.lowestValue;
				current.highPoint = current.highestValue;
				if(current.HasHeightData) current.SetHeight(x, nrows - y - 1, value);
			}
		}

		static bool NextGridValue(FileStream stream, out float value) {
			var d = NextValue(stream);
			if(d == "") {
				//Premature EOF
				WriteError("Premature EOF reached!");
				value = 0;
				return false;
			}
			value = float.Parse(d);
			return true;
		}

		static string NextValue(FileStream stream) {
			return ReadDataRaw(stream, true);
		}

		static string ReadDataRaw(FileStream stream, bool valueOnly) {
			StringBuilder str = new StringBuilder();
			int b = stream.ReadByte();
			if(b < 0) {
				WriteWarning("WARNING: EOF reached!");
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

		static string ReadLine(FileStream stream) {
			return GetCleanedString(ReadDataRaw(stream, false));
		}

		static string ReadHeaderLine(FileStream stream) {
			string str = ReadDataRaw(stream, false);
			return GetCleanedString(str);
		}

		static string GetCleanedString(string line) {
			line = line.Trim();
			line = line.Replace("\r", "");
			while(line.Contains("  ")) line = line.Replace("  ", " ");
			return line;
		}

		static bool EndString(int b, bool spaces) {
			if(b < 0) return true;
			char c = (char)b;
			if(spaces && c == ' ') return true;
			if(c == '\n') return true;
			return false;
		}

		static string ExtractString(string input, string keyname) {
			input = input.ToLower();
			input = input.Replace(keyname.ToLower(), "");
			input = input.Replace(" ", "");
			return input;
		}

		static int ExtractInt(string input, string keyname) {
			try {
				return int.Parse(ExtractString(input, keyname));
			} catch(Exception e) {
				WriteError("Failed to parse to int: " + ExtractString(input, keyname));
				throw e;
			}
		}

		static float ExtractFloat(string input, string keyname) {
			try {
				return float.Parse(ExtractString(input, keyname));
			} catch(Exception e) {
				WriteError("Failed to parse to float: " + ExtractString(input, keyname));
				throw e;
			}
		}
	}
}
