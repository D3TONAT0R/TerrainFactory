using MCUtils;

namespace HMConMC.PostProcessors {

	public enum PostProcessType {
		RegionOnly,
		Block,
		Surface,
		Both
	}

	public enum Priority
	{
		First = 0,
		AfterFirst = 1,
		BeforeDefault = 2,
		Default = 3,
		AfterDefault = 4,
		BeforeLast = 5,
		Last = 6
	}

	public abstract class PostProcessor {

		public virtual Priority OrderPriority => Priority.Default;

		public abstract PostProcessType PostProcessorType { get; }

		public virtual int BlockProcessYMin => 0;
		public virtual int BlockProcessYMax => 255;

		public virtual int NumberOfPasses => 1;

		protected int worldOriginOffsetX;
		protected int worldOriginOffsetZ;

		public float[,] mask = null;

		public PostProcessor()
		{

		}

		public PostProcessor(string maskPath, ColorChannel channel, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			if (maskPath != null) {
				mask = SplatmapImporter.GetMask(maskPath, channel, offsetX, offsetZ, sizeX, sizeZ);
			}
		}

		public void ProcessBlock(World world, int x, int y, int z, int pass)
		{
			float maskValue = mask != null ? mask[x, z] : 1;
			if(maskValue > 0)
			{
				OnProcessBlock(world, x, y, z, pass, maskValue);
			}
		}

		public void ProcessSurface(World world, int x, int y, int z, int pass)
		{
			float maskValue = mask != null ? mask[x, z] : 1;
			if (maskValue > 0)
			{
				OnProcessSurface(world, x, y, z, pass, maskValue);
			}
		}

		protected virtual void OnProcessBlock(World world, int x, int y, int z, int pass, float mask)
		{

		}

		protected virtual void OnProcessSurface(World world, int x, int y, int z, int pass, float mask)
		{

		}

		public virtual void ProcessRegion(World world, Region reg, int rx, int rz, int pass)
		{

		}

		public virtual void OnFinish(MCUtils.World world) {

		}
	}
}