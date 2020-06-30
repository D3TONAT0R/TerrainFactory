using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using Ionic.Zlib;
using System.Collections;
using static MinecraftNBTContent;

namespace ASCReader.Import {
	public static class MinecraftRegionImporter {

		static MemoryStream stream;

		public static ASCData ImportHeightmap(string filepath) {
			string fname = Path.GetFileName(filepath);
			int regionX = int.Parse(fname.Split('.')[1]);
			int regionZ = int.Parse(fname.Split('.')[2]);
			FileStream fs = File.Open(filepath, FileMode.Open);
			stream = new MemoryStream();
			fs.CopyTo(stream);
			byte[] locationBytes = Read(0, 4096);
			byte[] timestampBytes = Read(4096, 4096);
			uint[] locations = new uint[1024];
			byte[] sizes = new byte[1024];
			for(uint i = 0; i < 1024; i++) {
				locations[i] = ReadAsInt(locationBytes, i*4, 3);
				sizes[i] = Read(i*4+3, 1)[0];
			}
			float[,] hm = new float[512, 512];
			for(int i = 0; i < 1024; i++) {
				if(locations[i] > 0 && sizes[i] > 0) {
					var nbt = new MinecraftNBTContent(GetChunkData(locations[i], sizes[i]));
					int localChunkX = (int)nbt.contents.Get("xPos") - regionX * 32;
					int localChunkZ = (int)nbt.contents.Get("zPos") - regionZ * 32;
					WriteToHeightmap(hm, nbt, localChunkX, localChunkZ);
				}
			}
			ASCData asc = new ASCData(512,512);
			asc.filename = Path.GetFileNameWithoutExtension(filepath);
			asc.data = hm;
			asc.cellsize = 1;
			asc.nodata_value = -9999;
			asc.RecalculateValues(false);
			asc.lowPoint = 0;
			asc.highPoint = 1;
			stream.Close();
			asc.isValid = true;
			Program.WriteLine("Lowest: " + asc.lowestValue);
			Program.WriteLine("Hightest: " + asc.highestValue);
			asc.lowestValue = 0;
			asc.highestValue = 255;
			fs.Close();
			return asc;
		}

		private static byte[] Read(uint start, int length) {
			byte[] buffer = new byte[length];
			stream.Position = start;
			for(int i = 0; i < length; i++) {
				int result = stream.ReadByte();
				if(result >= 0) {
					buffer[i] = (byte)result;
				} else {
					buffer[i] = 0;
			//		throw new EndOfStreamException();
				}
			}
			return buffer;
		}

		private static uint ReadAsInt(byte[] arr, uint start, int length) {
			byte[] bytes = new byte[4];
			int padding = 4 - length;
			for(int i = 0; i < length; i++) bytes[i+ padding] = arr[start+i];
			if(BitConverter.IsLittleEndian) Array.Reverse(bytes);
			return BitConverter.ToUInt32(bytes);
		}

		private static byte[] GetChunkData(uint loc, byte size) {
			loc *= 4096;
			int length = size*4096;
			byte[] compressed = Read(loc+5, length);
			return ZlibStream.UncompressBuffer(compressed);
		}

		private static void WriteToHeightmap(float[,] hm, MinecraftNBTContent nbt, int localChunkX, int localChunkZ) {
			try {
				if(nbt.contents.Contains("Heightmaps")) {
					//It's the "new" format
					long[] hmlongs = (long[])nbt.contents.GetAsCompound("Heightmaps").Get("OCEAN_FLOOR");
					string hmbits = "";
					for(int i = 0; i < 36; i++) {
						byte[] bytes = BitConverter.GetBytes(hmlongs[i]);
						//Array.Reverse(bytes);
						for(int j = 0; j < 8; j++) {
							hmbits += ByteToBinary(bytes[j], true);
						}
					}
					ushort[] hmap = new ushort[256];
					for(int i = 0; i < 256; i++) {
						hmap[i] = Read9BitValue(hmbits, i);
					}

					if(hmbits != null) {
						for(int z = 0; z < 16; z++) {
							for(int x = 0; x < 16; x++) {
								var value = hmap[z * 16 + x];
								hm[localChunkX * 16 + x, 511 - (localChunkZ * 16 + z)] = value;
							}
						}
					}
				} else {
					//It's the old, simple format
					int[] hmints = (int[])nbt.contents.Get("HeightMap");
					for(int z = 0; z < 16; z++) {
						for(int x = 0; x < 16; x++) {
							var value = hmints[z * 16 + x];
							hm[localChunkX * 16 + x, 511 - (localChunkZ * 16 + z)] = value;
						}
					}
				}
			} catch {
				Program.WriteWarning("Error while reading chunk " + localChunkX + "," + localChunkZ);
			}
		}

		private static string ByteToBinary(byte b, bool bigendian) {
			string s = Convert.ToString((int)b, 2);
			s = s.PadLeft(8, '0');
			if(bigendian) s = ReverseString(s);
			return s;
		}

		private static ushort Read9BitValue(string bitString, int index) {
			string bits = ReverseString(bitString.Substring(index * 9, 9));
			bits = "0000000" + bits;
			bool[] bitArr = new bool[16];
			for(int i = 0; i < 16; i++) {
				bitArr[i] = bits[i] == '1';
			}
			return Convert.ToUInt16(bits, 2);
		}

		private static string ReverseString(string input) {
			char[] chrs = input.ToCharArray();
			Array.Reverse(chrs);
			return new string(chrs);
		}
	}
}