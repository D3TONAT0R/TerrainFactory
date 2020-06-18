using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using Ionic.Zlib;

namespace ASCReader.Import {
	public static class MinecraftRegionImporter {

		static MemoryStream stream;

		public static ASCData ImportHeightmap(string filepath) {
			FileStream fs = File.Open(filepath, FileMode.Open);
			fs.CopyTo(stream);
			byte[] locationBytes = Read(0, 4096);
			byte[] timestampBytes = Read(4096, 4096);
			int[] locations = new int[1024];
			byte[] sizes = new byte[1024];
			for(int i = 0; i < 1024; i++) {
				locations[i] = ReadAsInt(locationBytes, i*4, 3);
				sizes[i] = Read(i*4+3, 1)[0];
			}
			ASCData asc = new ASCData(512,512);
			asc.cellsize = 1;
			asc.nodata_value = -9999;
			asc.RecalculateValues(false);
			asc.lowPoint = 0;
			asc.highPoint = 1;
			stream.Close();
			asc.isValid = true;
			return asc;
		}

		private static byte[] Read(int start, int length) {
			byte[] buffer = new byte[length];
			stream.Read(buffer, start, length);
			return buffer;
		}

		private static int ReadAsInt(byte[] arr, int start, int length) {
			byte[] bytes = new byte[length];
			for(int i = 0; i < length; i++) bytes[i] = arr[start+i];
			if(BitConverter.IsLittleEndian) Array.Reverse(arr);
			return BitConverter.ToInt32(arr);
		}

		private static byte[] GetChunkData(int loc, byte size) {
			loc *= 4096;
			int length = ReadAsInt(Read(loc,4),0,4);
			byte[] compressed = Read(loc+5, length-1);
			return ZlibStream.UncompressBuffer(compressed);
		}
	}
}