using TerrainFactory.Formats;
using TerrainFactory.Import;
using TerrainFactory.Modification;
using System;
using System.Collections.Generic;
using System.IO;
using static TerrainFactory.ConsoleOutput;
using TerrainFactory.Export;

namespace TerrainFactory
{
	public class Project
	{
		public class InputDataCollection
		{
			public List<string> Files { get; } = new List<string>();

			public ElevationData Current { get; private set; }
			public int CurrentIndex { get; private set; } = -1;
			public string CurrentFileName => CurrentIndex >= 0 ? Files[CurrentIndex] : null;
			public bool HasNext => CurrentIndex + 1 < Files.Count;
			public int FileCount => Files.Count;
			public bool HasMultiple => FileCount > 1;

			public event Action<int, string> FileImported;
			public event Action<int, string, Exception> FileImportFailed;

			public void Add(string file, bool checkFileExists = true)
			{
				if(checkFileExists && !File.Exists(file))
				{
					throw new FileNotFoundException($"File not found: {file}");
				}
				Files.Add(file);
			}

			public void Clear()
			{
				Files.Clear();
				CurrentIndex = -1;
				Current = null;
			}

			public void LoadFirst()
			{
				if(Files.Count > 0)
				{
					CurrentIndex = 0;
					Current = ImportManager.ImportFile(Files[CurrentIndex]);
				}
				else
				{
					throw new InvalidOperationException("No input files specified.");
				}
			}

			public void Next()
			{
				if(CurrentIndex + 1 < Files.Count)
				{
					CurrentIndex++;
					Current = ImportManager.ImportFile(Files[CurrentIndex]);
				}
				else
				{
					throw new InvalidOperationException("No more files to import.");
				}
			}

			public void Load(int index)
			{
				if(index < 0 || index >= Files.Count)
				{
					throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
				}
				Current = null;
				CurrentIndex = index;

				string f = Files[index];
				string ext = Path.GetExtension(f).ToLower().Replace(".", "");
				try
				{
					string path = ExtractArgs(Files[CurrentIndex], out var importArgs);
					var importedData = ImportManager.ImportFile(path.Replace("\"", ""), importArgs);
					if(importedData == null)
					{
						throw new IOException("Unsupported file type: " + ext);
					}

					Current = importedData;
					FileImported?.Invoke(CurrentIndex, f);
				}
				catch(Exception e)
				{
					FileImportFailed?.Invoke(CurrentIndex, f, e);
				}
			}

			public void LoadIfRequired()
			{
				if(Current == null)
				{
					if(CurrentIndex < 0) LoadFirst();
					else Load(CurrentIndex);
				}
			}
		}


		public InputDataCollection InputData { get; } = new InputDataCollection();

		public bool AllowFileOverwrite { get; set; } = true;
		public bool ForceBatchNamingPattern { get; set; } = false;
		public bool UseBatchNamingPattern => ForceBatchNamingPattern || InputData.FileCount > 1;


		public FileFormatList outputFormats = new FileFormatList();
		public int OutputFormatCount => outputFormats.Count;

		public Dictionary<string, string> Wildcards { get; } = new Dictionary<string, string>();

		public ModificationChain modificationChain = new ModificationChain();
		public ExportSettings exportSettings = new ExportSettings();

		public string OutputPath { get; set; } = null;

		//Events
		public event Action<int, string> FileImported;
		public event Action<int, string, Exception> FileImportFailed;
		public event Action<int, string> FileExported;
		public event Action<int, string, Exception> FileExportFailed;
		public event Action ExportCompleted;

		public Project()
		{
			InputData.FileImported += (index, file) => FileImported?.Invoke(index, file);
			InputData.FileImportFailed += (index, file, e) => FileImportFailed?.Invoke(index, file, e);
		}

		public Project(string inputFile, string outputFile) : this()
		{
			AddInputFile(inputFile);
			InputData.LoadFirst();
			OutputPath = outputFile;
		}

		public Project(string inputFile, string outputFile, params FileFormat[] formats) : this()
		{
			AddInputFile(inputFile);
			InputData.LoadFirst();
			outputFormats.AddFormats(formats);
			OutputPath = outputFile;
		}

		public Project(string inputFile, string outputFile, params Type[] formats) : this()
		{
			AddInputFile(inputFile);
			InputData.LoadFirst();
			outputFormats.AddFormats(formats);
			OutputPath = outputFile;
		}

		public void AddInputFile(string file)
		{
			InputData.Add(ResolveWildcards(file, null));
		}

		public void AddInputFiles(IEnumerable<string> files)
		{
			foreach(var s in files)
			{
				InputData.Add(ResolveWildcards(s, null));
			}
		}

