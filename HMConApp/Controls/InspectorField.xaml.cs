using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HMConApp.Controls
{
	/// <summary>
	/// Interaction logic for InspectorField.xaml
	/// </summary>
	public partial class InspectorField : UserControl
	{

		public string Label
		{
			get;
			set;
		}

		public object BindingTarget
		{
			get
			{
				return field.GetValue(target);
			}
			set
			{
				try
				{
					var conv = Convert.ChangeType(value, field.FieldType);
					field.SetValue(target, conv);
				}
				catch
				{
					//MessageBox.Show("Invalid input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		public FieldInfo field;
		private object target;

		public InspectorField(string label, object targetObject, FieldInfo targetField)
		{
			Label = label;
			target = targetObject;
			field = targetField;
			InitializeComponent();
		}

		public void UpdateBinding()
		{
			valueBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
		}
	}
}
