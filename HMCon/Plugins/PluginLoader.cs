using HMCon.Export;
using HMCon.Import;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace HMCon {
	static class PluginLoader {

		public static void LoadPlugins(string pluginPath) {
			Program.WriteLineSpecial(pluginPath);
			foreach(var path in Directory.GetFiles(pluginPath, "*.dll")) {
				try {
					var assembly = Assembly.LoadFrom(path);
					Program.WriteLineSpecial("Loading assembly: " + assembly.FullName);
					foreach(var t in assembly.GetTypes()) {
						if(t.IsSubclassOf(typeof(ASCReaderPlugin)) && !t.IsAbstract) {
							var plugin = (ASCReaderPlugin)Activator.CreateInstance(t);
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
							if(attribute != null) {
								Program.WriteLine($"Loaded Plugin '{attribute.Name}' [{info}]");
							} else {
								Program.WriteWarning($"Plugin with class '{t.FullName}' does not specify a Plugin name!");
							}
							Program.numPluginsLoaded++;
						}
					}
				}
				catch {

				}
			}
		}

	}
}
