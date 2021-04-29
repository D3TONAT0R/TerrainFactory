using HMCon;
using System.Drawing;

public class CommonSplatmapColors {

	public static Color white = Color.White;
	public static Color black = Color.FromArgb(255, 0, 0, 0);

	//Primary splat colors
	public static Color red = Color.FromArgb(255, 255, 0, 0);
	public static Color green = Color.FromArgb(255, 0, 255, 0);
	public static Color blue = Color.FromArgb(255, 0, 0, 255);

	//Secondary splat colors
	public static Color yellow = Color.FromArgb(255, 255, 255, 0);
	public static Color cyan = Color.FromArgb(255, 0, 255, 255);
	public static Color magenta = Color.FromArgb(255, 255, 0, 255);

	public static Color NameToColor(string s) {
		switch(s.ToLower()) {
			case "white": return white;
			case "black": return black;
			case "red": return red;
			case "green": return green;
			case "blue": return blue;
			case "yellow": return yellow;
			case "cyan": return cyan;
			case "magenta": return magenta;
			default: ConsoleOutput.WriteWarning("Unknown splat color: " + s); return black;
		}
	}
}