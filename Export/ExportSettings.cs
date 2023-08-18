using TerrainFactory.Formats;
using TerrainFactory.Modification;
using TerrainFactory.Util;
using System;
using System.Collections.Generic;

namespace TerrainFactory.Export {
	public class ExportSettings {

		public int splitInterval = -1;

		private readonly Dictionary<string, object> customSettings = new Dictionary<string, object>();

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