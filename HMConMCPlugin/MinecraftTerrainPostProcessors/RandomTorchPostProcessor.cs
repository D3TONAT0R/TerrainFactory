using MCUtils;
using System;

namespace ASCReaderMC.PostProcessors {
	public class RandomTorchPostProcessor : MinecraftTerrainPostProcessor {

		public float chance;
		private Random random;

		public RandomTorchPostProcessor(float torchAmount) {
			chance = torchAmount;
			random = new Random();
		}

		public override void ProcessSurface(MCUtils.World world, int x, int y, int z) {
			if(random.NextDouble() <= chance && world.IsAir(x, y + 1, z)) world.SetBlock(x, y + 1, z, "minecraft:torch");
		}
	}
}