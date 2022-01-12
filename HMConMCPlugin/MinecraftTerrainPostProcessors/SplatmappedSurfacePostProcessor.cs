using HMCon;
using HMCon.Import;
using HMConImage;
using HMConMC.PostProcessors.Splatmapper;
using MCUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{
	public class SplatmappedSurfacePostProcessor : AbstractWorldDecorator
	{

		public Dictionary<string, Schematic> schematics = new Dictionary<string, Schematic>();

		public SplatmappedSurfacePostProcessor(string importedFilePath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			var xmlPath = Path.ChangeExtension(importedFilePath, null) + "-splat.xml";
			LoadSettings(xmlPath, ditherLimit, offsetX, offsetZ, sizeX, sizeZ);
			ConsoleOutput.WriteLine("Splatmapper loaded successfully");
		}

		void LoadSettings(string xmlPath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			string root = Path.GetDirectoryName(xmlPath);
			XElement xmlRoot = XDocument.Parse(File.ReadAllText(xmlPath)).Root;
			try
			{
				foreach (var schematicsContainer in xmlRoot.Descendants("schematics"))
				{
					foreach (var elem in schematicsContainer.Elements())
					{
						RegisterStructure(Path.Combine(root, elem.Value), elem.Name.LocalName);
					}
				}

				foreach (var splatXml in xmlRoot.Element("splatmaps").Elements())
				{
					LoadGenerator(splatXml, false, root, ditherLimit, offsetX, offsetZ, sizeX, sizeZ);
				}
			}
			catch (Exception e)
			{
				ConsoleOutput.WriteError("Error occured while loading settings for splatmapper:");
				ConsoleOutput.WriteError(e.Message);
			}
		}

		void LoadGenerator(XElement splatXml, bool fromInclude, string rootPath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			if (splatXml.Name.LocalName == "splat")
			{
				generators.Add(new SplatmappedGenerator(this, splatXml, rootPath, ditherLimit, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if (splatXml.Name.LocalName == "water")
			{
				generators.Add(new WatermappedGenerator(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if (splatXml.Name.LocalName == "merger")
			{
				generators.Add(new MaskedWorldMerger(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if (splatXml.Name.LocalName == "oregen")
			{
				generators.Add(new OreGenerator(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if (splatXml.Name.LocalName == "deice")
			{
				generators.Add(new DeIcingPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if (splatXml.Name.LocalName == "naturalize")
			{
				generators.Add(new NaturalTerrainPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if (splatXml.Name.LocalName == "vegetation")
			{
				generators.Add(new VegetationPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if (splatXml.Name.LocalName == "torches")
			{
				generators.Add(new RandomTorchPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if (splatXml.Name.LocalName == "caves")
			{
				generators.Add(new CavesPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if (splatXml.Name.LocalName == "bedrock")
			{
				generators.Add(new BedrockPostProcessor(rootPath, splatXml, offsetX, offsetZ, sizeX, sizeZ));
			}
			else if (splatXml.Name.LocalName == "include")
			{
				if (fromInclude)
				{
					ConsoleOutput.WriteError("Recursive includes are not allowed");
					return;
				}
				//Include external xml
				var includePathElem = splatXml.Attribute("file");
				if (includePathElem == null)
				{
					throw new KeyNotFoundException("The include's file must be specified with a 'file' attribute");
				}
				var includePath = Path.Combine(rootPath, includePathElem.Value);

				var include = XDocument.Parse(File.ReadAllText(includePath)).Root;

				foreach (var elem in include.Elements())
				{
					if (elem.Name == "schematics")
					{
						foreach (var se in elem.Elements())
						{
							RegisterStructure(Path.Combine(rootPath, se.Value), se.Name.LocalName);
						}
					}
					else
					{
						LoadGenerator(elem, true, Path.GetDirectoryName(includePath), ditherLimit, offsetX, offsetZ, sizeX, sizeZ);
					}
				}
			}
			else
			{
				ConsoleOutput.WriteWarning("Unknown element type in splatmaps list: " + splatXml.Name.LocalName);
			}
		}

		public bool ContinsGeneratorOfType(Type type)
		{
			foreach (var g in generators)
			{
				if (g.GetType() == type)
				{
					return true;
				}
			}
			return false;
		}

		private void RegisterStructure(string filename, string key)
		{
			try
			{
				schematics.Add(key, new Schematic(filename));
				ConsoleOutput.WriteLine($"Registered new schematic: {key}");
			}
			catch
			{
				ConsoleOutput.WriteWarning("Failed to import structure '" + filename + "'");
			}
		}

		public override void DecorateTerrain(MCWorldExporter exporter)
		{

			int processorIndex = 0;
			foreach (var post in generators)
			{
				for (int pass = 0; pass < post.NumberOfPasses; pass++)
				{
					string name = post.GetType().Name;
					if (post.PostProcessorType == PostProcessType.Block || post.PostProcessorType == PostProcessType.Both)
					{
						//Iterate the postprocessors over every block
						for (int x = 0; x < exporter.heightmapLengthX; x++)
						{
							for (int z = 0; z < exporter.heightmapLengthZ; z++)
							{
								for (int y = post.BlockProcessYMin; y <= post.BlockProcessYMax; y++)
								{
									post.ProcessBlock(exporter.world, x + exporter.regionOffsetX * 512, y, z + exporter.regionOffsetZ * 512, pass);
								}
							}
							UpdateProgressBar(processorIndex, "Decorating terrain", name, (x + 1) / (float)exporter.heightmapLengthX, pass, post.NumberOfPasses);
						}
					}

					if (post.PostProcessorType == PostProcessType.Surface || post.PostProcessorType == PostProcessType.Both)
					{
						//Iterate the postprocessors over every surface block
						for (int x = 0; x < exporter.heightmapLengthX; x++)
						{
							for (int z = 0; z < exporter.heightmapLengthZ; z++)
							{
								post.ProcessSurface(exporter.world, x + exporter.regionOffsetX * 512, exporter.heightmap[x, z], z + exporter.regionOffsetZ * 512, pass);
							}
							UpdateProgressBar(processorIndex, "Decorating surface", name, (x + 1) / (float)exporter.heightmapLengthX, pass, post.NumberOfPasses);
						}
					}

					//Run every postprocessor once for every region (rarely used)
					Parallel.ForEach(exporter.world.regions.Values, (MCUtils.Region reg) =>
					{
						post.ProcessRegion(exporter.world, reg, reg.regionPos.x, reg.regionPos.z, pass);
					});
				}
				processorIndex++;
			}
			foreach (var post in generators)
			{
				post.OnFinish(exporter.world);
			}
		}

		private void UpdateProgressBar(int index, string title, string name, float progress, int currentPass, int numPasses)
		{
			string passInfo = numPasses > 1 ? $" Pass {currentPass}/{numPasses}" : "";
			float progressWithPasses = (currentPass + progress) / numPasses;
			ConsoleOutput.UpdateProgressBar($"{index + 1}/{generators.Count} {title} [{name}{passInfo}]", progressWithPasses);
		}

		public void ProcessSurface(World world, int x, int y, int z, int pass, float mask)
		{
			var gen = generators[pass];
			gen.ProcessSurface(world, x, y, z, 0);
		}

		public void ProcessRegion(World world, MCUtils.Region reg, int rx, int rz, int pass)
		{
			var gen = generators[pass];
			gen.ProcessRegion(world, reg, rx, rz, 0);
		}
	}
}