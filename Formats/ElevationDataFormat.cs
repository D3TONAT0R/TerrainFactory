using System.IO;
using System.Numerics;
using TerrainFactory.Export;

namespace TerrainFactory.Formats
{
	public class ElevationDataFormat : FileFormat
	{
		private const byte FORMAT_VERSION = 1;

		public override string Identifier => "EDG";
		public override string ReadableName => "Elevation Data Grid";
		public override string Command => "edg";
		public override string Description => "TerrainFactory Elevation Data in binary";
		public override string Extension => "edg";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		protected override bool ExportFile(string path, ExportTask task)
		{
			var data = task.data;
			using(var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				using(var writer = new BinaryWriter(stream))
				{
					writer.Write(FORMAT_VERSION);
					writer.Write(data.CellCountX);
					writer.Write(data.CellCountY);
					writer.Write(data.LowerCornerPosition.X);
					writer.Write(data.LowerCornerPosition.Y);
					writer.Write(data.CellSize);
					writer.Write(data.MinElevation);
					writer.Write(data.MaxElevation);
					var grid = data.GetDataGrid();
					for(int y = data.CellCountY - 1; y >= 0; y--)
					{
						for(int x = 0; x < data.CellCountX; x++)
						{
							writer.Write(grid[x, y]);
						}
					}
				}
			}
			return true;
		}

		protected override ElevationData ImportFile(string importPath, params string[] args)
		{
			using(var stream = File.OpenRead(importPath))
			{
				using(var reader = new BinaryReader(stream))
				{
					byte version = reader.ReadByte();
					if(version != FORMAT_VERSION)
					{
						throw new InvalidDataException($"Invalid file version: {version}. Expected: {FORMAT_VERSION}");
					}
					int cellCountX = reader.ReadInt32();
					int cellCountY = reader.ReadInt32();
					float lowerX = reader.ReadSingle();
					float lowerY = reader.ReadSingle();
					float cellSize = reader.ReadSingle();
					float[,] grid = new float[cellCountX, cellCountY];
					for(int y = cellCountY - 1; y >= 0; y--)
					{
						for(int x = 0; x < cellCountX; x++)
						{
							grid[x, y] = reader.ReadSingle();
						}
					}
					var data = new ElevationData(grid, cellSize)
					{
						LowerCornerPosition = new Vector2(lowerX, lowerY)
					};
					data.RecalculateElevationRange(true);
					return data;
				}
			}
		}
	}
}