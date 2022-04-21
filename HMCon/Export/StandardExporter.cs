using HMCon.Export.Exporters;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using static HMCon.ConsoleOutput;

namespace HMCon.Export {

	class StandardExporter : HMConExportHandler {

		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("HMC_CMDS", "cmds", "cmds", "HMCon command stack", this));
			list.Add(new FileFormat("GEO_META", "meta", "txt", "Geo metadata", this));
			list.Add(new FileFormat("ASC", "asc", "asc", "ESRI ASCII grid (same as input)", this));
			list.Add(new FileFormat("PTS_XYZ", "xyz", "xyz", "ASCII-XYZ points", this));
			list.Add(new FileFormat("R16", "r16", "r16", "16 Bit raw data", this));
			list.Add(new FileFormat("R32", "r32", "r32", "32 Bit raw data", this));
			list.Add(new FileFormat("DXF", "dxf", "dxf", "AutoCAD DXF labeled point grid", this));
			list.Add(new FileFormat("DXF_3D", "dxf", "dxf3d", "AutoCAD DXF 3D point grid", this)); //TODO: Add support for 3D DXF
		}

		public override bool Export(ExportJob job) {
			if(job.format.IsFormat("HMC_CMD", "GEO_META")) {
				return WriteFileMeta(job);
			} else if(job.format.IsPointFormat()) {
				return WriteFilePointData(job);
			} else if(job.format.IsFormat("R16", "R32")) {
				return WriteFileRaw(job);
			} else if (job.format.IsFormat("DXF")) {
				return WriteFileDxf(job);
			} else {
				return false;
			}
		}

		public static bool WriteFileMeta(ExportJob job)
		{
			try
			{
				ExportUtility.WriteFile(new MetadataExporter(job), job.FilePath, job.format);
				return true;
			}
			catch (Exception e)
			{
				WriteError("Failed to create Meta data file!");
				WriteLine(e.ToString());
				return false;
			}
		}

		public static bool WriteFilePointData(ExportJob job) {
			try {
				if(job.format.IsFormat("ASC", "PTS_XYZ")) {
					IExporter exporter;
					exporter = new PointDataExporter(job.data);
					ExportUtility.WriteFile(exporter, job.FilePath, job.format);
					return true;
				} else {
					WriteError("Don't know how to export " + job.format.ToString());
					return false;
				}
			} catch(Exception e) {
				WriteError("Failed to create Point data file!");
				WriteLine(e.ToString());
				return false;
			}
		}

		public static bool WriteFileRaw(ExportJob job)
		{
			try
			{
				IExporter exporter;
				exporter = new RawDataExporter(job.data);
				ExportUtility.WriteFile(exporter, job.FilePath, job.format);
				return true;
			}
			catch (Exception e)
			{
				WriteError("Failed to create raw data file!");
				WriteLine(e.ToString());
				return false;
			}
		}

		public static bool WriteFileDxf(ExportJob job)
		{
			try
			{
				IExporter exporter;
				exporter = new VectorDrawingExporter(job);
				ExportUtility.WriteFile(exporter, job.FilePath, job.format);
				return true;
			}
			catch (Exception e)
			{
				WriteError("Failed to create vector drawing file!");
				WriteLine(e.ToString());
				return false;
			}
		}

		public override void EditFileName(ExportJob exportJob, FileNameBuilder nameBuilder)
		{
			if(exportJob.format.IsFormat("GEO_META"))
			{
				nameBuilder.suffix = "geodata";
			}
		}

		public override bool AreExportSettingsValid(ExportSettings options, FileFormat format, HeightData data) {
			if(options.outputFormats.Count == 0) {
				WriteWarning("No export format is defined! choose at least one format for export!");
				return false;
			}
			return true;
		}
	}
}
