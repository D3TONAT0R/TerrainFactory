using MCUtils;
using System;

namespace ASCReaderMC.PostProcessors {
	public class VegetationPostProcessor : MinecraftTerrainPostProcessor {

		readonly byte[,,] blueprintOakTreeTop = new byte[,,] {
		//YZX
		{
		{0,0,0,0,0},
		{0,0,2,0,0},
		{0,2,1,2,0},
		{0,0,2,0,0},
		{0,0,0,0,0},
		},{
		{0,2,2,2,0},
		{2,2,2,2,2},
		{2,2,1,2,2},
		{2,2,2,2,2},
		{0,2,2,2,0}
		},{
		{0,2,2,2,0},
		{2,2,2,2,2},
		{2,2,1,2,2},
		{2,2,2,2,2},
		{0,2,2,2,0}
		},{
		{0,0,0,0,0},
		{0,2,2,2,0},
		{0,2,1,2,0},
		{0,2,2,2,0},
		{0,0,0,0,0}
		},{
		{0,0,0,0,0},
		{0,0,2,0,0},
		{0,2,2,2,0},
		{0,0,2,0,0},
		{0,0,0,0,0}
		}
	};
		readonly int treeRadius = 2;
		readonly int treeTopHeight = 5;

		private Random random;
		private float grassChance;
		private float treesChance;

		public VegetationPostProcessor(float grassAmount, float treesAmount) {
			random = new Random();
			grassChance = grassAmount;
			treesChance = treesAmount / 128f;
		}

		public override void ProcessSurface(MCUtils.World world, int x, int y, int z) {
			//Place trees
			if(random.NextDouble() <= treesChance) {
				if(PlaceTree(world, x, y + 1, z)) {
					//A tree was placed, there is nothing left to do here
					return;
				}
			}
			//Place tall grass
			if(random.NextDouble() <= grassChance) {
				PlaceGrass(world, x, y + 1, z);
			}
		}

		private bool PlaceTree(MCUtils.World world, int x, int y, int z) {
			var b = world.GetBlock(x, y - 1, z);
			if(b == null || !CanGrowPlant(b)) return false;
			int bareTrunkHeight = random.Next(1, 4);
			int w = treeRadius;
			if(!world.IsAir(x, y + 1, z)) return false;
			//if(IsObstructed(region, x, y+1, z, x, y+bareTrunkHeight, z) || IsObstructed(region, x-w, y+bareTrunkHeight, z-w, x+w, y+bareTrunkHeight+treeTopHeight, z+w)) return false;
			world.SetBlock(x, y - 1, z, "minecraft:dirt");
			for(int i = 0; i <= bareTrunkHeight; i++) {
				world.SetBlock(x, y + i, z, "minecraft:oak_log");
			}
			for(int ly = 0; ly < treeTopHeight; ly++) {
				for(int lz = 0; lz < 2 * treeRadius + 1; lz++) {
					for(int lx = 0; lx < 2 * treeRadius + 1; lx++) {
						int palette = blueprintOakTreeTop[ly, lz, lx];
						if(palette > 0) {
							string block = palette == 1 ? "minecraft:oak_log" : "minecraft:oak_leaves";
							world.SetBlock(x + lx - treeRadius, y + ly + bareTrunkHeight + 1, z + lz - treeRadius, block);
						}
					}
				}
			}
			return true;
		}

		private bool PlaceGrass(World world, int x, int y, int z) {
			var b = world.GetBlock(x, y - 1, z);
			if(b == null || b != "minecraft:grass_block") return false;
			return world.SetBlock(x, y, z, "minecraft:grass");
		}

		private bool IsObstructed(World world, int x1, int y1, int z1, int x2, int y2, int z2) {
			for(int y = y1; y <= y2; y++) {
				for(int z = z1; z <= z2; z++) {
					for(int x = x1; x <= x2; x++) {
						if(!world.IsAir(x, y, z)) return false;
					}
				}
			}
			return true;
		}

		private bool CanGrowPlant(string block) {
			return block == "minecraft:grass_block" || block == "minecraft:dirt";
		}
	}
}