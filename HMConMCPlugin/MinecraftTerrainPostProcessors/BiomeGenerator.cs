using ASCReader;
using ASCReader.Export;
using MCUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace ASCReaderMC.PostProcessors {
	public class BiomeGenerator {

		Random rand = new Random();

		public byte biomeID;

		public Dictionary<string, float> mainStructures;
		public Dictionary<string, float> decorStructures;

		Dictionary<string, Structure> structureDatas = new Dictionary<string, Structure>();

		public BiomeGenerator(byte biome) {
			biomeID = biome;
			mainStructures = new Dictionary<string, float>();
			decorStructures = new Dictionary<string, float>();
		}

		public void RunGenerator(MCUtils.World world, int x, int y, int z) {
			if(world.GetBlock(x, y, z) != "minecraft:grass_block") return;
			foreach(var k in mainStructures.Keys) {
				if(rand.NextDouble() < mainStructures[k] && BuildStructure(k, world, x, y + 1, z)) return;
			}
			foreach(var k in decorStructures.Keys) {
				if(rand.NextDouble() < decorStructures[k] && BuildSingleBlock(k, world, x, y + 1, z)) return;
			}
		}

		private bool BuildStructure(string s, MCUtils.World world, int x, int y, int z) {
			if(!structureDatas.ContainsKey(s)) {
				RegisterStructure(s);
			}
			return structureDatas[s].Generate(world, x, y, z, rand);
		}

		private bool BuildSingleBlock(string s, MCUtils.World world, int x, int y, int z) {
			world.SetBlock(x, y, z, s);
			return true;
		}

		private void RegisterStructure(string s) {
			string f = Path.GetDirectoryName(CurrentExportJobInfo.importedFilePath) + "\\" + s;
			try {
				string[] lns = File.ReadAllLines(f);
				bool inArray = false;
				int dimX = 0;
				int dimY = 0;
				int dimZ = 0;
				byte[,,] arr = null;
				int y = 0;
				int z = 0;
				Structure structure = new Structure();
				Dictionary<byte, (string block, float prob)> blocks = new Dictionary<byte, (string block, float prob)>();
				foreach(string ln in lns) {
					if(!inArray) {
						if(ln.StartsWith("ARRAY:")) {
							inArray = true;
							arr = new byte[dimX, dimY, dimZ];
							continue;
						}
						if(ln.StartsWith("trunk=")) structure.trunkBlock = ln.Split('=')[1];
						if(ln.StartsWith("trunk_min=")) structure.trunkHeightMin = byte.Parse(ln.Split('=')[1]);
						if(ln.StartsWith("trunk_max=")) structure.trunkHeightMax = byte.Parse(ln.Split('=')[1]);
						if(ln.StartsWith("dim_x=")) dimX = int.Parse(ln.Split('=')[1]);
						if(ln.StartsWith("dim_y=")) dimY = int.Parse(ln.Split('=')[1]);
						if(ln.StartsWith("dim_z=")) dimZ = int.Parse(ln.Split('=')[1]);
						if(ln.StartsWith("block ")) {
							string[] split = ln.Split(' ')[1].Split('=');
							string[] bsplit = split[1].Split(',');
							structure.blocks.Add(byte.Parse(split[0]), (bsplit[0], float.Parse(bsplit[1])));
						}
					} else {
						if(ln.StartsWith(";")) {
							z = 0;
							y++;
						} else {
							string[] lnsplit = ln.Split(",");
							for(int i = 0; i < dimX; i++) {
								arr[i, y, z] = byte.Parse(lnsplit[i]);
							}
							z++;
						}
					}
				}
				structure.structure = arr;
				structureDatas.Add(s, structure);
			} catch {
				Program.WriteWarning("Failed to import structure '" + s + "'");
			}
		}
	}
}
