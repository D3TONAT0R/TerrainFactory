using System;
using System.Drawing;

public class SplatmapMapping {

	public string mapName;
	public Color color;
	public int value;

	public SplatmapMapping(string map, Color c, int b) {
		mapName = map;
		color = c;
		value = b;
	}

	public SplatmapMapping(string map, string cName, int b) {
		mapName = map;
		color = CommonSplatmapColors.NameToColor(cName);
		value = b;
	}
}