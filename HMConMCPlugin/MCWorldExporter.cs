
using HMCon;
using HMCon.Export;
using HMCon.Util;
using HMConImage;
using HMConMC.PostProcessors;
using HMConMCPlugin;
using Ionic.Zlib;
using MCUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MCUtils.NBTContent;

namespace HMConMC
{
	public class MCWorldExporter : IExporter
	{

		public static readonly string defaultBlock = "minecraft:stone";

		public World world;
		public byte[,] heightmap;

		public int regionNumX;
		public int regionNumZ;

		public int regionOffsetX;
		public int regionOffsetZ;

		public int heightmapLengthX;
		public int heightmapLengthZ;

		public Bounds worldBounds;

		public List<MinecraftTerrainPostProcessor> postProcessors;

		public MCWorldExporter(ExportJob job)
		{
			regionOffsetX = job.exportNumX + job.settings.GetCustomSetting("mcaOffsetX", 0);
			regionOffsetZ = job.exportNumZ + job.settings.GetCustomSetting("mcaOffsetZ", 0);
			int xmin = regionOffsetX * 512;
			int zmin = regionOffsetZ * 512;
			var hmapFlipped = job.data.GetDataGridFlipped();
			heightmapLengthX = hmapFlipped.GetLength(0);
			heightmapLengthZ = hmapFlipped.GetLength(1);
			worldBounds = new Bounds(xmin, zmin, xmin + heightmapLengthX - 1, zmin + heightmapLengthZ - 1);
			heightmap = new byte[heightmapLengthX, heightmapLengthZ];
			for (int x = 0; x < heightmapLengthX; x++)
			{
				for (int z = 0; z < heightmapLengthZ; z++)
				{
					heightmap[x, z] = (byte)MathUtils.Clamp((float)Math.Round(hmapFlipped[x, z], MidpointRounding.AwayFromZero), 0, 255);
				}
			}
			regionNumX = (int)Math.Ceiling(heightmapLengthX / 512f);
			regionNumZ = (int)Math.Ceiling(heightmapLengthZ / 512f);
			if (heightmapLengthX % 512 > 0 || heightmapLengthZ % 512 > 0)
			{
				ConsoleOutput.WriteWarning("Input heightmap is not a multiple of 512. Void borders will be present in the world.");
			}
		}

