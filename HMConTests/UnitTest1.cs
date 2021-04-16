using ASCReaderImagePlugin;
using HMCon;
using HMCon.Export;
using HMCon.Import;
using HMCon.Util;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;

namespace HMConTests {
	public class Tests {

		string sampleASCFile = "sample-maps.zh.asc";
		string sampleASCFileHS = "sample-maps.zh_hs.png";
		string sampleCroppedASCFile = "sample-cropped-maps.zh.asc";
		string resizedASCFileHS = "sample-resized-maps.zh.png";
		string sampleHeightmapFile = "sample-hm.png";
		string sampleMCAFile = "sample-mca.16.26.mca";

		string inputPath;
		string outputPath;

		[OneTimeSetUp]
		public void Start() {
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
			string filePath = AppDomain.CurrentDomain.BaseDirectory;
			for(int i = 0; i < 4; i++) filePath = Directory.GetParent(filePath).FullName;
			inputPath = Path.Combine(filePath, "TestFiles", "in");
			outputPath = Path.Combine(filePath, "TestFiles", "out");
			string loc = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "HMConApp", "bin", "Debug", "netcoreapp3.1"));
			Program.Initialize(loc);
			if(Program.numPluginsLoaded == 0) {
				throw new FileLoadException($"No plugins were loaded from location '{loc}'.");
			}
		}

		[SetUp]
		public void Setup() {
			CurrentExportJobInfo.Reset();
			CurrentExportJobInfo.importedFilePath = inputPath; //TODO: is this correct?
		}

		[Test]
		public void ExportDefaultHillshadeTest() {
			ASCData data = new ASCData(Path.Combine(inputPath, sampleASCFile));
			AssertExport(data, "IMG_PNG-HS", Path.Combine(outputPath, sampleASCFileHS));
		}

		[Test]
		public void TestASCExport() {
			ASCData data = new ASCData(Path.Combine(inputPath, sampleASCFile));
			var sampleLocations = GetSampleLocations(data.ncols, data.nrows);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			AssertExport(data, "ASC", Path.Combine(outputPath, sampleASCFile));
			data = new ASCData(Path.Combine(outputPath, sampleASCFile));
			var exportedSamples = GetHeightSamples(data, sampleLocations);
			Assert.AreEqual(sourceSamples, exportedSamples);
		}

		[Test]
		public void TestCroppedASCExport() {
			ASCData data = new ASCData(Path.Combine(inputPath, sampleASCFile));
			int x1 = 250;
			int y1 = 1180;
			int x2 = 850;
			int y2 = 1920;
			var sampleLocations = GetSampleLocations(x1, y1, x2, y2);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			CurrentExportJobInfo.bounds = new Bounds(x1, y1, x2, y2);
			AssertExport(data, "ASC", Path.Combine(outputPath, sampleCroppedASCFile));
			data = new ASCData(Path.Combine(outputPath, sampleCroppedASCFile));
			for(int i = 0; i < sampleLocations.Length; i++) {
				sampleLocations[i].x -= x1;
				sampleLocations[i].y -= y1;
			}
			var exportedSamples = GetHeightSamples(data, sampleLocations);
			Assert.AreEqual(sourceSamples, exportedSamples);
		}

		[Test]
		public void TestHeightmapHandling() {
			ASCData data = ImportManager.ImportFile(Path.Combine(inputPath, sampleHeightmapFile), "png");
			var sampleLocations = GetSampleLocations(data.ncols, data.nrows);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			AssertExport(data, "IMG_PNG-HM", Path.Combine(outputPath, sampleHeightmapFile));
			data = ImportManager.ImportFile(Path.Combine(outputPath, sampleHeightmapFile), "png");
			var exportedSamples = GetHeightSamples(data, sampleLocations);
			for(int i = 0; i < sourceSamples.Length; i++) {
				Assert.AreEqual(sourceSamples[i], exportedSamples[i], 0.0001d, $"Index {i}, location [{sampleLocations[i].x},{sampleLocations[i].y}]");
			}
		}

		[Test]
		public void TestMCAFileHandling() {
			ASCData data = ImportManager.ImportFile(Path.Combine(inputPath, sampleMCAFile), "mca");
			data.lowPoint = 0;
			data.highPoint = 255;
			var sampleLocations = GetSampleLocations(data.ncols, data.nrows);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			CurrentExportJobInfo.mcaGlobalPosX = 16;
			CurrentExportJobInfo.mcaGlobalPosZ = 26;
			AssertExport(data, "MCA", Path.Combine(outputPath, sampleMCAFile));
			AssertExport(data, "IMG_PNG-HS", Path.Combine(outputPath, sampleMCAFile));
			AssertExport(data, "IMG_PNG-HM", Path.Combine(outputPath, sampleMCAFile));
		}

		[Test]
		public void TestMCAAccuracy() {
			var heights = HeightmapImporter.ImportHeightmapRaw(Path.Combine(inputPath, sampleHeightmapFile), 0, 0, 512, 512);
			ASCData data = new ASCData(512, 512, null);
			data.lowPoint = 0;
			data.highPoint = 255;
			var heightsF = new float[512, 512];
			for(int i = 0; i < 512; i++) {
				for(int j = 0; j < 512; j++) {
					heightsF[i, j] = heights[i, j];
				}
			}
			data.data = heightsF;
			var sampleLocations = GetSampleLocations(data.ncols, data.nrows);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			string mcaname = "accuracy-test-r.0.0.mca";
			AssertExport(data, "MCA-RAW", Path.Combine(outputPath, mcaname));
			var reimported = ImportManager.ImportFile(Path.Combine(outputPath, mcaname), "mca");
			var convSamples = GetHeightSamples(reimported, sampleLocations);
			AssertExport(data, "IMG_PNG-HM", Path.Combine(outputPath, "reconstructed_mca.png"));
			Assert.AreEqual(sourceSamples, convSamples);
		}

		[Test]
		public void TestASCResizing() {
			ASCData data = ImportManager.ImportFile(Path.Combine(inputPath, sampleASCFile), "asc");
			var sampleLocationsOriginal = GetSampleLocations(data.ncols, data.nrows);
			var sourceSamples = GetHeightSamples(data, sampleLocationsOriginal);
			int scale = (int)(data.ncols * 1.39f);
			ASCData resized = data.Resize(scale, false);
			ASCData rescaled = data.Resize(scale, true);
			ExportUtility.ExportFile(rescaled, ExportUtility.GetFormatFromIdenfifier("IMG_PNG-HS"), Path.Combine(outputPath, resizedASCFileHS));
			var resizedSamples = GetHeightSamples(resized, GetSampleLocations(resized.ncols, resized.nrows));
			double delta = 0.4f;
			for(int i = 0; i < sourceSamples.Length; i++) {
				Assert.AreEqual(sourceSamples[i], resizedSamples[i], delta, $"Index {i}, location in original [{sampleLocationsOriginal[i].x},{sampleLocationsOriginal[i].y}]");
			}
			Assert.AreEqual(data.highestValue, resized.highestValue, delta / 2);
			Assert.AreEqual(data.lowestValue, resized.lowestValue, delta / 2);
		}

		[Test]
		public void TestASCAccurateResizing() {
			ASCData data = ImportManager.ImportFile(Path.Combine(inputPath, sampleASCFile), "asc");
			var sampleLocationsOriginal = GetSampleLocations(data.ncols, data.nrows);
			var sourceSamples = GetHeightSamples(data, sampleLocationsOriginal);
			ASCData resized = data.Resize(data.ncols * 2, false);
			Assert.AreEqual(4000, resized.ncols);
			AssertExport(data, "IMG_PNG-HM", Path.Combine(outputPath, "asc-original"));
			AssertExport(resized, "IMG_PNG-HM", Path.Combine(outputPath, "asc-resized"));
			var resizedLocations = GetSampleLocations(resized.ncols, resized.nrows);
			var resizedSamples = GetHeightSamples(resized, resizedLocations);
			double delta = 0.05f;
			for(int i = 0; i < sourceSamples.Length; i++) {
				Assert.AreEqual(sourceSamples[i], resizedSamples[i], delta, $"Index {i}, location in original [{sampleLocationsOriginal[i].x},{sampleLocationsOriginal[i].y}], in resized [{resizedLocations[i].x},{resizedLocations[i].y}]");
			}
			Assert.AreEqual(data.highestValue, resized.highestValue, delta);
			Assert.AreEqual(data.lowestValue, resized.lowestValue, delta);
		}

		void AssertExport(ASCData data, string filetype, string path) {
			var format = ExportUtility.GetFormatFromIdenfifier(filetype);
			path = Path.ChangeExtension(path, format.extension);
			Assert.IsTrue(ExportUtility.ExportFile(data, format, path), filetype + " export failed");
			Assert.IsTrue(File.Exists(path), "Written file not found");
		}

		(int x, int y)[] GetSampleLocations(int maxX, int maxY) {
			return GetSampleLocations(0, 0, maxX - 1, maxY - 1);
		}

		(int x, int y)[] GetSampleLocations(int x1, int y1, int x2, int y2) {
			var sampleLocations = new (int x, int y)[65];
			for(int x = 0; x < 8; x++) {
				for(int y = 0; y < 8; y++) {
					int i = y * 8 + x;
					sampleLocations[i].x = x1 + (int)Math.Round((x2+1 - x1) * (x / 8f), MidpointRounding.ToPositiveInfinity);
					sampleLocations[i].y = y1 + (int)Math.Round((y2+1 - y1) * (y / 8f), MidpointRounding.ToPositiveInfinity);
				}
			}
			sampleLocations[64].x = x2;
			sampleLocations[64].y = y2;
			return sampleLocations;
		}

		float[] GetHeightSamples(ASCData data, (int x, int y)[] locations) {
			float[] samples = new float[locations.Length];
			for(int i = 0; i < locations.Length; i++) {
				var l = locations[i];
				samples[i] = data.GetData(l.x, l.y);
			}
			return samples;
		}
	}
}