﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TerrainFactory.Formats
{
	public class FileFormatList : IEnumerable<FileFormat>
	{
		public List<FileFormat> list = new List<FileFormat>();

		public int Count => list.Count;

		public void SetFormats(string[] inputs, bool add)
		{
			if(!add) list.Clear();
			foreach(string input in inputs)
			{
				if(string.IsNullOrWhiteSpace(input)) continue;
				var ff = FileFormat.GetFromCommandInput(input);
				if(ff != null)
				{
					list.Add(ff);
				}
				else
				{
					ConsoleOutput.WriteWarning("Unknown or unsupported format: " + input);
				}
			}
		}

		public void AddFormat(FileFormat format)
		{
			if(list.FirstOrDefault(f => f.GetType() == format.GetType()) != null)
			{
				throw new InvalidOperationException("Format is already present in the list.");
			}
			list.Add(format);
		}

		public void AddFormats(IEnumerable<FileFormat> formats)
		{
			foreach(var f in formats) AddFormat(f);
		}

		public void AddFormat(Type formatType)
		{
			AddFormat(FileFormat.GetByType(formatType));
		}

		public void AddFormats(IEnumerable<Type> formats)
		{
			foreach(var f in formats) AddFormat(f);
		}

		public bool ContainsFormat(Type formatType)
		{
			foreach(var f in list)
			{
				if(f.GetType() == formatType) return true;
			}
			return false;
		}


		public IEnumerator<FileFormat> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}
	}
}
