
using ASCReader;
using ASCReader.Export;
using ASCReaderMC.MinecraftTerrainPostProcessors;
using ASCReaderMC.PostProcessors;
using Ionic.Zlib;
using MCUtils;
using System;
using System.Collections.Generic;
using System.IO;
using static MCUtils.NBTContent;

namespace ASCReaderMC {
	public class MCWorldExporter : IExporter {

		public static readonly string defaultBlock = "minecraft:stone";

		public World world;
		public byte[,] heightmap;

		public int[,,] finalBiomeData;

		public MinecraftTerrainPostProcessor[] postProcessors;

		public MCWorldExporter(float[,] hmap) {
			heightmap = new byte[512, 512];
			for(int x = 0; x < Math.Min(512, hmap.GetLength(0)); x++) {
				for(int z = 0; z < Math.Min(512, hmap.GetLength(1)); z++) {
					heightmap[x, z] = (byte)Math.Round(hmap[x, z]);
				}
			}
		}

		public MCWorldExporter(string importPath, float[,] hmap, bool useDefaultPostProcessors, bool useSplatmaps) : this(hmap) {
			List<MinecraftTerrainPostProcessor> pps = new List<MinecraftTerrainPostProcessor>();
			if(useSplatmaps) {
				pps.Add(new SplatmappedSurfacePostProcessor(importPath, 255, CurrentExportJobInfo.exportNumX, CurrentExportJobInfo.exportNumZ));
			}
			if(useDefaultPostProcessors) {
				if(!useSplatmaps) {
					pps.Add(new NaturalTerrainPostProcessor(true));
					pps.Add(new VegetationPostProcessor(0.1f, 0.01f));
				}
				pps.AddRange(new MinecraftTerrainPostProcessor[] {
					new BedrockPostProcessor(),
					new CavesPostProcessor(),
					new OrePostProcessor(2),
				});
			}
			postProcessors = pps.ToArray();
		}

		private void CreateWorld() {
			world = new World(512, 512);
			MakeBaseTerrain();
			DecorateTerrain();
			MakeBiomeArray();
		}

		private void MakeBaseTerrain() {
			for(int x = 0; x < 512; x++) {
				for(int z = 0; z < 512; z++) {
					for(int y = 0; y <= heightmap[x, z]; y++) {
						world.SetDefaultBlock(x, y, z);
					}
				}
				if((x + 1) % 8 == 0) Program.WriteProgress("Generating base terrain", (x + 1) / 512f);
			}
		}

		private void DecorateTerrain() {
			foreach(var post in postProcessors) {
				//Iterate the postprocessors over every block
				for(int x = 0; x < 512; x++) {
					for(int z = 0; z < 512; z++) {
						for(int y = 0; y <= heightmap[x, z]; y++) {
							post.ProcessBlock(world, x, y, z);
						}
					}
					if((x + 1) % 8 == 0) Program.WriteProgress("Decorating terrain", (x + 1) / 512f);
				}
				//Iterate the postprocessors over every surface block
				for(int x = 0; x < 512; x++) {
					for(int z = 0; z < 512; z++) {
						post.ProcessSurface(world, x, heightmap[x, z], z);
					}
					if((x + 1) % 8 == 0) Program.WriteProgress("Decorating surface", (x + 1) / 512f);
				}
			}
			foreach(var post in postProcessors) {
				post.OnFinish(world);
			}
		}

		private void MakeBiomeArray() {
			foreach(Region r in world.regions.Values) r.MakeBiomeArray();
		}

		public void WriteFile(FileStream stream, FileFormat filetype) {
			CreateWorld();
			world.WriteRegionFiles(stream, 0, 0);
		}
	}
}