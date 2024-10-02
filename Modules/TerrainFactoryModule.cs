using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Import;
using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory
{

	/// <summary>
	/// Main class defining a module and its supported file formats and commands.
	/// </summary>
	public abstract class TerrainFactoryModule
	{
		public abstract string ModuleID { get; }
		public abstract string ModuleName { get; }
		public abstract string ModuleVersion { get; }

		public List<FileFormat> SupportedFormats { get; private set; } = new List<FileFormat>();
		public List<Type> CommandDefiningTypes { get; private set; } = new List<Type>();


		public virtual void Initialize()
		{

		}
	}
}
