using HMCon;
using HMCon.Util;
using HMConImage;
using MCUtils;
using System;
using System.Drawing;
using System.IO;
using System.Xml.Linq;

namespace HMConMC.PostProcessors.Splatmapper
{
	public class MaskedWorldMerger : Generator
	{

		Bitmap mask;
		int upperLeftCornerRegionX;
		int upperLeftCornerRegionZ;
		string otherRegionFolder;
		string otherRegionPrefix = "";
		bool chunkMode = false;

		public MaskedWorldMerger(XElement xml, string rootPath, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			string path = Path.Combine(rootPath, xml.Element("mask").Value);
			otherRegionFolder = Path.Combine(rootPath, xml.Element("regions").Value);
			otherRegionPrefix = xml.Element("prefix")?.Value ?? "";
			upperLeftCornerRegionX = int.Parse(xml.Element("origin_x").Value);
			upperLeftCornerRegionZ = int.Parse(xml.Element("origin_z").Value);
			chunkMode = xml.Element("mode")?.Value.ToLower() == "chunk";
			mask = new Bitmap(path);
		}

		public override void RunGeneratorForRegion(World w, MCUtils.Region r, int rx, int rz)
		{
			ConsoleOutput.WriteLine($"Starting merge for region [{rx},{rz}] ...");
			int scale = chunkMode ? 32 : 512;
			Bitmap fraction;
			lock (mask)
			{
				fraction = mask.Clone(new Rectangle((rx - upperLeftCornerRegionX) * scale, (rz - upperLeftCornerRegionZ) * scale, scale, scale), mask.PixelFormat);
			}
			string otherRegionName = otherRegionPrefix + $"r.{rx}.{rz}.mca";
			var filename = Path.Combine(otherRegionFolder, otherRegionName);
			if (File.Exists(filename))
			{
				var otherRegion = RegionImporter.OpenRegionFile(filename);
				var merger = new RegionMerger(otherRegion, r, fraction);
				var mergedRegion = merger.Merge();
				for (int x = 0; x < 32; x++)
				{
					for (int z = 0; z < 32; z++)
					{
						r.chunks[x, z] = mergedRegion.chunks[x, z];
					}
				}
			}
			else
			{
				ConsoleOutput.WriteWarning($"Merge region '{otherRegionName}' was not found, no merging was done");
			}
		}
	}
}