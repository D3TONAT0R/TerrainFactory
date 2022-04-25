using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {

	/// <summary>
	/// Base class for adding different file formats that can be imported or exported.
	/// </summary>
	public abstract class HMConPlugin {

		public abstract HMConCommandHandler GetCommandHandler();

		public abstract void RegisterFormats(List<FileFormat> registry);
	}
}
