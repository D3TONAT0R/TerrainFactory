using MCUtils;
using System;
using System.Xml.Linq;

namespace HMConMC.PostProcessors {
	public class BedrockPostProcessor : AbstractPostProcessor {

		public bool flatBedrock = false;
		Random random;

		public override Priority OrderPriority => Priority.First;

		public override PostProcessType PostProcessorType => PostProcessType.Block;

		public override int BlockProcessYMin => 0;
		public override int BlockProcessYMax => flatBedrock ? 0 : 3;

		public BedrockPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			random = new Random();
		}

		protected override void OnProcessBlock(World world, int x, int y, int z, int pass, float mask)
		{
			if(random.NextDouble() < 1f - y / 4f && !world.IsAir(x,y,z)) world.SetBlock(x, y, z, "minecraft:bedrock");
		}
	}
}