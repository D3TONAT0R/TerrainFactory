using HMCon.Commands;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using System;
using System.Collections.Generic;
using static HMCon.ConsoleOutput;

namespace HMCon {
	public static class HMConManager {

		public static bool ModuleLoadingEnabled { get; set; } = true;
		public static List<string> ModuleLocations { get; private set; } = new List<string>();
		public static bool IsInitialized { get; private set; } = false;

		static HMConManager()
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
