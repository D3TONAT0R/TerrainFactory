using HMCon;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace HMConImage
{
	public static class NormalMapper
	{

		const double Rad2Deg = 180f / Math.PI;

		public static Vector3[,] CalculateNormals(HeightData grid, bool sharpMode)
		{
			if (sharpMode)
			{
				var normals = new Vector3[grid.GridWidth, grid.GridHeight];
				for (int x = 0; x < grid.GridWidth - 1; x++)
				{
					for (int y = 0; y < grid.GridHeight - 1; y++)
					{
						float ll = grid.GetHeightBounded(x, y);
						float lr = grid.GetHeightBounded(x + 1, y);
						float ul = grid.GetHeightBounded(x, y + 1);
						float ur = grid.GetHeightBounded(x + 1, y + 1);
						float nrmX = (GetSlope(lr, ll, grid.cellSize) + GetSlope(ur, ul, grid.cellSize)) / 2f;
						float nrmY = (GetSlope(ul, ll, grid.cellSize) + GetSlope(ur, lr, grid.cellSize)) / 2f;
						float power = Math.Abs(nrmX) + Math.Abs(nrmY);
						if (power > 1)
						{
							nrmX /= power;
							nrmY /= power;
						}
						float nrmZ = 1f - power;
						normals[x, y] = Vector3.Normalize(new Vector3(nrmX, nrmY, nrmZ));
					}
				}
				return normals;
			}
			else
			{
				var normals = new Vector3[grid.GridWidth, grid.GridHeight];
				for (int x = 0; x < grid.GridWidth; x++)
				{
					for (int y = 0; y < grid.GridHeight; y++)
					{
						float m = grid.GetHeightBounded(x, y);
						float r = GetSlope(grid.GetHeightBounded(x + 1, y), m, grid.cellSize);
						float l = GetSlope(m, grid.GetHeightBounded(x - 1, y), grid.cellSize);
						float u = GetSlope(grid.GetHeightBounded(x, y + 1), m, grid.cellSize);
						float d = GetSlope(m, grid.GetHeightBounded(x, y - 1), grid.cellSize);
						float nrmX = (r + l) / 2f;
						float nrmY = (u + d) / 2f;
						float power = Math.Abs(nrmX) + Math.Abs(nrmY);
						if (power > 1)
						{
							nrmX /= power;
							nrmY /= power;
						}
						float nrmZ = 1f - power;
						normals[x, y] = Vector3.Normalize(new Vector3(nrmX, nrmY, nrmZ));
					}
				}
				return normals;
			}
		}

		/*
		private static Vector3 Normalize(Vector3 src)
		{
			float power = Math.Abs(src.X) + Math.Abs(src.Y) + Math.Abs(src.Z);
			return src / power;
		}
		*/

		private static float GetSlope(float from, float to, float gridSpacing)
		{
			float hdiff = to - from;
			return (float)(Math.Atan(hdiff / gridSpacing) * Rad2Deg / 90f);
		}

		private static int Clamp(int v, int min, int max)
		{
			return Math.Max(min, Math.Min(max, v));
		}

		private static float Clamp(float v, float min, float max)
		{
			return Math.Max(min, Math.Min(max, v));
		}
	}
}
