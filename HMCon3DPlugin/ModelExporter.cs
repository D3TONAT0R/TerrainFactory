using HMCon;
using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ASCReader3DPlugin {
	class ModelExporter : ASCReaderExportHandler {
		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("3DM_3DS", "3ds", "3ds", "3DS 3d model", this));
			list.Add(new FileFormat("3DM_FBX", "fbx", "fbx", "FBX 3d model", this));
		}

		public override bool Export(ASCData data, FileFormat ff, string fullPath) {
			if(ff.Identifier.StartsWith("3DM")) {
				return WriteFile3D(data, fullPath, CurrentExportJobInfo.exportSettings.subsampling, CurrentExportJobInfo.bounds ?? data.GetBounds(), ff);
			}
			return false;
		}

		public override bool ValidateExportOptions(ExportSettings options, FileFormat format, ASCData data) {
			int cellsPerFile = Program.GetTotalExportCellsPerFile();
			if(options.ContainsFormat("MDL_3DS")) {
				if(cellsPerFile >= 65535) {
					Console.WriteLine("ERROR: Cannot export more than 65535 cells in a single 3ds file! Current amount: "+cellsPerFile);
					Console.WriteLine("       Reduce splitting interval or increase subsampling to allow for exporting 3ds Files");
					return false;
				}
			}
			return true;
		}

		bool WriteFile3D(ASCData source, string filename, int subsampling, Bounds bounds, FileFormat ff) {
			var meshList = new List<(List<Vector3> verts, List<int> tris, List<Vector2> uvs)>();
			//Increase boundaries for lossless tiling
			if(bounds.xMax < source.ncols) bounds.xMax++;
			if(bounds.yMax < source.nrows) bounds.yMax++;
			int sizeX = bounds.NumCols - 1;
			int sizeY = bounds.NumRows - 1;
			int splitX = (int)Math.Ceiling(sizeX / 128f / subsampling);
			int splitY = (int)Math.Ceiling(sizeY / 128f / subsampling);
			int y = 0;
			while(y < sizeY) {
				int cellsY = (int)Math.Ceiling(sizeY / (float)splitY);
				int x = 0;
				while(x < sizeX) {
					int cellsX = (int)Math.Ceiling(sizeX / (float)splitX);
					meshList.Add(CreateMeshData(source, subsampling, bounds.xMin + x, bounds.yMin + y, bounds.xMin + x + cellsX, bounds.yMin + y + cellsY));
					x += cellsX;
				}
				y += cellsY;
			}
			try {
				IExporter exporter;
				if(Program.exported3dFiles < 50) {
					exporter = new Aspose3DExporter(meshList);
				} else {
					Program.WriteWarning("Aspose3D's export limit was reached! Attempting to export using Assimp, which may throw an error.");
					exporter = new Assimp3DExporter(meshList);
				}
				ExportUtility.WriteFile(exporter, filename, ff);
			} catch(Exception e) {
				Program.WriteError("Failed to create 3D file!");
				Program.WriteLine(e.ToString());
				return false;
			}
			Program.exported3dFiles++;
			return true;
		}

		(List<Vector3> verts, List<int> tris, List<Vector2> uvs) CreateMeshData(ASCData source, int subsampling, int xMin, int yMin, int xMax, int yMax) {
			Vector3[,] points = new Vector3[xMax - xMin + 1, yMax - yMin + 1];
			List<Vector3> verts = new List<Vector3>();
			List<int> tris = new List<int>();
			List<Vector2> uvs = new List<Vector2>();
			for(int i = 0; i <= xMax - xMin; i++) for(int j = 0; j <= yMax - yMin; j++) points[i, j] = Vector3.Zero;
			for(int y = yMin; y <= yMax; y++) {
				for(int x = xMin; x <= xMax; x++) {
					if(x % subsampling == 0 && y % subsampling == 0) {
						float f = source.GetData(x, y);
						if(f != source.nodata_value) {
							Vector3 vec = new Vector3(-x * source.cellsize, source.data[x, y], y * source.cellsize);
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
			}
			int nodatas = 0;
			for(int y = yMin; y < yMax; y++) {
				for(int x = xMin; x < xMax; x++) {
					if(x % subsampling == 0 && y % subsampling == 0) {
						Vector3[] pts = GetPointsForFace(points, x - xMin, y - yMin, subsampling);
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
							if(Program.debugLogging) Program.WriteWarning("[#" + nodatas + "] NODATA_VALUE or missing data at point " + x + " " + y);
						}
					}
				}
			}
			return (verts, tris, uvs);
		}

		Vector3[] GetPointsForFace(Vector3[,] points, int x, int y, int subsample) {
			Vector3[] pts = new Vector3[4];
			int i = subsample > 1 ? subsample : 1;
			int x1 = x;
			int y1 = y;
			int x2 = Math.Min(x1 + i, points.GetLength(0) - 1);
			int y2 = Math.Min(y1 + i, points.GetLength(1) - 1);
			pts[0] = points[x1, y1];
			pts[1] = points[x2, y1];
			pts[2] = points[x1, y2];
			pts[3] = points[x2, y2];
			foreach(Vector3 pt in pts) if(pt.Equals(Vector3.Zero)) return null;
			return pts;
		}
	}
}
