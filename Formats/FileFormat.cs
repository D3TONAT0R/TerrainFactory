using TerrainFactory.Export;
using TerrainFactory.Import;
using TerrainFactory.Util;
using System;
using System.IO;
using System.Text;

namespace TerrainFactory.Formats
{

	public abstract class FileFormat
	{
		[Flags]
		public enum FileSupportFlags
		{
			None = 0b00,
			Import = 0b01,
			Export = 0b10,
			ImportAndExport = 0b11
		}

		/// <summary>
		/// A short, uppercase, unique identifier for this format.
		/// </summary>
		public abstract string Identifier { get; }
		/// <summary>
		/// A human-readable name for this format.
		/// </summary>
		public abstract string ReadableName { get; }
		/// <summary>
		/// Input string for selecting this format when entering commands.
		/// </summary>
		public abstract string CommandKey { get; }
		/// <summary>
		/// A short description about this format.
		/// </summary>
		public abstract string Description { get; }
		/// <summary>
		/// The target file's extension, without the '.' dot.
		/// </summary>
		public abstract string Extension { get; }

		public abstract FileSupportFlags SupportedActions { get; }
		public bool HasImporter => SupportedActions.HasFlag(FileSupportFlags.Import);
		public bool HasExporter => SupportedActions.HasFlag(FileSupportFlags.Export);

		public static FileFormat GetFromIdentifier(string identifier)
		{
			return FileFormatManager.GetFormatFromIdentifier(identifier);
		}

		public static FileFormat GetFromCommandInput(string key)
		{
			return FileFormatManager.GetFormatFromCommandKey(key);
		}

		public static FileFormat GetFromFileName(string filename)
		{
			return FileFormatManager.GetFormatByFileName(filename);
		}

		public static FileFormat GetFromExtension(string ext)
		{
			return FileFormatManager.GetFormatByExtension(ext);
		}

		public static FileFormat GetFromType(Type type)
		{
			return FileFormatManager.GetFormatFromType(type);
		}

		public ElevationData Import(string importPath, params string[] args)
		{
			if (HasImporter)
			{
				return ImportFile(importPath, args);
			}
			else
			{
				throw new NotSupportedException($"The format '{Identifier}' does not support importing.");
			}
		}

		public bool Export(string path, ExportTask task)
		{
			if(HasExporter)
			{
				return ExportFile(path, task);
			}
			else
			{
				throw new NotSupportedException($"The format '{Identifier}' does not support exporting.");
			}
		}

		protected virtual ElevationData ImportFile(string importPath, params string[] args)
		{
			throw new NotImplementedException("Import functionality is not implemented.");
		}

		protected virtual bool ExportFile(string path, ExportTask task)
		{
			throw new NotImplementedException("Export functionality is not implemented.");
		}

		/// <summary>
		/// Modifies the target file name prior to exporting.
		/// </summary>
		public virtual void ModifyFileName(ExportTask task, FileNameBuilder nameBuilder)
		{

		}

		/// <summary>
		/// Modifies the file naming pattern for this export.
		/// </summary>
		public virtual void ModifyNamingPattern(ExportTask task, FileNameBuilder namingPattern)
		{

		}

		public virtual bool ValidateSettings(ExportSettings settings, ElevationData data)
		{
			return true;
		}

		protected FileStream BeginWriteStream(string path)
		{
			return new FileStream(path, FileMode.Create);
		}

		protected FileStream BeginReadStream(string path)
		{
			return new FileStream(path, FileMode.Open);
		}
	}
}