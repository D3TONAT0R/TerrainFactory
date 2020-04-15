using System;
using System.Collections.Generic;

namespace ASCReader.Export {
	public class ExportOptions {

		public List<FileFormat> outputFormats = new List<FileFormat>();
		public int subsampling {
			get { return subsampling_value; }
			set { subsampling_value = Math.Max(1, value); }
		}
		private int subsampling_value = 1;
		public int fileSplitDims = -1;
		public (int xMin, int yMin, int xMax, int yMax) exportRange = (0,0,0,0);
		public bool useExportRange {
			get{
				return exportRange.xMax > 0 && exportRange.yMax > 0;
			}
		}

		public void SetOutputFormats(string[] inputs, bool append) {
			if(!append) outputFormats.Clear();
			foreach(string s in inputs) {
				if(string.IsNullOrEmpty(s)) continue;
				var ff = s.GetFileFormat();
				if(ff != FileFormat.UNKNOWN) {
					outputFormats.Add(ff);
				} else {
					Program.WriteWarning("Unknown format: " + s);
				}
			}
		}

		public bool SetExportRange(ASCData checkData, int x1, int y1, int x2, int y2) {
			if(x1 < 0 || x1 >= checkData.ncols) return false;
			if(y1 < 0 || y1 >= checkData.nrows) return false;
			if(x2 < 0 || x2 >= checkData.ncols) return false;
			if(y2 < 0 || y2 >= checkData.nrows) return false;
			if(x1 > x2) return false;
			if(y1 > y2) return false;
			exportRange = (x1,y1,x2,y2);
			return true;
		}

		public int ExportRangeCellCount {
			get {
				if(useExportRange) {
					return (exportRange.xMax-exportRange.xMin+1)*(exportRange.yMax-exportRange.yMin+1);
				} else {
					return 0;
				}
			}
		}
	} 
}