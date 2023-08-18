using TerrainFactory.Commands;
using TerrainFactory.Export;
using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace TerrainFactory
{
	public static class ConsoleOutput
	{

		public static IConsoleHandler consoleHandler;
		public static bool debugLogging = false;

		static long lastProgressBarUpdateMillisecond = -1;
		static string fullProgressString;

		public static event Action<string> ErrorOccurred;

		static readonly object lockObj = new object();

		static void WriteConsoleLine(string str)
		{
			if (consoleHandler != null)
			{
				consoleHandler.WriteLine(str);
			}
			else if (GetConsoleWindow() != IntPtr.Zero)
			{
				try
				{
					Console.CursorVisible = false;
					if (fullProgressString != null)
					{
						var bg = Console.BackgroundColor;
						var fg = Console.ForegroundColor;
						ClearProgressBar();
						Console.SetCursorPosition(0, Console.CursorTop - 1);
						Console.BackgroundColor = bg;
						Console.ForegroundColor = fg;
					}
					Console.Write(str);
					Console.ResetColor();
					Console.WriteLine();
				}
				catch
				{

				}
			}
		}

		public static void WriteLine(string str)
		{
			WriteConsoleLine(str);
		}

		public static void WriteSuccess(string str)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			WriteConsoleLine(str);
		}

		public static void WriteLineSpecial(string str)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			WriteConsoleLine(str);
		}

		public static void WriteWarning(string str)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			WriteConsoleLine(str);
		}

		public static void WriteError(string str)
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;
			WriteConsoleLine(str);
			CommandHandler.ClearCommandQueue();
			ErrorOccurred?.Invoke(str);
		}

		public static void UpdateProgressBar(string str, float progress, bool forceWrite = false)
		{
			lock (lockObj)
			{
				long millisecond = DateTime.Now.Ticks / 10000;
				if(forceWrite || millisecond > lastProgressBarUpdateMillisecond + 50)
				{
					lastProgressBarUpdateMillisecond = millisecond;
					WriteProgress(str, progress);
				}
			}
		}

		public static void ClearProgressBar()
		{
			WriteProgress("", -1);
			fullProgressString = null;
		}

		private static void WriteProgress(string str, float progress)
		{
			lock (lockObj)
			{
				if (consoleHandler != null)
				{
					consoleHandler.DisplayProgressBar(str, progress);
				}
				else if (GetConsoleWindow() != IntPtr.Zero)
				{
					Console.CursorVisible = false;
					Console.ForegroundColor = ConsoleColor.DarkGray;
					if (!string.IsNullOrEmpty(fullProgressString))
					{
						Console.SetCursorPosition(0, Console.CursorTop - 1);
						Console.Write(" ".PadRight(fullProgressString.Length + 1, ' '));
						Console.SetCursorPosition(0, Console.CursorTop);
					}
					fullProgressString = str;
					if (progress >= 0) fullProgressString += " " + GetProgressBar(progress) + " " + (int)Math.Round(progress * 100) + "%";
					Console.WriteLine(fullProgressString);
					Console.ResetColor();
				}
			}
		}

		public static void WriteBox(string text)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("┌");
			sb.Append("".PadLeft(text.Length + 2, '─'));
			sb.AppendLine("┐");
			sb.Append("│ ");
			sb.Append(text);
			sb.AppendLine(" │");
			sb.Append("└");
			sb.Append("".PadLeft(text.Length + 2, '─'));
			sb.Append("┘");
			WriteLine(sb.ToString());
		}

		static string GetProgressBar(float prog)
		{
			StringBuilder s = new StringBuilder();
			for (int i = 0; i < 20; i++)
			{
				s.Append((prog >= (i + 1) / 20f) ? "█" : "░");
			}
			return s.ToString();
		}

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
	}
}
