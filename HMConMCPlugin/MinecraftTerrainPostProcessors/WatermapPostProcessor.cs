using HMCon;
using HMConImage;
using MCUtils;
using System;

namespace HMConMC.PostProcessors {
	public class WatermapPostProcessor : MinecraftTerrainPostProcessor {

		int waterLevel = 63;
		public string waterBlock = "minecraft:water";
		byte[,] waterSurfaceMap;


		public override Priority OrderPriority => Priority.AfterFirst;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public WatermapPostProcessor(string waterMapPath, int offsetX, int offsetZ, int sizeX, int sizeZ, int waterLevel, string waterBlock) {
			waterSurfaceMap = HeightmapImporter.ImportHeightmapRaw(waterMapPath, offsetX, offsetZ, sizeX, sizeZ);
			this.waterLevel = waterLevel;
			this.waterBlock = waterBlock;
			Program.WriteLine("Water mapping enabled");
		}

		public override void ProcessSurface(MCUtils.World world, int x, int y, int z) {
			for(byte y2 = waterSurfaceMap[x, z]; y2 > y; y2--) {
				world.SetBlock(x, y2, z, waterBlock);
			}
		}
	}
}