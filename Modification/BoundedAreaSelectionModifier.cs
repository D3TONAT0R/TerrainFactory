using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modification {
	public class BoundedAreaSelectionModifier : Modifier {

		[DrawInInspector("Bounds")]
		public Bounds bounds;

		public BoundedAreaSelectionModifier()
		{

		}

		public BoundedAreaSelectionModifier(int x1, int y1, int x2, int y2) : this(new Bounds(x1, y1, x2, y2)) {

		}

		public BoundedAreaSelectionModifier(Bounds? b) {
			if(b == null)
			{
				bounds = new Bounds(0, 0, 0, 0);
			} else
			{
				bounds = b.Value;
			}
		}

		protected override void ModifyData(HeightData data) {
			if(bounds.CellCount <= 10) return;
			float[,] grid = new float[bounds.NumCols, bounds.NumRows];
			for(int y = 0; y < bounds.NumRows; y++) {
				for(int x = 0; x < bounds.NumCols; x++) {
					grid[x, y] = data.GetHeight(bounds.xMin + x, bounds.yMin + y);
				}
			}
			data.offsetFromSource.x += bounds.xMin;
			data.offsetFromSource.y += bounds.yMin;
			data.lowerCornerPos += new System.Numerics.Vector2(bounds.xMin, bounds.yMin) * data.cellSize;
			data.SetDataGrid(grid);
		}
	}
}
