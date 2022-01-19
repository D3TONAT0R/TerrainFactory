using MCUtils;
using System;

namespace HMConMC.PostProcessors
{
	public abstract class Layer
	{
		public abstract void ProcessBlockColumn(World world, Random random, int x, int topY, int z, float mask);
	}
}
