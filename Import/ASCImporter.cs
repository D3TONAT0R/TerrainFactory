using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using static TerrainFactory.ConsoleOutput;

namespace TerrainFactory.Import {
	public class ASCImporter {

		public static HeightData Import(string filepath, params string[] args) {
			int sub = 1;
			if(args != null && args.TryGetArgument("sub", out int arg))
			{
				sub = Math.Max(1, arg);
			}
			HeightData data;
			try {
				string filename = Path.GetFileNameWithoutExtension(filepath);
				using(FileStream stream = File.OpenRead(filepath)) {
					data = CreateBaseData(stream, filepath, sub, out int ncols, out int nrows);
					//Read the actual data
					ReadGridData(stream, data, ncols, nrows, sub, data.GridCellCount > 200000);
				}
				data.isValid = true;
				WriteLine($"Height Range: low {data.lowestValue}, high {data.highestValue}, range {data.highestValue - data.lowestValue}");
			} catch(Exception e) {
				throw new IOException("ASC import failed", e);
			}
			return data;
		}

		public static void GetDataInfo(string filepath, out float lowest, out float highest, out float average) {
			try {
				using(FileStream stream = File.OpenRead(filepath)) {
					var asc = CreateBaseData(stream, filepath, 1, out int ncols, out int nrows);
					//Read the actual data
					//The cells will not be saved as long as grid is null
					asc.SetDataGrid(null);
					ReadGridData(stream, asc, ncols, nrows, 1, false);
					double sum = 0;
					for(int i = 0; i < asc.GridLengthX * asc.GridLengthY; i++) {
						float value;
						if(!NextGridValue(stream, out value)) break;
						sum += value;
					}
					lowest = asc.lowestValue;
					highest = asc.highestValue;
					average = (float)(sum / (asc.GridLengthX * asc.GridLengthY));
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


		static HeightData CreateBaseData(FileStream stream, string filename, int sub, out int ncols, out int nrows) {
			ncols = ExtractInt(ReadHeaderLine(stream), "ncols");
			nrows = ExtractInt(ReadHeaderLine(stream), "nrows");
			WriteLine("Dimensions: " + ncols + "x" + nrows);
			HeightData d = new HeightData((int)Math.Ceiling(ncols / (float)sub), (int)Math.Ceiling(nrows / (float)sub), filename);
			var xllcorner = ExtractFloat(ReadHeaderLine(stream), "xllcorner");
			var yllcorner = ExtractFloat(ReadHeaderLine(stream), "yllcorner");
			d.lowerCornerPos = new Vector2(xllcorner, yllcorner);
			d.cellSize = ExtractFloat(ReadHeaderLine(stream), "cellsize") * sub;
			d.nodataValue = ExtractFloat(ReadHeaderLine(stream), "NODATA_value");
			return d;
		}

		static void ReadGridData(FileStream stream, HeightData data, int ncols, int nrows, int sub, bool displayProgressBar) {
			int length = ncols * nrows;
			for(int i = 0; i < length; i++) {
				if(displayProgressBar && (i % 1000 == 0))
				{
					UpdateProgressBar("Loading cells", i / (float)length);
				}
				int y = (int)Math.Floor(i / (double)ncols);
				int x = i % ncols;
				if(!NextGridValue(stream, out float value)) break;
				if(x % sub > 0 || y % sub > 0)
				{
					//Skip this cell due to import subsampling
					continue;
				}
				if(Math.Abs(value - data.nodataValue) > 0.1f) {
					if(value < data.lowestValue) data.lowestValue = value;
					if(value > data.highestValue) data.highestValue = value;
				}
				data.lowPoint = data.lowestValue;
				data.highPoint = data.highestValue;
				if(data.HasHeightData) data.SetHeight(x / sub, data.GridLengthY - (y / sub) - 1, value);
			}
			if(displayProgressBar)
			{
				ClearProgressBar();
			}
		}

		static bool NextGridValue(FileStream stream, out float value) {
			var d = ReadDataRaw(stream);
			if(d == "") {
				//Premature EOF
				WriteError("Premature EOF reached!");
				value = 0;
				return false;
			}
			value = float.Parse(d);
			return true;
		}

		static string ReadLine(FileStream stream)
		{
			int b = stream.ReadByte();
			if (b < 0)
			{
				WriteWarning("WARNING: EOF reached!");
				return "";
			}
			StringBuilder sb = new StringBuilder();
			char c = (char)b;
			while(c != '\n')
			{
				sb.Append(c);
				b = stream.ReadByte();
				if(b < 0)
				{
					return sb.ToString();
				}
				else
				{
					c = (char)b;
				}
			}
			return sb.ToString().Replace("\r", "");
		}

		static string ReadDataRaw(FileStream stream) {
			int b = stream.ReadByte();
			if(b < 0) {
				WriteWarning("WARNING: EOF reached!");
				return "";
			}
			//Skip leading whitespaces
			while(char.IsWhiteSpace((char)b))
			{
				b = stream.ReadByte();
				if(b < 0)
				{
					WriteWarning("WARNING: EOF reached!");
					return "";
				}
			}
			StringBuilder str = new StringBuilder();
			while(b >= 0 && !char.IsWhiteSpace((char)b))
			{
				str.Append((char)b);
				b = stream.ReadByte();
			}
			return str.ToString();
		}

		static string ReadHeaderLine(FileStream stream) {
			string str = ReadLine(stream);
			return str.Trim();
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
			input = input.Trim();
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
