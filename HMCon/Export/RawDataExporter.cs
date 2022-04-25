﻿using HMCon.Formats;
using HMCon.Util;
using System;
using System.IO;
using System.Text;

namespace HMCon.Export.Exporters {
	public class RawDataExporter {

		private HeightData data;

		public RawDataExporter(HeightData source) {
			data = source;
		}

		public bool NeedsFileStream(FileFormat format) {
			return true;
		}

		public void WriteFile(FileStream stream, string path, FileFormat filetype) {
			bool is32bit = filetype.IsFormat("R32");
			int byteCount = filetype.IsFormat("R32") ? 4 : 2;
			int convert = 1 << (byteCount * 8);
			//Decode with 1f / convert;
			for (int y = data.GridHeight-1; y >= 0; y--)
			{
				for (int x = 0; x < data.GridWidth; x++)
				{
					float height = data.GetHeight(x, y);
					float height01 = Math.Min(1, Math.Max(0, MathUtils.InverseLerp(data.lowPoint, data.highPoint, height)));
					byte[] bytes;
					if(is32bit)
					{
						int h = (int)Math.Round(height01 * convert);
						bytes = BitConverter.GetBytes(h);
					}
					else
					{
						short h = (short)Math.Round(height01 * convert);
						bytes = BitConverter.GetBytes(h);
					}
					stream.Write(bytes, 0, bytes.Length);
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
