using System;
using System.Collections.Generic;
using static MinecraftNBTContent;

public class MinecraftChunkData {

	public class BlockState {
		public string block;
		public CompoundContainer properties = new CompoundContainer();

		public BlockState(string name) {
			if(name.StartsWith("minecraft:")) {
				block = name;
			} else {
				block = "minecraft:"+name;
			}
		}
	}

	public ushort[][,,]blocks = new ushort[16][,,];
	public List<BlockState>[] palettes = new List<BlockState>[16];
	public byte[,] biomes = new byte[16, 16];
	public int[,,] finalBiomeArray;

	public MinecraftChunkData(string defaultBlock) {
		for(int i = 0; i < 16; i++) {
			palettes[i] = new List<BlockState>();
			palettes[i].Add(new BlockState("air"));
			palettes[i].Add(new BlockState(defaultBlock));
		}
	}

	public MinecraftChunkData(MinecraftNBTContent chunk) {
		for(int i = 0; i < 16; i++) {
			palettes[i] = new List<BlockState>();
		}
		ReadFromNBT(chunk.contents.GetAsList("Sections"), chunk.dataVersion < 2504);
		for(int x = 0; x < 16; x++) {
			for(int y = 0; y < 16; y++) {
				biomes[x,y] = 1; //Defaults to plains biome
			}
		}
	}

	public ushort GetPaletteIndex(BlockState state, int palette) {
		for(short i = 0; i < palettes[palette].Count; i++) {
			if(palettes[palette][i].block == state.block && palettes[palette][i].properties.HasSameContent(state.properties)) return (ushort)i;
		}
		return 9999;
	}

	public void SetBlockAt(int x, int y, int z, BlockState block) {
		int section = (int)Math.Floor(y/16f);
		ushort index = GetPaletteIndex(block, section);
		if(index == 9999) {
			palettes[section].Add(block);
			index = (ushort)(palettes[section].Count-1);
		}
		if(blocks[section] == null) blocks[section] = new ushort[16,16,16];
		blocks[section][x,y%16,z] = index;
	}

	public void SetDefaultBlockAt(int x, int y, int z) {
		int section = (int)Math.Floor(y/16f);
		if(blocks[section] == null) blocks[section] = new ushort[16,16,16];
		blocks[section][x,y%16,z] = 1; //1 is always the default block in a region generated from scratch
	}

	public BlockState GetBlockAt(int x, int y, int z) {
		int section = (int)Math.Floor(y/16f);
		if(blocks[section] == null) return new BlockState("minecraft:air");
		return palettes[section][blocks[section][x,y%16,z]];
	}

	public void SetBiomeAt(int x, int z, byte biomeID) {
		biomes[x, z] = biomeID;
	}

	public void ReadFromNBT(ListContainer sectionsList, bool isVersion_prior_1_16) {
		foreach(var o in sectionsList.cont) {
			var compound = (CompoundContainer)o;
			if(!compound.Contains("Y") || (byte)compound.Get("Y") > 7 || !compound.Contains("Palette")) continue;
			byte secY = (byte)compound.Get("Y");
			var palette = palettes[secY];
			foreach(var cont in compound.GetAsList("Palette").cont) {
				CompoundContainer block = (CompoundContainer)cont;
				BlockState bs = new BlockState((string)block.Get("Name"));
				if(block.Contains("Properties")) bs.properties = block.GetAsCompound("Properties");
				palette.Add(bs);
			}
			//1.15 uses the full range of bits where 1.16 doesn't use the last bits if they can't contain a block index
			int indexLength = Math.Max(4, (int)Math.Log(palette.Count-1, 2.0) + 1); 
			long[] longs = (long[])compound.Get("BlockStates");
			string bits = "";
			for(int i = 0; i < longs.Length; i++) {
				string newBits = "";
				byte[] bytes = BitConverter.GetBytes(longs[i]);
				for(int j = 0; j < 8; j++) {
					newBits += ByteToBinary(bytes[j], true);
				}
				if(isVersion_prior_1_16) {
					bits += newBits;
				} else {
					bits += newBits.Substring(0, (int)Math.Floor(newBits.Length/(double)indexLength)*indexLength);
				}
			}
			blocks[secY] = new ushort[16,16,16];
			for(int y = 0; y < 16; y++) {
				for(int z = 0; z < 16; z++) {
					for(int x = 0; x < 16; x++) {
						blocks[secY][x,y,z] = BitsToValue(bits, y*256+z*16+x, indexLength);
					}
				}
			}
		}
	}

	public void MakeBiomeArray() {
		finalBiomeArray = new int[4,64,4];
		for(int x = 0; x < 4; x++) {
			for(int z = 0; z < 4; z++) {
				int biome = GetPredominantBiomeIn4x4Area(x,z);
				for(int y = 0; y < 64; y++) finalBiomeArray[x,y,z] = biome;
			}
		}
	}

