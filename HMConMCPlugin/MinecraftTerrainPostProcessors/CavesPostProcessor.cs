using HMCon;
using HMConMC.PostProcessors;
using MCUtils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{
	public class CavesPostProcessor : AbstractPostProcessor
	{

		const double avgBlocksPerChunkRate = 1f / (16 * 16 * 64);

		public override Priority OrderPriority => Priority.AfterFirst;

		public float caveChancePerChunk = 0.04f;
		public int lavaHeight = 8;

		Random random;

		public override PostProcessType PostProcessorType => PostProcessType.Block;
		public override int BlockProcessYMin => 8;
		public override int BlockProcessYMax => 72;

		public CavesPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			random = new Random();
		}

		protected override void OnProcessBlock(World world, int x, int y, int z, int pass, float mask)
		{
			if (y < 5) return;

			//Increase number of caves underground and decrease them on the surface
			float yChanceScale = 2f * (1 - (y - BlockProcessYMin) / (float)(BlockProcessYMax - BlockProcessYMin));

			if (Chance(caveChancePerChunk * avgBlocksPerChunkRate * yChanceScale))
			{
				GenerateCave(world, new Vector3(x, y, z), 0);
			}
		}

		private void GenerateCave(World world, Vector3 pos, int iteration, float maxDelta = 1f)
		{
			float delta = RandomRange(maxDelta * 0.25f, maxDelta);
			int life = (int)(RandomRange(50, 300) * delta);
			float size = RandomRange(2, 6 * delta);
			float variation = RandomRange(0.2f, 1f);
			Vector3 direction = GetRandomVector3(iteration == 0);
			direction = Vector3.Normalize(direction);
			float branchingChance = 0;
			bool breakSurface = Chance(0.4f);
			if (delta > 0.25f && iteration < 3)
			{
				branchingChance = size * 0.005f;
			}
			while (life > 0)
			{
				life--;
				if (!Carve(world, pos, size, breakSurface))
				{
					//Nothing was carved, the cave is dead
					return;
				}
				Vector3 newDirection = ApplyYWeights(pos.Y, GetRandomVector3(true));
				direction += newDirection * variation * 0.5f;
				direction = Vector3.Normalize(direction);
				size = Lerp(size, RandomRange(2, 6) * delta, 0.15f);
				variation = Lerp(variation, RandomRange(0.2f, 1f), 0.1f);
				if (Chance(branchingChance))
				{
					//Start a new branch at the current position
					GenerateCave(world, pos, iteration + 1, maxDelta * 0.8f);
				}
				pos += direction;
			}
		}

		private bool Carve(World world, Vector3 pos, float radius, bool breakSurface)
		{
			bool hasCarved = false;
			int x1 = (int)Math.Floor(pos.X - radius);
			int x2 = (int)Math.Ceiling(pos.X + radius);
			int y1 = (int)Math.Floor(pos.Y - radius);
			int y2 = (int)Math.Ceiling(pos.Y + radius);
			int z1 = (int)Math.Floor(pos.Z - radius);
			int z2 = (int)Math.Ceiling(pos.Z + radius);
			for (int x = x1; x <= x2; x++)
			{
				for (int y = y1; y <= y2; y++)
				{
					for (int z = z1; z <= z2; z++)
					{
						if (Vector3.Distance(new Vector3(x, y, z), pos) < radius)
						{
							var b = world.GetBlockState(x, y, z);

							if (b == null || Blocks.IsAir(b)) continue;

							//Can the cave break the surface?
							if(b.CompareMultiple(Blocks.terrainSurfaceBlocks) && !breakSurface)
							{
								continue;
							}

							if (b != null && !b.CompareMultiple("minecraft:bedrock") && !Blocks.IsLiquid(b))
							{
								hasCarved = true;
								if (y <= lavaHeight)
								{
									world.SetBlock(x, y, z, "minecraft:lava");
								}
								else
								{
									world.SetBlock(x, y, z, "minecraft:air");
								}
							}
						}
					}
				}
			}
			return hasCarved;
		}

		private float RandomRange(float min, float max)
		{
			return min + (float)random.NextDouble() * (max - min);
		}

		private bool Chance(double chance)
		{
			return random.NextDouble() <= chance;
		}

		private Vector3 GetRandomVector3(bool allowUpwards)
		{
			return Vector3.Normalize(new Vector3()
			{
				X = RandomRange(-1, 1),
				Y = RandomRange(-1, allowUpwards ? 1 : 0),
				Z = RandomRange(-1, 1)
			});
		}

		private float Smoothstep(float v, float a, float b)
		{
			if (v <= a) return a;
			if (v >= b) return b;

			float t = Math.Min(Math.Max((v - a) / (b - a), 0), 1);
			return t * t * (3f - 2f * t);
		}

		private Vector3 ApplyYWeights(float y, Vector3 dir)
		{
			float weight = 0;
			if (y < 16)
			{
				weight = Smoothstep(1f - y / 16f, 0, 1);
			}
			return Vector3.Lerp(dir, new Vector3(0, 1, 0), weight);
		}

		private float Lerp(float a, float b, float t)
		{
			return a + (b - a) * t;
		}
	}
}
