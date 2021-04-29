using HMCon.Import;
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
					d = ImportManager.ImportFile(InputFileList[CurrentFileIndex], importArgs);
					if(d != null) {
						CurrentExportJobInfo.importedFilePath = f;
						CurrentData = d;
						FileImported(CurrentFileIndex, f);
						return d;
					} else {
						throw new IOException("Unsupported file type: " + ext);
					}
				} catch(Exception e) {
					FileImportFailed(CurrentFileIndex, f, e);
					return null;
				}
			} else {
				return null;
			}
		}

		public void ExportAll() {
			if(string.IsNullOrWhiteSpace(outputPath)) {
				throw new ArgumentException("outputPath is null");
			}
			if(CurrentData == null) {
				NextFile();
			}
			if(!ExportUtility.ValidateExportSettings(exportSettings, CurrentData)) {
				throw new InvalidOperationException("Current export settings are invalid for at least one of the selected formats");
			}
			if(batchMode) {
				string fullPath = Path.Combine(outputPath, Path.GetFileName(InputFileList[CurrentFileIndex]));
				ExportFile(CurrentData, fullPath);
				while(HasNextFile) {
					NextFile();
					fullPath = Path.Combine(outputPath, Path.GetFileName(InputFileList[CurrentFileIndex]));
					if(CurrentData != null) {
						ExportFile(CurrentData, fullPath);
					}
				}
			} else {
				ExportFile(CurrentData, outputPath);
			}
			ExportCompleted();
		}

		void ExportFile(HeightData data, string outPath) {
			try {
				data.WriteAllFiles(outPath, exportSettings);
				FileExported(CurrentFileIndex, outputPath);
			} catch(Exception e) {
				FileExportFailed(CurrentFileIndex, outputPath, e);
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
