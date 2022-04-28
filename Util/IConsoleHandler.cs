using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon.Util {
	public interface IConsoleHandler {

		void WriteLine(string line);

		void DisplayProgressBar(string text, float progress);
	}
}
