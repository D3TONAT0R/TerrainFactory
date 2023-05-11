using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon
{

	/// <summary>
	/// Base class for adding different file formats that can be imported or exported.
	/// </summary>
	public abstract class HMConModule
	{
		public abstract string ModuleID { get; }
		public abstract string ModuleName { get; }
		public abstract string ModuleVersion { get; }

		public abstract HMConCommandHandler GetCommandHandler();

		public abstract void RegisterFormats(List<FileFormat> registry);
	}
}
