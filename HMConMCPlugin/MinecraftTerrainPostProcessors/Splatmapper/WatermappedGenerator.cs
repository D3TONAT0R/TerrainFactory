using HMCon;
using HMCon.Util;
using HMConImage;
using MCUtils;
using System;
using System.IO;
using System.Xml.Linq;

namespace HMConMC.PostProcessors.Splatmapper
{
	public class WatermappedGenerator : Generator
	{

		short waterLevel = -1;
		public string waterBlock = "minecraft:water";
		byte[,] waterSurfaceMap;

		public WatermappedGenerator(string waterMapPath, int offsetX, int offsetZ, int sizeX, int sizeZ, short? waterLevel, string waterBlock)
		{
			waterSurfaceMap = ArrayConverter.Flip(HeightmapImporter.ImportHeightmapRaw(waterMapPath, offsetX, offsetZ, sizeX, sizeZ));
			if (waterLevel != null) this.waterLevel = waterLevel.Value;
			if (waterBlock != null) this.waterBlock = waterBlock;
			ConsoleOutput.WriteLine("Water mapping enabled");
		}

		public WatermappedGenerator(XElement xml, string rootPath, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			string path = Path.Combine(rootPath, xml.Element("file").Value);
			waterSurfaceMap = ArrayConverter.Flip(HeightmapImporter.ImportHeightmapRaw(path, offsetX, offsetZ, sizeX, sizeZ));
			if (xml.Element("waterlevel") != null) waterLevel = short.Parse(xml.Element("waterlevel").Value);
			if (xml.Element("waterblock") != null) waterBlock = xml.Element("waterblock").Value;
			ConsoleOutput.WriteLine("Water mapping enabled");
		}

		public override void RunGenerator(World world, int x, int y, int z)
		{
			short start = Math.Max(waterSurfaceMap?[x, z] ?? (short)-1, waterLevel);
			for (short y2 = start; y2 > y; y2--)
			{
				if (world.IsAir(x, y2, z))
				{
					world.SetBlock(x, y2, z, waterBlock);
				}
			}
		}
	}
}