using TerrainFactory.Commands;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Import;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace TerrainFactory
{
	public static class ModuleLoader
	{

		public static Dictionary<string, TerrainFactoryModule> loadedModules;
		public static int LoadedModulesCount => loadedModules.Count;

		internal static void LoadModules(string directory)
		{
			loadedModules = new Dictionary<string, TerrainFactoryModule>();
			var infoFiles = Directory.GetFiles(directory, "module.info", SearchOption.AllDirectories);
			AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;
			foreach (var infoFilePath in infoFiles)
			{
				try
				{
					string dllName = File.ReadAllText(infoFilePath).Trim();
					string dllPath = Path.Combine(Path.GetDirectoryName(infoFilePath), dllName + ".dll");

					var assembly = Assembly.LoadFrom(dllPath);
					foreach (var t in assembly.GetTypes())
					{
						if (typeof(TerrainFactoryModule).IsAssignableFrom(t) && !t.IsAbstract)
						{
							var module = (TerrainFactoryModule)Activator.CreateInstance(t);
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

							var c = module.GetCommandDefiningTypes().ToArray();
							if (c.Length > 0)
							{
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

		private static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
		{
			string assemblyName = args.Name.Substring(0, args.Name.IndexOf(","));
			if(assemblyName.EndsWith(".resources"))
			{
				return null;
			}
			else
			{
				string assemblyPath = Path.Combine(Path.GetDirectoryName(args.RequestingAssembly.Location), assemblyName + ".dll");
				return Assembly.LoadFile(assemblyPath);
			}
		}

		public static bool IsModuleLoaded(string id)
		{
			return loadedModules.ContainsKey(id.ToUpper());
		}
	}
}
