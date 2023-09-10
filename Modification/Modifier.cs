using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TerrainFactory.Modification {
	public abstract class Modifier : ICloneable {

		public static List<Type> availableModifierTypes;

		public string sourceCommandString;

		public virtual string Name => GetType().Name;

		protected Modifier()
		{

		}
		
		internal static void InitializeList()
		{
			availableModifierTypes = new List<Type>();
			foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach(var modType in assembly.GetTypes().Where(t => typeof(Modifier).IsAssignableFrom(t) && !t.IsAbstract))
				{
					//Check if there is a parameterless constructor
					if(modType.GetConstructor(new Type[0]) == null)
					{
						ConsoleOutput.WriteError($"Modifier '{modType}' does not contain a parameterless constructor.");
					}
					availableModifierTypes.Add(modType);
				}
			}
		}

		public static Modifier CreateModifier(Type modifierType)
		{
			if(!typeof(Modifier).IsAssignableFrom(modifierType)) throw new ArgumentException("Type is not a modifier type.");
			return (Modifier)Activator.CreateInstance(modifierType);
		}

		protected abstract void ModifyData(ElevationData data);

		public ElevationData Modify(ElevationData inputData, bool keepOriginal) {
			ElevationData data = keepOriginal ? new ElevationData(inputData) : inputData;
			ModifyData(data);
			return data;
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public string VerboseOutput()
		{
			StringBuilder sb = new StringBuilder();
			foreach(var t in GetType().GetFields())
			{
				var attr = t.GetCustomAttributes(typeof(DrawInInspectorAttribute), true);
				if (attr.Length > 0)
				{
					var a = (DrawInInspectorAttribute)attr[0];
					if (sb.Length > 0) sb.Append(", ");
					sb.Append(a.label+"="+t.GetValue(this));
				}
			}
			return sb.ToString();
		}
	}
}
