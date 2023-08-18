using TerrainFactory.Export;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TerrainFactory.Formats
{
	public class Raw16Format : FileFormat
	{
		public override string Identifier => "R16";
		public override string ReadableName => "16 Bit Raw Data";
		public override string CommandKey => "r16";
		public override string Description => ReadableName;
		public override string Extension => "r16";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportTask task)
		{
			using (var stream = BeginWriteStream(path))
			{
				WriteBytes(stream, task.data, 2);
			}
			return true;
		}

		protected void WriteBytes(FileStream stream, HeightData data, int byteCount)
		{
			int convert = 1 << (byteCount * 8);
			//Decode with 1f / convert;
			for (int y = data.GridLengthY - 1; y >= 0; y--)
			{
				for (int x = 0; x < data.GridLengthX; x++)
				{
					float height = data.GetHeight(x, y);
					float height01 = Math.Min(1, Math.Max(0, MathUtils.InverseLerp(data.lowPoint, data.highPoint, height)));
					byte[] bytes;
					if (byteCount > 2)
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

		//Taken from Unity's Heightmap Import Wizard:
		/*
		if (m_Depth == Depth.Bit16)
        {
            float normalize = 1.0F / (1 << 16);
            for (int y = 0; y < heightmapRes; ++y)
            {
                for (int x = 0; x < heightmapRes; ++x)
                {
                    int index = Mathf.Clamp(x, 0, m_Resolution - 1) + Mathf.Clamp(y, 0, m_Resolution - 1) * m_Resolution;
                    if ((m_ByteOrder == ByteOrder.Mac) == System.BitConverter.IsLittleEndian)
                    {
                        // Yay, seems like this is the easiest way to swap bytes in C#. NUTS
                        byte temp;
                        temp = data[index * 2];
                        data[index * 2 + 0] = data[index * 2 + 1];
                        data[index * 2 + 1] = temp;
                    }

                    ushort compressedHeight = System.BitConverter.ToUInt16(data, index * 2);

                    float height = compressedHeight * normalize;
                    int destY = m_FlipVertically ? heightmapRes - 1 - y : y;
                    heights[destY, x] = height;
                }
            }
        }
		*/
	}
}
