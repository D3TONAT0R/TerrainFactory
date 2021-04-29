
using HMCon;
using HMCon.Export;
using HMCon.Util;
using HMConImage;
using HMConMC.MinecraftTerrainPostProcessors;
using HMConMC.PostProcessors;
using HMConMCPlugin;
using Ionic.Zlib;
using MCUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static MCUtils.NBTContent;

namespace HMConMC {
	public class MCWorldExporter : IExporter {

		public static readonly string defaultBlock = "minecraft:stone";

		public World world;
		public byte[,] heightmap;

		public int regionNumX;
		public int regionNumZ;

		public int heightmapLengthX;
		public int heightmapLengthZ;

		public Bounds worldBounds;

		public List<MinecraftTerrainPostProcessor> postProcessors;

		public MCWorldExporter(float[,] hmapFlipped) {
			int xmin = CurrentExportJobInfo.mcaGlobalPosX * 512;
			int zmin = CurrentExportJobInfo.mcaGlobalPosZ * 512;
			heightmapLengthX = hmapFlipped.GetLength(0);
			heightmapLengthZ = hmapFlipped.GetLength(1);
			worldBounds = new Bounds(xmin, zmin, xmin + heightmapLengthX - 1, zmin + heightmapLengthZ - 1);
			heightmap = new byte[heightmapLengthX, heightmapLengthZ];
			for(int x = 0; x < heightmapLengthX; x++) {
				for(int z = 0; z < heightmapLengthZ; z++) {
					heightmap[x, z] = (byte)MathUtils.Clamp((float)Math.Round(hmapFlipped[x, z], MidpointRounding.AwayFromZero), 0, 255);
				}
			}
			regionNumX = (int)Math.Ceiling(heightmapLengthX / 512f);
			regionNumZ = (int)Math.Ceiling(heightmapLengthZ / 512f);
			if(heightmapLengthX % 512 > 0 || heightmapLengthZ % 512 > 0) {
				ConsoleOutput.WriteWarning("Input heightmap is not a multiple of 512. Void borders will be present in the world.");
			}
		}

		public MCWorldExporter(float[,] hmapFlipped, bool useDefaultPostProcessors, bool useSplatmaps) : this(hmapFlipped) {
			postProcessors = new List<MinecraftTerrainPostProcessor>();
			if(useSplatmaps) {
				postProcessors.Add(new SplatmappedSurfacePostProcessor(this, CurrentExportJobInfo.importedFilePath, 255, 0, 0, hmapFlipped.GetLength(0), hmapFlipped.GetLength(1)));
			}
			if(useDefaultPostProcessors) {
				if(!useSplatmaps) {
					postProcessors.Add(new NaturalTerrainPostProcessor(true));
					postProcessors.Add(new VegetationPostProcessor(0.1f, 0.01f));
				}
				postProcessors.AddRange(new MinecraftTerrainPostProcessor[] {
					new BedrockPostProcessor(),
					new CavesPostProcessor(),
					new OrePostProcessor(2),
				});
			}
		}

		public bool NeedsFileStream(FileFormat format) {
			return format.Identifier.StartsWith("MCR");
		}

		private void CreateWorld() {
			world = new World(CurrentExportJobInfo.mcaGlobalPosX, CurrentExportJobInfo.mcaGlobalPosZ, CurrentExportJobInfo.mcaGlobalPosX + regionNumX - 1, CurrentExportJobInfo.mcaGlobalPosZ + regionNumZ - 1);
			MakeBaseTerrain();
			DecorateTerrain();
			MakeBiomeArray();
		}

		private void MakeBaseTerrain() {
			int ox = CurrentExportJobInfo.mcaGlobalPosX * 512;
			int oz = CurrentExportJobInfo.mcaGlobalPosZ * 512;
			for(int x = 0; x < heightmapLengthX; x++) {
				for(int z = 0; z < heightmapLengthZ; z++) {
					for(int y = 0; y <= heightmap[x, z]; y++) {
						world.SetDefaultBlock(ox + x, y, oz + z);
					}
				}
				if((x + 1) % 8 == 0) ConsoleOutput.WriteProgress("Generating base terrain", (x + 1) / (float)heightmapLengthX);
			}
		}

		private void DecorateTerrain() {
			//Sort the postProcessors by priority
			postProcessors = postProcessors.OrderBy(post => post.OrderPriority).ToList();

			int i = 0;
			foreach(var post in postProcessors) {
				string name = post.GetType().Name;
				if(post.PostProcessorType == PostProcessType.Block || post.PostProcessorType == PostProcessType.Both) {
					//Iterate the postprocessors over every block
					for(int x = 0; x < heightmapLengthX; x++) {
						for(int z = 0; z < heightmapLengthZ; z++) {
							for(int y = post.BlockProcessYMin; y <= Math.Min(heightmap[x, z], post.BlockProcessYMax); y++) {
								post.ProcessBlock(world, x, y, z);
							}
						}
						if((x + 1) % 8 == 0) ConsoleOutput.WriteProgress($"{i + 1}/{postProcessors.Count} Decorating terrain [{name}]", (x + 1) / (float)heightmapLengthX);
					}
				}
				if(post.PostProcessorType == PostProcessType.Surface || post.PostProcessorType == PostProcessType.Both) {
					//Iterate the postprocessors over every surface block
					for(int x = 0; x < heightmapLengthX; x++) {
						for(int z = 0; z < heightmapLengthZ; z++) {
							post.ProcessSurface(world, x, heightmap[x, z], z);
						}
						if((x + 1) % 8 == 0) ConsoleOutput.WriteProgress($"{i + 1}/{postProcessors.Count} Decorating surface [{name}]", (x + 1) / (float)heightmapLengthX);
					}
				}
				i++;
			}
			foreach(var post in postProcessors) {
				post.OnFinish(world);
			}
		}

		private void MakeBiomeArray() {
			foreach(Region r in world.regions.Values) r.MakeBiomeArray();
		}

		public void WriteFile(FileStream stream, string path, FileFormat filetype) {
			CreateWorld();
			if(filetype.IsFormat("MCR") || filetype.IsFormat("MCR-RAW")) {
				world.WriteRegionFile(stream, CurrentExportJobInfo.mcaGlobalPosX, CurrentExportJobInfo.mcaGlobalPosZ);
			} else {
				path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
				Directory.CreateDirectory(path);
				var mapPath = Path.Combine(path, "overviewmap.png");
				using(var mapStream = new FileStream(mapPath, FileMode.Create)) {
					var mapExporter = new OverviewmapExporter(this);
					mapExporter.WriteFile(mapStream, mapPath, null);
				}
				world.WriteWorldSave(path);
			}
		}

		public short[,] GetHeightmap(HeightmapType type, bool keepFlippedZ) {
			var hm = world.GetHeightmap(worldBounds.xMin, worldBounds.yMin, worldBounds.xMax, worldBounds.yMax, type);
			if(!keepFlippedZ) {
				hm = ArrayConverter.Flip(hm);
			}
			return hm;
		}
	}
}