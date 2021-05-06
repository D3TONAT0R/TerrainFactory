using HMCon.Util;
using System;
using System.IO;
using System.Text;

namespace HMCon.Export.Exporters {
	public class PointDataExporter : IExporter {

		private HeightData data;

		public PointDataExporter(HeightData source) {
			data = source;
		}

		public bool NeedsFileStream(FileFormat format) {
			return true;
		}

		public void WriteFile(FileStream stream, string path, FileFormat filetype) {
			if(filetype.IsFormat("ASC")) {
				WriteFileASC(stream);
			} else if(filetype.IsFormat("PTS_XYZ")) {
				WriteFileXYZ(stream);
			}
		}

		private void WriteFileASC(FileStream stream, int decimals = 2) {
			WriteString(stream, "ncols        " + (data.GridWidth) + "\n");
			WriteString(stream, "nrows        " + (data.GridHeight) + "\n");
			WriteString(stream, "xllcorner    " + (data.lowerCornerPos.X) + "\n");
			WriteString(stream, "yllcorner    " + (data.lowerCornerPos.Y) + "\n");
			WriteString(stream, "cellsize     " + (data.cellSize) + "\n");
			WriteString(stream, "NODATA_value " + data.nodata_value + "\n");
			var grid = data.GetDataGrid();

			string format = "";
			int mostZeroes = Math.Max(Math.Abs((int)data.highestValue).ToString().Length, Math.Abs((int)data.lowestValue).ToString().Length);
			for(int i = 0; i < mostZeroes; i++) {
				format += '0';
			}
			format += ".";
			for(int i = 0; i < decimals; i++) {
				format += '0';
			}

			format = " " + format + ";" + "-" + format;

			int y = data.GridHeight - 1;
			while(y >= 0) {
				int x = 0;
				StringBuilder str = new StringBuilder();
				while(x < data.GridWidth) {
					if(str.Length > 0) str.Append(" ");
					string value = grid[x, y].ToString(format);
					str.Append(value);
					x++;
				}
				str.Append("\n");
				WriteString(stream, str.ToString());
				y--;
			}
		}

		private void WriteFileXYZ(FileStream stream) {
			var grid = data.GetDataGrid();
			for(int y = 0; y < data.GridHeight; y++) {
				for(int x = 0; x < data.GridWidth; x++) {
					float f = grid[x, y];
					if(f != data.nodata_value) {
						var bytes = Encoding.ASCII.GetBytes(x * data.cellSize + " " + y * data.cellSize + " " + f + "\n");
						stream.Write(bytes, 0, bytes.Length);
					}
				}
			}
		}

		private void WriteString(FileStream stream, string str) {
			var bytes = Encoding.ASCII.GetBytes(str);
			stream.Write(bytes, 0, bytes.Length);
		}
	}
}
