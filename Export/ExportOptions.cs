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
	} 
}