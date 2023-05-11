using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
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
			var infoFiles = Directory.GetFiles(moduleDLLPath, "module.info", SearchOption.AllDirectories);
			foreach (var infoFilePath in infoFiles)
			{
				try
				{
					string dllName = File.ReadAllText(infoFilePath).Trim();
					string dllPath = Path.Combine(Path.GetDirectoryName(infoFilePath), dllName + ".dll");
					var assembly = Assembly.LoadFrom(dllPath);
					foreach (var t in assembly.GetTypes())
					{
						if (typeof(HMConModule).IsAssignableFrom(t) && !t.IsAbstract)
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
							if(hasImporter) info += " [I]";
							if(hasExporter) info += " [E]";

							var c = module.GetCommandHandler();
							if (c != null)
							{
								CommandHandler.commandHandlers.Add(c);
								info += " [C]";
							}

							if (loadedModules.ContainsKey(module.ModuleID))
							{
								throw new InvalidOperationException($"Duplicate Module with ID '{module.ModuleID}' detected.");
							}
							loadedModules.Add(module.ModuleID, module);
							ConsoleOutput.WriteLine($"Loaded Module '{module.ModuleName}' ({module.ModuleVersion}) {info}");
						}
					}
					//ConsoleOutput.WriteWarning("Not a module dll: " + path);
				}
				catch(Exception e)
				{
					ConsoleOutput.WriteWarning($"Failed to load module at '{Path.GetDirectoryName(infoFilePath)}'\n{e.Message}");
				}
			}
		}

		public static bool IsModuleLoaded(string id)
		{
			return loadedModules.ContainsKey(id.ToUpper());
		}
	}
}
