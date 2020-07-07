using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ASCReader.Export.Exporters.MinecraftTerrainPostProcessors {
	public class Structure {

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

		public bool Generate(MinecraftRegionExporter region, int x, int y, int z, Random r) {
			byte h = (byte)r.Next(trunkHeightMin, trunkHeightMax);
			if(IsObstructed(region, x, y + h, z)) {
				return false;
			}
			if(!string.IsNullOrWhiteSpace(trunkBlock) && trunkHeightMax > 0) {
				for(int i = 0; i < h; i++) {
					region.SetBlock(x, y + i, z, trunkBlock);
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
							region.SetBlock(xm + x1, y + h + y1, zm + z1, b.block);
						}
					}
				}
			}
			return true;
		}

		private bool IsObstructed(MinecraftRegionExporter region, int lx, int ly, int lz) {
			int x1 = lx-(int)Math.Floor(structureSizeX/2f);
			int x2 = lx+(int)Math.Ceiling(structureSizeX / 2f);
			int y1 = ly;
			int y2 = ly+ structureSizeY;
			int z1 = lz - (int)Math.Floor(structureSizeZ / 2f);
			int z2 = lz + (int)Math.Ceiling(structureSizeZ / 2f);
			for(int y = y1; y <= y2; y++) {
				for(int z = z1; z <= z2; z++) {
					for(int x = x1; x <= x2; x++) {
						if(!region.IsAir(x, y, z) || !region.IsWithinBoundaries(x, y, z)) return false;
					}
				}
			}
			return true;
		}
	}
}
