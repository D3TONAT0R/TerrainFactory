using MCUtils;

namespace ASCReaderMC.PostProcessors {
	public abstract class MinecraftTerrainPostProcessor {

		public virtual int RequiredPasses {
			get { return 1; }
		}

		public virtual void ProcessBlock(MCUtils.World world, int x, int y, int z) {

		}

		public virtual void ProcessSurface(MCUtils.World world, int x, int y, int z) {

		}

		public virtual void OnFinish(MCUtils.World world) {

		}
	}
}