using MCUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace HMConMC.PostProcessors {
	public class Schematic {

		public byte[,,] structure;

		public int structureSizeX {
			get { return structure.GetLength(0); }
		}
		public int structureSizeY {
			get { return structure.GetLength(1); }
		}
		public int structureSizeZ {
			get { return structure.GetLength(2); }
		}

		public Dictionary<byte, (string block, float prob)> blocks = new Dictionary<byte, (string block, float prob)>();

		public string trunkBlock;
		public byte trunkHeightMin;
		public byte trunkHeightMax;

		public Schematic(string filepath)
		{
			string[] lns = File.ReadAllLines(filepath);
			bool inArray = false;
			int dimX = 0;
			int dimY = 0;
			int dimZ = 0;
			byte[,,] arr = null;
			int y = 0;
			int z = 0;
			Dictionary<byte, (string block, float prob)> blocks = new Dictionary<byte, (string block, float prob)>();
			foreach (string ln in lns)
			{
				if (!inArray)
				{
					if (ln.StartsWith("ARRAY:"))
					{
						inArray = true;
						arr = new byte[dimX, dimY, dimZ];
						continue;
					}
					if (ln.StartsWith("trunk=")) trunkBlock = ln.Split('=')[1];
					if (ln.StartsWith("trunk_min=")) trunkHeightMin = byte.Parse(ln.Split('=')[1]);
					if (ln.StartsWith("trunk_max=")) trunkHeightMax = byte.Parse(ln.Split('=')[1]);
					if (ln.StartsWith("dim_x=")) dimX = int.Parse(ln.Split('=')[1]);
					if (ln.StartsWith("dim_y=")) dimY = int.Parse(ln.Split('=')[1]);
					if (ln.StartsWith("dim_z=")) dimZ = int.Parse(ln.Split('=')[1]);
					if (ln.StartsWith("block "))
					{
						string[] split = ln.Split(' ')[1].Split('=');
						string[] bsplit = split[1].Split(',');
						blocks.Add(byte.Parse(split[0]), (bsplit[0], float.Parse(bsplit[1])));
					}
				}
				else
				{
					if (ln.StartsWith(";"))
					{
						z = 0;
						y++;
					}
					else
					{
						string[] lnsplit = ln.Split(',');
						for (int i = 0; i < dimX; i++)
						{
							arr[i, y, z] = byte.Parse(lnsplit[i]);
						}
						z++;
					}
				}
			}
			structure = arr;
		}

		public bool Build(MCUtils.World world, int x, int y, int z, Random r) {
			byte h = (byte)r.Next(trunkHeightMin, trunkHeightMax);
			if(IsObstructed(world, x, y + h, z)) {
				return false;
			}
			if(!string.IsNullOrWhiteSpace(trunkBlock) && trunkHeightMax > 0) {
				for(int i = 0; i < h; i++) {
					world.SetBlock(x, y + i, z, trunkBlock);
				}
			}
			int xm = x - (int)Math.Floor((float)structureSizeX / 2);
			int zm = z - (int)Math.Floor((float)structureSizeZ / 2);
			for(int x1 = 0; x1 < structureSizeX; x1++) {
				for(int y1 = 0; y1 < structureSizeY; y1++) {
					for(int z1 = 0; z1 < structureSizeZ; z1++) {
						var d = structure[x1, y1, z1];
						if(d == 0) continue;
						var b = blocks[d];
						if(r.NextDouble() < b.prob) {
							world.SetBlock(xm + x1, y + h + y1, zm + z1, b.block);
						}
					}
				}
			}
			return true;
		}

		private bool IsObstructed(MCUtils.World world, int lx, int ly, int lz) {
			int x1 = lx - (int)Math.Floor(structureSizeX / 2f);
			int x2 = lx + (int)Math.Ceiling(structureSizeX / 2f);
			int y1 = ly;
			int y2 = ly + structureSizeY;
			int z1 = lz - (int)Math.Floor(structureSizeZ / 2f);
			int z2 = lz + (int)Math.Ceiling(structureSizeZ / 2f);
			int sy = 0;
			for(int y = y1; y < y2; y++) {
				int sz = 0;
				for(int z = z1; z < z2; z++) {
					int sx = 0;
					for(int x = x1; x < x2; x++) {
						if(structure[sx, sy, sz] == 0) continue; //Do not check this block if the result is nothing anyway
						if(!world.IsAir(x, y, z) || !world.IsWithinBoundaries(x, y, z)) return true;
						sx++;
					}
					sz++;
				}
				sy++;
			}
			return false;
		}

		private string GetRandomColor(Random r) {
			string[] colors = new string[] {
				"white", "light_gray", "gray", "black",
				"red", "pink", "orange", "yellow",
				"lime", "green", "cyan", "light_blue",
				"blue", "magenta", "purple", "brown"
			};
			return colors[r.Next(colors.Length)];
		}
	}
}
