using System;
using ASCReader.Export.Exporters;

public class OrePostProcessor : IMinecraftTerrainPostProcessor
{

	public class Ore {

		public string block;
		public int veinSizeMax;
		public float spawnsPerBlock;

		public Ore(string b, int v, float r) {
			block = b;
			veinSizeMax = v;
			spawnsPerBlock = r;
		}
	}

	public OrePostProcessor() {
		random = new Random();
	}

	public Random random;
	public static readonly Ore[] ores = new Ore[] {
		new Ore("iron_ore", 9, 1f/4000),
		new Ore("iron_ore", 11, 1f/2000)
	}

	public void ProcessBlock(MinecraftRegionExporter region, int x, int y, int z) {
		foreach(Ore o in ores) {
			if(random.NextDouble() < o.spawnsPerBlock) SpawnOre(region, o, x, y, z);
		}
	}

	private void SpawnOre(MinecraftRegionExporter region, Ore ore, int x, int y, int z) {
		
	}

	public void ProcessSurface(MinecraftRegionExporter region, int x, int y, int z)	{

	}
}