﻿using HMCon;
using HMConMC.PostProcessors.Splatmapper;
using MCUtils;
using NoiseGenerator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{
	public abstract class SurfaceLayerGenerator
	{
		public abstract bool Generate(World w, int x, int y, int z);

		protected bool SetBlock(World w, int x, int y, int z, string b)
		{
			if (!string.IsNullOrWhiteSpace(b) && !w.IsAir(x, y, z))
			{
				return w.SetBlock(x, y, z, b);
			}
			else
			{
				return false;
			}
		}
	}

	public class StandardSurfaceLayerGenerator : SurfaceLayerGenerator
	{
		public List<string> blocks = new List<string>();

		public StandardSurfaceLayerGenerator(IEnumerable<string> blockLayer)
		{
			blocks.AddRange(blockLayer);
		}

		public StandardSurfaceLayerGenerator(params string[] blockLayer) : this((IEnumerable<string>)blockLayer)
		{

		}

		public override bool Generate(World w, int x, int y, int z)
		{
			bool b = false;
			for (int i = 0; i < blocks.Count; i++)
			{
				b |= SetBlock(w, x, y - i, z, blocks[i]);
			}
			return b;
		}
	}

	public class PerlinSurfaceLayerGenerator : StandardSurfaceLayerGenerator
	{
		private PerlinGenerator perlinGen;

		public float perlinThreshold;

		public PerlinSurfaceLayerGenerator(IEnumerable<string> blockLayer, float scale, float threshold) : base(blockLayer)
		{
			scale *= 6f;
			perlinGen = new PerlinGenerator(scale);
			perlinThreshold = threshold;
		}

		public override bool Generate(World w, int x, int y, int z)
		{
			if (perlinGen.GetPerlinAtCoord(x, z) < perlinThreshold)
			{
				return base.Generate(w, x, y, z);
			}
			else
			{
				return false;
			}
		}
	}

	public class SchematicInstanceGenerator : SurfaceLayerGenerator
	{

		private float chance;
		private Schematic schematic;
		private string block;

		private Random random = new Random();

		public SchematicInstanceGenerator(Schematic schem, float chance)
		{
			this.chance = chance;
			schematic = schem;
		}

		public SchematicInstanceGenerator(string blockID, float chance)
		{
			this.chance = chance;
			block = blockID;
		}

		public override bool Generate(World world, int x, int y, int z)
		{
			if (!Blocks.IsPlantSustaining(world.GetBlockState(x, y, z)) || !world.IsAir(x, y + 1, z)) return false;

			if (random.NextDouble() < chance / 128f)
			{
				if (schematic != null)
				{
					return schematic.Build(world, x, y + 1, z, random);
				}
				else
				{
					return world.SetBlock(x, y, z, block);
				}
			}
			else
			{
				return false;
			}
		}
	}

	public class BiomeGenerator : SurfaceLayerGenerator
	{
		private byte biomeID;

		public BiomeGenerator(byte biome)
		{
			biomeID = biome;
		}

		public override bool Generate(World w, int x, int y, int z)
		{
			w.SetBiome(x, z, biomeID);
			return true;
		}
	}


	public class SurfaceLayer
	{

		public string name;
		public Color layerColor;
		public List<SurfaceLayerGenerator> generators = new List<SurfaceLayerGenerator>();

		public SurfaceLayer(Color color, string name = null)
		{
			layerColor = color;
			this.name = name;
		}

		public bool AddSurfaceGenerator(XElement xml)
		{
			string type = xml.Attribute("type")?.Value ?? "standard";
			string[] blocks = xml.Attribute("blocks").Value.Split(',');
			if (type == "standard" || string.IsNullOrWhiteSpace(type))
			{
				generators.Add(new StandardSurfaceLayerGenerator(blocks));
				return true;
			}
			else if (type == "perlin")
			{
				float scale = float.Parse(xml.Attribute("scale")?.Value ?? "1.0");
				float threshold = float.Parse(xml.Attribute("threshold")?.Value ?? "0.5");
				generators.Add(new PerlinSurfaceLayerGenerator(blocks, scale, threshold));
				return false;
			}
			else
			{
				ConsoleOutput.WriteError("Unknwon generator type: " + type);
				return false;
			}
		}

		public bool AddSchematicGenerator(SplatmappedGenerator gen, XElement xml)
		{
			var schem = xml.Attribute("schem");
			var amount = float.Parse(xml.Attribute("amount")?.Value ?? "1.0");
			if (schem != null)
			{
				generators.Add(new SchematicInstanceGenerator(gen.postProcessor.schematics[schem.Value], amount));
				return true;
			}
			else
			{
				var block = xml.Attribute("block");
				if (block != null)
				{
					generators.Add(new SchematicInstanceGenerator(block.Value, amount));
					return true;
				}
				else
				{
					ConsoleOutput.WriteError("block/schematic generator has missing arguments (must have either 'block' or 'schem')");
					return false;
				}
			}
		}

		public bool AddBiomeGenerator(XElement xml)
		{
			var id = xml.Attribute("id");
			if (id != null)
			{
				generators.Add(new BiomeGenerator(byte.Parse(id.Value)));
				return true;
			}
			else
			{
				ConsoleOutput.WriteError("biome generator is missing 'id' attribute");
				return false;
			}
		}

		public void RunGenerator(World w, int x, int y, int z)
		{
			for (int i = 0; i < generators.Count; i++)
			{
				generators[i].Generate(w, x, y, z);
			}
		}
	}
}