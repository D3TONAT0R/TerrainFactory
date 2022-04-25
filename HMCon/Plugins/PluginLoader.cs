using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace HMCon
{
	public static class PluginLoader
	{

		public static Dictionary<string, HMConPlugin> loadedPlugins;
		public static int NumPluginsLoaded => loadedPlugins.Count;

		internal static void LoadPlugins(string pluginPath)
		{
			loadedPlugins = new Dictionary<string, HMConPlugin>();
			var dllFiles = Directory.GetFiles(pluginPath, "*.dll");
			foreach (var path in dllFiles)
			{
				try
				{
					var assembly = Assembly.LoadFrom(path);
					foreach (var t in assembly.GetTypes())
					{
						if (t.BaseType != null && t.BaseType.Name == typeof(HMConPlugin).Name && !t.IsAbstract)
						{
							var plugin = (HMConPlugin)Activator.CreateInstance(t);
							string info = "";

							bool hasImporter = false;
							bool hasExporter = false;
							List<FileFormat> formats = new List<FileFormat>();
							plugin.RegisterFormats(formats);
							foreach(var f in formats)
							{
								FileFormatManager.RegisterFormat(f);
								hasImporter |= f.HasImporter;
								hasExporter |= f.HasExporter;
							}
							if (hasImporter && hasExporter) info = "I+E";
							else if (hasImporter) info = "I";
							else if (hasExporter) info = "E";

							var c = plugin.GetCommandHandler();
							if (c != null)
							{
								CommandHandler.commandHandlers.Add(c);
								info += info.Length > 0 ? "+C" : "C";
							}

							var attribute = t.GetCustomAttribute<PluginInfoAttribute>();
							string pluginID;
							string pluginAttr;
							if (attribute != null)
							{
								pluginAttr = attribute.Name;
								pluginID = attribute.ID.ToUpper();
							}
							else
							{
								ConsoleOutput.WriteWarning($"Plugin with class '{t.FullName}' does not specify a Plugin name!");
								pluginID = "[" + t.Name + "]";
								pluginAttr = t.Name;
							}
							if (loadedPlugins.ContainsKey(pluginID))
							{
								continue;
							}
							loadedPlugins.Add(pluginID, plugin);
							ConsoleOutput.WriteLine($"Loaded Plugin '{pluginAttr}' [{info}]");
						}
					}
					//ConsoleOutput.WriteWarning("Not a plugin dll: " + path);
				}
				catch
				{
					//ConsoleOutput.WriteWarning("Failed to load dll: " + path);
				}
			}
		}

		public static bool IsPluginLoaded(string id)
		{
			return loadedPlugins.ContainsKey(id.ToUpper());
		}
	}
}
