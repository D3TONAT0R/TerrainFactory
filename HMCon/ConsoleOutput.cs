using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMCon {
	public static class ConsoleOutput {

		public static IConsoleHandler consoleHandler;
		public static bool debugLogging = false;

		static string progressString = null;


		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		static void WriteConsoleLine(string str) {
			if(consoleHandler != null) {
				consoleHandler.WriteLine(str);
			} else if(GetConsoleWindow() != IntPtr.Zero) {
				try {
					Console.CursorVisible = false;
					if(progressString != null) {
						WriteProgress("", -1);
						progressString = null;
						Console.SetCursorPosition(0, Console.CursorTop);
					}
					Console.WriteLine(str);
					Console.ResetColor();
				} catch {

				}
			}
		}

		public static void WriteLine(string str) {
			WriteConsoleLine(str);
		}

		public static void WriteSuccess(string str) {
			Console.ForegroundColor = ConsoleColor.Green;
			WriteConsoleLine(str);
		}

		public static void WriteLineSpecial(string str) {
			Console.ForegroundColor = ConsoleColor.Cyan;
			WriteConsoleLine(str);
		}

		public static void WriteAutoTask(string str) {
			Console.BackgroundColor = ConsoleColor.DarkBlue;
			WriteConsoleLine(str);
		}

		public static void WriteLine(string str, params Object[] args) {
			Console.WriteLine(str, args);
		}

		public static void WriteWarning(string str) {
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			WriteConsoleLine(str);
		}

		public static void WriteError(string str) {
			Console.ForegroundColor = ConsoleColor.DarkRed;
			WriteConsoleLine(str);
#if DEBUG
			HMConManager.autoInputActive = false; //Stop any upcoming automated inputs
#endif
		}

		public static void WriteProgress(string str, float progress) {
			if(consoleHandler != null) {
				consoleHandler.DisplayProgressBar(str, progress);
			} else if(GetConsoleWindow() != IntPtr.Zero) {
				Console.CursorVisible = false;
				int lastLength = 0;
				if(progressString != null) {
					lastLength = progressString.Length;
					Console.SetCursorPosition(0, Console.CursorTop);
				}
				progressString = str;
				if(progress >= 0) progressString += " " + GetProgressBar(progress) + " " + (int)Math.Round(progress * 100) + "%";
				if(lastLength > 0) progressString = progressString.PadRight(lastLength, ' ');
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(progressString);
				Console.ResetColor();
			}
		}

		static string GetProgressBar(float prog) {
			StringBuilder s = new StringBuilder();
			for(int i = 0; i < 20; i++) {
				s.Append((prog >= (i + 1) / 20f) ? "█" : "░");
			}
			return s.ToString();
		}

	}
}
