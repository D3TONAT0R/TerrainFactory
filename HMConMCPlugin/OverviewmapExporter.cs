using HMConImage;
using HMConMC;
using HMCon;
using HMCon.Export;
using MCUtils;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using HMCon.Util;

namespace HMConMCPlugin {
	public class OverviewmapExporter : IExporter {

		Bitmap map;

		public OverviewmapExporter(string regionPath) {
			if(Path.GetExtension(regionPath).ToLower() != ".mca") {
				throw new System.ArgumentException("The file '" + regionPath + "' is not a .mca file");
			}
			var data = MinecraftRegionImporter.ImportHeightmap(regionPath, HeightmapType.SolidBlocks);
			map = RegionImporter.GetSurfaceMap(regionPath, HeightmapType.SolidBlocks);
			map = GenerateMap(data, map);
		}

		public OverviewmapExporter(MCWorldExporter world) {
			var heightmap = world.GetHeightmap(HeightmapType.SolidBlocks, true);
			ASCData heightData = new ASCData(ArrayConverter.ToFloatMap(ArrayConverter.Flip(heightmap)), 1);
			heightData.lowPoint = 0;
			heightData.highPoint = 256;
			map = world.world.GetSurfaceMap(world.worldBounds.xMin, world.worldBounds.yMin, heightmap);
			map = GenerateMap(heightData, map);
		}

		private Bitmap GenerateMap(ASCData data, Bitmap surface) {
			return ImageExporter.GenerateCompositeMap(data, surface, 0.3f, 0.3f);
		}

		public bool NeedsFileStream(FileFormat format) {
			return true;
		}

		public void WriteFile(FileStream stream, string path, FileFormat filetype) {
			map.Save(stream, ImageFormat.Png);
		}
	}
}
