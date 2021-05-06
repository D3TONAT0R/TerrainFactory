using HMCon.Modification;
using HMCon.Util;
using System;
using System.Collections.Generic;

namespace HMCon.Export {
	public class ExportSettings {

		public List<Modifier> modificationChain = new List<Modifier>();

		public List<FileFormat> outputFormats = new List<FileFormat>();
		public int fileSplitDims = -1;

		private readonly Dictionary<string, object> customSettings = new Dictionary<string, object>();

		//Format specific settings
		/*public int mcaOffsetX = 0;
		public int mcaOffsetZ = 0;
		public bool useSplatmaps;*/

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

		public bool ContainsFormat(params string[] ids) {
			foreach(var f in outputFormats) {
				foreach(var id in ids) {
					if(f.Identifier == id.ToUpper()) return true;
				}
			}
			return false;
		}

		public void SetCustomSetting<T>(string key, T value) {
			if(customSettings.ContainsKey(key)) {
				customSettings[key] = value;
			} else {
				customSettings.Add(key, value);
			}
		}

		public bool ToggleCustomBoolSetting(string key) {
			if(customSettings.ContainsKey(key) && customSettings[key] is bool b) {
				b = !b;
				customSettings[key] = b;
				return b;
			} else {
				customSettings.Add(key, true);
				return true;
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

		public void AddModifierToChain(Modifier modifier, bool replaceSameType = true) {
			if(!replaceSameType) {
				modificationChain.Add(modifier);
			} else {
				for(int i = 0; i < modificationChain.Count; i++) {
					if(modificationChain[i].GetType() == modifier.GetType()) {
						modificationChain[i] = modifier;
						return;
					}
				}
				//A modifier of the same type was not found, append it instead
				modificationChain.Add(modifier);
			}
		}
	}
}