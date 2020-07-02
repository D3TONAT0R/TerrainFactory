using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ASCReader;

public class SplatmapDescriptorReader {

	public Dictionary<string,string> maps = new Dictionary<string, string>();
	public Dictionary<SplatmapMapping, string> layers = new Dictionary<SplatmapMapping, string>();

	public SplatmapDescriptorReader(string importedFilePath) {
		string path = importedFilePath+".splat";
		if(!File.Exists(path)) {
			Program.WriteError("Splatmap file "+path+" does not exist!");
		}
		string[] lines = File.ReadAllLines(path);
		Dictionary<SplatmapMapping, string[]> dic = new Dictionary<SplatmapMapping, string[]>();
		foreach(string ln in lines) {
			if(string.IsNullOrWhiteSpace(ln) || ln.StartsWith("#")) continue;
			if(ln.StartsWith("map ")) {
				string s = ln.Substring(4, ln.Length-4);
				string[] split = s.Split('=');
				if(split.Length == 2) {
					maps.Add(split[0],split[1]);
				} else {
					Program.WriteError("Syntax error in splat map description: "+s);
				}
			} else if(ln.StartsWith("layer ")) {
				string s = ln.Substring(6, ln.Length-6);
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
						Program.WriteError("Syntax error in splat layer description: "+s);
					}
				} else {
					Program.WriteError("Syntax error in splat layer description: "+s);
				}
			} else if(ln.StartsWith("assign ")) {
				string s = ln.Substring(7, ln.Length-7);
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
						Program.WriteError("Syntax error in splat assignment description: "+s);
					}
				} else {
					Program.WriteError("Syntax error in splat assignment description: "+s);
				}
			} else {
				Program.WriteWarning("Unknown token in splat description: "+ln.Split(' ')[0]);
			}
		}
	}
}