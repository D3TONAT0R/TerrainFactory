using ASCReader;
using ASCReaderMC.PostProcessors;
using MCUtils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ASCReaderMC.MinecraftTerrainPostProcessors {
	public class CavesPostProcessor : MinecraftTerrainPostProcessor {

		public float caveChancePerChunk = 0.2f;

		Random random;

		public CavesPostProcessor() {
			random = new Random();
		}

		public override void OnFinish(World world) {
			for(float v = 0; v <= 1; v += 0.1f) {
				var smooth = Smoothstep(v, 0, 1);
				Program.WriteLine($"smoothstep of {v} = {smooth}");
			}
		}

		public override void ProcessBlock(World world, int x, int y, int z) {
			if(y < 5) return;
			if(random.NextDouble() <= caveChancePerChunk / 16384.0) {
				Program.WriteLine($"Beginning new cave at {x},{y},{z}");
				GenerateCave(world, new Vector3(x, y, z));
			}
		}

		private void GenerateCave(World world, Vector3 pos) {
			float delta = RandomRange(0.25f, 1f);
			int life = (int)(RandomRange(100, 300) * delta);
			float size = RandomRange(4, 8) * delta;
			float variation = RandomRange(0.2f, 1f);
			Vector3 direction = GetRandomVector3();
			direction = Vector3.Normalize(direction);
			while(life > 0) {
				life--;
				Carve(world, pos, size);
				Vector3 newDirection = ApplyYWeights(pos.Y, GetRandomVector3());
				direction += newDirection * variation / 20f;
				direction = Vector3.Normalize(direction);
				size = Lerp(size, RandomRange(4, 8) * delta, 0.05f);
				variation = Lerp(variation, RandomRange(0.2f, 1f), 0.025f);
				pos += direction;
			}
		}

		private void Carve(World world, Vector3 pos, float radius) {
			int x1 = (int)Math.Floor(pos.X - radius);
			int x2 = (int)Math.Ceiling(pos.X + radius);
			int y1 = (int)Math.Floor(pos.Y - radius);
			int y2 = (int)Math.Ceiling(pos.Y + radius);
			int z1 = (int)Math.Floor(pos.Z - radius);
			int z2 = (int)Math.Ceiling(pos.Z + radius);
			for(int x = x1; x <= x2; x++) {
				for(int y = y1; y <= y2; y++) {
					for(int z = z1; z <= z2; z++) {
						if(Vector3.Distance(new Vector3(x, y, z), pos) < radius) {
							var b = world.GetBlock(x, y, z);
							if(b != null && b != "minecraft:bedrock" && b != "minecraft:air") world.SetBlock(x, y, z, "air");
						}
					}
				}
			}
		}

		private float RandomRange(float min, float max) {
			return min + (float)random.NextDouble() * (max - min);
		}

		private Vector3 GetRandomVector3() {
			return Vector3.Normalize(new Vector3() {
				X = RandomRange(-1, 1),
				Y = RandomRange(-1, 1),
				Z = RandomRange(-1, 1)
			});
		}

		private float Smoothstep(float v, float a, float b) {
			if(v <= a) return a;
			if(v >= b) return b;
			return 0.5f - MathF.Cos(a - v) * MathF.PI / (b - a) / 2f;
		}

		private Vector3 ApplyYWeights(float y, Vector3 dir) {
			float weight = 0;
			if(y < 16) {
				weight = Smoothstep(1f - y / 16f, 0, 1);
			}
			return Vector3.Lerp(dir, new Vector3(0, 1, 0), weight);
		}

		private float Lerp(float a, float b, float t) {
			return a + (b - a) * t;
		}
	}
}
