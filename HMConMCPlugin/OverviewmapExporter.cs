using HMConImage;
using HMCon;
using HMCon.Export;
using MCUtils;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using HMCon.Util;

namespace HMConMC {
	public class OverviewmapExporter {

		Bitmap map;

		public OverviewmapExporter(string regionPath, bool mcMapStyle) {
			if(Path.GetExtension(regionPath).ToLower() != ".mca") {
				throw new System.ArgumentException("The file '" + regionPath + "' is not a .mca file");
			}
			var data = MinecraftRegionImporter.ImportHeightmap(regionPath, HeightmapType.SolidBlocks);
			map = RegionLoader.GetSurfaceMap(regionPath, HeightmapType.SolidBlocks, mcMapStyle);
			if (!mcMapStyle)
			{
				map = GenerateShadedMap(data, map);
			}
		}

		public OverviewmapExporter(MCWorldExporter world, bool mcMapStyle, HeightmapType type = HeightmapType.SolidBlocks) {
			var heightmap = world.GetHeightmap(type, true);
			HeightData heightData = new HeightData(ArrayConverter.ToFloatMap(ArrayConverter.Flip(heightmap)), 1)
			{
				lowPoint = 0,
				highPoint = 256
			};
			map = world.world.GetSurfaceMap(world.worldBounds.xMin, world.worldBounds.yMin, heightmap, mcMapStyle);
			if (!mcMapStyle)
			{
				map = GenerateShadedMap(heightData, map);
			}
		}

		private Bitmap GenerateShadedMap(HeightData data, Bitmap surface) {
			return ImageExporter.GenerateCompositeMap(data, surface, 0.3f, 0.3f);
		}

		public void WriteFile(FileStream stream, string path) {
			map.Save(stream, ImageFormat.Png);
		}
	}
}
