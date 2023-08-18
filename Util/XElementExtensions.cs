using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace TerrainFactory.Util
{
	public static class XElementExtensions
	{

		public static bool TryGetElement(this XElement e, string elementName, out XElement element)
		{
			element = e.Element(elementName);
			return element != null;
		}

		public static bool TryGetAttribute(this XElement e, string attributeName, out XAttribute attribute)
		{
			attribute = e.Attribute(attributeName);
			return attribute != null;
		}

		public static bool TryParseFloat(this XElement e, string elementName, ref float value)
		{
			if(e.TryGetElement(elementName, out var elem))
			{
				value = float.Parse(elem.Value);
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool TryParseFloatAttribute(this XElement e, string attributeName, ref float value)
		{
			if(e.TryGetAttribute(attributeName, out var attr))
			{
				value = float.Parse(attr.Value);
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool TryParseInt(this XElement e, string elementName, ref int value)
		{
			if(e.TryGetElement(elementName, out var elem))
			{
				value = int.Parse(elem.Value);
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool TryParseShort(this XElement e, string elementName, ref short value)
		{
			if(e.TryGetElement(elementName, out var elem))
			{
				value = short.Parse(elem.Value);
				return true;
			}
			else
			{
				return false;
			}
		}


		public static bool TryParseIntAttribute(this XElement e, string attributeName, ref int value)
		{
			if (e.TryGetAttribute(attributeName, out var attr))
			{
				value = int.Parse(attr.Value);
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool TryParseBool(this XElement e, string elementName, ref bool value)
		{
			if(e.TryGetElement(elementName, out var elem))
			{
				value = bool.Parse(elem.Value);
				return true;
			}
			else
			{
				return false;
			}
		}


		public static bool TryParseBoolAttribute(this XElement e, string attributeName, ref bool value)
		{
			if (e.TryGetAttribute(attributeName, out var attr))
			{
				value = bool.Parse(attr.Value);
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
