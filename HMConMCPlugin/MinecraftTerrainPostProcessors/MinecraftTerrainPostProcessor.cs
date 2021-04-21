using MCUtils;

namespace HMConMC.PostProcessors {

	public enum PostProcessType {
		Block,
		Surface,
		Both
	}

	public abstract class MinecraftTerrainPostProcessor {

		public enum Priority {
			First = 0,
			AfterFirst = 1,
			BeforeDefault = 2,
			Default = 3,
			AfterDefault = 4,
			BeforeLast = 5,
			Last = 6
		}

		public virtual Priority OrderPriority => Priority.Default;

		public abstract PostProcessType PostProcessorType { get; }

		public virtual int BlockProcessYMin => 0;
		public virtual int BlockProcessYMax => 255;

		public virtual void ProcessBlock(MCUtils.World world, int x, int y, int z) {

		}

		public virtual void ProcessSurface(MCUtils.World world, int x, int y, int z) {

		}

		public virtual void OnFinish(MCUtils.World world) {

		}
	}
}