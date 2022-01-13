using HMCon;
using MCUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{
	class DeIcingPostProcessor : AbstractPostProcessor
	{
		public static Dictionary<BiomeID, BiomeID> biomeDeIcingTable = new Dictionary<BiomeID, BiomeID>()
		{
			{ BiomeID.snowy_tundra, BiomeID.plains },
			{ BiomeID.ice_spikes, BiomeID.plains },
			{ BiomeID.snowy_taiga, BiomeID.taiga },
			{ BiomeID.snowy_taiga_hills, BiomeID.taiga_hills },
			{ BiomeID.snowy_taiga_mountains, BiomeID.taiga_mountains },
			{ BiomeID.snowy_mountains, BiomeID.mountains },
			{ BiomeID.frozen_river, BiomeID.river },
			{ BiomeID.frozen_ocean, BiomeID.ocean },
			{ BiomeID.snowy_beach, BiomeID.beach },
			{ BiomeID.deep_frozen_ocean, BiomeID.deep_ocean }
		};

		public static Dictionary<string, string> blockReplacementTable = new Dictionary<string, string>()
		{
			{ "minecraft:snow", "minecraft:air" },
			{ "minecraft:snow_block", "minecraft:air" },
			{ "minecraft:ice", "minecraft:water" },
			{ "minecraft:packed_ice", "minecraft:water" },
			{ "minecraft:blue_ice", "minecraft:water" },
			{ "minecraft:powder_snow", "minecraft:air" }
		};

		public override PostProcessType PostProcessorType => PostProcessType.Both;

		public DeIcingPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{

		}

		protected override void OnProcessSurface(World world, int x, int y, int z, int pass, float mask)
		{
			//Replace snowy biomes with normal biomes
			var biome = world.GetBiome(x, z);
			if (biome.HasValue)
			{
				if (biomeDeIcingTable.ContainsKey(biome.Value))
				{
					world.SetBiome(x, z, biomeDeIcingTable[biome.Value]);
				}
			}
			else
			{
				ConsoleOutput.WriteError($"Biome at [{x},{z}] was null");
			}
		}

		protected override void OnProcessBlock(World world, int x, int y, int z, int pass, float mask)
		{
			if (world.IsAir(x, y, z)) return;
			//Replace snowy blocks with air or water
			BlockState block = world.GetBlockState(x, y, z);
			if (block == null) return;
			if (block.properties.Contains("snowy"))
			{
				//Replace block with itself to get rid of the "snowy" property
				world.SetBlock(x, y, z, block.block.ID);
			}
			else if (blockReplacementTable.ContainsKey(block.block.ID))
			{
				world.SetBlock(x, y, z, blockReplacementTable[block.block.ID]);
			}
		}
	}
}
