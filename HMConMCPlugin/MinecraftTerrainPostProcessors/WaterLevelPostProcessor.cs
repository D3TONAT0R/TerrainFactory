using HMCon;
using HMCon.Util;
using HMConImage;
using MCUtils;
using System;
using System.IO;
using System.Xml.Linq;

namespace HMConMC.PostProcessors.Splatmapper
{
	public class WaterLevelPostProcessor : AbstractPostProcessor
	{

		int waterLevel = 62;
		public string waterBlock = "minecraft:water";
		byte[,] waterSurfaceMap;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public WaterLevelPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			worldOriginOffsetX = offsetX;
			worldOriginOffsetZ = offsetZ;
			var fileXml = xml.Element("file");
			if (fileXml != null)
			{
				string path = Path.Combine(rootPath, xml.Element("file").Value);
				waterSurfaceMap = ArrayConverter.Flip(HeightmapImporter.ImportHeightmapRaw(path, 0, 0, sizeX, sizeZ));
			}
			xml.TryParseInt("waterlevel", ref waterLevel);
			if (xml.Element("waterblock") != null) waterBlock = xml.Element("waterblock").Value;
			ConsoleOutput.WriteLine("Water mapping enabled");
		}

		protected override void OnProcessSurface(World world, int x, int y, int z, int pass, float mask)
		{
			int start = waterLevel;
			if (waterSurfaceMap != null)
			{
				start = Math.Max(waterSurfaceMap?[x - worldOriginOffsetX, z - worldOriginOffsetZ] ?? (short)-1, waterLevel);
			}
			for (int y2 = start; y2 > y; y2--)
			{
				if (world.IsAir(x, y2, z))
				{
					world.SetBlock(x, y2, z, waterBlock);
				}
			}
		}
	}
}