	private int GetPredominantBiomeIn4x4Area(int x, int z) {
		Dictionary<byte,byte> occurences = new Dictionary<byte, byte>();
		for(int x1 = 0; x1 < 4; x1++) {
			for(int z1 = 0; z1 < 4; z1++) {
				var b = biomes[x*4+x1,z*4+z1];
				if(!occurences.ContainsKey(b)) {
					occurences.Add(b, 0);
				}
			}
		}
		int predominantBiome = 0;
		int predominantCells = 0;
		foreach(var k in occurences.Keys) {
			if(occurences[k] > predominantCells) {
				predominantCells = occurences[k];
				predominantBiome = k;
			}
		}
		return predominantBiome;
	}

	private string ByteToBinary(byte b, bool bigendian) {
		string s = Convert.ToString((int)b, 2);
		s = s.PadLeft(8, '0');
		if(bigendian) s = ReverseString(s);
		return s;
	}

	private ushort BitsToValue(string bitString, int index, int length) {
		string bits = ReverseString(bitString.Substring(index * length, length));
		bits = bits.PadLeft(16, '0');
		bool[] bitArr = new bool[16];
		for(int i = 0; i < 16; i++) {
			bitArr[i] = bits[i] == '1';
		}
		return Convert.ToUInt16(bits, 2);
	}

	private string ReverseString(string input) {
		char[] chrs = input.ToCharArray();
		Array.Reverse(chrs);
		return new string(chrs);
	}

	public void WriteToNBT(CompoundContainer level, bool use_1_16_Format) {
		ListContainer sectionsList = level.GetAsList("Sections");
		for(byte secY = 0; secY < 16; secY++) {
			if(IsSectionEmpty(secY)) continue;
			var comp = GetSection(sectionsList, secY);
			if(comp == null) {
				comp = new CompoundContainer();
				comp.Add("Y", secY);
				ListContainer palette = new ListContainer(NBTTag.TAG_Compound);
				foreach(var bs in palettes[secY]) {
					CompoundContainer paletteBlock = new CompoundContainer();
					paletteBlock.Add("Name", bs.block);
					if(bs.properties != null) paletteBlock.Add("Properties", bs.properties);
					palette.Add("", paletteBlock);
				}
				comp.Add("Palette", palette);
				//Encode block indexes to bits and longs, oof
				int indexLength = Math.Max(4, (int)Math.Log(palettes[secY].Count-1, 2.0) + 1); 
				long[] longs = new long[(int)Math.Ceiling(4096*indexLength/64.0)];
				string[] longsBinary = new string[longs.Length];
				Array.Fill(longsBinary, "");
				int i = 0;
				for(int y = 0; y < 16; y++) {
					for(int z = 0; z < 16; z++) {
						for(int x = 0; x < 16; x++) {
							string bin = NumToBits(blocks[secY][x,y,z], indexLength);
							bin = ReverseString(bin);
							if(use_1_16_Format) {
								if(longsBinary[i].Length + indexLength > 64) {
									//The full value doesn't fit, start on the next long
									i++;
									longsBinary[i] += bin;
								} else {
									for(int j = 0; j < indexLength; j++) {
										if(longsBinary[i].Length >= 64) i++;
										longsBinary[i] += bin[j];
									}
								}
							}
						}
					}
				}
				for(int j = 0; j < longs.Length; j++) {
					string s = longsBinary[j];
					s = s.PadRight(64, '0');
					s = ReverseString(s);
					longs[j] = Convert.ToInt64(s, 2);
				}
				comp.Add("BlockStates", longs); 
				sectionsList.Add("", comp);
			}
		}
		//Make the biomes
		List<int> biomes = new List<int>();
		for(int y = 0; y < 64; y++) {
			for(int x = 0; x < 4; x++) {
				for(int z = 0; z < 4; z++) {
					biomes.Add(finalBiomeArray[x,y,z]);
				}
			}
		}
		level.Add("Biomes", biomes.ToArray());
	}

	private bool IsSectionEmpty(int secY) {
		var arr = blocks[secY];
		if(arr == null) return true;
		bool allSame = true;
		var i = arr[0,0,0];
		foreach(var j in arr) {
			allSame &= i == j;
		}
		if(allSame && palettes[secY][i].block == "minecraft:air") return true;
		return false;
	}

	private long BitsToLong(string bits) {
		bits = bits.PadLeft(64, '0');
		return Convert.ToInt64(bits, 2);
	}

	private string NumToBits(ushort num, int length) {
		string s = Convert.ToString(num, 2);
		if(s.Length > length) {
			throw new IndexOutOfRangeException("The number "+num+" does not fit in a binary string with length "+length);
		}
		return s.PadLeft(length, '0');
	}

	private CompoundContainer GetSection(ListContainer sectionsList, byte y) {
		foreach(var o in sectionsList.cont) {
			var compound = (CompoundContainer)o;
			if(!compound.Contains("Y") || (byte)compound.Get("Y") > 15 || !compound.Contains("Palette")) continue;
			if((byte)compound.Get("Y") == y) return compound;
		}
		return null;
	}
}