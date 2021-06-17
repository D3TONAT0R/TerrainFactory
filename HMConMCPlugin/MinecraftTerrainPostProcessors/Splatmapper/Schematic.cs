using HMCon;
using MCUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{
	public class Schematic
	{

		public byte[,,] schematic;

		public int StructureSizeX
		{
			get { return schematic.GetLength(0); }
		}
		public int StructureSizeY
		{
			get { return schematic.GetLength(1); }
		}
		public int StructureSizeZ
		{
			get { return schematic.GetLength(2); }
		}

		public Dictionary<byte, (string block, float prob)> blocks = new Dictionary<byte, (string block, float prob)>();

		public string trunkBlock = null;
		public byte trunkHeightMin = 1;
		public byte trunkHeightMax = 1;

		public Schematic(string filepath)
		{
			LoadFromFile(filepath);
		}

		public bool Build(World world, int x, int y, int z, Random r)
		{
			byte h = (byte)r.Next(trunkHeightMin, trunkHeightMax + 1);
			if (IsObstructed(world, x, y + h, z))
			{
				return false;
			}
			if (!string.IsNullOrWhiteSpace(trunkBlock) && trunkHeightMax > 0)
			{
				for (int i = 0; i < h; i++)
				{
					world.SetBlock(x, y + i, z, trunkBlock);
				}
			}
			int xm = x - (int)Math.Floor((float)StructureSizeX / 2);
			int zm = z - (int)Math.Floor((float)StructureSizeZ / 2);
			for (int x1 = 0; x1 < StructureSizeX; x1++)
			{
				for (int y1 = 0; y1 < StructureSizeY; y1++)
				{
					for (int z1 = 0; z1 < StructureSizeZ; z1++)
					{
						var d = schematic[x1, y1, z1];
						if (d == 0) continue;
						var (block, prob) = blocks[d];
						if (r.NextDouble() < prob)
						{
							world.SetBlock(xm + x1, y + h + y1, zm + z1, block);
						}
					}
				}
			}
			return true;
		}

		private bool IsObstructed(MCUtils.World world, int lx, int ly, int lz)
		{
			int x1 = lx - (int)Math.Floor(StructureSizeX / 2f);
			int x2 = lx + (int)Math.Ceiling(StructureSizeX / 2f);
			int y1 = ly;
			int y2 = ly + StructureSizeY;
			int z1 = lz - (int)Math.Floor(StructureSizeZ / 2f);
			int z2 = lz + (int)Math.Ceiling(StructureSizeZ / 2f);
			int sy = 0;
			for (int y = y1; y < y2; y++)
			{
				int sz = 0;
				for (int z = z1; z < z2; z++)
				{
					int sx = 0;
					for (int x = x1; x < x2; x++)
					{
						if (schematic[sx, sy, sz] == 0) continue; //Do not check this block if the result is nothing anyway
						if (!world.IsAir(x, y, z) || world.TryGetRegion(x, z) == null) return true;
						sx++;
					}
					sz++;
				}
				sy++;
			}
			return false;
		}

		private void LoadFromFile(string xmlPath)
		{
			XElement root = XDocument.Parse(File.ReadAllText(xmlPath)).Root;
			var type = root.Element("type")?.Value ?? null;
			if (type == null)
			{
				throw new ArgumentNullException($"The schematic {Path.GetFileName(xmlPath)} does not specify it's type");
			}
			ParseCommonData(root);
			if (type == "tree")
			{
				var trunkElem = root.Element("trunk");
				if (trunkElem != null)
				{
					trunkBlock = ParseBlock(trunkElem.Element("block").Value);
					trunkHeightMin = byte.Parse(trunkElem.Element("height_min")?.Value ?? "1");
					trunkHeightMax = byte.Parse(trunkElem.Element("height_max")?.Value ?? "1");
				}
				else
				{
					ConsoleOutput.WriteWarning("Tree schematic is missing 'trunk' element");
					trunkBlock = blocks[1].block;
				}
			}
			else
			{
				throw new ArgumentNullException($"Unknown schematic type '{type}'");
			}
		}

		private void ParseCommonData(XElement root)
		{
			var sizeElem = root.Element("size");
			int sx = int.Parse(sizeElem.Element("x").Value);
			int sy = int.Parse(sizeElem.Element("y").Value);
			int sz = int.Parse(sizeElem.Element("z").Value);
			schematic = new byte[sx, sy, sz];

			blocks.Clear();
			foreach (var elem in root.Element("blocks").Elements())
			{
				if (elem.Name == "block")
				{
					byte id = byte.Parse(elem.Attribute("id").Value);
					float chance = float.Parse(elem.Attribute("chance")?.Value ?? "1");
					string block = ParseBlock(elem.Value);
					blocks.Add(id, (block, chance));
				}
			}

			ParseBlockArray(root.Element("schematic"));
		}

		private void ParseBlockArray(XElement array)
		{
			int y = 0;
			foreach (var plane in array.Elements())
			{
				ParseBlockPlane(plane.Value, y);
				y++;
			}
		}

		private void ParseBlockPlane(string plane, int y)
		{
			string[] rows = plane.Split('\n');
			int i = 0;
			foreach (string r in rows)
			{
				if (string.IsNullOrWhiteSpace(r)) continue;
				int z = StructureSizeZ - i - 1;
				string[] split = r.Split(',');
				for (int x = 0; x < split.Length; x++)
				{
					schematic[x, y, z] = byte.Parse(split[x]);
				}
				i++;
			}
		}

		private string ParseBlock(string s)
		{
			if (s.StartsWith("#"))
			{
				s = s.Substring(1, s.Length - 1);
				return blocks[byte.Parse(s)].block;
			}
			else
			{
				return s;
			}
		}
	}
}
