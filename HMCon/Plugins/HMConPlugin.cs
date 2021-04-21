using HMCon.Export;
using HMCon.Import;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {
	public abstract class HMConPlugin {

		public abstract HMConExportHandler GetExportHandler();

		public abstract HMConImportHandler GetImportHandler();

		public abstract HMConCommandHandler GetCommandHandler();
	}
}
