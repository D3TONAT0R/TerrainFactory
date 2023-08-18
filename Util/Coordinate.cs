using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Util
{
	public struct Coordinate
	{
		const string geoReferenceLiteral = "$";

		public float value;
		public bool isGeoReferenced;

		public Coordinate(float value, bool isGeoReferenced)
		{
			this.value = value;
			this.isGeoReferenced = isGeoReferenced;
		}

		public static Coordinate Parse(string input)
		{
			bool geo = false;
			if (input.StartsWith(geoReferenceLiteral))
			{
				input = input.Substring(geoReferenceLiteral.Length);
				geo = true;
			}
			return new Coordinate(float.Parse(input), geo);
		}

		public float GetLocalValue(float referenceOffset)
		{
			if (isGeoReferenced)
			{
				return value - referenceOffset;
			}
			else
			{
				return value;
			}
		}

		public float GetGeoReferencedValue(float referenceOffset)
		{
			if (isGeoReferenced)
			{
				return value + referenceOffset;
			}
			else
			{
				return value;
			}
		}

		public override string ToString()
		{
			return isGeoReferenced ? "$" + value.ToString() : value.ToString();
		}
	}
}
