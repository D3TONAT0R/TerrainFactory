using HMCon;
using HMCon.Import;
using MCUtils;
using System.Collections.Generic;
using System.IO;

namespace HMConMC {
	public static class MinecraftRegionImporter {

		public static HeightData Import(string importPath, params string[] args) {
			return ImportHeightmap(importPath, HeightmapType.TerrainBlocksNoLiquid);
		}

		public static HeightData ImportHeightmap(string filepath, HeightmapType type) {
			short[,] hms = RegionLoader.GetHeightmap(filepath, type);
			HeightData asc = new HeightData(512, 512, filepath);
			for(int x = 0; x < 512; x++) {
				for(int z = 0; z < 512; z++) {
					asc.SetHeight(x, z, hms[x, 511 - z]);
				}
			}
			asc.filename = Path.GetFileNameWithoutExtension(filepath);
			asc.cellSize = 1;
			asc.nodata_value = -9999;
			asc.RecalculateValues(false);
			asc.lowPoint = 0;
			asc.highPoint = 255;
			asc.isValid = true;
			ConsoleOutput.WriteLine("Lowest: " + asc.lowestValue);
			ConsoleOutput.WriteLine("Hightest: " + asc.highestValue);
			asc.lowestValue = 0;
			asc.highestValue = 255;
			return asc;
		}
	}
}