using MCUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConMC.PostProcessors.Splatmapper
{
	public abstract class Generator
	{
		protected int worldOriginOffsetX;
		protected int worldOriginOffsetZ;

		public virtual void RunGenerator(World w, int x, int y, int z)
		{

		}

		public virtual void RunGeneratorForRegion(World w, Region r, int rx, int rz)
		{

		}
	}
}
