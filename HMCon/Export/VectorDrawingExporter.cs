using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Objects;
using netDxf.Tables;

namespace HMCon.Export.Exporters
{
	class VectorDrawingExporter : IExporter
	{
		ExportJob job;

		bool visualPoints = true;

		public VectorDrawingExporter(ExportJob job)
		{
			this.job = job;
		}

		public bool NeedsFileStream(FileFormat format)
		{
			return false;
		}

		public void WriteFile(FileStream stream, string path, FileFormat filetype)
		{
			DxfDocument doc = CreateDrawing();
			doc.Save(path);
		}

		DxfDocument CreateDrawing()
		{
			var d = job.data;
			DxfDocument doc = new DxfDocument();
			
			Layer ptLayer = new Layer("HEIGHT_PTS");
			Layer textLayer = new Layer("HEIGHT_TEXT");
			Layer originLayer = new Layer("HEIGHT_ORIGIN");

			doc.Layers.Add(ptLayer);
			doc.Layers.Add(textLayer);
			doc.Layers.Add(originLayer);

			float lineOffset = d.cellSize * 0.1f;
			
			for(int y = 0; y < d.GridHeight; y++)
			{
				for (int x = 0; x < d.GridWidth; x++)
				{
					var h = d.GetHeight(x, y);
					if (h == d.nodata_value) continue;

					Vector2 pos = new Vector2(d.lowerCornerPos.X + x * d.cellSize, d.lowerCornerPos.Y + y * d.cellSize);

					var cellGroup = new Group($"X{x}Y{y}");

					var text = new Text(h.ToString("F3"), pos, 0.1f * d.cellSize)
					{
						Layer = textLayer,
						Alignment = TextAlignment.TopLeft
					};
					cellGroup.Entities.Add(text);

					if (visualPoints)
					{
						var lnx = new Line(pos - new Vector2(lineOffset, 0), pos + new Vector2(lineOffset, 0)) { Layer = ptLayer };
						var lny = new Line(pos - new Vector2(0, lineOffset), pos + new Vector2(0, lineOffset)) { Layer = ptLayer };
						cellGroup.Entities.Add(lnx);
						cellGroup.Entities.Add(lny);
					}
					else
					{
						cellGroup.Entities.Add(new Point(pos) {
							Layer = ptLayer
						});
					}
					doc.Groups.Add(cellGroup);
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
	}
}
