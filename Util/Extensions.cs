using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Util {
	public static class Extensions {

		public static bool TryGetArgument(this string[] array, string name) {
			for(int i = 0; i < array.Length; i++) {
				if(array[i].ToLower().StartsWith(name.ToLower())) {
					return true;
				}
			}
			return false;
		}

		public static bool TryGetArgument<T>(this string[] array, string name, out T value) {
			for(int i = 0; i < array.Length; i++) {
				if(array[i].ToLower().StartsWith(name.ToLower())) {
					var split = array[i].Split('=');
					if(split.Length > 1) {
						value = (T)Convert.ChangeType(split[1], typeof(T));
					} else {
						value = default;
						return false;
					}
					return true;
				}
			}
			value = default;
			return false;
		}
	}
}
