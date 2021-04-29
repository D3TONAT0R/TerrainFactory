using HMCon;
using MCUtils;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace HMConMC.PostProcessors {
	public class SplatmappedBiomePostProcessor : MinecraftTerrainPostProcessor {

		public Dictionary<string, byte[,]> maps = new Dictionary<string, byte[,]>();
		public Dictionary<byte, BiomeGenerator> biomes;

		public override Priority OrderPriority => Priority.Default;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public SplatmappedBiomePostProcessor(string filepath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ) {
			var desc = new SplatmapDescriptorReader(filepath, false);
			biomes = desc.biomes;
			foreach(string k in desc.maps.Keys) {
				string path = Path.GetDirectoryName(filepath);
				List<SplatmapMapping> mappings = new List<SplatmapMapping>();
				foreach(var sm in desc.layers.Keys) {
					if(sm.mapName == k) mappings.Add(sm);
				}
				mappings.Add(new SplatmapMapping(k, Color.Black, 0));
				maps.Add(k, SplatmapImporter.GetFixedSplatmap(path + "\\" + desc.maps[k], mappings.ToArray(), ditherLimit, offsetX, offsetZ, sizeX, sizeZ));
			}
			/*foreach(var sm in desc.layers.Keys) {
				layers.Add((byte)sm.value, desc.layers[sm].Split(','));
			}*/
			ConsoleOutput.WriteLine("Biome mapping & decoration enabled");
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
