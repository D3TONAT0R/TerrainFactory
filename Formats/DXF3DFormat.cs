using HMCon.Export;
using HMCon.Util;
using netDxf;
using netDxf.Entities;
using netDxf.Objects;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Formats
{
	public class DXF3DFormat : DXFFormat
	{
		public override string Identifier => "DXF_3D";
		public override string ReadableName => "AutoCAD 3D DXF Drawing";
		public override string CommandKey => "dxf3d";
		public override string Description => ReadableName;
		public override string Extension => "dxf";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		public override void ModifyFileName(ExportTask task, FileNameBuilder nameBuilder)
		{
			nameBuilder.suffix = "3d";
		}

		protected override bool ExportFile(string path, ExportTask task)
		{
			var doc = CreateDrawing(task);
			doc.Save(path);
			return true;
		}

		protected override void AddPoint(DxfDocument doc, int ix, int iy, float z, HeightData d)
		{
			//TO DO
			base.AddPoint(doc, ix, iy, z, d);
		}
	}
}
