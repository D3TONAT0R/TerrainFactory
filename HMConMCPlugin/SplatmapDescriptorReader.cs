using ASCReader;
using ASCReaderMC.PostProcessors;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ASCReaderMC {
	public class SplatmapDescriptorReader {

		/*public enum Type {
			SurfaceSplatmapDesc,
			BiomeSplatmapDesc
		}*/

		public Dictionary<string, string> maps = new Dictionary<string, string>();
		public Dictionary<SplatmapMapping, string> layers = new Dictionary<SplatmapMapping, string>();
		public Dictionary<string, string> structures = new Dictionary<string, string>();
		public Dictionary<byte, BiomeGenerator> biomes = new Dictionary<byte, BiomeGenerator>();

		public string biomeMapperPath = null;

		public string watermapPath = null;
		public int waterLevel = 0;
		public string waterBlock = "water";

		public SplatmapDescriptorReader(string path, bool mainFile) {
			//type = t;
			if(!File.Exists(path)) {
				Program.WriteError("Splatmap file " + path + " does not exist!");
			}
			string[] lines = File.ReadAllLines(path);
			Dictionary<SplatmapMapping, string[]> dic = new Dictionary<SplatmapMapping, string[]>();
			for(int i = 0; i < lines.Length; i++) {
				var ln = lines[i].Replace("\r", "");
				if(string.IsNullOrWhiteSpace(ln) || ln.StartsWith("#")) continue;
				if(ln.StartsWith("map ")) {
					ReadMapToken(ln);
				} else if(ln.StartsWith("layer ")) {
					ReadLayerToken(ln);
				} else if(ln.StartsWith("assign ")) {
					ReadAssignmentToken(ln);
				} else if(ln.StartsWith("struct ")) {
					ReadStructureToken(ln);
				} else if(ln.StartsWith("gen ")) {
					ReadGenToken(ln);
				} else if(ln.StartsWith("biomeid ")) {
					ReadBiomeIDToken(ln);
				} else if(ln.StartsWith("biomemapper")) {
					biomeMapperPath = ln.Split('=')[1];
				} else if(ln.StartsWith("watermap")) {
					watermapPath = ln.Split('=')[1];
				} else if(ln.StartsWith("waterlevel")) {
					waterLevel = int.Parse(ln.Split('=')[1]);
				} else if(ln.StartsWith("waterblock")) {
					waterBlock = ln.Split('=')[1];
				} else {
					Program.WriteWarning("Unknown token in splat description: " + ln.Split(' ')[0]);
				}
			}
		}

		private void ReadMapToken(string ln) {
			string s = ln.Substring(4, ln.Length - 4);
			string[] split = s.Split('=');
			if(split.Length == 2) {
				maps.Add(split[0], split[1]);
			} else {
				Program.WriteError("Syntax error in splat map description: " + s);
			}
		}

		private void ReadLayerToken(string ln) {
			string s = ln.Substring(6, ln.Length - 6);
			string[] split = s.Split('=');
			if(split.Length == 2) {
				try {
					string[] key = split[0].Split(':');
					Color c;
					if(key[1].Contains(',')) {
						//It's a manually defined color
						string[] cs = key[1].Split(',');
						int r = int.Parse(cs[0]);
						int g = int.Parse(cs[1]);
						int b = int.Parse(cs[2]);
						c = Color.FromArgb(255, r, g, b);
					} else {
						c = CommonSplatmapColors.NameToColor(key[1]);
					}
					SplatmapMapping mapping = new SplatmapMapping(key[0], c, int.Parse(split[1]));
					layers.Add(mapping, "red_wool");
				} catch {
					Program.WriteError("Syntax error in splat layer description: " + s);
				}
			} else {
				Program.WriteError("Syntax error in splat layer description: " + s);
			}
		}

		void ReadAssignmentToken(string ln) {
			string s = ln.Substring(7, ln.Length - 7);
			string[] split = s.Split('=');
			if(split.Length == 2) {
				try {
					int n = int.Parse(split[0]);
					for(int i = 0; i < layers.Keys.Count; i++) {
						var m = layers.Keys.ElementAt(i);
						if(m.value == n) {
							layers[m] = split[1];
						}
					}
				} catch {
					Program.WriteError("Syntax error in splat assignment description: " + s);
				}
			} else {
				Program.WriteError("Syntax error in splat assignment description: " + s);
			}
		}

		void ReadStructureToken(string ln) {
			string s = ln.Substring(7, ln.Length - 7);
			string[] split = s.Split('=');
			if(split.Length == 2) {
				try {
					structures.Add(split[0], split[1]);
				} catch {
					Program.WriteError("Syntax error in splat structure description: " + s);
				}
			} else {
				Program.WriteError("Syntax error in splat structure description: " + s);
			}
		}

		void ReadGenToken(string ln) {
			string s = ln.Substring(4, ln.Length - 4);
			string[] split = s.Split('=');
			if(split.Length == 2) {
				var gen = biomes[byte.Parse(split[0])];
				string[] def = split[1].Split(',');
				if(def[0].ToLower().StartsWith("b:")) {
					//It's a single block, add it to the seconary generators
					def[0] = def[0].Substring(2, def[0].Length - 2);
					gen.decorStructures.Add(def[0], float.Parse(def[1]) / 256f);
				} else {
					//It's a structure
					string filename = structures[def[0]];
					gen.mainStructures.Add(filename, float.Parse(def[1]) / 256f);
				}
			} else {
				Program.WriteError("Syntax error in splat gen description: " + s);
			}
		}

		void ReadBiomeIDToken(string ln) {
			string s = ln.Substring(8, ln.Length - 8);
			string[] split = s.Split('=');
			if(split.Length == 2) {
				BiomeGenerator bgen = new BiomeGenerator(byte.Parse(split[1]));
				biomes.Add(byte.Parse(split[0]), bgen);
			} else {
				Program.WriteError("Syntax error in splat gen description: " + s);
			}
		}
	}
}