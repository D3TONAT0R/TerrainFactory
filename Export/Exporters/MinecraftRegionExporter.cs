
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using static MinecraftChunkData;
using static MinecraftNBTContent;

namespace ASCReader.Export.Exporters {
	public class MinecraftRegionExporter : IExporter {

		public static readonly string defaultBlock = "minecraft:stone";

		public byte[,] heightmap;
		public MinecraftChunkData[,] chunks;

		public IMinecraftTerrainPostProcessor[] postProcessors;

		public MinecraftRegionExporter(float[,] hmap) {
			chunks = new MinecraftChunkData[32, 32];
			for(int x = 0; x < 32; x++) {
				for(int z = 0; z < 32; z++) {
					chunks[x, z] = new MinecraftChunkData();
				}
			}
			heightmap = new byte[512, 512];
			for(int x = 0; x < Math.Min(512, hmap.GetLength(0)); x++) {
				for(int z = 0; z < Math.Min(512, hmap.GetLength(1)); z++) {
					heightmap[x, z] = (byte)Math.Round(hmap[x, z]);
				}
			}
		}

		public MinecraftRegionExporter(string importPath, float[,] hmap, bool useDefaultPostProcessors, bool useSplatmaps) : this(hmap) {
			List<IMinecraftTerrainPostProcessor> pps = new List<IMinecraftTerrainPostProcessor>();
			if(useSplatmaps) {
				pps.Add(new SplatmappedSurfacePostProcessor(importPath, 255, CurrentExportJobInfo.exportNumX, CurrentExportJobInfo.exportNumZ));
			}
			if(useDefaultPostProcessors) {
				if(!useSplatmaps) {
					pps.Add(new NaturalTerrainPostProcessor(true));
					pps.Add(new VegetationPostProcessor(0.1f, 0.01f));
				}
				pps.AddRange(new IMinecraftTerrainPostProcessor[] {
					new OrePostProcessor(2),
					new RandomTorchPostProcessor(0.001f)
				});
			}
			postProcessors = pps.ToArray();
		}

		private void CreateWorld() {
			MakeBaseTerrain();
			MakeBiomes();
			DecorateTerrain();
		}