		public MCWorldExporter(ExportJob job, bool useDefaultPostProcessors, bool useSplatmaps) : this(job)
		{
			postProcessors = new List<MinecraftTerrainPostProcessor>();
			if (useSplatmaps)
			{
				postProcessors.Add(new SplatmappedSurfacePostProcessor(job.data.filename, 255, regionOffsetX * 512, regionOffsetZ * 512, job.data.GridWidth, job.data.GridHeight));
			}
			if (useDefaultPostProcessors)
			{
				if (!useSplatmaps)
				{
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

		public bool NeedsFileStream(FileFormat format)
		{
			return format.Identifier.StartsWith("MCR");
		}

		//private void GetMCAOffset(Job job, out int offsetX, out int offsetZ) {

		//}

		private void CreateWorld()
		{
			world = new World(regionOffsetX, regionOffsetZ, regionOffsetX + regionNumX - 1, regionOffsetZ + regionNumZ - 1);
			MakeBaseTerrain();
			DecorateTerrain();
			MakeBiomeArray();
		}

		private void MakeBaseTerrain()
		{
			int progress = 0;
			Parallel.For(0, (int)Math.Ceiling(heightmapLengthX / 16f),
				delegate (int cx, ParallelLoopState s)
				{
					for (int bx = 0; bx < Math.Min(16, heightmapLengthX - cx * 16); bx++)
					{
						int x = cx * 16 + bx;
						for (int z = 0; z < heightmapLengthZ; z++)
						{
							for (int y = 0; y <= heightmap[x, z]; y++)
							{
								world.SetDefaultBlock(regionOffsetX * 512 + x, y, regionOffsetZ * 512 + z);
							}
						}
					}
				}
			);
		}

		private void DecorateTerrain()
		{
			//Sort the postProcessors by priority
			postProcessors = postProcessors.OrderBy(post => post.OrderPriority).ToList();

			int processorIndex = 0;
			foreach (var post in postProcessors)
			{
				for (int pass = 0; pass < post.NumberOfPasses; pass++)
				{
					string name = post.GetType().Name;
					if (post.PostProcessorType == PostProcessType.Block || post.PostProcessorType == PostProcessType.Both)
					{
						//Iterate the postprocessors over every block
						for (int x = 0; x < heightmapLengthX; x++)
						{
							for (int z = 0; z < heightmapLengthZ; z++)
							{
								for (int y = post.BlockProcessYMin; y <= Math.Min(heightmap[x, z], post.BlockProcessYMax); y++)
								{
									post.ProcessBlock(world, x, y, z, pass);
								}
							}
							//TODO: Account for multiple passes
							if ((x + 1) % 8 == 0) ConsoleOutput.WriteProgress($"{processorIndex + 1}/{postProcessors.Count} Decorating terrain [{name}]", (x + 1) / (float)heightmapLengthX);
						}
					}

					if (post.PostProcessorType == PostProcessType.Surface || post.PostProcessorType == PostProcessType.Both)
					{
						//Iterate the postprocessors over every surface block
						for (int x = 0; x < heightmapLengthX; x++)
						{
							for (int z = 0; z < heightmapLengthZ; z++)
							{
								post.ProcessSurface(world, x + regionOffsetX * 512, heightmap[x, z], z + regionOffsetZ * 512, pass);
							}
							//TODO: Account for multiple passes
							if ((x + 1) % 8 == 0) ConsoleOutput.WriteProgress($"{processorIndex + 1}/{postProcessors.Count} Decorating surface [{name}]", (x + 1) / (float)heightmapLengthX);
						}
					}

					//Run every postprocessor once for every region (rarely used)
					foreach (var reg in world.regions.Values)
					{
						post.ProcessRegion(world, reg, reg.regionPosX, reg.regionPosZ, pass);
					}

				}
				processorIndex++;
			}
			foreach (var post in postProcessors)
			{
				post.OnFinish(world);
			}
		}

		private void MakeBiomeArray()
		{
			foreach (Region r in world.regions.Values) r.MakeBiomeArray();
		}

		public void WriteFile(FileStream stream, string path, FileFormat filetype)
		{
			CreateWorld();
			if (filetype.IsFormat("MCR") || filetype.IsFormat("MCR-RAW"))
			{
				world.WriteRegionFile(stream, regionOffsetX, regionOffsetZ);
			}
			else
			{
				path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
				Directory.CreateDirectory(path);
				var mapPath = Path.Combine(path, "overviewmap.png");
				using (var mapStream = new FileStream(mapPath, FileMode.Create))
				{
					var mapExporter = new OverviewmapExporter(this, true);
					mapExporter.WriteFile(mapStream, mapPath, null);
				}
				mapPath = Path.Combine(path, "overviewmap_no-water.png");
				using (var mapStream = new FileStream(mapPath, FileMode.Create))
				{
					var mapExporter = new OverviewmapExporter(this, true, HeightmapType.SolidBlocksNoLiquid);
					mapExporter.WriteFile(mapStream, mapPath, null);
				}
				world.WriteWorldSave(path, regionOffsetX * 512 + 50, regionOffsetZ * 512 + 50);
			}
		}

		public short[,] GetHeightmap(HeightmapType type, bool keepFlippedZ)
		{
			var hm = world.GetHeightmap(worldBounds.xMin, worldBounds.yMin, worldBounds.xMax, worldBounds.yMax, type);
			if (!keepFlippedZ)
			{
				hm = ArrayConverter.Flip(hm);
			}
			return hm;
		}
	}
}