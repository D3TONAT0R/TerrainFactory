using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using ASCReader.Export.Exporters;

namespace ASCReader.Export {
	public static class ExportUtility {

		public static bool CreateFilesForSection(ASCData source, string path, string subname, ExportOptions options, int xMin, int yMin, int xMax, int yMax) {
			if(!string.IsNullOrEmpty(subname)) {
				string ext = Path.GetExtension(path);
				string p = path.Substring(0, path.Length - ext.Length);
				path = p + "_" + subname;
			}
			foreach(FileFormat ff in options.outputFormats) {
				string fullpath = path + ff.GetSuffixWithExtension();
				Program.WriteLine("Creating file " + fullpath + " ...");
				if(ff.IsPointFormat()) {
					if(!WriteFilePointData(source, fullpath, options.subsampling, xMin, yMin, xMax, yMax, ff)) return false;
				} else if(ff.Is3DFormat()) {
					if(!WriteFile3D(source, fullpath, options.subsampling, xMin, yMin, xMax, yMax, ff)) return false;
				} else if(ff.IsImage()) {
					if(!WriteFileImage(source, fullpath, options.subsampling, xMin, yMin, xMax, yMax, ff)) return false;
				} else if(ff == FileFormat.MINECRAFT_REGION) {
					if(!WriteFileMCA(source, fullpath, options.subsampling, xMin, yMin, xMax, yMax)) return false;
				}
				Program.WriteSuccess(ff.GetFiletypeString() + " file created successfully!");
			}
			return true;
		}

		public static bool WriteFilePointData(ASCData source, string filename, int subsampling, int xMin, int yMin, int xMax, int yMax, FileFormat ff) {
			try {
				if(ff == FileFormat.ASC || ff == FileFormat.PTS_XYZ) {
					IExporter exporter;
					exporter = new PointDataExporter(source, subsampling, xMin, yMin, xMax, yMax);
					WriteFile(exporter, filename, ff);
					return true;
				} else {
					Program.WriteError("Don't know how to export " + ff.ToString());
					return false;
				}
			} catch(Exception e) {
				Program.WriteError("Failed to create Point data file!");
				Program.WriteLine(e.ToString());
				return false;
			}
		}

		public static bool WriteFile3D(ASCData source, string filename, int subsampling, int xMin, int yMin, int xMax, int yMax, FileFormat ff) {
			var meshList = new List<(List<Vector3> verts, List<int> tris, List<Vector2> uvs)>();
			//Increase boundaries for lossless tiling
			if(xMax < source.ncols) xMax++;
			if(yMax < source.nrows) yMax++;
			int sizeX = xMax - xMin;
			int sizeY = yMax - yMin;
			int splitX = (int)Math.Ceiling(sizeX / 128f / subsampling);
			int splitY = (int)Math.Ceiling(sizeY / 128f / subsampling);
			int y = 0;
			while(y < sizeY) {
				int cellsY = (int)Math.Ceiling(sizeY / (float)splitY);
				int x = 0;
				while(x < sizeX) {
					int cellsX = (int)Math.Ceiling(sizeX / (float)splitX);
					meshList.Add(CreateMeshData(source, subsampling, xMin + x, yMin + y, xMin + x + cellsX, yMin + y + cellsY));
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
				WriteFile(exporter, filename, ff);
			} catch(Exception e) {
				Program.WriteError("Failed to create 3D file!");
				Program.WriteLine(e.ToString());
				return false;
			}
			Program.exported3dFiles++;
			return true;
		}

		public static bool WriteFileImage(ASCData source, string filename, int subsampling, int xMin, int yMin, int xMax, int yMax, FileFormat ff) {
			if(subsampling < 1) subsampling = 1;
			float[,] grid = new float[(xMax - xMin) / subsampling, (yMax - yMin) / subsampling];
			for(int x = 0; x < grid.GetLength(0); x++) {
				for(int y = 0; y < grid.GetLength(1); y++) {
					grid[x, y] = source.data[xMin + x * subsampling, yMin + y * subsampling];
				}
			}
			try {
				IExporter exporter = new ImageExporter(grid, source.cellsize, ff.GetImageType(), source.lowestValue, source.highestValue);
				WriteFile(exporter, filename, ff);
				return true;
			} catch(Exception e) {
				Program.WriteError("Failed to create Image file!");
				Program.WriteLine(e.ToString());
				return false;
			}
		}

		public static bool WriteFileMCA(ASCData source, string filename, int subsampling, int xMin, int yMin, int xMax, int yMax) {
			if(subsampling < 1) subsampling = 1;
			float[,] grid = new float[(xMax - xMin) / subsampling, (yMax - yMin) / subsampling];
			for(int x = 0; x < grid.GetLength(0); x++) {
				for(int y = 0; y < grid.GetLength(1); y++) {
					grid[x, y] = source.data[xMin + x * subsampling, yMin + y * subsampling];
				}
			}
			try {
				IExporter exporter = new MinecraftRegionExporter(grid, true);
				WriteFile(exporter, filename, FileFormat.MINECRAFT_REGION);
				return true;
			}
			catch(Exception e) {
				Program.WriteError("Failed to create Image file!");
				Program.WriteLine(e.ToString());
				return false;
			}
		}

		public static void WriteFile(IExporter ie, string path, FileFormat ff) {
			FileStream stream = new FileStream(path, FileMode.CreateNew);
			ie.WriteFile(stream, ff);
			stream.Close();
		}

		private static (List<Vector3> verts, List<int> tris, List<Vector2> uvs) CreateMeshData(ASCData source, int subsampling, int xMin, int yMin, int xMax, int yMax) {
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

		private static Vector3[] GetPointsForFace(Vector3[,] points, int x, int y, int subsample) {
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
