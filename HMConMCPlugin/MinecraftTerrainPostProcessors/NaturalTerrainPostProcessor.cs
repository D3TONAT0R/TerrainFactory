using MCUtils;

namespace HMConMC.PostProcessors {
	public class NaturalTerrainPostProcessor : PostProcessor {

		public override Priority OrderPriority => Priority.BeforeDefault;

		public int waterLevel = -1;
		public override PostProcessType PostProcessorType => PostProcessType.Both;

		public NaturalTerrainPostProcessor(bool fillWithWater) {
			waterLevel = fillWithWater ? 63 : -1;
		}

		protected override void OnProcessBlock(MCUtils.World world, int x, int y, int z, int pass, float mask)
		{
			//Make flat bedrock
			if(y == 0) {
				if(world.IsDefaultBlock(x, 0, z)) world.SetBlock(x, 0, z, "minecraft:bedrock");
			}
			//Fill the terrain with water up to the waterLevel
			if(y <= waterLevel) {
				if(world.IsAir(x, y, z)) world.SetBlock(x, y, z, "minecraft:water");
			}
		}

		protected override void OnProcessSurface(MCUtils.World world, int x, int y, int z, int pass, float mask)
		{
			//Place grass on top & 3 layers of dirt below
			if(y > waterLevel + 1) {
				world.SetBlock(x, y, z, "minecraft:grass_block");
				for(int i = 1; i < 4; i++) {
					world.SetBlock(x, y - i, z, "minecraft:dirt");
				}
			} else {
				for(int i = 0; i < 4; i++) {
					world.SetBlock(x, y - i, z, "minecraft:gravel");
				}
			}
		}
	}
}