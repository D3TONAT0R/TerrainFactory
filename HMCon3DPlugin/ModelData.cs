using HMCon;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace HMCon3D
{
	internal class ModelData
	{
		public List<MeshData> meshes = new List<MeshData>();

		public static ModelData Create(HeightData source)
		{
			var model = new ModelData();
			int sizeX = source.GridWidth - 1;
			int sizeY = source.GridHeight - 1;
			int splitX = (int)Math.Ceiling(sizeX / 128f);
			int splitY = (int)Math.Ceiling(sizeY / 128f);
			int y = 0;
			while (y < sizeY)
			{
				int cellsY = (int)Math.Ceiling(sizeY / (float)splitY);
				int x = 0;
				while (x < sizeX)
				{
					int cellsX = (int)Math.Ceiling(sizeX / (float)splitX);
					model.meshes.Add(model.CreateMeshData(source, x, y, x + cellsX, y + cellsY));
					x += cellsX;
				}
				y += cellsY;
			}
			return model;
		}

		MeshData CreateMeshData(HeightData source, int xMin, int yMin, int xMax, int yMax)
		{
			Vector3[,] points = new Vector3[xMax - xMin + 1, yMax - yMin + 1];
			MeshData mesh = new MeshData();
			for (int i = 0; i <= xMax - xMin; i++) for (int j = 0; j <= yMax - yMin; j++) points[i, j] = Vector3.Zero;
			for (int y = yMin; y <= yMax; y++)
			{
				for (int x = xMin; x <= xMax; x++)
				{
					float f = source.GetHeight(x, y);
					if (f != source.nodata_value)
					{
						Vector3 vec = new Vector3(-x * source.cellSize, source.GetHeight(x, y), y * source.cellSize);
						points[x - xMin, y - yMin] = vec;
						if (!mesh.vertices.Contains(vec))
						{
							mesh.vertices.Add(vec);
							float uvX = (x - xMin) / (float)(xMax - xMin);
							float uvY = (y - yMin) / (float)(yMax - yMin);
							mesh.uvs.Add(new Vector2(uvX, uvY));
						}
					}
				}
			}
			int nodatas = 0;
			for (int y = yMin; y < yMax; y++)
			{
				for (int x = xMin; x < xMax; x++)
				{
					Vector3[] pts = GetPointsForFace(points, x - xMin, y - yMin);
					if (pts != null)
					{ //if the list is null, then a nodata-value was found
						int i0 = mesh.vertices.IndexOf(pts[0]);
						int i1 = mesh.vertices.IndexOf(pts[1]);
						int i2 = mesh.vertices.IndexOf(pts[2]);
						int i3 = mesh.vertices.IndexOf(pts[3]);
						//Lower-Right triangle
						mesh.tris.Add(i0);
						mesh.tris.Add(i1);
						mesh.tris.Add(i3);
						//Upper-Left triangle
						mesh.tris.Add(i0);
						mesh.tris.Add(i3);
						mesh.tris.Add(i2);
					}
					else
					{
						nodatas++;
						if (ConsoleOutput.debugLogging) ConsoleOutput.WriteWarning("[#" + nodatas + "] NODATA_VALUE or missing data at point " + x + " " + y);
					}
				}
			}
			return mesh;
		}

		Vector3[] GetPointsForFace(Vector3[,] points, int x, int y)
		{
			Vector3[] pts = new Vector3[4];
			int x1 = x;
			int y1 = y;
			int x2 = Math.Min(x1 + 1, points.GetLength(0) - 1);
			int y2 = Math.Min(y1 + 1, points.GetLength(1) - 1);
			pts[0] = points[x1, y1];
			pts[1] = points[x2, y1];
			pts[2] = points[x1, y2];
			pts[3] = points[x2, y2];
			foreach (Vector3 pt in pts) if (pt.Equals(Vector3.Zero)) return null;
			return pts;
		}
	}
}
