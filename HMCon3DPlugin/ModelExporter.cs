using HMCon;
using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace HMCon3D {
	class ModelExporter : HMConExportHandler {

		public static int exported3dFiles = 0;

		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("3DM_3DS", "3ds", "3ds", "3DS 3d model", this));
			list.Add(new FileFormat("3DM_FBX", "fbx", "fbx", "FBX 3d model", this));
		}

		public override bool Export(ExportJob job) {
			if(job.format.Identifier.StartsWith("3DM")) {
				return WriteFile3D(job.data, job.FilePath, job.format);
			}
			return false;
		}

		public override bool AreExportSettingsValid(ExportSettings options, FileFormat format, HeightData data) {
			int cellsPerFile = 0;// HMConManager.GetTotalExportCellsPerFile();
			if(options.ContainsFormat("MDL_3DS")) {
				if(cellsPerFile >= 65535) {
					Console.WriteLine("ERROR: Cannot export more than 65535 cells in a single 3ds file! Current amount: " + cellsPerFile);
					Console.WriteLine("       Reduce splitting interval or increase subsampling to allow for exporting 3ds Files");
					return false;
				}
			}
			return true;
		}

		bool WriteFile3D(HeightData source, string filename, FileFormat ff) {
			var meshList = new List<(List<Vector3> verts, List<int> tris, List<Vector2> uvs)>();
			int sizeX = source.GridWidth - 1;
			int sizeY = source.GridHeight - 1;
			int splitX = (int)Math.Ceiling(sizeX / 128f);
			int splitY = (int)Math.Ceiling(sizeY / 128f);
			int y = 0;
			while(y < sizeY) {
				int cellsY = (int)Math.Ceiling(sizeY / (float)splitY);
				int x = 0;
				while(x < sizeX) {
					int cellsX = (int)Math.Ceiling(sizeX / (float)splitX);
					meshList.Add(CreateMeshData(source, x, y, x + cellsX, y + cellsY));
					x += cellsX;
				}
				y += cellsY;
			}
			try {
				IExporter exporter;
				if(exported3dFiles < 50) {
					exporter = new Aspose3DExporter(meshList);
				} else {
					ConsoleOutput.WriteWarning("Aspose3D's export limit was reached! Attempting to export using Assimp, which may throw an error.");
					exporter = new Assimp3DExporter(meshList);
				}
				ExportUtility.WriteFile(exporter, filename, ff);
			} catch(Exception e) {
				ConsoleOutput.WriteError("Failed to create 3D file!");
				ConsoleOutput.WriteLine(e.ToString());
				return false;
			}
			exported3dFiles++;
			return true;
		}

		(List<Vector3> verts, List<int> tris, List<Vector2> uvs) CreateMeshData(HeightData source, int xMin, int yMin, int xMax, int yMax) {
			Vector3[,] points = new Vector3[xMax - xMin + 1, yMax - yMin + 1];
			List<Vector3> verts = new List<Vector3>();
			List<int> tris = new List<int>();
			List<Vector2> uvs = new List<Vector2>();
			for(int i = 0; i <= xMax - xMin; i++) for(int j = 0; j <= yMax - yMin; j++) points[i, j] = Vector3.Zero;
			for(int y = yMin; y <= yMax; y++) {
				for(int x = xMin; x <= xMax; x++) {
					float f = source.GetHeight(x, y);
					if(f != source.nodata_value) {
						Vector3 vec = new Vector3(-x * source.cellSize, source.GetHeight(x, y), y * source.cellSize);
						points[x - xMin, y - yMin] = vec;
						if(!verts.Contains(vec)) {
							verts.Add(vec);
							float uvX = (x - xMin) / (float)(xMax - xMin);
							float uvY = (y - yMin) / (float)(yMax - yMin);
							uvs.Add(new Vector2(uvX, uvY));
						}
					}
				}
			}
			int nodatas = 0;
			for(int y = yMin; y < yMax; y++) {
				for(int x = xMin; x < xMax; x++) {
					Vector3[] pts = GetPointsForFace(points, x - xMin, y - yMin);
					if(pts != null) { //if the list is null, then a nodata-value was found
						int i0 = verts.IndexOf(pts[0]);
						int i1 = verts.IndexOf(pts[1]);
						int i2 = verts.IndexOf(pts[2]);
						int i3 = verts.IndexOf(pts[3]);
						//Lower-Right triangle
						tris.Add(i0);
						tris.Add(i1);
						tris.Add(i3);
						//Upper-Left triangle
						tris.Add(i0);
						tris.Add(i3);
						tris.Add(i2);
					} else {
						nodatas++;
						if(ConsoleOutput.debugLogging) ConsoleOutput.WriteWarning("[#" + nodatas + "] NODATA_VALUE or missing data at point " + x + " " + y);
					}
				}
			}
			return (verts, tris, uvs);
		}

		Vector3[] GetPointsForFace(Vector3[,] points, int x, int y) {
			Vector3[] pts = new Vector3[4];
			int x1 = x;
			int y1 = y;
			int x2 = Math.Min(x1 + 1, points.GetLength(0) - 1);
			int y2 = Math.Min(y1 + 1, points.GetLength(1) - 1);
			pts[0] = points[x1, y1];
			pts[1] = points[x2, y1];
			pts[2] = points[x1, y2];
			pts[3] = points[x2, y2];
			foreach(Vector3 pt in pts) if(pt.Equals(Vector3.Zero)) return null;
			return pts;
		}
	}
}
