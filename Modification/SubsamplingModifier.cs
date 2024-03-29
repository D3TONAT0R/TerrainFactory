﻿using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modification {
	public class SubsamplingModifier : Modifier {

		[DrawInInspector("Subsample amount")]
		public int subsampleAmount = 1;

		public SubsamplingModifier()
		{

		}

		public SubsamplingModifier(int subsample)
		{
			subsampleAmount = subsample;
		}

		protected override void ModifyData(ElevationData data) {
			if(subsampleAmount <= 1) return;
			float[,] grid = new float[data.CellCountX / subsampleAmount, data.CellCountY / subsampleAmount];
			for(int y = 0; y < grid.GetLength(1); y++) {
				for(int x = 0; x < grid.GetLength(0); x++) {
					grid[x, y] = data.GetElevationAtCell(x * subsampleAmount, y * subsampleAmount);
				}
			}
			data.CellSize *= subsampleAmount;
			data.ReplaceData(grid);
		}
	}
}
