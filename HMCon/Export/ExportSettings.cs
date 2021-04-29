using HMCon.Util;
using System;
using System.Collections.Generic;

namespace HMCon.Export {
	public class ExportSettings {

		public List<FileFormat> outputFormats = new List<FileFormat>();
		public int Subsampling {
			get { return subsampling_value; }
			set { subsampling_value = Math.Max(1, value); }
		}
		private int subsampling_value = 1;
		public int fileSplitDims = -1;
		public Bounds exportRange = new Bounds(0, 0, 0, 0);
		public bool UseExportRange {
			get {
				return exportRange.xMax > 0 && exportRange.yMax > 0;
			}
		}

		private readonly Dictionary<string, object> customSettings = new Dictionary<string, object>();

		//Format specific settings
		public int mcaOffsetX = 0;
		public int mcaOffsetZ = 0;
		public bool useSplatmaps;

		public void SetOutputFormats(string[] inputs, bool append) {
			if(!append) outputFormats.Clear();
			foreach(string input in inputs) {
				if(string.IsNullOrWhiteSpace(input)) continue;
				var ff = ExportUtility.GetFormatFromInput(input);
				if(ff != null) {
					outputFormats.Add(ff);
				} else {
					ConsoleOutput.WriteWarning("Unknown or unsupported format: " + input);
				}
			}
		}

		public bool SetExportRange(HeightData checkData, int x1, int y1, int x2, int y2) {
			if(x1 < 0 || x1 >= checkData.GridWidth) return false;
			if(y1 < 0 || y1 >= checkData.GridHeight) return false;
			if(x2 < 0 || x2 >= checkData.GridWidth) return false;
			if(y2 < 0 || y2 >= checkData.GridHeight) return false;
			if(x1 > x2) return false;
			if(y1 > y2) return false;
			exportRange = new Bounds(x1, y1, x2, y2);
			return true;
		}


		public bool ContainsFormat(params string[] ids) {
			foreach(var f in outputFormats) {
				foreach(var id in ids) {
					if(f.Identifier == id.ToUpper()) return true;
				}
			}
			return false;
		}

		public int ExportRangeCellCount {
			get {
				if(UseExportRange) {
					return (exportRange.xMax - exportRange.xMin + 1) * (exportRange.yMax - exportRange.yMin + 1);
				} else {
					return 0;
				}
			}
		}

		public void SetCustomSetting<T>(string key, T value) {
			if(customSettings.ContainsKey(key)) {
				customSettings[key] = value;
			} else {
				customSettings.Add(key, value);
			}
		}

		public bool HasCustomSetting<T>(string key) {
			if(customSettings.ContainsKey(key)) {
				return customSettings[key].GetType() == typeof(T);
			} else {
				return false;
			}
		}

		public T GetCustomSetting<T>(string key, T defaultValue) {
			if(HasCustomSetting<T>(key)) {
				return (T)customSettings[key];
			} else {
				return defaultValue;
			}
		}
	}
}