using TerrainFactory.Export;
using TerrainFactory.Import;
using TerrainFactory.Util;
using System;
using System.IO;
using System.Linq;
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
		public abstract string Command { get; }
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

		public static T Get<T>() where T : FileFormat
		{
			try
			{
				return FileFormatRegistry.availableFormats.First(f => f.GetType() == typeof(T)) as T;
			}
			catch(InvalidOperationException)
			{
				throw new InvalidOperationException($"File format '{typeof(T).Name}' is not registered.");
			}
		}

		public static FileFormat GetByType(Type type)
		{
			try
			{
				return FileFormatRegistry.availableFormats.First(f => f.GetType() == type);
			}
			catch(InvalidOperationException)
			{
				throw new InvalidOperationException($"File format '{type.Name}' is not registered.");
			}
		}

		public static FileFormat GetById(string identifier)
		{
			return FileFormatRegistry.availableFormats.FirstOrDefault(f => string.Equals(f.Identifier, identifier, StringComparison.OrdinalIgnoreCase));
		}

		public static FileFormat GetFromCommandInput(string input)
		{
			return FileFormatRegistry.availableFormats.FirstOrDefault(f => string.Equals(f.Command, input, StringComparison.OrdinalIgnoreCase));
		}

		public static FileFormat GetFromFileName(string filename)
		{
			var ext = Path.GetExtension(filename);
			if(ext.Length > 0)
			{
				return GetFromExtension(ext);
			}
			else
			{
				throw new InvalidOperationException("Unable to determine file format: Filename does not have an extension.");
			}
		}

		public static FileFormat GetFromExtension(string extension)
		{
			if(extension.StartsWith("."))
			{
				extension = extension.Substring(1);
			}
			return FileFormatRegistry.availableFormats.FirstOrDefault(f => string.Equals(f.Command, extension, StringComparison.OrdinalIgnoreCase));
		}

		public static bool IsImportSupported(string path)
		{
			var format = GetFromFileName(path);
			return format != null && format.HasImporter;
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