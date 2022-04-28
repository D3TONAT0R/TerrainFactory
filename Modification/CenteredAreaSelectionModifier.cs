using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Modification
{
	public class CenteredAreaSelectionModifier : Modifier
	{

		[DrawInInspector("Center X")]
		public Coordinate centerX;
		[DrawInInspector("Center Y")]
		public Coordinate centerY;

		public float size = -1;

		public CenteredAreaSelectionModifier() { }

		public CenteredAreaSelectionModifier(Coordinate centerX, Coordinate centerY, float size)
		{
			this.centerX = centerX;
			this.centerY = centerY;
			this.size = size;
		}

		protected override void ModifyData(HeightData data)
		{
			if (size <= 0) return;

			var cx = centerX.GetLocalValue(data.lowerCornerPos.X);
			var cy = centerY.GetLocalValue(data.lowerCornerPos.Y);

			var llx = cx - size / 2f;
			var lly = cy - size / 2f;
			var urx = cx + size / 2f;
			var ury = cy + size / 2f;

			int x1 = ClampRounded(llx / data.cellSize, 0, data.GridWidth - 1);
			int y1 = ClampRounded(lly / data.cellSize, 0, data.GridHeight - 1);
			int x2 = ClampRounded(urx / data.cellSize, 0, data.GridWidth - 1);
			int y2 = ClampRounded(ury / data.cellSize, 0, data.GridHeight - 1);

			var bounds = new Bounds(x1, y1, x2, y2);
			if (bounds.CellCount <= 10) return;
			float[,] grid = new float[bounds.NumCols, bounds.NumRows];
			for (int y = 0; y < bounds.NumRows; y++)
			{
				for (int x = 0; x < bounds.NumCols; x++)
				{
					grid[x, y] = data.GetHeight(bounds.xMin + x, bounds.yMin + y);
				}
			}
			data.offsetFromSource.x += bounds.xMin;
			data.offsetFromSource.y += bounds.yMin;
			data.lowerCornerPos += new System.Numerics.Vector2(bounds.xMin, bounds.yMin) * data.cellSize;
			data.SetDataGrid(grid);
		}

		private int ClampRounded(float f, int min, int max)
		{
			int i = (int)Math.Round(f);
			return Math.Max(min, Math.Min(max, i));
		}
	}
}
