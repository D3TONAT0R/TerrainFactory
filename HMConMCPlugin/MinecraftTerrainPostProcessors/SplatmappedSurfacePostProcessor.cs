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
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{
	public class SplatmappedSurfacePostProcessor : MinecraftTerrainPostProcessor
	{

		public List<Generator> generators = new List<Generator>();
		public Dictionary<string, Schematic> schematics = new Dictionary<string, Schematic>();

		public override Priority OrderPriority => Priority.BeforeDefault;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public override int NumberOfPasses => generators.Count;

		public SplatmappedSurfacePostProcessor(MCWorldExporter exporter, string importedFilePath, int ditherLimit, int offsetX, int offsetZ, int sizeX, int sizeZ)
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
				foreach(var schematicsContainer in xmlRoot.Descendants("schematics"))
				{
					foreach(var elem in schematicsContainer.Elements())
					{
						RegisterStructure(Path.Combine(Path.GetDirectoryName(root), elem.Value), elem.Name.LocalName);
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
				var gen = new SplatmappedGenerator(this, splatXml, rootPath, ditherLimit, offsetX, offsetZ, sizeX, sizeZ);
				generators.Add(gen);
			}
			else if (splatXml.Name.LocalName == "water")
			{
				var gen = new WatermappedGenerator(splatXml, rootPath, offsetX, offsetZ, sizeX, sizeZ);
				generators.Add(gen);
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

				foreach(var elem in include.Elements())
				{
					if(elem.Name == "schematics")
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

		public override void ProcessSurface(World world, int x, int y, int z, int pass)
		{
			var gen = generators[pass];
			gen.RunGenerator(world, x, y, z);
		}
	}
}