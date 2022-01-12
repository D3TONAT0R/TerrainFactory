using HMConMC.PostProcessors;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConMC
{
	public abstract class AbstractWorldDecorator
	{
		public List<AbstractPostProcessor> generators = new List<AbstractPostProcessor>();

		public abstract void DecorateTerrain(MCWorldExporter exporter);

	}
}
