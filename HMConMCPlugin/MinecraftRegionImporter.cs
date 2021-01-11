//using System.Drawing;
using HMCon;
using HMCon.Import;
using MCUtils;
using System.Collections.Generic;
using System.IO;

namespace ASCReaderMC {
	public class MinecraftRegionImporter : ASCReaderImportHandler {

		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("MCA", "MCA", "mca", "Minecraft region format", this));
		}

		public override ASCData Import(string importPath, FileFormat ff) {
			return ImportHeightmap(importPath);
		}

		ASCData ImportHeightmap(string filepath) {
			ushort[,] hms = RegionImporter.GetHeightmap(filepath, true);
			float[,] hm = new float[512, 512];
			for(int x = 0; x < 512; x++) {
				for(int z = 0; z < 512; z++) {
					hm[x, z] = (float)hms[x, z];
				}
			}
			ASCData asc = new ASCData(512, 512, filepath);
			asc.filename = Path.GetFileNameWithoutExtension(filepath);
			asc.data = hm;
			asc.cellsize = 1;
			asc.nodata_value = -9999;
			asc.RecalculateValues(false);
			asc.lowPoint = 0;
			asc.highPoint = 255;
			asc.isValid = true;
			Program.WriteLine("Lowest: " + asc.lowestValue);
			Program.WriteLine("Hightest: " + asc.highestValue);
			asc.lowestValue = 0;
			asc.highestValue = 255;
			return asc;
		}
	}
}