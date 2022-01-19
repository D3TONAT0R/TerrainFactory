﻿using HMCon;
using MCUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace HMConMC.PostProcessors.Splatmapper
{
	public class SplatmappedTerrainPostProcessor : AbstractPostProcessor
	{

		public Weightmap<byte> map;
		public List<SurfaceLayer> layers = new List<SurfaceLayer>();

		public WorldPostProcessingStack postProcessor;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public SplatmappedTerrainPostProcessor(WorldPostProcessingStack post, XElement xml, string rootPath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
			: base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			postProcessor = post;
			string mapFileName = Path.Combine(rootPath, xml.Attribute("file").Value);
			foreach (var layer in xml.Elements("layer"))
			{
				XAttribute colorAttr = layer.Attribute("color");
				if (colorAttr == null)
				{
					ConsoleOutput.WriteError("layer is missing required attribute 'color': " + layer.ToString().Trim());
					continue;
				}
				var color = ParseColor(colorAttr.Value);
				var surfaceLayer = new SurfaceLayer(color, layer.Attribute("name")?.Value);
				layers.Add(surfaceLayer);
				foreach (var elem in layer.Elements())
				{
					if (elem.Name.LocalName == "surface")
					{
						surfaceLayer.AddSurfaceGenerator(elem);
					}
					else if (elem.Name.LocalName == "gen")
					{
						surfaceLayer.AddSchematicGenerator(this, elem);
					}
					else if (elem.Name.LocalName == "biome")
					{
						surfaceLayer.AddBiomeGenerator(elem);
					}
				}
			}

			Color[] mappedColors = new Color[layers.Count];
			for (int i = 0; i < layers.Count; i++)
			{
				mappedColors[i] = layers[i].layerColor;
			}

			map = Weightmap<byte>.GetFixedWeightmap(mapFileName, mappedColors, ditherLimit, 0, 0, sizeX, sizeZ);
		}


		Color ParseColor(string input)
		{
			Color c;
			if (input.Contains(","))
			{
				//It's a manually defined color
				string[] cs = input.Split(',');
				int r = int.Parse(cs[0]);
				int g = int.Parse(cs[1]);
				int b = int.Parse(cs[2]);
				c = Color.FromArgb(255, r, g, b);
			}
			else
			{
				c = CommonSplatmapColors.NameToColor(input);
			}
			return c;
		}

		protected override void OnProcessSurface (World w, int x, int y, int z, int pass, float mask)
		{
			byte i = map.GetValue(x - worldOriginOffsetX, z - worldOriginOffsetZ);
			if (i < 255)
			{
				layers[i].RunGenerator(w, x, y, z);
			}
		}
	}
}
