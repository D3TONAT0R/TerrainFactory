using HMCon;
using HMCon.Util;
using HMConImage;
using MCUtils;
using MCUtils.Coordinates;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;

namespace HMConMC.PostProcessors.Splatmapper
{
	public class WorldMergerPostProcessor : AbstractPostProcessor
	{
		int upperLeftCornerRegionX;
		int upperLeftCornerRegionZ;
		string otherRegionFolder;
		string otherRegionPrefix = "";
		bool chunkMode = false;
		float threshold = 0.5f;

		public WorldMergerPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ)
			: base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			if (mask == null)
			{
				throw new NullReferenceException("mask is not set in world merger");
			}
			otherRegionFolder = Path.Combine(rootPath, xml.Element("regions").Value);
			otherRegionPrefix = xml.Element("prefix")?.Value ?? "";
			upperLeftCornerRegionX = int.Parse(xml.Element("origin_x").Value);
			upperLeftCornerRegionZ = int.Parse(xml.Element("origin_z").Value);
			chunkMode = xml.Element("mode")?.Value.ToLower() == "chunk";
			threshold = float.Parse(xml.Element("threshold")?.Value ?? "0.5");
		}

		public override PostProcessType PostProcessorType => PostProcessType.RegionOnly;

		public override void ProcessRegion(World world, MCUtils.Region reg, int rx, int rz, int pass)
		{
			ConsoleOutput.WriteLine($"Starting merge for region [{rx},{rz}] ...");
			int scale = chunkMode ? 32 : 512;
			bool[,] fraction;
			lock (mask)
			{
				fraction = GetSubMask((rx - upperLeftCornerRegionX) * scale, (rz - upperLeftCornerRegionZ) * scale, scale, scale);
			}
			string otherRegionName = otherRegionPrefix + $"r.{rx}.{rz}.mca";
			var filename = Path.Combine(otherRegionFolder, otherRegionName);
			if (File.Exists(filename))
			{
				var otherRegion = RegionLoader.LoadRegion(filename);
				var merger = new RegionMerger(otherRegion, reg, fraction);
				var mergedRegion = merger.Merge();
				for (int x = 0; x < 32; x++)
				{
					for (int z = 0; z < 32; z++)
					{
						reg.chunks[x, z] = mergedRegion.chunks[x, z];
					}
				}
			}
			else
			{
				ConsoleOutput.WriteWarning($"Merge region '{otherRegionName}' was not found, no merging was done");
			}
		}

		private bool[,] GetSubMask(int x, int y, int width, int height)
		{
			bool[,] subMask = new bool[width, height];
			for (int x1 = 0; x1 < width; x1++)
			{
				for (int y1 = 0; y1 < height; y1++)
				{
					subMask[x1, y1] = mask.GetValue(x + x1, y + y1) >= threshold;
				}
			}
			return subMask;
		}
	}
}