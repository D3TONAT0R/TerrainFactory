using HMCon.Formats;
using HMCon.Import;
using HMCon.Modification;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.IO;
using static HMCon.ConsoleOutput;

namespace HMCon.Export {
	public class Job {

		public bool allowOverwrite = true;
		public bool batchMode = false;

		public int CurrentFileIndex { get; private set; } = -1;
		public HeightData CurrentData { get; private set; }
		public bool HasNextFile => CurrentFileIndex+1 < InputFileList.Count;

		public List<string> InputFileList { get; private set; } = new List<string>();
		public Dictionary<string, string> variables = new Dictionary<string, string>();

		public ModificationChain modificationChain = new ModificationChain();
		public ExportSettings exportSettings = new ExportSettings();

		//public int exportNumX;
		//public int exportNumZ;

		public string outputPath = null;

		//Events
		public event Action<int, string> FileImported;
		public event Action<int, string, Exception> FileImportFailed;
		public event Action<int, string> FileExported;
		public event Action<int, string, Exception> FileExportFailed;
		public event Action ExportCompleted;

		public void AddInputFiles(params string[] files) {
			foreach(var s in files) {
				InputFileList.Add(ParseVariables(s));
			}
		}

		public HeightData NextFile() {
			CurrentData = null;
			CurrentFileIndex++;
			if(CurrentFileIndex < InputFileList.Count) {
				string f = InputFileList[0];
				string ext = Path.GetExtension(f).ToLower().Replace(".", "");
				HeightData d;
				try {
					string path = ExtractArgs(InputFileList[CurrentFileIndex], out var importArgs);
					d = ImportManager.ImportFile(path.Replace("\"", ""), importArgs);
					if(d != null) {
						CurrentData = d;
						FileImported?.Invoke(CurrentFileIndex, f);
						return d;
					} else {
						throw new IOException("Unsupported file type: " + ext);
					}
				} catch(Exception e) {
					FileImportFailed?.Invoke(CurrentFileIndex, f, e);
					return null;
				}
			} else {
				return null;
			}
		}

		static string ExtractArgs(string input, out string[] args)
		{
			var split = input.Split(new string[] { " -" }, StringSplitOptions.RemoveEmptyEntries);
			List<string> argList = new List<string>();
			input = split[0];
			for(int i = 1; i < split.Length; i++)
			{
				argList.Add(split[i].Trim());
			}
			args = argList.ToArray();
			return input;
		}

		public HeightData ApplyModificationChain(HeightData inputData)
		{
			return modificationChain.Apply(inputData);
		}

		public void ApplyModificationChain() {
			if(CurrentData == null) {
				throw new NullReferenceException("CurrentData is null");
			}
			UpdateProgressBar($"Applying {modificationChain.chain.Count} modifiers...", -1);
			CurrentData = ApplyModificationChain(CurrentData);
			UpdateProgressBar("", -1);
		}

		public void ExportAll() {
			if(string.IsNullOrWhiteSpace(outputPath)) {
				throw new ArgumentException("outputPath is null");
			}
			if(CurrentData == null) {
				NextFile();
			}

			ApplyModificationChain();

			if (!ExportManager.ValidateExportSettings(exportSettings, CurrentData))
			{
				throw new InvalidOperationException("Current export settings are invalid for at least one of the selected formats.");
			}

			if (batchMode) {
				string fullPath = Path.Combine(outputPath, Path.GetFileName(InputFileList[CurrentFileIndex]));
				Export(CurrentData, fullPath);
				while(HasNextFile) {
					NextFile();
					fullPath = Path.Combine(outputPath, Path.GetFileName(InputFileList[CurrentFileIndex]));
					if(CurrentData != null) {
						ApplyModificationChain();
						Export(CurrentData, fullPath);
					}
				}
			} else {
				Export(CurrentData, outputPath);
			}
			ExportCompleted?.Invoke();
		}

		void Export(HeightData data, string outPath) {
			try {
				string dir = Path.GetDirectoryName(outPath);
				string fname = Path.GetFileNameWithoutExtension(outPath);
				if(Directory.Exists(dir))
				{
					foreach(var tile in GetSplitTiles(data))
					{
						foreach(FileFormat format in exportSettings.outputFormats) {
							ExportTask exportTask = new ExportTask(tile.data, format, exportSettings, dir, fname);

							if(tile.HasMultiple) {
								exportTask.filenameBuilder.tileIndex = (tile.xIndex, tile.yIndex);
							}

							format.ModifyFileName(exportTask, exportTask.filenameBuilder);
							string fullpath = exportTask.filenameBuilder.GetFullPath();

							WriteLine($"Creating file {fullpath} ...");
							try {
								var startTime = DateTime.Now;
								exportTask.Export();
								var span = DateTime.Now - startTime;
								if(span.TotalSeconds > 5)
								{
									WriteLine($"Time: {span.TotalSeconds:F2}");
								}
								WriteSuccess($"{format.Identifier} file created successfully!");
							} catch(Exception e) {
								throw new IOException($"Failed to write {format.Identifier} file!", e);
							}
						}
					}
				} else {
					throw new IOException($"Directory '{dir}' does not exist!");
				}
				FileExported?.Invoke(CurrentFileIndex, outputPath);
			} catch(Exception e) {
				FileExportFailed?.Invoke(CurrentFileIndex, outputPath, e);
			}
		}

		IEnumerable<ExportTile> GetSplitTiles(HeightData data)
		{
			if(exportSettings.splitInterval > 2)
			{
				ExportTile.CalcTileCount(data, exportSettings.splitInterval, out int xCount, out int yCount);
				for(int y = 0; y < yCount; y++)
				{
					for(int x = 0; x < xCount; x++)
					{
						yield return ExportTile.GetTile(data, exportSettings.splitInterval, x, y);
					}
				}
			}
			else
			{
				yield return ExportTile.CreateFullTile(CurrentData);
			}
		}

		public string ParseVariables(string input) {

			foreach(var kv in variables)
			{
				input = input.Replace($"{{{kv.Key}}}", kv.Value);
			}

			input = input.Replace("{datetime}", DateTime.Now.ToString("yy-MM-dd_HH-mm-ss"));
			input = input.Replace("{datetimeshort}", DateTime.Now.ToString("yyMMddHHmmss"));
			input = input.Replace("{user}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
			return input;
		}
	}
}
