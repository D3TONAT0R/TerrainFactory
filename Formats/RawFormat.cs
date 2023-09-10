using System;
using System.IO;
using TerrainFactory.Export;

namespace TerrainFactory.Formats
{
	public abstract class RawFormat : FileFormat
	{
		protected abstract bool Is32BitFormat { get; }

		protected override bool ExportFile(string path, ExportTask task)
		{
			using(var stream = BeginWriteStream(path))
			{
				WriteBytes(stream, task.data);
			}
			return true;
		}

		protected void WriteBytes(FileStream stream, ElevationData data)
		{
			int byteCount = Is32BitFormat ? 4 : 2;
			int convert = 1 << (byteCount * 8);
			//Decode with 1f / convert;
			for(int y = data.CellCountY - 1; y >= 0; y--)
			{
				for(int x = 0; x < data.CellCountX; x++)
				{
					float height = data.GetElevationAtCell(x, y);
					float height01 = MathUtils.Clamp01(MathUtils.InverseLerp(data.LowPoint, data.HighPoint, height));
					byte[] bytes;
					if(byteCount > 2)
					{
						int h = (int)Math.Round(height01 * convert);
						bytes = BitConverter.GetBytes(h);
					}
					else
					{
						short h = (short)Math.Round(height01 * convert);
						bytes = BitConverter.GetBytes(h);
					}
					stream.Write(bytes, 0, byteCount);
				}
			}
		}
	}
}
