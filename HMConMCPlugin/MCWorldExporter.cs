
using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Util;
using HMConImage;
using HMConMC.PostProcessors;
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
	public class MCWorldExporter
	{

		public static readonly string defaultBlock = "minecraft:stone";

		public MCUtils.Version desiredVersion;
		public World world;
		public byte[,] heightmap;

		public int regionNumX;
		public int regionNumZ;

		public int regionOffsetX;
		public int regionOffsetZ;

		public int heightmapLengthX;
		public int heightmapLengthZ;

		public Bounds worldBounds;

		public WorldPostProcessingStack postProcessor = null;

		public MCWorldExporter(ExportJob job)
		{
			regionOffsetX = job.exportNumX + job.settings.GetCustomSetting("mcaOffsetX", 0);
			regionOffsetZ = job.exportNumZ + job.settings.GetCustomSetting("mcaOffsetZ", 0);
			if(job.settings.HasCustomSetting<string>("version"))
			{
				desiredVersion = MCUtils.Version.Parse(job.settings.GetCustomSetting("version", "")); 
			}
			else
			{
				desiredVersion = MCUtils.Version.DefaultVersion;
			}
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

		public MCWorldExporter(ExportJob job, bool customPostProcessing, bool useDefaultPostProcessing) : this(job)
		{
			if (customPostProcessing)
			{
				string xmlPath;
				if (job.settings.HasCustomSetting<string>("mcpostfile")) {
					xmlPath = Path.Combine(Path.GetDirectoryName(job.data.filename), job.settings.GetCustomSetting("mcpostfile", ""));
					if (Path.GetExtension(xmlPath).Length == 0) xmlPath += ".xml";
				} else {
					xmlPath = Path.ChangeExtension(job.FilePath, null) + "-postprocess.xml";
				}
				try
				{
					postProcessor = WorldPostProcessingStack.CreateFromXML(job.FilePath, xmlPath, 255, regionOffsetX * 512, regionOffsetZ * 512, job.data.GridWidth, job.data.GridHeight);
				}
				catch(Exception e)
				{
					if(useDefaultPostProcessing)
					{
						ConsoleOutput.WriteWarning("Failed to create post processing stack from xml, falling back to default post processing stack. " + e.Message);
						postProcessor = WorldPostProcessingStack.CreateDefaultPostProcessor(job.FilePath, 255, regionOffsetX * 512, regionOffsetZ * 512, job.data.GridWidth, job.data.GridHeight);
					}
					else
					{
						ConsoleOutput.WriteError("Failed to create post processing stack from xml, the terrain will not be decorated. " + e.Message);
					}
				}
			}
			else if(useDefaultPostProcessing)
			{
				postProcessor = WorldPostProcessingStack.CreateDefaultPostProcessor(job.FilePath, 255, regionOffsetX * 512, regionOffsetZ * 512, job.data.GridWidth, job.data.GridHeight);
			}
		}

		private void CreateWorld(string worldName)
		{
			world = new World(desiredVersion, regionOffsetX, regionOffsetZ, regionOffsetX + regionNumX - 1, regionOffsetZ + regionNumZ - 1);
			world.worldName = worldName;
			MakeBaseTerrain();
			DecorateTerrain();
		}

		private void MakeBaseTerrain()
		{
			int progress = 0;
			int iterations = (int)Math.Ceiling(heightmapLengthX / 16f);
			Parallel.For(0, iterations, (int cx) => {
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
					progress++;
					ConsoleOutput.UpdateProgressBar("Generating base terrain", progress / (float)iterations);
				}
			);
		}

		public void DecorateTerrain()
		{
			if(postProcessor != null)
			{
				postProcessor.DecorateTerrain(this);
			}
		}

		public void WriteFile(string path, FileStream stream, FileFormat filetype)
		{
			string name = Path.GetFileNameWithoutExtension(path);
			CreateWorld(name);
			if (filetype is MCRegionFormat)
			{
				world.WriteRegionFile(stream, regionOffsetX, regionOffsetZ);
			}
			else if(filetype is MCWorldFormat)
			{
				path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
				Directory.CreateDirectory(path);
				var mapPath = Path.Combine(path, "overviewmap.png");
				using (var mapStream = new FileStream(mapPath, FileMode.Create))
				{
					var mapExporter = new OverviewmapExporter(this, true);
					mapExporter.WriteFile(mapStream, mapPath);
				}
				mapPath = Path.Combine(path, "overviewmap_no-water.png");
				using (var mapStream = new FileStream(mapPath, FileMode.Create))
				{
					var mapExporter = new OverviewmapExporter(this, true, HeightmapType.SolidBlocksNoLiquid);
					mapExporter.WriteFile(mapStream, mapPath);
				}
				world.WriteWorldSave(path, regionOffsetX * 512 + 50, regionOffsetZ * 512 + 50);
			}
			else
			{
				throw new InvalidOperationException("Unsupported format: " + filetype.Identifier);
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