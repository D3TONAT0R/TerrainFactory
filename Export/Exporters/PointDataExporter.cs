using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Text;

namespace ASCReader.Export.Exporters {
	public class PointDataExporter : IExporter {

		private ASCData data;
		private int subsampling;
		private int xMin, xMax, yMin, yMax;

		public PointDataExporter(ASCData source, int subsampling, int xMin, int yMin, int xMax, int yMax) {
			data = source;
			this.subsampling = subsampling;
			this.xMin = xMin;
			this.yMin = yMin;
			this.xMax = xMax;
			this.yMax = yMax;
		}

		public void WriteFile(FileStream stream, FileFormat filetype) {
			if(filetype == FileFormat.ASC) {
				WriteFileASC(stream);
			} else if(filetype == FileFormat.PTS_XYZ) {
				WriteFileXYZ(stream);
			}
		}

		private void WriteFileASC(FileStream stream) {
			WriteString(stream, "ncols        " + ((xMax-xMin) / subsampling) + "\n");
			WriteString(stream, "nrows        " + ((yMax-yMin) / subsampling) + "\n");
			WriteString(stream, "xllcorner    " + (data.xllcorner + xMin * data.cellsize) + "\n");
			WriteString(stream, "yllcorner    " + (data.xllcorner + yMin * data.cellsize) + "\n");
			WriteString(stream, "cellsize     " + (data.cellsize * subsampling) + "\n");
			WriteString(stream, "NODATA_value " + data.nodata_value + "\n");
			int y = yMax-1;
			while(y >= yMin) {
				int x = xMin;
				StringBuilder str = new StringBuilder();
				while(x < xMax) {
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
			for(int y = yMin; y < yMax; y++) {
				for(int x = xMin; x < xMax; x++) {
					if(x % subsampling == 0 && y % subsampling == 0) {
						float f = data.data[x, y];
						if(f != data.nodata_value) {
							stream.Write(Encoding.ASCII.GetBytes(x * data.cellsize + " " + y * data.cellsize + " " + f + "\n"));
						}
					}
				}
			}
			stream.Close();
		}

		private void WriteString(FileStream stream, string str) {
			stream.Write(Encoding.ASCII.GetBytes(str));
		}
	}
}
