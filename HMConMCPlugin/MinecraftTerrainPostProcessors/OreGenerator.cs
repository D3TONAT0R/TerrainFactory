using MCUtils;
using System;
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{
	public class OreGenerator
	{

		public BlockState block;
		public int veinSizeMax;
		public float spawnsPerColumn;
		public int heightMin;
		public int heightMax;

		public OreGenerator(string block, int veinSize, float rarityPerChunk, int yMin, int yMax)
		{
			this.block = new BlockState(BlockList.Find(block));
			veinSizeMax = veinSize;
			spawnsPerColumn = rarityPerChunk / 256f;
			heightMin = yMin;
			heightMax = yMax;
		}

		public static OreGenerator ParseFromXML(XElement elem)
		{
			string block = elem.Attribute("block").Value;
			int veinSize = int.Parse(elem.Attribute("size").Value);
			float rarity = float.Parse(elem.Attribute("rarity").Value);
			int yMin = int.Parse(elem.Attribute("y-min")?.Value ?? "1");
			int yMax = int.Parse(elem.Attribute("y-max")?.Value ?? "32");
			return new OreGenerator(block, veinSize, rarity, yMin, yMax);
		}

		public void Generate(MCUtils.World world, Random random, int x, int z)
		{
			int y = RandomRange(random, heightMin, heightMax);
			int span = (int)Math.Floor((veinSizeMax - 1) / 16f) + 1;
			for (int i = 0; i < veinSizeMax; i++)
			{
				int x1 = x + RandomRange(random, -span, span);
				int y1 = y + RandomRange(random, -span, span);
				int z1 = z + RandomRange(random, -span, span);
				if (world.IsDefaultBlock(x1, y1, z1)) world.SetBlock(x1, y1, z1, block);
			}
		}

		private int RandomRange(Random random, int min, int max)
		{
			return random.Next(min, max + 1);
		}
	}
}
