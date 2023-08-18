using TerrainFactory.Export;
using netDxf;
using netDxf.Entities;
using netDxf.Objects;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Formats
{
	public class DXFFormat : FileFormat
	{
		public override string Identifier => "DXF";
		public override string ReadableName => "AutoCAD DXF Drawing";
		public override string CommandKey => "dxf";
		public override string Description => ReadableName;
		public override string Extension => "dxf";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected static bool visualPoints;
		protected static float visualPointRadius;

		protected static Layer ptLayer;
		protected static Layer textLayer;

		protected override bool ExportFile(string path, ExportTask task)
		{
			var doc = CreateDrawing(task);
			doc.Save(path);
			return true;
		}

		protected virtual DxfDocument CreateDrawing(ExportTask task)
		{
			var d = task.data;
			DxfDocument doc = new DxfDocument();

			visualPoints = true;
			visualPointRadius = d.cellSize * 0.1f;

			ptLayer = new Layer("HEIGHT_PTS");
			textLayer = new Layer("HEIGHT_TEXT");
			Layer originLayer = new Layer("HEIGHT_ORIGIN");

			doc.Layers.Add(ptLayer);
			doc.Layers.Add(textLayer);
			doc.Layers.Add(originLayer);

			for (int y = 0; y < d.GridLengthY; y++)
			{
				for (int x = 0; x < d.GridLengthX; x++)
				{
					var h = d.GetHeight(x, y);
					if (h == d.nodataValue) continue;
					AddPoint(doc, x, y, h, d);
				}
			}

			var info = new Text($"[ {d.lowerCornerPos.X} , {d.lowerCornerPos.Y} ]", new Vector2(d.lowerCornerPos.X, d.lowerCornerPos.Y), 0.2f * d.cellSize)
			{
				Alignment = TextAlignment.TopRight,
				Layer = originLayer
			};
			doc.AddEntity(info);

			return doc;
		}

		protected virtual void AddPoint(DxfDocument doc, int ix, int iy, float z, HeightData d)
		{
			Vector2 pos = new Vector2(d.lowerCornerPos.X + ix * d.cellSize, d.lowerCornerPos.Y + iy * d.cellSize);

			var cellGroup = new Group($"X{ix}Y{iy}");

			var text = new Text(z.ToString("F3"), pos, 0.1f * d.cellSize)
			{
				Layer = textLayer,
				Alignment = TextAlignment.TopLeft
			};
			cellGroup.Entities.Add(text);

			if (visualPoints)
			{
				var lnx = new Line(pos - new Vector2(visualPointRadius, 0), pos + new Vector2(visualPointRadius, 0)) { Layer = ptLayer };
				var lny = new Line(pos - new Vector2(0, visualPointRadius), pos + new Vector2(0, visualPointRadius)) { Layer = ptLayer };
				cellGroup.Entities.Add(lnx);
				cellGroup.Entities.Add(lny);
			}
			else
			{
				cellGroup.Entities.Add(new Point(pos)
				{
					Layer = ptLayer
				});
			}
			doc.Groups.Add(cellGroup);
		}
	}
}
