using MCUtils;
using System;

namespace ASCReaderMC.PostProcessors {
	public class BedrockPostProcessor : MinecraftTerrainPostProcessor {

		public bool flatBedrock = false;
		Random random;

		public BedrockPostProcessor() {
			random = new Random();
		}

		public override void ProcessBlock(MCUtils.World world, int x, int y, int z) {
			if(y == 0) {
				world.SetBlock(x, 0, z, "bedrock");
				if(!flatBedrock) {
					if(random.NextDouble() < 0.75f) world.SetBlock(x, 1, z, "bedrock");
					if(random.NextDouble() < 0.50f) world.SetBlock(x, 2, z, "bedrock");
					if(random.NextDouble() < 0.25f) world.SetBlock(x, 3, z, "bedrock");
				}
			}
		}
	}
}