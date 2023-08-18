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
	/// Base class for adding different file formats that can be imported or exported.
	/// </summary>
	public abstract class TerrainFactoryModule
	{
		public abstract string ModuleID { get; }
		public abstract string ModuleName { get; }
		public abstract string ModuleVersion { get; }

		public abstract void RegisterFormats(List<FileFormat> registry);

		public virtual IEnumerable<Type> GetCommandDefiningTypes()
		{
			yield break;
		}
	}
}
