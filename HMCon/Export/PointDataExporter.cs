using HMCon.Util;
using System.IO;
using System.Text;

namespace HMCon.Export.Exporters {
	public class PointDataExporter : IExporter {

		private ASCData data;
		private int subsampling;
		private Bounds bounds;

		public PointDataExporter(ASCData source, int subsampling, Bounds bounds) {
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

		private void WriteFileASC(FileStream stream) {
			WriteString(stream, "ncols        " + (bounds.NumCols / subsampling) + "\n");
			WriteString(stream, "nrows        " + (bounds.NumRows / subsampling) + "\n");
			WriteString(stream, "xllcorner    " + (data.xllcorner + bounds.xMin * data.cellsize) + "\n");
			WriteString(stream, "yllcorner    " + (data.xllcorner + bounds.yMin * data.cellsize) + "\n");
			WriteString(stream, "cellsize     " + (data.cellsize * subsampling) + "\n");
			WriteString(stream, "NODATA_value " + data.nodata_value + "\n");
			int y = bounds.yMax;
			while(y >= bounds.yMin) {
				int x = bounds.xMin;
				StringBuilder str = new StringBuilder();
				while(x <= bounds.xMax) {
					if(str.Length > 0) str.Append(" ");
					str.Append(data.data[x, y]);
					x += subsampling;
				}
				str.Append("\n");
				WriteString(stream, str.ToString());
				y -= subsampling;
			}
			stream.Close();
		}

		private void WriteFileXYZ(FileStream stream) {
			for(int y = bounds.yMin; y <= bounds.yMax; y++) {
				for(int x = bounds.xMin; x <= bounds.xMax; x++) {
					if(x % subsampling == 0 && y % subsampling == 0) {
						float f = data.data[x, y];
						if(f != data.nodata_value) {
							var bytes = Encoding.ASCII.GetBytes(x * data.cellsize + " " + y * data.cellsize + " " + f + "\n");
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
