using MCUtils;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ASCReaderMC.PostProcessors {
	public class SplatmappedBiomePostProcessor : MinecraftTerrainPostProcessor {

		public Dictionary<string, byte[,]> maps = new Dictionary<string, byte[,]>();
		public Dictionary<byte, BiomeGenerator> biomes;

		public SplatmappedBiomePostProcessor(string filepath, int ditherLimit, int localRegionX, int localRegionZ) {
			var desc = new SplatmapDescriptorReader(filepath, false);
			biomes = desc.biomes;
			foreach(string k in desc.maps.Keys) {
				string path = Path.GetDirectoryName(filepath);
				List<SplatmapMapping> mappings = new List<SplatmapMapping>();
				foreach(var sm in desc.layers.Keys) {
					if(sm.mapName == k) mappings.Add(sm);
				}
				mappings.Add(new SplatmapMapping(k, Color.Black, 0));
				maps.Add(k, SplatmapImporter.GetFixedSplatmap(path + "\\" + desc.maps[k], mappings.ToArray(), ditherLimit, localRegionX, localRegionZ));
			}
			/*foreach(var sm in desc.layers.Keys) {
				layers.Add((byte)sm.value, desc.layers[sm].Split(','));
			}*/
		}

		public override void ProcessSurface(MCUtils.World world, int x, int y, int z) {
			var id = maps["main"][x, z];
			if(biomes.ContainsKey(id)) {
				biomes[id].RunGenerator(world, x, y, z);
				world.SetBiome(x, z, biomes[id].biomeID);
			}
		}
	}
}
