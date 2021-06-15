using MCUtils;
using System;

namespace HMConMC.PostProcessors {
	public class RandomTorchPostProcessor : PostProcessor {

		public float chance;
		private Random random;

		public override Priority OrderPriority => Priority.AfterDefault;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public RandomTorchPostProcessor(float torchAmount) {
			chance = torchAmount;
			random = new Random();
		}

		protected override void OnProcessSurface(MCUtils.World world, int x, int y, int z, int pass, float mask)
		{
			if(random.NextDouble() <= chance && world.IsAir(x, y + 1, z)) world.SetBlock(x, y + 1, z, "minecraft:torch");
		}
	}
}