using HMCon.Formats;
using HMCon.Modification;
using HMCon.Util;
using System;
using System.Collections.Generic;

namespace HMCon.Export {
	public class ExportSettings {

		//TODO: move to Job class? Redundant when using ExportManager.ExportAs()
		public List<FileFormat> outputFormats = new List<FileFormat>();
		public int splitInterval = -1;

		private readonly Dictionary<string, object> customSettings = new Dictionary<string, object>();

		public void SetOutputFormats(string[] inputs, bool append) {
			if(!append) outputFormats.Clear();
			foreach(string input in inputs) {
				if(string.IsNullOrWhiteSpace(input)) continue;
				var ff = FileFormat.GetFromCommandInput(input);
				if(ff != null) {
					outputFormats.Add(ff);
				} else {
					ConsoleOutput.WriteWarning("Unknown or unsupported format: " + input);
				}
			}
		}

		public bool ContainsFormat(params Type[] formatTypes) {
			foreach(var f in outputFormats) {
				foreach(var t in formatTypes) {
					if(f.GetType() == t) return true;
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

		public void RemoveCustomSetting(string key)
		{
			if(customSettings.ContainsKey(key))
			{
				customSettings.Remove(key);
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
	}
}