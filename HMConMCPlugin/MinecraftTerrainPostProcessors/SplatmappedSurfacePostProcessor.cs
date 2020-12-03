using HMCon;
using HMCon.Import;
using ASCReaderImagePlugin;
using MCUtils;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ASCReaderMC.PostProcessors {
	public class SplatmappedSurfacePostProcessor : MinecraftTerrainPostProcessor {
		public Dictionary<string, byte[,]> maps = new Dictionary<string, byte[,]>();
		public Dictionary<byte, string[]> layers = new Dictionary<byte, string[]>();

		public byte[,] waterSurfaceMap;
		public int waterLevel;
		public string waterBlock;
		public SplatmappedBiomePostProcessor biomePostProcessor;

		public SplatmappedSurfacePostProcessor(string importedFilePath, int ditherLimit, int localRegionX, int localRegionZ) {
			var desc = new SplatmapDescriptorReader(importedFilePath + ".splat", true);
			string root = Path.GetDirectoryName(importedFilePath);
			foreach(string k in desc.maps.Keys) {
				List<SplatmapMapping> mappings = new List<SplatmapMapping>();
				foreach(var sm in desc.layers.Keys) {
					if(sm.mapName == k) mappings.Add(sm);
				}
				mappings.Add(new SplatmapMapping(k, Color.Black, 0));
				maps.Add(k, SplatmapImporter.GetFixedSplatmap(root + "\\" + desc.maps[k], mappings.ToArray(), ditherLimit, localRegionX, localRegionZ));
			}
			foreach(var sm in desc.layers.Keys) {
				layers.Add((byte)sm.value, desc.layers[sm].Split(','));
			}
			Program.WriteLine("Splatmapping enabled");
			if(!string.IsNullOrWhiteSpace(desc.watermapPath)) {
				waterSurfaceMap = HeightmapImporter.ImportHeightmapRaw(root + "\\" + desc.watermapPath, localRegionX * 512, localRegionZ * 512, 512, 512);
				waterLevel = desc.waterLevel;
				waterBlock = desc.waterBlock;
				Program.WriteLine("Water mapping enabled");
			}
			if(!string.IsNullOrWhiteSpace(desc.biomeMapperPath)) {
				biomePostProcessor = new SplatmappedBiomePostProcessor(root + "\\" + desc.biomeMapperPath, 0, localRegionX, localRegionZ);
				Program.WriteLine("Biome mapping & decoration enabled");
			}
		}

		public override void ProcessSurface(MCUtils.World world, int x, int y, int z) {
			foreach(string map in maps.Keys) {
				byte mappedValue = maps[map][x, z];
				if(mappedValue > 0) {
					MakeLayer(world, x, y, z, layers[mappedValue]);
				}
			}
			if(waterSurfaceMap != null) {
				for(byte y2 = waterSurfaceMap[x, z]; y2 > y; y2--) {
					world.SetBlock(x, y2, z, waterBlock);
				}
			}
			if(biomePostProcessor != null) {
				biomePostProcessor.ProcessSurface(world, x, y, z);
			}
		}

		private void MakeLayer(MCUtils.World world, int x, int y, int z, string[] blocks) {
			for(int i = 0; i < blocks.Length; i++) {
				if(!string.IsNullOrWhiteSpace(blocks[i]) && !world.IsAir(x, y, z)) {
					world.SetBlock(x, y - i, z, blocks[i]);
				}
			}
		}
	}
}