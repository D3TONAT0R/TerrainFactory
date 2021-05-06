using HMCon.Export;
using HMCon.Import;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace HMCon {
	public static class PluginLoader {

		public static Dictionary<string, HMConPlugin> loadedPlugins;
		public static int NumPluginsLoaded => loadedPlugins.Count;

		internal static void LoadPlugins(string pluginPath) {
			loadedPlugins = new Dictionary<string, HMConPlugin>();
			var dllFiles = Directory.GetFiles(pluginPath, "*.dll");
			foreach (var path in dllFiles) {
				try {
					var assembly = Assembly.LoadFrom(path);
					foreach(var t in assembly.GetTypes()) {
						if(t.BaseType != null && t.BaseType.Name == typeof(HMConPlugin).Name && !t.IsAbstract) {
							var plugin = (HMConPlugin)Activator.CreateInstance(t);
							string info = "";

							var i = plugin.GetImportHandler();
							if(i != null) {
								ImportManager.RegisterHandler(i);
								info += info.Length > 0 ? "+I" : "I";
							}
							var e = plugin.GetExportHandler();
							if(e != null) {
								ExportUtility.RegisterHandler(e);
								info += info.Length > 0 ? "+E" : "E";
							}
							var c = plugin.GetCommandHandler();
							if(c != null) {
								CommandHandler.commandHandlers.Add(c);
								info += info.Length > 0 ? "+C" : "C";
							}


							var attribute = t.GetCustomAttribute<PluginInfoAttribute>();
							string pluginID;
							if(attribute != null) {
								ConsoleOutput.WriteLine($"Loaded Plugin '{attribute.Name}' [{info}]");
								pluginID = attribute.ID.ToUpper();
							} else {
								ConsoleOutput.WriteWarning($"Plugin with class '{t.FullName}' does not specify a Plugin name!");
								pluginID = "["+t.Name+"]";
							}
							loadedPlugins.Add(pluginID, plugin);
							continue;
						}
					}
					//ConsoleOutput.WriteWarning("Not a plugin dll: " + path);
				}
				catch {
					//ConsoleOutput.WriteWarning("Failed to load dll: " + path);
				}
			}
		}

		public static bool IsPluginLoaded(string id) {
			return loadedPlugins.ContainsKey(id.ToUpper());
		}
	}
}
