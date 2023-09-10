using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modification
{
	public class ModificationChain
	{
		public List<Modifier> chain = new List<Modifier>();

		public bool IsEmpty => chain == null || chain.Count == 0;

		public void AddModifier(Modifier modifier, bool replaceSameType = true)
		{
			if (!replaceSameType)
			{
				chain.Add(modifier);
			}
			else
			{
				for (int i = 0; i < chain.Count; i++)
				{
					if (chain[i].GetType() == modifier.GetType())
					{
						chain[i] = modifier;
						return;
					}
				}
				//A modifier of the same type was not found, append it instead
				chain.Add(modifier);
			}
		}

		public ElevationData Apply(ElevationData inputData)
		{
			ElevationData data = new ElevationData(inputData);
			for (int i = 0; i < chain.Count; i++)
			{
				data = chain[i].Modify(data, true);
			}
			data.RecalculateElevationRange(false);
			return data;
		}
	}
}
