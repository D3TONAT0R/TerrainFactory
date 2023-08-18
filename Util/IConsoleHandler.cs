using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Util {
	public interface IConsoleHandler {

		void WriteLine(string line);

		void DisplayProgressBar(string text, float progress);

		string GetInput(string prompt, List<string> queue);
	}
}
