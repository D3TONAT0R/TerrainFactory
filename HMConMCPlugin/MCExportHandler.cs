using HMCon;
using HMCon.Export;
using HMCon.Util;
using HMConMCPlugin;
using System.Collections.Generic;

namespace HMConMC {
	public class MCExportHandler : HMConExportHandler {
		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("MCR", "mca", "mca", "Minecraft Region File (1.16)", this));
			list.Add(new FileFormat("MCR-RAW", "mca-raw", "mca", "Minecraft Region File (stone heightmap only) (1.16)", this));
			if(HMCon.PluginLoader.IsPluginLoaded("ImagePlugin")) {
				list.Add(new FileFormat("IMG_MCR", "mca-map", "png", "Region overview map (MC)", this));
			}
			list.Add(new FileFormat("MCW", "mcw", "", "Minecraft World Save (1.16)", this));
		}

		public override bool Export(ExportJob job) {
			if(job.format.IsFormat("MCR", "MCR-RAW")) {
				return WriteFileMCA(job, !job.format.IsFormat("MCR-RAW"), job.settings.GetCustomSetting("mcaUseSplatmaps", false));
			} else if(job.format.IsFormat("IMG_MCR")) {
				ExportUtility.WriteFile(new OverviewmapExporter(job.data.filename, true), job.FilePath, job.format);
			} else if(job.format.IsFormat("MCW")) {
				return WriteWorldSave(job, true, job.settings.GetCustomSetting("mcaUseSplatmaps", false));
			}
			return false;
		}

		public override bool AreExportSettingsValid(ExportSettings options, FileFormat format, HeightData data) {
			if(options.ContainsFormat("MCR", "MCR-RAW")) {
				bool sourceIs512 = (data.GridHeight == 512 && data.GridWidth == 512);
				if(options.fileSplitDims != 512 && !sourceIs512) {
					ConsoleOutput.WriteError("File splitting dimensions must be 512 when exporting to minecraft regions!");
					return false;
				}
			}
			return true;
		}

		public override void EditFileName(ExportJob job, FileNameBuilder nameBuilder) {
			if(job.settings.ContainsFormat("MCR")) {
				nameBuilder.gridNum = (job.exportNumX + job.settings.GetCustomSetting("mcaOffsetX", 0), job.exportNumZ + job.settings.GetCustomSetting("mcaOffsetZ", 0));
				nameBuilder.gridNumFormat = "r.{0}.{1}";
			}
			if(job.format.Identifier == "MCW") {
				nameBuilder.gridNumFormat = "";
			}
			if(job.format.IsFormat("IMG_MCR")) nameBuilder.suffix = "overview";
		}

		public static bool WriteFileMCA(ExportJob job, bool decorate, bool useSplatmaps) {
			IExporter exporter = new MCWorldExporter(job, decorate, useSplatmaps);
			ExportUtility.WriteFile(exporter, job.FilePath, ExportUtility.GetFormatFromIdenfifier("MCR"));
			return true;
		}

		public static bool WriteWorldSave(ExportJob job, bool decorate, bool useSplatmaps) {
			IExporter exporter = new MCWorldExporter(job, decorate, useSplatmaps);
			ExportUtility.WriteFile(exporter, job.FilePath, ExportUtility.GetFormatFromIdenfifier("MCW"));
			return true;
		}
	}
}