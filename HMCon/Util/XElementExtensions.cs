using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace HMCon.Util
{
	public static class XElementExtensions
	{

		public static bool TryGetElement(this XElement e, string elementName, out XElement element)
		{
			element = e.Element(elementName);
			return element != null;
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
	}
}
