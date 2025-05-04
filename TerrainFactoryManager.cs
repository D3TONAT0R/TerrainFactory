using System;
using TerrainFactory.Commands;
using TerrainFactory.Formats;
using System.Collections.Generic;
using TerrainFactory.Modification;

namespace TerrainFactory {
	public static class TerrainFactoryManager {

		public static List<string> ModuleDirectories { get; private set; } = new List<string>();
		public static bool IsInitialized { get; private set; } = false;

		static TerrainFactoryManager()
		{
			ModuleDirectories.Add(AppContext.BaseDirectory);
		}

		public static void Initialize() {
			if(IsInitialized)
			{
				throw new InvalidOperationException("Already initialized.");
			}
			FileFormatRegistry.RegisterStandardFormats();
			foreach(var moduleLoc in ModuleDirectories)
			{
				ModuleLoader.LoadModules(moduleLoc);
			}
			Modifier.InitializeList();
			CommandHandler.Initialize();
			IsInitialized = true;
		}

		public static void InitializeIfRequired()
		{
			if(!IsInitialized)
			{
				Initialize();
			}
		}
	}
}