		public ElevationData ApplyModificationChain(ElevationData inputData, bool reportProgress = false)
		{
			if(inputData == null)
			{
				throw new NullReferenceException("Data is null");
			}
			if(reportProgress) UpdateProgressBar($"Applying {modificationChain.chain.Count} modifiers...", -1);
			var modified = modificationChain.Apply(inputData);
			if(reportProgress) UpdateProgressBar("", -1);
			return modified;
		}

		public void ProcessAll()
		{
			if(InputData.FileCount == 0)
			{
				throw new InvalidOperationException("No input files specified.");
			}
			if(OutputPath == null)
			{
				throw new InvalidOperationException("No output path specified.");
			}
			if(InputData.CurrentIndex == -1)
			{
				InputData.LoadFirst();
			}
			while(InputData.HasNext)
			{
				ProcessData(InputData.Current, UseBatchNamingPattern);
				InputData.Next();
			}
			ExportCompleted?.Invoke();
		}

		public void ProcessData(ElevationData inputData, bool useBatchNamingPattern)
		{
			if(string.IsNullOrWhiteSpace(OutputPath))
			{
				throw new ArgumentException("outputPath is null");
			}

			ApplyModificationChain(inputData, true);

			if(!ExportManager.ValidateExportSettings(outputFormats, exportSettings, inputData))
			{
				throw new InvalidOperationException("Current export settings are invalid for at least one of the selected formats.");
			}

			string finalOutputPath;
			if(useBatchNamingPattern)
			{
				finalOutputPath = ResolveWildcards(Path.Combine(OutputPath, Path.GetFileName(InputData.CurrentFileName)), inputData.SourceFileName);
			}
			else
			{
				finalOutputPath = ResolveWildcards(OutputPath, inputData.SourceFileName);
			}
			ExportData(inputData, finalOutputPath);
		}

		private void ExportData(ElevationData data, string destinationPath)
		{
			try
			{
				string dir = Path.GetDirectoryName(destinationPath);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(destinationPath);
				if(!Directory.Exists(dir))
				{
					throw new IOException($"Directory '{dir}' does not exist!");
				}

				foreach(var tile in GetExportTiles(data))
				{
					ExportTile(tile, dir, fileNameWithoutExtension);
				}

				FileExported?.Invoke(InputData.CurrentIndex, OutputPath);
			}
			catch(Exception e)
			{
				FileExportFailed?.Invoke(InputData.CurrentIndex, OutputPath, e);
			}
		}

		private void ExportTile(ExportTileInfo tileInfo, string rootDir, string fileNameWithoutExtension)
		{
			foreach(FileFormat format in outputFormats)
			{
				ExportTileWithFormat(tileInfo, rootDir, fileNameWithoutExtension, format);
			}
		}

		private void ExportTileWithFormat(ExportTileInfo tileInfo, string rootDir, string fileNameWithoutExtension, FileFormat format)
		{
			ExportTask exportTask = new ExportTask(tileInfo.data, format, exportSettings, rootDir, fileNameWithoutExtension);

			if(tileInfo.HasMultiple)
			{
				exportTask.filenameBuilder.tileIndex = (tileInfo.xIndex, tileInfo.yIndex);
			}

			format.ModifyFileName(exportTask, exportTask.filenameBuilder);
			string fullPath = exportTask.filenameBuilder.GetFullPath();

			WriteLine($"Creating file {fullPath} ...");
			try
			{
				var startTime = DateTime.Now;
				exportTask.Export();
				var span = DateTime.Now - startTime;
				if(span.TotalSeconds > 5)
				{
					WriteLine($"Time: {span.TotalSeconds:F2}");
				}
				WriteSuccess($"{format.Identifier} file created successfully!");
			}
			catch(Exception e)
			{
				throw new IOException($"Failed to write {format.Identifier} file!", e);
			}
		}

		private IEnumerable<ExportTileInfo> GetExportTiles(ElevationData data)
		{
			if(exportSettings.splitInterval > 2)
			{
				ExportTileInfo.CalcTileCount(data, exportSettings.splitInterval, out int xCount, out int yCount);
				for(int y = 0; y < yCount; y++)
				{
					for(int x = 0; x < xCount; x++)
					{
						yield return ExportTileInfo.GetTile(data, exportSettings.splitInterval, x, y);
					}
				}
			}
			else
			{
				yield return ExportTileInfo.CreateFullTile(InputData.Current);
			}
		}

		public string ResolveWildcards(string input, string inputFileName)
		{

			foreach(var kv in Wildcards)
			{
				input = input.Replace($"{{{kv.Key}}}", kv.Value);
			}

			input = input.Replace("{dt}", DateTime.Now.ToString("yy-MM-dd_HH-mm-ss"));
			input = input.Replace("{dtc}", DateTime.Now.ToString("yyMMddHHmmss"));
			input = input.Replace("{user}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
			input = input.Replace("{name}", Path.GetFileNameWithoutExtension(inputFileName ?? ""));
			return input;
		}

		private static string ExtractArgs(string input, out string[] args)
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
	}
}
