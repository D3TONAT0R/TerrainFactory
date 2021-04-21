using HMCon;
using HMCon.Import;
using HMConImage;
using MCUtils;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace HMConMC.PostProcessors {
	public class SplatmappedSurfacePostProcessor : MinecraftTerrainPostProcessor {
		public Dictionary<string, byte[,]> maps = new Dictionary<string, byte[,]>();
		public Dictionary<byte, string[]> layers = new Dictionary<byte, string[]>();

		public override Priority OrderPriority => Priority.BeforeDefault;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public SplatmappedSurfacePostProcessor(MCWorldExporter exporter, string importedFilePath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ) {
			var desc = new SplatmapDescriptorReader(importedFilePath + ".splat", true);
			string root = Path.GetDirectoryName(importedFilePath);
			foreach(string k in desc.maps.Keys) {
				List<SplatmapMapping> mappings = new List<SplatmapMapping>();
				foreach(var sm in desc.layers.Keys) {
					if(sm.mapName == k) mappings.Add(sm);
				}
				mappings.Add(new SplatmapMapping(k, Color.Black, 0));
				maps.Add(k, SplatmapImporter.GetFixedSplatmap(root + "\\" + desc.maps[k], mappings.ToArray(), ditherLimit, offsetX, offsetZ, sizeX, sizeZ));
			}
			foreach(var sm in desc.layers.Keys) {
				layers.Add((byte)sm.value, desc.layers[sm].Split(','));
			}
			Program.WriteLine("Splatmapping enabled");
			if(!string.IsNullOrWhiteSpace(desc.watermapPath)) {
				exporter.postProcessors.Add(new WatermapPostProcessor(Path.Combine(root, desc.watermapPath), offsetX, offsetZ, sizeX, sizeZ, desc.waterLevel, desc.waterBlock));
			}
			if(!string.IsNullOrWhiteSpace(desc.biomeMapperPath)) {
				exporter.postProcessors.Add(new SplatmappedBiomePostProcessor(Path.Combine(root, desc.biomeMapperPath), 0, offsetX, offsetZ, sizeX, sizeZ));
			}
		}

		public override void ProcessSurface(World world, int x, int y, int z) {
			foreach(string map in maps.Keys) {
				byte mappedValue = maps[map][x, z];
				if(mappedValue > 0) {
					MakeLayer(world, x, y, z, layers[mappedValue]);
				}
			}
		}

		private void MakeLayer(World world, int x, int y, int z, string[] blocks) {
			for(int i = 0; i < blocks.Length; i++) {
				if(!string.IsNullOrWhiteSpace(blocks[i]) && !world.IsAir(x, y, z)) {
					world.SetBlock(x, y - i, z, blocks[i]);
				}
			}
		}
	}
}