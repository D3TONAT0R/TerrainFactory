using System.IO;
//using System.Drawing;
using System.Drawing.Imaging;
using System;
using Ionic.Zlib;
using System.Collections;
using MCUtils;

namespace ASCReader.Import {
	public static class MinecraftRegionImporter {

		public static ASCData ImportHeightmap(string filepath) {
			ushort[,] hms = RegionImporter.GetHeightmap(filepath);
			float[,] hm = new float[512,512];
			for(int x = 0; x < 512; x++) {
				for(int z = 0; z < 512; z++) {
					hm[x,z] = (float)hms[x,z];
				}
			}
			ASCData asc = new ASCData(512, 512, filepath);
			asc.filename = Path.GetFileNameWithoutExtension(filepath);
			asc.data = hm;
			asc.cellsize = 1;
			asc.nodata_value = -9999;
			asc.RecalculateValues(false);
			asc.lowPoint = 0;
			asc.highPoint = 1;
			asc.isValid = true;
			Program.WriteLine("Lowest: " + asc.lowestValue);
			Program.WriteLine("Hightest: " + asc.highestValue);
			asc.lowestValue = 0;
			asc.highestValue = 255;
			return asc;
		}
	}
}