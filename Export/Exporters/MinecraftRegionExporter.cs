
using System;
using System.IO;
using static MinecraftChunkData;

namespace ASCReader.Export.Exporters {
	public class MinecraftRegionExporter : IExporter {
		
		public static readonly string defaultBlock = "minecraft:stone";

		public byte[,] heightmap;
		public MinecraftChunkData[,] chunks;

		public IMinecraftTerrainPostProcessor[] postProcessors;

		public MinecraftRegionExporter(float[,] hmap, params IMinecraftTerrainPostProcessor[] postProcessors) {
			this.postProcessors = postProcessors;
			chunks = new MinecraftChunkData[32,32];
			for(int x = 0; x < 32; x++) {
				for(int z = 0; z < 32; z++) {
					chunks[x,z] = new MinecraftChunkData();
				}
			}
		}

		public MinecraftRegionExporter(float[,] hmap, bool useDefaultPostProcessors) : this(hmap) {
			postProcessors = new IMinecraftTerrainPostProcessor[] {
				new NaturalTerrainPostProcessor(true),
				new VegetationPostProcessor(0.25f, 0.1f),
				new RandomTorchPostProcessor(0.05f)
			};
		}

		private void CreateWorld() {
			MakeBaseTerrain();
			MakeBiomes();
			DecorateTerrain();
		}

		private void MakeBaseTerrain() {
			for(int x = 0; x < 512; x++) {
				for(int z = 0; z < 512; z++) {
					for(int y = 0; y <= heightmap[x,z]; y++) {
						SetBlock(x, y, z, defaultBlock);
					}
				}
			}
		}

		private void MakeBiomes() {
			//To do, but how?
		}

		private void DecorateTerrain() {
			//Iterate the postprocessors over every block
			for(int x = 0; x < 512; x++) {
				for(int z = 0; z < 512; z++) {
					for(int y = 0; y <= heightmap[x,z]; y++) {
						foreach(var post in postProcessors) {
							post.ProcessBlock(this, x, y, z);
						}
					}
				}
			}
			//Iterate the postprocessors over every surface block
			for(int x = 0; x < 512; x++) {
				for(int z = 0; z < 512; z++) {
					foreach(var post in postProcessors) {
						post.ProcessBlock(this, x, heightmap[x,z], z);
					}
				}
			}
		}

		public bool IsAir(int x, int y, int z) {
			var b = GetBlock(x,y,z);
			return b == null || b == "minecraft:air"; 
		}

		public bool IsDefaultBlock(int x, int y, int z) {
			var b = GetBlock(x,y,z);
			if(b == null) return false;
			return b == defaultBlock; 
		}

		public string GetBlock(int x, int y, int z) {
			int chunkX = (int)Math.Floor(x/16.0);
			int chunkZ = (int)Math.Floor(z/16.0);
			if(chunks[chunkX,chunkZ] != null) {
				var b = chunks[chunkX,chunkZ].GetBlockAt(x%16,y,z%16);
				return b != null ? b.block : "minecraft:air";
			} else {
				return null;
			}
		}

		public bool SetBlock(int x, int y, int z, string block) {
			int chunkX = (int)Math.Floor(x/16.0);
			int chunkZ = (int)Math.Floor(z/16.0);
			if(chunks[chunkX,chunkZ] != null) {
				chunks[chunkX,chunkZ].SetBlockAt(x%16,y,z%16,new BlockState(block));
				return true;
			} else {
				return false;
			}
		}

		public void WriteFile(FileStream stream, FileFormat filetype) {
			
		}
	}
}