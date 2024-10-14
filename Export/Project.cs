using TerrainFactory.Formats;
using TerrainFactory.Import;
using TerrainFactory.Modification;
using System;
using System.Collections.Generic;
using System.IO;
using static TerrainFactory.ConsoleOutput;

namespace TerrainFactory.Export
{
	public class Project
	{

		public bool AllowFileOverwrite { get; set; } = true;
		public bool ForceBatchNamingPattern { get; set; } = false;
		public bool UseBatchNamingPattern => ForceBatchNamingPattern || InputFileCount > 1;

		public ElevationData CurrentData { get; private set; }

		public int CurrentFileIndex { get; private set; } = -1;
		public bool HasNextFile => CurrentFileIndex + 1 < InputFileList.Count;
		public int InputFileCount => InputFileList.Count;
		public bool HasMultipleInputs => InputFileCount > 1;


		public FileFormatList outputFormats = new FileFormatList();
		public int OutputFormatCount => outputFormats.Count;


		public List<string> InputFileList { get; } = new List<string>();
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

		}

		public Project(string inputFile, string outputFile)
		{
			AddInputFile(inputFile);
		}

		public Project(string inputFile, string outputFile, params FileFormat[] formats)
		{
			AddInputFile(inputFile);
			outputFormats.AddFormats(formats);
			OutputPath = outputFile;
		}

		public Project(string inputFile, string outputFile, params Type[] formats)
		{
			AddInputFile(inputFile);
			outputFormats.AddFormats(formats);
			OutputPath = outputFile;
		}

		public void AddInputFile(string file)
		{
			InputFileList.Add(ResolveWildcards(file, null));
		}

		public void AddInputFiles(IEnumerable<string> files)
		{
			foreach(var s in files)
			{
				InputFileList.Add(ResolveWildcards(s, null));
			}
		}

		private void ImportNext()
		{
			CurrentData = null;
			CurrentFileIndex++;
			if(CurrentFileIndex < InputFileList.Count)
			{
				string f = InputFileList[0];
				string ext = Path.GetExtension(f).ToLower().Replace(".", "");
				try
				{
					string path = ExtractArgs(InputFileList[CurrentFileIndex], out var importArgs);
					var importedData = ImportManager.ImportFile(path.Replace("\"", ""), importArgs);
					if (importedData == null)
					{
						throw new IOException("Unsupported file type: " + ext);
					}

					CurrentData = importedData;
					FileImported?.Invoke(CurrentFileIndex, f);
				}
				catch(Exception e)
				{
					FileImportFailed?.Invoke(CurrentFileIndex, f, e);
				}
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
			if(InputFileCount == 0)
			{
				throw new InvalidOperationException("No input files specified.");
			}
			if(OutputPath == null)
			{
				throw new InvalidOperationException("No output path specified.");
			}
			if(CurrentFileIndex == -1)
			{
				ImportNext();
			}
			while(true)
			{
				ProcessData(CurrentData, UseBatchNamingPattern);
				if(HasNextFile)
				{
					ImportNext();
				}
				else
				{
					break;
				}
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
				finalOutputPath = ResolveWildcards(Path.Combine(OutputPath, Path.GetFileName(InputFileList[CurrentFileIndex])), inputData.SourceFileName);
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

				FileExported?.Invoke(CurrentFileIndex, OutputPath);
			}
			catch(Exception e)
			{
				FileExportFailed?.Invoke(CurrentFileIndex, OutputPath, e);
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
				yield return ExportTileInfo.CreateFullTile(CurrentData);
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
