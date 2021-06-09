using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace HMCon
{
	public static class ConsoleOutput
	{

		public static IConsoleHandler consoleHandler;
		public static bool debugLogging = false;

		static string progressString;
		static float progressValue;
		static string newProgressString;
		static int progressBarUpdateTick;
		static int lastProgressBarUpdateTick = -1;

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		static Timer progressBarUpdateTimer;

		internal static void Initialize()
		{
			if (GetConsoleWindow() != null)
			{
				progressBarUpdateTimer = new Timer(250);
				progressBarUpdateTimer.Elapsed += OnProgressBarUpdate;
				progressBarUpdateTimer.Start();
			}
		}

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
					if (progressString != null)
					{
						WriteProgress("", -1);
						progressString = null;
						lastProgressBarUpdateTick = -1;
						Console.SetCursorPosition(0, Console.CursorTop);
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

		public static void WriteAutoTask(string str)
		{
			Console.BackgroundColor = ConsoleColor.DarkBlue;
			WriteConsoleLine(str);
		}

		public static void WriteLine(string str, params Object[] args)
		{
			Console.WriteLine(str, args);
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
#if DEBUG
			HMConManager.autoInputActive = false; //Stop any upcoming automated inputs
#endif
		}

		static object locker = new object();

		static void OnProgressBarUpdate(object sender, ElapsedEventArgs e)
		{
			if (progressBarUpdateTick == lastProgressBarUpdateTick)
			{
				WriteProgress(progressString, progressValue);
			}
			progressBarUpdateTick++;
		}

		public static void UpdateProgressBar(string str, float progress)
		{
			lock (locker)
			{
				progressString = str;
				progressValue = progress;
				lastProgressBarUpdateTick = progressBarUpdateTick;
			}
		}

		public static void ClearProgressBar()
		{
			progressString = null;
			progressValue = -1;
		}

		static object lockObj = new object();

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
					int lastLength = 0;
					if (progressString != null)
					{
						lastLength = progressString.Length;
						Console.SetCursorPosition(0, Console.CursorTop);
					}
					progressString = str;
					if (progress >= 0) progressString += " " + GetProgressBar(progress) + " " + (int)Math.Round(progress * 100) + "%";
					if (lastLength > 0) progressString = progressString.PadRight(lastLength, ' ');
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.Write(progressString);
					Console.ResetColor();
				}
			}
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

	}
}
