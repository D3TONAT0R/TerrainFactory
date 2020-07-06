using ASCReader.Export.Exporters.MinecraftTerrainPostProcessors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace ASCReader.Export.Exporters {
	public class SplatmappedBiomePostProcessor : IMinecraftTerrainPostProcessor {

		public Dictionary<string, byte[,]> maps = new Dictionary<string, byte[,]>();
		public Dictionary<byte, BiomeGenerator> biomes;

		public SplatmappedBiomePostProcessor(string importedFilePath, int ditherLimit, int localRegionX, int localRegionZ) {
			var desc = new SplatmapDescriptorReader(importedFilePath, SplatmapDescriptorReader.Type.BiomeSplatmapDesc);
			biomes = desc.biomes;
			foreach(string k in desc.maps.Keys) {
				string path = Path.GetDirectoryName(importedFilePath);
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
		
		public void ProcessBlock(MinecraftRegionExporter region, int x, int y, int z) {
		}

		public void ProcessSurface(MinecraftRegionExporter region, int x, int y, int z) {
			var id = maps["main"][x, z];
			biomes[id].RunGenerator(region, x, y, z);
		}
	}
}
