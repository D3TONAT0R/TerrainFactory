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
		public string[] importArgs = new string[0];

		public ExportSettings exportSettings = new ExportSettings();

		public int exportNumX;
		public int exportNumZ;

		public string outputPath = null;

		//Events
		public event Action<int, string> FileImported;
		public event Action<int, string, Exception> FileImportFailed;
		public event Action<int, string> FileExported;
		public event Action<int, string, Exception> FileExportFailed;
		public event Action ExportCompleted;

		public void AddInputFiles(params string[] files) {
			foreach(var s in files) {
				InputFileList.Add(ReplacePathVars(s.Replace("\"", "")));
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
					var format = 
					d = ImportManager.ImportFile(InputFileList[CurrentFileIndex], importArgs);
					if(d != null) {
						//CurrentExportJobInfo.importedFilePath = f;
						CurrentData = d;
						if(FileImported != null) FileImported(CurrentFileIndex, f);
						return d;
					} else {
						throw new IOException("Unsupported file type: " + ext);
					}
				} catch(Exception e) {
					if (FileImportFailed != null) FileImportFailed(CurrentFileIndex, f, e);
					return null;
				}
			} else {
				return null;
			}
		}

		public HeightData ApplyModificationChain(HeightData inputData) {
			HeightData data = new HeightData(inputData) {
				wasModified = true
			};
			for(int i = 0; i < exportSettings.modificationChain.Count; i++) {
				data = exportSettings.modificationChain[i].Modify(data, true);
			}
			return data;
		}

		public void ApplyModificationChain() {
			if(CurrentData == null) {
				throw new NullReferenceException("CurrentData is null");
			}
			UpdateProgressBar($"Applying {exportSettings.modificationChain.Count} modifiers...", -1);
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

			if (!ExportUtility.ValidateExportSettings(exportSettings, CurrentData))
			{
				throw new InvalidOperationException("Current export settings are invalid for at least one of the selected formats.");
			}

			if (batchMode) {
				string fullPath = Path.Combine(outputPath, Path.GetFileName(InputFileList[CurrentFileIndex]));
				ExportFile(CurrentData, fullPath);
				while(HasNextFile) {
					NextFile();
					fullPath = Path.Combine(outputPath, Path.GetFileName(InputFileList[CurrentFileIndex]));
					if(CurrentData != null) {
						ApplyModificationChain();
						ExportFile(CurrentData, fullPath);
					}
				}
			} else {
				ExportFile(CurrentData, outputPath);
			}
			if(ExportCompleted != null) ExportCompleted();
		}

		void ExportFile(HeightData data, string outPath) {
			try {
				string dir = Path.GetDirectoryName(outPath);
				string fname = Path.GetFileNameWithoutExtension(outPath);
				if(Directory.Exists(dir)) {
					HeightDataSplitter splitter = new HeightDataSplitter(data, exportSettings.fileSplitDims);
					for(int z = 0; z < splitter.NumDataY; z++) {
						for(int x = 0; x < splitter.NumDataX; x++) {
							exportNumX = x;
							exportNumZ = z;
							foreach(FileFormat format in exportSettings.outputFormats) {
								ExportJob exportJob = new ExportJob(splitter.GetDataChunk(x,z), format, exportSettings, dir, fname);
								if(splitter.NumChunks > 1) {
									exportJob.nameBuilder.gridNum = (x, z);
								}
								format.ModifyFileName(exportJob, exportJob.nameBuilder);
								string fullpath = exportJob.nameBuilder.GetFullPath();

								WriteLine($"Creating file {fullpath} ...");
								try {
									var startTime = DateTime.Now;
									exportJob.Export();
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
					}
				} else {
					throw new IOException($"Directory '{dir}' does not exist!");
				}
				if(FileExported != null) FileExported(CurrentFileIndex, outputPath);
			} catch(Exception e) {
				if (FileExportFailed != null) FileExportFailed(CurrentFileIndex, outputPath, e);
			}
		}

		public static string ReplacePathVars(string path) {
			path = path.Replace("{datetime}", System.DateTime.Now.ToString("yy-MM-dd_HH-mm-ss"));
			path = path.Replace("{datetimeshort}", System.DateTime.Now.ToString("yyMMddHHmmss"));
			path = path.Replace("{user}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
			return path;
		}
	}
}
