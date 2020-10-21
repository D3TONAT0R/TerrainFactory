using ASCReader.Export.Exporters;
using ASCReader.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using static ASCReader.Program;

namespace ASCReader.Export {

	class ExportHandler : ASCReaderExportHandler {

		public string[] supportedFormats = new string[] {
			"ASC", "PTS_XYZ", "MDL_3DS", "MDS_FBX", "PNG_IMG-HEIGHT", "PNG_IMG-NORMAL", "PNG_IMG-HILLSHADE"
		};

		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("ASC", "asc", "asc", "ASCII-Grid (same as input)", this));
			list.Add(new FileFormat("PTS_XYZ", "xyz", "xyz", "ASCII-XYZ points", this));
			list.Add(new FileFormat("MDL_3DS", "3ds", "3ds", "3DS 3d model", this));
			list.Add(new FileFormat("MDL_FBX", "fbx", "fbx", "FBX 3d model", this));
			list.Add(new FileFormat("IMG_PNG-HEIGHT", "png-hm", "png", "Heightmap", this));
			list.Add(new FileFormat("IMG_PNG-NORMAL", "png-nm", "png", "Normalmap", this));
			list.Add(new FileFormat("IMG_PNG-HILLSHADE", "png-hs", "png", "Hillshade", this));
		}

		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("subsample", "N", "Only export every N-th cell", this));
			list.Add(new ConsoleCommand("split", "N", "Split files every NxN cells (minimum 32)", this));
			list.Add(new ConsoleCommand("selection", "x1 y1 x2 y2", "Export only the selected data range(use 'preview' to see the data grid)", this));
			list.Add(new ConsoleCommand("overridecellsize", "N", "Override size per cell", this));
			list.Add(new ConsoleCommand("setrange", "N N", "Change the height data range (min - max)", this));
		}

		public override void HandleCommand(string cmd, string[] args, ExportOptions exportOptions, ASCData data) {
			if(cmd == "subsample") {
				if(args.Length > 0) {
					if(int.TryParse(args[0], out int i)) {
						exportOptions.subsampling = i;
						WriteLine("Subsampling set to: " + i);
					} else {
						WriteWarning("Can't parse to int: " + args[0]);
					}
				} else {
					WriteWarning("An integer is required!");
				}
			} else if(cmd == "split") {
				if(args.Length > 0) {
					if(int.TryParse(args[0], out int i)) {
						exportOptions.fileSplitDims = i;
						WriteLine("File splitting set to: " + i + "x" + i);
					} else {
						WriteWarning("Can't parse to int: " + args[0]);
					}
				} else {
					WriteWarning("An integer is required!");
				}
			} else if(cmd == "overridecellsize") {
				if(args.Length > 0) {
					if(float.TryParse(args[0], out float f)) {
						WriteLine("Cellsize changed from {0} to {1}", data.cellsize, f);
						data.cellsize = f;
					} else {
						WriteWarning("Can't parse to float: " + args[0]);
					}
				} else {
					WriteWarning("A number is required!");
				}
			} else if(cmd == "selection") {
				if(args.Length > 3) {
					int[] nums = new int[4];
					bool b = true;
					for(int i = 0; i < 4; i++) {
						b &= int.TryParse(args[i], out nums[i]);
					}
					if(b) {
						if(exportOptions.SetExportRange(data, nums[0], nums[1], nums[2], nums[3])) {
							WriteLine("Selection set (" + exportOptions.ExportRangeCellCount + " cells total)");
						} else {
							WriteWarning("The specified input is invalid!");
						}
					} else {
						WriteWarning("Failed to parse to int");
					}
				} else {
					if(args.Length == 0) {
						WriteLine("Selection reset");
					} else {
						WriteWarning("Four integers are required!");
					}
				}
			} else if(cmd == "setrange") {
				if(args.Length > 1) {
					bool b = true;
					b &= float.TryParse(args[0], out float min) & float.TryParse(args[1], out float max);
					if(b) {
						data.SetRange(min, max);
						WriteLine("Height rescaled successfully");
					} else {
						WriteWarning("Failed to parse to float");
					}
				} else {
					WriteWarning("Two numbers are required!");
				}
			}
		}

		public override bool Export(string sourceFilePath, FileFormat ff, ASCData source, string fullpath, string fileSubName, ExportOptions options, Bounds bounds) {
			if(ff.IsPointFormat()) {
				return WriteFilePointData(source, fullpath, options.subsampling, bounds, ff);
			} else if(ff.Is3DFormat()) {
				return WriteFile3D(source, fullpath, options.subsampling, bounds, ff);
			} else if(ff.IsImage()) {
				return WriteFileImage(source, fullpath, options.subsampling, bounds, ff);
			}
			return false;
		}


		public static bool WriteFilePointData(ASCData source, string filename, int subsampling, Bounds bounds, FileFormat ff) {
			try {
				if(ff.IsFormat("ASC") || ff.IsFormat("PTS_XYZ")) {
					IExporter exporter;
					exporter = new PointDataExporter(source, subsampling, bounds);
					ExportUtility.WriteFile(exporter, filename, ff);
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

		public static bool WriteFile3D(ASCData source, string filename, int subsampling, Bounds bounds, FileFormat ff) {
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

		public static bool WriteFileImage(ASCData source, string filename, int subsampling, Bounds bounds, FileFormat ff) {
			if(subsampling < 1) subsampling = 1;
			float[,] grid = new float[bounds.NumCols / subsampling, bounds.NumRows / subsampling];
			for(int x = 0; x < grid.GetLength(0); x++) {
				for(int y = 0; y < grid.GetLength(1); y++) {
					grid[x, y] = source.data[bounds.xMin + x * subsampling, bounds.yMin + y * subsampling];
				}
			}
			try {
				IExporter exporter = new ImageExporter(grid, source.cellsize, ff.GetImageType(), source.lowestValue, source.highestValue);
				ExportUtility.WriteFile(exporter, filename, ff);
				return true;
			} catch(Exception e) {
				Program.WriteError("Failed to create Image file!");
				Program.WriteLine(e.ToString());
				return false;
			}
		}

		public override string GetSuffixWithExtension(FileFormat ff) {
			string str = "";
			if(ff.IsFormat("IMG_PNG-HEIGHT")) str = "_height";
			else if(ff.IsFormat("IMG_PNG-NORMAL")) str = "_normal";
			else if(ff.IsFormat("IMG_PNG-HILLSHADE")) str = "_hillshade";
			string ext = GetFiletypeString(ff);
			if(!string.IsNullOrEmpty(ext)) {
				return str + "." + ext;
			} else {
				return str;
			}
		}

		string GetFiletypeString(FileFormat ff) {
			if(ff.IsFormat("ASC")) return "asc";
			else if(ff.IsFormat("PTS_XYZ")) return "xyz";
			else if(ff.IsFormat("MDL_3DS")) return "3ds";
			else if(ff.IsFormat("MDL_FBX")) return "fbx";
			else if(ff.IsFormat("IMG_PNG-HEIGHT")) return "png";
			else if(ff.IsFormat("IMG_PNG-NORMAL")) return "png";
			else if(ff.IsFormat("IMG_PNG-HILLSHADE")) return "png";
			else return "";
		}

		public override void ValidateExportOptions(ExportOptions options, FileFormat format, ref bool valid, ASCData data) {
			int cellsPerFile = Program.GetTotalExportCellsPerFile();
			if(options.outputFormats.Count == 0) {
				Program.WriteWarning("No export format is defined! choose at least one format for export!");
				valid = false;
			}
			if(options.ContainsFormat("MDL_3DS")) {
				/*if(cellsPerFile >= 65535) {
					Console.WriteLine("ERROR: Cannot export more than 65535 cells in a single 3ds file! Current amount: "+cellsPerFile);
					Console.WriteLine("       Reduce splitting interval or increase subsampling to allow for exporting 3ds Files");
					valid = false;
				}*/
			}
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
