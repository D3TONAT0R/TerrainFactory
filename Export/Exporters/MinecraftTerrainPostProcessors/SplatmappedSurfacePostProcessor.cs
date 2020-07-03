using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ASCReader.Export.Exporters;

public class SplatmappedSurfacePostProcessor : IMinecraftTerrainPostProcessor
{
	public Dictionary<string, byte[,]> maps = new Dictionary<string, byte[,]>();
	public Dictionary<byte, string[]> layers = new Dictionary<byte, string[]>();

	public SplatmappedSurfacePostProcessor(string importedFilePath, int ditherLimit) {
		var desc = new SplatmapDescriptorReader(importedFilePath);
		foreach(string k in desc.maps.Keys) {
			string path = Path.GetDirectoryName(importedFilePath);
			List<SplatmapMapping> mappings = new List<SplatmapMapping>();
			foreach(var sm in desc.layers.Keys) {
				if(sm.mapName == k) mappings.Add(sm);
			}
			mappings.Add(new SplatmapMapping(k, Color.Black, 0));
			maps.Add(k, SplatmapImporter.GetFixedSplatmap(path+"\\"+desc.maps[k], mappings.ToArray(), ditherLimit));
		}
		foreach(var sm in desc.layers.Keys) {
			layers.Add((byte)sm.value, desc.layers[sm].Split(','));
		}
	}

	public void ProcessBlock(MinecraftRegionExporter region, int x, int y, int z) {
	}

	public void ProcessSurface(MinecraftRegionExporter region, int x, int y, int z) {
		foreach(string map in maps.Keys) {
			byte mappedValue = maps[map][x,z];
			if(mappedValue > 0) {
				MakeLayer(region, x, y, z, layers[mappedValue]);
			}
		}
	}

	private void MakeLayer(MinecraftRegionExporter region, int x, int y, int z, string[] blocks) {
		for(int i = 0; i < blocks.Length; i++) {
			if(!string.IsNullOrWhiteSpace(blocks[i]) && !region.IsAir(x,y,z)) {
				region.SetBlock(x, y-i, z, blocks[i]);
			}
		}
	}
}