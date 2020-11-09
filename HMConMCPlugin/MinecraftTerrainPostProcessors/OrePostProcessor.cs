using MCUtils;
using System;

namespace ASCReaderMC.PostProcessors {
	public class OrePostProcessor : MinecraftTerrainPostProcessor {

		public class Ore {

			public string block;
			public int veinSizeMax;
			public float spawnsPerBlock;
			public int heightMin;
			public int heightMax;

			public Ore(string b, int v, float r, int min, int max) {
				block = "minecraft:" + b;
				veinSizeMax = v;
				spawnsPerBlock = r;
				heightMin = min;
				heightMax = max;
			}
		}

		public OrePostProcessor(float totalRarityMul) {
			random = new Random();
			rarityMul = totalRarityMul;
		}

		public Random random;
		public static readonly Ore[] ores = new Ore[] {
		new Ore("iron_ore", 9, 1f/2500, 2, 66),
		new Ore("coal_ore", 11, 1f/1500, 10, 120),
		new Ore("gold_ore", 8, 1f/6000, 2, 32)
		};
		public float rarityMul = 1;

		public override void ProcessBlock(MCUtils.World world, int x, int y, int z) {
			foreach(Ore o in ores) {
				if(random.NextDouble() * rarityMul < o.spawnsPerBlock) SpawnOre(world, o, x, y, z);
			}
		}

		private void SpawnOre(MCUtils.World world, Ore ore, int x, int y, int z) {
			for(int i = 0; i < ore.veinSizeMax; i++) {
				int x1 = x + RandomRange(-1, 1);
				int y1 = y + RandomRange(-1, 1);
				int z1 = z + RandomRange(-1, 1);
				if(world.IsDefaultBlock(x1, y1, z1)) world.SetBlock(x1, y1, z1, ore.block);
			}
		}

		private int RandomRange(int min, int max) {
			return random.Next(min, max + 1);
		}
	}
}