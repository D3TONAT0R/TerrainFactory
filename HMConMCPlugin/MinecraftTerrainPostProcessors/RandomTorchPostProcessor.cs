using MCUtils;
using System;
using System.Xml.Linq;

namespace HMConMC.PostProcessors {
	public class RandomTorchPostProcessor : AbstractPostProcessor {

		public float chance;
		private Random random;

		public override Priority OrderPriority => Priority.AfterDefault;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public RandomTorchPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			chance = float.Parse(xml.Element("amount")?.Value ?? "0.02");
			random = new Random();
		}

		protected override void OnProcessSurface(MCUtils.World world, int x, int y, int z, int pass, float mask)
		{
			if(random.NextDouble() <= chance && world.IsAir(x, y + 1, z)) world.SetBlock(x, y + 1, z, "minecraft:torch");
		}
	}
}