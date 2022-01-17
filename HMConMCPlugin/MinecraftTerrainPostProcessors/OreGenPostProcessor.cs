using MCUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace HMConMC.PostProcessors.Splatmapper
{
	public class OreGenPostProcessor : AbstractPostProcessor
	{

		public class Layer
		{
			public List<OreGenerator> ores = new List<OreGenerator>();

			public void ProcessBlockColumn(World world, Random random, int x, int z, float mask, float rarityMul)
			{
				foreach(var ore in ores)
				{
					if (Chance(random, ore.spawnsPerColumn * rarityMul * mask))
					{
						ore.Generate(world, random, x, z);
					}
				}
			}

			private bool Chance(Random random, float prob)
			{
				return random.NextDouble() <= prob;
			}
		}

		public OreGenPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			random = new Random();
			rarityMul = float.Parse(xml.Element("multiplier")?.Value ?? "1");
			var map = xml.Element("map");
			if (map != null)
			{
				string mapFileName = Path.Combine(rootPath, xml.Attribute("file").Value);
				splatMask = SplatmapImporter.GetSplatMask(mapFileName, offsetX, offsetZ, sizeX, sizeZ);
				foreach(var elem in map.Elements())
				{
					string name = elem.Name.LocalName.ToLower();
					if(name == "r" || name == "red")
					{
						RegisterLayer(0, elem);
					}
					else if(name == "g" || name == "green")
					{
						RegisterLayer(1, elem);
					}
					else if(name == "b" || name == "blue")
					{
						RegisterLayer(2, elem);
					}
					else if(name == "a" || name == "alpha")
					{
						RegisterLayer(3, elem);
					}
					else if(name == "n" || name == "none")
					{
						RegisterLayer(-1, elem);
					}
					else
					{
						throw new ArgumentException("Unknown channel name: " + name);
					}
				}
			}
			else
			{
				Console.WriteLine("Generating ores with default settings.");
				var defaultLayer = new Layer();
				defaultLayer.ores.AddRange(defaultVanillaOres);
				layers.Add(-1, defaultLayer);
			}
		}

		private void RegisterLayer(int maskChannelIndex, XElement elem)
		{
			Layer layer;
			if (layers.ContainsKey(maskChannelIndex))
			{
				layer = layers[maskChannelIndex];
			}
			else
			{
				layer = new Layer();
				layers.Add(maskChannelIndex, layer);
			}
			foreach (var oreElem in elem.Elements())
			{
				var elemName = oreElem.Name.LocalName.ToLower();
				if (elemName == "gen")
				{
					layer.ores.Add(OreGenerator.ParseFromXML(oreElem));
				}
				else if(elemName == "default")
				{
					layer.ores.AddRange(defaultVanillaOres);
				}
				else
				{
					throw new ArgumentException("Unexpected element name: " + elemName);
				}
			}
		}

		public static readonly List<OreGenerator> defaultVanillaOres = new List<OreGenerator>()
		{
			new OreGenerator("iron_ore", 9, 12f, 2, 66),
			new OreGenerator("coal_ore", 32, 15f, 16, 120),
			new OreGenerator("gold_ore", 8, 1.2f, 2, 32),
			new OreGenerator("diamond_ore", 8, 0.4f, 2, 24),
			new OreGenerator("redstone_ore", 10, 3.5f, 4, 36),
			new OreGenerator("lapis_ore", 9, 0.8f, 4, 28)
		};

		public Dictionary<int, Layer> layers = new Dictionary<int, Layer>();
		public float[][,] splatMask;
		public float rarityMul = 1;

		private Random random;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		protected override void OnProcessSurface(World world, int x, int y, int z, int pass, float mask)
		{
			if (y < 4) return;
			foreach(var l in layers)
			{
				if(l.Key > -1)
				{
					mask *= splatMask[l.Key][x,z];
				}
				if(mask > 0.001f)
				{
					l.Value.ProcessBlockColumn(world, random, x, z, mask, rarityMul);
				}
			}
		}
	}
}