		private void MakeBaseTerrain() {
			for(int x = 0; x < 512; x++) {
				for(int z = 0; z < 512; z++) {
					for(int y = 0; y <= heightmap[x, z]; y++) {
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
					for(int y = 0; y <= heightmap[x, z]; y++) {
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
						post.ProcessSurface(this, x, heightmap[x, z], z);
					}
				}
			}
			foreach(var post in postProcessors) {
				post.OnFinish(this);
			}
		}

		public bool IsAir(int x, int y, int z) {
			var b = GetBlock(x, y, z);
			return b == null || b == "minecraft:air";
		}

		public bool IsWithinBoundaries(int x, int y, int z) {
			if(x < 0 || x >= 512 || y < 0 || y >= 256 || z < 0 || z >= 512) return false;
			else return true;
		}

		public bool IsDefaultBlock(int x, int y, int z) {
			var b = GetBlock(x, y, z);
			if(b == null) return false;
			return b == defaultBlock;
		}

		public string GetBlock(int x, int y, int z) {
			int chunkX = (int)Math.Floor(x / 16.0);
			int chunkZ = (int)Math.Floor(z / 16.0);
			if(x < 0 || x >= 512 || y < 0 || y >= 256 || z < 0 || z >= 512) return null;
			if(chunks[chunkX, chunkZ] != null) {
				var b = chunks[chunkX, chunkZ].GetBlockAt(x % 16, y, z % 16);
				return b != null ? b.block : "minecraft:air";
			} else {
				return null;
			}
		}

		public bool SetBlock(int x, int y, int z, string block) {
			int chunkX = (int)Math.Floor(x / 16.0);
			int chunkZ = (int)Math.Floor(z / 16.0);
			if(chunkX < 0 || chunkX > 31 || chunkZ < 0 || chunkZ > 31) return false;
			if(chunks[chunkX, chunkZ] != null) {
				chunks[chunkX, chunkZ].SetBlockAt(x % 16, y, z % 16, new BlockState(block));
				return true;
			} else {
				return false;
			}
		}

		public void SetBiome(int x, int z, byte biome) {
			int chunkX = (int)Math.Floor(x / 16.0);
			int chunkZ = (int)Math.Floor(z / 16.0);
			if(chunkX < 0 || chunkX > 31 || chunkZ < 0 || chunkZ > 31) return;
			if(chunks[chunkX, chunkZ] != null) {
				for(int y = 0; y < 256; y++) {
					chunks[chunkX, chunkZ].SetBiomeAt(x % 16, y, z % 16, biome);
				}
			}
		}

		public void WriteFile(FileStream stream, FileFormat filetype) {
			CreateWorld();
			int[] locations = new int[1024];
			byte[] sizes = new byte[1024];
			for(int i = 0; i < 8192; i++) {
				stream.WriteByte(0);
			}
			for(int z = 0; z < 32; z++) {
				for(int x = 0; x < 32; x++) {
					int i = z * 32 + x;
					locations[i] = (int)(stream.Position / 4096);
					var chunkData = MakeCompoundForChunk(chunks[x, z], 32 * (CurrentExportJobInfo.mcaGlobalPosX+CurrentExportJobInfo.exportNumX) + x, 32 * (CurrentExportJobInfo.mcaGlobalPosZ + CurrentExportJobInfo.exportNumZ) + z);
					List<byte> bytes = new List<byte>();
					chunkData.WriteToBytes(bytes);
					byte[] compressed = ZlibStream.CompressBuffer(bytes.ToArray());
					stream.Write(Reverse(BitConverter.GetBytes(compressed.Length)));
					stream.WriteByte(2);
					stream.Write(compressed);
					while(stream.Length % 4096 != 0) stream.WriteByte(0); //Padding
					sizes[i] = (byte)((int)(stream.Position / 4096) - locations[i]);
				}
			}
			stream.Position = 0;
			for(int i = 0; i < 1024; i++) {
				byte[] offsetBytes = Reverse(BitConverter.GetBytes(locations[i]));
				stream.WriteByte(offsetBytes[1]);
				stream.WriteByte(offsetBytes[2]);
				stream.WriteByte(offsetBytes[3]);
				stream.WriteByte(sizes[i]);
			}
		}

		private MinecraftNBTContent MakeCompoundForChunk(MinecraftChunkData chunk, int chunkX, int chunkZ) {
			var nbt = new MinecraftNBTContent();
			nbt.dataVersion = 2504; //1.16 version ID
			nbt.contents.Add("xPos", chunkX);
			nbt.contents.Add("zPos", chunkZ);
			nbt.contents.Add("Status", "light");
			ListContainer sections = new ListContainer(NBTTag.TAG_Compound);
			nbt.contents.Add("Sections", sections);
			chunk.WriteToNBT(sections, true);
			int[] biomes = new int[1024];
			Array.Fill(biomes, 1);
			nbt.contents.Add("Biomes", biomes);
			//Add the rest of the tags and leave them empty
			nbt.contents.Add("Heightmaps", new CompoundContainer());
			nbt.contents.Add("Structures", new CompoundContainer());
			nbt.contents.Add("Entities", new ListContainer(NBTTag.TAG_End));
			nbt.contents.Add("LiquidTicks", new ListContainer(NBTTag.TAG_End));
			ListContainer postprocessing = new ListContainer(NBTTag.TAG_List);
			for(int i = 0; i < 16; i++) postprocessing.Add("", new ListContainer(NBTTag.TAG_End));
			nbt.contents.Add("PostProcessing", postprocessing);
			nbt.contents.Add("TileEntities", new ListContainer(NBTTag.TAG_End));
			nbt.contents.Add("TileTicks", new ListContainer(NBTTag.TAG_End));
			nbt.contents.Add("InhabitedTime", 0L);
			nbt.contents.Add("LastUpdate", 0L);
			//To do: biomes
			return nbt;
		}

		private NBTTag GetTag(object o) {
			if(NBTTagDictionary.ContainsKey(o.GetType())) {
				return NBTTagDictionary[o.GetType()];
			} else {
				return NBTTag.UNSPECIFIED;
			}
		}

		byte[] Reverse(byte[] input) {
			if(BitConverter.IsLittleEndian) Array.Reverse(input);
			return input;
		}
	}
}