using HMCon;
using HMCon.Export;
using HMCon.Util;
using System.Collections.Generic;

namespace HMConMC {
	public class MCExportHandler {

		/*
		public override bool Export(ExportJob job) {
			if(job.format.IsFormat("MCR", "MCR-RAW")) {
				return WriteFileMCA(job, !job.format.IsFormat("MCR-RAW"), job.settings.GetCustomSetting("mcpostprocess", false));
			} else if(job.format.IsFormat("IMG_MCR")) {
				ExportUtility.WriteFile(new OverviewmapExporter(job.data.filename, true), job.FilePath, job.format);
			} else if(job.format.IsFormat("MCW")) {
				return WriteWorldSave(job, true, job.settings.GetCustomSetting("mcpostprocess", false));
			}
			return false;
		}
		*/

		/*
		public void EditFileName(ExportJob job, FileNameBuilder nameBuilder) {
			if(job.settings.ContainsFormat("MCR")) {
				nameBuilder.gridNum = (job.exportNumX + job.settings.GetCustomSetting("mcaOffsetX", 0), job.exportNumZ + job.settings.GetCustomSetting("mcaOffsetZ", 0));
				nameBuilder.gridNumFormat = "r.{0}.{1}";
			}
			if(job.format is MCWorldFormat) {
				nameBuilder.gridNumFormat = "";
			}
			//if(job.format.IsFormat("IMG_MCR")) nameBuilder.suffix = "overview";
		}
		*/
	}
}