using TerrainFactory.Commands;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Import;
using System;
using System.Collections.Generic;
using static TerrainFactory.ConsoleOutput;
using TerrainFactory.Modification;

namespace TerrainFactory {
	public static class TerrainFactoryManager {

		public static bool ModuleLoadingEnabled { get; set; } = true;
		public static List<string> ModuleLocations { get; private set; } = new List<string>();
		public static bool IsInitialized { get; private set; } = false;

		static TerrainFactoryManager()
		{
			ModuleLocations.Add(AppContext.BaseDirectory);
		}

		public static void Initialize() {
			if(IsInitialized)
			{
				throw new InvalidOperationException("Already initialized.");
			}
			FileFormatManager.RegisterStandardFormats();
			if(ModuleLoadingEnabled)
			{
				foreach(var moduleLoc in ModuleLocations)
				{
					ModuleLoader.LoadModules(moduleLoc);
				}
			} else {
				WriteLine("Module loading has been disabled.");
			}
			Modifier.InitializeList();
			CommandHandler.Initialize();
			IsInitialized = true;
		}

		public static void InitializeIfNeeded()
		{
			if(!IsInitialized)
			{
				Initialize();
			}
		}
	}
}
