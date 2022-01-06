using HMConImage;
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
using HMCon.Modification;
using MCUtils;
using static MCUtils.NBTContent;
using System.Collections.Generic;
using Ionic.Zlib;

namespace HMConTests {
	public class Tests {

		readonly string sampleASCFile = "sample-maps.zh.asc";
		readonly string sampleASCFileHS = "sample-maps.zh_hs.png";
		readonly string sampleCroppedASCFile = "sample-cropped-maps.zh.asc";
		readonly string resizedASCFileHS = "sample-resized-maps.zh.png";
		readonly string sampleHeightmapFile = "sample-hm.png";
		readonly string sampleMCAFile = "sample-mca.16.26.mca";
		readonly string sampleNBTFile = "sample-nbt.dat";
		readonly string gradientMCAFile = "gradient-mca";

		string inputPath;
		string outputPath;

		static Job currentJob;

		[OneTimeSetUp]
		public void Start() {
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
			string filePath = AppDomain.CurrentDomain.BaseDirectory;
			for(int i = 0; i < 4; i++) filePath = Directory.GetParent(filePath).FullName;
			inputPath = Path.Combine(filePath, "TestFiles", "in");
			outputPath = Path.Combine(filePath, "TestFiles", "out");
			string loc = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "HMConApp", "bin", "Debug"));
			HMConManager.Initialize(loc);
			if(PluginLoader.NumPluginsLoaded == 0) {
				throw new FileLoadException($"No plugins were loaded from location '{loc}'.");
			}
		}

		[SetUp]
		public void Setup() {
			currentJob = new Job();
		}

		[Test]
		[Property("Plugin", "Image")]
		public void ExportDefaultHillshadeTest() {
			HeightData data = ASCImporter.instance.Import(Path.Combine(inputPath, sampleASCFile));
			AssertExport(data, ImageExporter.HillshadePNG, sampleASCFileHS);
		}

		[Test]
		[Property("Plugin", "Basic")]
		public void TestASCExport() {
			HeightData data = ASCImporter.instance.Import(Path.Combine(inputPath, sampleASCFile));
			var sampleLocations = GetSampleLocations(data.GridWidth, data.GridHeight);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			AssertExport(data, "ASC", sampleASCFile);
			data = ASCImporter.instance.Import(Path.Combine(outputPath, sampleASCFile));
			var exportedSamples = GetHeightSamples(data, sampleLocations);
			Assert.AreEqual(sourceSamples, exportedSamples);
		}

		[Test]
		[Property("Plugin", "Basic")]
		public void TestCroppedASCExport() {
			HeightData data = ASCImporter.instance.Import(Path.Combine(inputPath, sampleASCFile));
			int x1 = 250;
			int y1 = 1180;
			int x2 = 850;
			int y2 = 1920;
			var sampleLocations = GetSampleLocations(x1, y1, x2, y2);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			data = new AreaSelectionModifier(x1, y1, x2, y2).Modify(data, false);
			AssertExport(data, "ASC", sampleCroppedASCFile);
			data = ASCImporter.instance.Import(Path.Combine(outputPath, sampleCroppedASCFile));
			for(int i = 0; i < sampleLocations.Length; i++) {
				sampleLocations[i].x -= x1;
				sampleLocations[i].y -= y1;
			}
			var exportedSamples = GetHeightSamples(data, sampleLocations);
			Assert.AreEqual(sourceSamples, exportedSamples);
		}

		[Test]
		[Property("Plugin", "Image")]
		public void TestHeightmapHandling() {
			HeightData data = ImportManager.ImportFile(Path.Combine(inputPath, sampleHeightmapFile));
			var sampleLocations = GetSampleLocations(data.GridWidth, data.GridHeight);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			AssertExport(data, ImageExporter.HeightmapPNG8Bit, sampleHeightmapFile);
			data = ImportManager.ImportFile(Path.Combine(outputPath, sampleHeightmapFile));
			var exportedSamples = GetHeightSamples(data, sampleLocations);
			for(int i = 0; i < sourceSamples.Length; i++) {
				Assert.AreEqual(sourceSamples[i], exportedSamples[i], 0.0001d, $"Index {i}, location [{sampleLocations[i].x},{sampleLocations[i].y}]");
			}
		}

		[Test]
		[Property("Plugin", "Minecraft")]
		public void TestMCAFileHandling() {
			HeightData data = ImportManager.ImportFile(Path.Combine(inputPath, sampleMCAFile));
			data.lowPoint = 0;
			data.highPoint = 255;
			//var sampleLocations = GetSampleLocations(data.GridWidth, data.GridHeight);
			//var sourceSamples = GetHeightSamples(data, sampleLocations);
			currentJob.exportSettings.SetCustomSetting("mcaOffsetX", 16);
			currentJob.exportSettings.SetCustomSetting("mcaOffsetZ", 26);
			AssertExport(data, "MCR", sampleMCAFile);
			AssertExport(data, ImageExporter.HillshadePNG, sampleMCAFile);
			AssertExport(data, ImageExporter.HeightmapPNG8Bit, sampleMCAFile);
		}

		[Test]
		[Property("Plugin", "Minecraft")]
		public void TestNBTHandling()
		{
			using (var stream = RegionImporter.CreateGZipDecompressionStream(File.ReadAllBytes(Path.Combine(inputPath, sampleNBTFile))))
			{
				var nbt = new NBTContent(stream, false);
				CheckNBTContent(nbt.contents.GetAsCompound("Data"));
				//Write and reload the nbt from the new byte array (Tests correct serialization).
				var byteList = new List<byte>();
				nbt.WriteToBytes(byteList, false);
				var stream2 = new MemoryStream(byteList.ToArray());
				nbt = new NBTContent(stream2, false);
				CheckNBTContent(nbt.contents.GetAsCompound("Data"));
			}
		}

		private void CheckNBTContent(CompoundContainer nbt)
		{
			Assert.AreEqual(nbt.Get<byte>("sampleByte"), (byte)8);
			Assert.AreEqual(nbt.Get<short>("sampleShort"), (short)1616);
			Assert.AreEqual(nbt.Get<int>("sampleInt1"), 1234);
			Assert.AreEqual(nbt.Get<int>("sampleInt2"), -5678);
			Assert.AreEqual(nbt.Get<long>("sampleLong"), 12345678L);
			Assert.AreEqual(nbt.Get<float>("sampleFloat"), 32.32f);
			Assert.AreEqual(nbt.Get<double>("sampleDouble"), 64.64d);
			Assert.AreEqual(nbt.Get<string>("sampleString"), "Lorem ipsum dolor sit amet!");
			Assert.AreEqual(nbt.Get<byte[]>("sampleByteArray"), new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
			Assert.AreEqual(nbt.Get<int[]>("sampleIntArray"), new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512 });
			Assert.AreEqual(nbt.Get<long[]>("sampleLongArray"), new long[] { 1, 20, 300, 4000, 50000 });
			var comp = nbt.GetAsCompound("sampleCompound");
			Assert.AreEqual(comp.Get<byte>("byte1"), (byte)5);
			var nested = comp.GetAsCompound("nestedCompound");
			Assert.AreEqual(nested.Get<string>("string"), "Lorem ipsum");
			var list = nbt.GetAsList("sampleList");
			for (int i = 0; i < 4; i++)
			{
				Assert.AreEqual(list.Get<CompoundContainer>(i).Get<string>("string"), "Compound "+i);
			}
		}

		[Test]
		[Property("Plugin", "Minecraft")]
		public void TestMCAAccuracy() {
			var heights = HeightmapImporter.ImportHeightmapRaw(Path.Combine(inputPath, sampleHeightmapFile), 0, 0, 512, 512);
			HeightData data = new HeightData(512, 512, null) {
				lowPoint = 0,
				highPoint = 255
			};
			for(int i = 0; i < 512; i++) {
				for(int j = 0; j < 512; j++) {
					data.SetHeight(i, j, heights[i, j]);
				}
			}
			var sampleLocations = GetSampleLocations(data.GridWidth, data.GridHeight);
			var sourceSamples = GetHeightSamples(data, sampleLocations);
			string mcaname = "accuracy-test-r.0.0.mca";
			AssertExport(data, "MCR-RAW", mcaname);
			var reimported = ImportManager.ImportFile(Path.Combine(outputPath, mcaname));
			var convSamples = GetHeightSamples(reimported, sampleLocations);
			AssertExport(data, ImageExporter.HeightmapPNG8Bit, "reconstructed_mca.png");
			Assert.AreEqual(sourceSamples, convSamples);
		}

		[Test]
		[Property("Plugin", "Basic")]
		public void TestASCResizing() {
			HeightData data = ImportManager.ImportFile(Path.Combine(inputPath, sampleASCFile));
			var sampleLocationsOriginal = GetSampleLocations(data.GridWidth, data.GridHeight);
			var sourceSamples = GetHeightSamples(data, sampleLocationsOriginal);
			int scale = (int)(data.GridWidth * 1.39f);
			HeightData resized = new HeightData(data);
			resized.Resize(scale, false);
			HeightData rescaled = new HeightData(data); 
			rescaled.Resize(scale, true);
			//AssertExport(rescaled, ImageExporter.HillshadePNG, resizedASCFileHS);
			var resizedSamples = GetHeightSamples(resized, GetSampleLocations(resized.GridWidth, resized.GridHeight));
			double delta = 0.4f;
			for(int i = 0; i < sourceSamples.Length; i++) {
				Assert.AreEqual(sourceSamples[i], resizedSamples[i], delta, $"Index {i}, location in original [{sampleLocationsOriginal[i].x},{sampleLocationsOriginal[i].y}]");
			}
			Assert.AreEqual(data.highestValue, resized.highestValue, delta / 2);
			Assert.AreEqual(data.lowestValue, resized.lowestValue, delta / 2);
		}

		[Test]
		[Property("Plugin", "Basic")]
		public void TestASCAccurateResizing() {
			HeightData data = ImportManager.ImportFile(Path.Combine(inputPath, sampleASCFile));
			var sampleLocationsOriginal = GetSampleLocations(data.GridWidth, data.GridHeight);
			var sourceSamples = GetHeightSamples(data, sampleLocationsOriginal);
			HeightData resized = new HeightData(data);
			resized.Resize(data.GridWidth * 2, false);
			Assert.AreEqual(data.GridWidth * 2, resized.GridWidth);
			Assert.AreEqual(data.GridHeight * 2, resized.GridHeight);
			//AssertExport(data, ImageExporter.HeightmapPNG8Bit, "asc-original");
			//AssertExport(resized, ImageExporter.HeightmapPNG8Bit, "asc-resized");
			var resizedLocations = GetSampleLocations(resized.GridWidth, resized.GridHeight);
			var resizedSamples = GetHeightSamples(resized, resizedLocations);
			double delta = 0.05f;
			for(int i = 0; i < sourceSamples.Length; i++) {
				Assert.AreEqual(sourceSamples[i], resizedSamples[i], delta, $"Index {i}, location in original [{sampleLocationsOriginal[i].x},{sampleLocationsOriginal[i].y}], in resized [{resizedLocations[i].x},{resizedLocations[i].y}]");
			}
			Assert.AreEqual(data.highestValue, resized.highestValue, delta);
			Assert.AreEqual(data.lowestValue, resized.lowestValue, delta);
		}

		[Test]
		[Property("Plugin", "Minecraft")]
		public void TestMCAHeightRounding() {
			HeightData data = new HeightData(512, 512, null);
			for(int z = 0; z < 128; z++) {
				for(int x = 0; x < 512; x++) {
					float h1 = MathUtils.Clamp01(x / 510f);
					float h2 = MathUtils.Clamp01(x / 255f);
					float h3 = 0.5f;
					float h4 = (x+1) / 512f * 0.33f;
					data.SetHeight(x, z, h1);
					data.SetHeight(x, 128 + z, h2);
					data.SetHeight(x, 256 + z, h3);
					data.SetHeight(x, 384 + z, h4);
				}
			}
			data.cellSize = 1;
			data.lowPoint = 0;
			data.highPoint = 1;
			data.Rescale(0, 255);
			for(int x = 0; x < 512; x++) {
				data.SetHeight(x, 1, (float)Math.Round(data.GetHeight(x, 1), MidpointRounding.AwayFromZero));
			}
			var samples = GetHeightSamples(data, GetSampleLocations(512, 512));
			for(int i = 0; i < samples.Length; i++) {
				samples[i] = (float)Math.Round(samples[i], MidpointRounding.AwayFromZero);
			}

			AssertExport(data, "ASC", gradientMCAFile + "_asc");
			AssertExport(data, ImageExporter.HillshadePNG, gradientMCAFile + "_pre_hs");
			AssertExport(data, "MCR-RAW", gradientMCAFile);

			data = ImportManager.ImportFile(Path.Combine(outputPath, gradientMCAFile) + ".mca");
			AssertExport(data, ImageExporter.HeightmapPNG8Bit, gradientMCAFile + "_conv_hm");

			var mcaSamples = GetHeightSamples(data, GetSampleLocations(512, 512));

			Assert.AreEqual(samples, mcaSamples);
		}

		void AssertExport(HeightData data, string filetype, string path) {
			var format = ExportUtility.GetFormatFromIdenfifier(filetype);
			path = Path.ChangeExtension(Path.Combine(outputPath, path), format.Extension);
			var job = new ExportJob(data, format, new ExportSettings(), outputPath, path);
			job.Export();
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
					sampleLocations[i].x = x1 + (int)Math.Round((x2 + 1 - x1) * (x / 8f), MidpointRounding.AwayFromZero);
					sampleLocations[i].y = y1 + (int)Math.Round((y2 + 1 - y1) * (y / 8f), MidpointRounding.AwayFromZero);
				}
			}
			sampleLocations[64].x = x2;
			sampleLocations[64].y = y2;
			return sampleLocations;
		}

		float[] GetHeightSamples(HeightData data, (int x, int y)[] locations) {
			float[] samples = new float[locations.Length];
			for(int i = 0; i < locations.Length; i++) {
				var (x, y) = locations[i];
				samples[i] = data.GetHeight(x, y);
			}
			return samples;
		}
	}
}