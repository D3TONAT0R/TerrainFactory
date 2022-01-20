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

		public class OreGenLayer : Layer
		{
			public List<OreGenerator> ores = new List<OreGenerator>();
			public float multiplier = 1;

			public override void ProcessBlockColumn(World world, Random random, int x, int topY, int z, float mask)
			{
				foreach(var ore in ores)
				{
					if (Chance(random, ore.spawnsPerColumn * multiplier * mask))
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

		public static readonly List<OreGenerator> defaultVanillaOres = new List<OreGenerator>()
		{
			new OreGenerator("iron_ore", 9, 8f, 2, 66),
			new OreGenerator("coal_ore", 32, 10f, 16, 120),
			new OreGenerator("gold_ore", 8, 1f, 2, 32),
			new OreGenerator("diamond_ore", 8, 0.25f, 2, 24),
			new OreGenerator("redstone_ore", 10, 3.2f, 4, 36),
			new OreGenerator("lapis_ore", 9, 0.6f, 4, 28)
		};

		public Dictionary<int, Layer> layers = new Dictionary<int, Layer>();
		public Weightmap<float> weightmap;
		public float rarityMul = 1;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public OreGenPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			random = new Random();
			rarityMul = float.Parse(xml.Element("multiplier")?.Value ?? "1");
			var map = xml.Element("map");
			weightmap = LoadWeightmapAndLayers(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ, layers, CreateLayer);
			if (weightmap == null)
			{
				Console.WriteLine("Generating ores with default settings.");
				var defaultLayer = new OreGenLayer();
				defaultLayer.ores.AddRange(defaultVanillaOres);
				layers.Add(-1, defaultLayer);
			}
		}

		private Layer CreateLayer(XElement elem)
		{
			var layer = new OreGenLayer();
			foreach (var oreElem in elem.Elements())
			{
				var elemName = oreElem.Name.LocalName.ToLower();
				if (elemName == "gen")
				{
					layer.ores.Add(new OreGenerator(oreElem));
				}
				else if(elemName == "default")
				{
					layer.ores.AddRange(defaultVanillaOres);
				}
				else if(elemName == "multiplier")
				{
					layer.multiplier = float.Parse(oreElem.Value);
				}
				else
				{
					throw new ArgumentException("Unexpected element name: " + elemName);
				}
			}
			return layer;
		}

		protected override void OnProcessSurface(World world, int x, int y, int z, int pass, float mask)
		{
			if (y < 4) return;
			ProcessSplatmapLayersSurface(layers, weightmap, world, x, y, z, pass, mask);
		}
	}
}
