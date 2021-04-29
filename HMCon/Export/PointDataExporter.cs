using HMCon.Util;
using System;
using System.IO;
using System.Text;

namespace HMCon.Export.Exporters {
	public class PointDataExporter : IExporter {

		private HeightData data;
		private int subsampling;
		private Bounds bounds;

		public PointDataExporter(HeightData source, int subsampling, Bounds bounds) {
			data = source;
			this.subsampling = subsampling;
			this.bounds = bounds;
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
			WriteString(stream, "ncols        " + (bounds.NumCols / subsampling) + "\n");
			WriteString(stream, "nrows        " + (bounds.NumRows / subsampling) + "\n");
			WriteString(stream, "xllcorner    " + (data.lowerCornerPos.X + bounds.xMin * data.cellSize) + "\n");
			WriteString(stream, "yllcorner    " + (data.lowerCornerPos.Y + bounds.yMin * data.cellSize) + "\n");
			WriteString(stream, "cellsize     " + (data.cellSize * subsampling) + "\n");
			WriteString(stream, "NODATA_value " + data.nodata_value + "\n");
			int y = bounds.yMax;
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

			while(y >= bounds.yMin) {
				int x = bounds.xMin;
				StringBuilder str = new StringBuilder();
				while(x <= bounds.xMax) {
					if(str.Length > 0) str.Append(" ");
					string value = grid[x, y].ToString(format);
					str.Append(value);
					x += subsampling;
				}
				str.Append("\n");
				WriteString(stream, str.ToString());
				y -= subsampling;
			}
		}

		private void WriteFileXYZ(FileStream stream) {
			var grid = data.GetDataGrid();
			for(int y = bounds.yMin; y <= bounds.yMax; y++) {
				for(int x = bounds.xMin; x <= bounds.xMax; x++) {
					if(x % subsampling == 0 && y % subsampling == 0) {
						float f = grid[x, y];
						if(f != data.nodata_value) {
							var bytes = Encoding.ASCII.GetBytes(x * data.cellSize + " " + y * data.cellSize + " " + f + "\n");
							stream.Write(bytes, 0, bytes.Length);
						}
					}
				}
			}
			stream.Close();
		}

		private void WriteString(FileStream stream, string str) {
			var bytes = Encoding.ASCII.GetBytes(str);
			stream.Write(bytes, 0, bytes.Length);
		}
	}
}
