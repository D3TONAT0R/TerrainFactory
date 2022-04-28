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
	public static class ModuleLoader
	{

		public static Dictionary<string, HMConModule> loadedModules;
		public static int LoadedModulesCount => loadedModules.Count;

		internal static void LoadModules(string moduleDLLPath)
		{
			loadedModules = new Dictionary<string, HMConModule>();
			var dllFiles = Directory.GetFiles(moduleDLLPath, "*.dll");
			foreach (var path in dllFiles)
			{
				try
				{
					var assembly = Assembly.LoadFrom(path);
					foreach (var t in assembly.GetTypes())
					{
						if (t.BaseType != null && t.BaseType.Name == typeof(HMConModule).Name && !t.IsAbstract)
						{
							var module = (HMConModule)Activator.CreateInstance(t);
							string info = "";

							bool hasImporter = false;
							bool hasExporter = false;
							List<FileFormat> formats = new List<FileFormat>();
							module.RegisterFormats(formats);
							foreach(var f in formats)
							{
								FileFormatManager.RegisterFormat(f);
								hasImporter |= f.HasImporter;
								hasExporter |= f.HasExporter;
							}
							if (hasImporter && hasExporter) info = "I+E";
							else if (hasImporter) info = "I";
							else if (hasExporter) info = "E";

							var c = module.GetCommandHandler();
							if (c != null)
							{
								CommandHandler.commandHandlers.Add(c);
								info += info.Length > 0 ? "+C" : "C";
							}

							var attribute = t.GetCustomAttribute<ModuleInfoAttribute>();
							string moduleID;
							string moduleAttr;
							if (attribute != null)
							{
								moduleAttr = attribute.Name;
								moduleID = attribute.ID.ToUpper();
							}
							else
							{
								ConsoleOutput.WriteWarning($"Module with class '{t.FullName}' does not specify a Module name!");
								moduleID = "[" + t.Name + "]";
								moduleAttr = t.Name;
							}
							if (loadedModules.ContainsKey(moduleID))
							{
								continue;
							}
							loadedModules.Add(moduleID, module);
							ConsoleOutput.WriteLine($"Loaded Module '{moduleAttr}' [{info}]");
						}
					}
					//ConsoleOutput.WriteWarning("Not a module dll: " + path);
				}
				catch
				{
					//ConsoleOutput.WriteWarning("Failed to load dll: " + path);
				}
			}
		}

		public static bool IsModuleLoaded(string id)
		{
			return loadedModules.ContainsKey(id.ToUpper());
		}
	}
}
