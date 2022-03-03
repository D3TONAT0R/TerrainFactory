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
			case "w":
			case "white": return white;
			case "k":
			case "black": return black;
			case "r":
			case "red": return red;
			case "g":
			case "green": return green;
			case "b":
			case "blue": return blue;
			case "y":
			case "yellow": return yellow;
			case "c":
			case "cyan": return cyan;
			case "m":
			case "magenta": return magenta;
			default: ConsoleOutput.WriteWarning("Unknown splat color: " + s); return black;
		}
	}
}