using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace HMConMC.PostProcessors.Splatmapper
{
	public class OreGenPostProcessor : AbstractPostProcessor
	{
		public class Ore
		{

			public string block;
			public int veinSizeMax;
			public float spawnsPerBlock;
			public int heightMin;
			public int heightMax;

			public Ore(string block, int veinSize, float rarity, int yMin, int yMax)
			{
				this.block = "minecraft:" + block;
				veinSizeMax = veinSize;
				spawnsPerBlock = rarity;
				heightMin = yMin;
				heightMax = yMax;
			}
		}

		public OreGenPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			random = new Random();
			rarityMul = float.Parse(xml.Element("multiplier")?.Value ?? "1");
		}

		public Random random;
		public static readonly Ore[] ores = new Ore[] {
			new Ore("iron_ore", 9, 1f/1500, 2, 66),
			new Ore("coal_ore", 11, 1f/800, 10, 120),
			new Ore("gold_ore", 8, 1f/3000, 2, 32)
		};
		public float rarityMul = 1;

		//public override Priority OrderPriority => Priority.Default;

		public override PostProcessType PostProcessorType => PostProcessType.Block;

		public override int BlockProcessYMin => 1;
		public override int BlockProcessYMax => 128;

		protected override void OnProcessBlock(MCUtils.World world, int x, int y, int z, int pass, float mask)
		{
			foreach (Ore o in ores)
			{
				if (random.NextDouble() * rarityMul < o.spawnsPerBlock) SpawnOre(world, o, x, y, z);
			}
		}

		private void SpawnOre(MCUtils.World world, Ore ore, int x, int y, int z)
		{
			for (int i = 0; i < ore.veinSizeMax; i++)
			{
				int x1 = x + RandomRange(-1, 1);
				int y1 = y + RandomRange(-1, 1);
				int z1 = z + RandomRange(-1, 1);
				if (world.IsDefaultBlock(x1, y1, z1)) world.SetBlock(x1, y1, z1, ore.block);
			}
		}

		private int RandomRange(int min, int max)
		{
			return random.Next(min, max + 1);
		}
	}
}
