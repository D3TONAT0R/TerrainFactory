using HMCon;
using HMCon.Export;
using HMCon.Modification;
using HMCon.Util;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace HMConApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		ConsoleWindowHandler console;

		ObservableCollection<FileFormat> formats = new ObservableCollection<FileFormat>();

		ObservableCollection<ModificationCommand> supportedModifiers = new ObservableCollection<ModificationCommand>();

		Job job = new Job();

		public MainWindow()
		{
			InitializeComponent();

			console = new ConsoleWindowHandler(consoleWindow);
			ConsoleOutput.consoleHandler = console;

			string pluginDirectory = AppContext.BaseDirectory;
			/*for(int i = 0; i < 4; i++)
			{
				pluginDirectory = Directory.GetParent(pluginDirectory).FullName;
			}
			pluginDirectory = Path.Combine(pluginDirectory, @"HMConConsole\bin\Debug\netcoreapp3.1");*/
			HMConManager.Initialize(pluginDirectory);

			InputList.ItemsSource = formats;

			formats.Add(new FileFormat("ASC", "asc", "asc", "ESRI ASCII Grid", (HMConExportHandler)null));
			formats.Add(new FileFormat("3DM_3DS", "3ds", "3ds", "3ds Model", (HMConExportHandler)null));
			formats.Add(new FileFormat("PNG", "png", "png", "PNG Image", (HMConExportHandler)null));

			RemoveFileButton.IsEnabled = false;
			PreviewFileButton.IsEnabled = false;

			List<ComboBoxItem> dropDownItems = new List<ComboBoxItem>();
			dropDownItems.Add(new ComboBoxItem()
			{
				Content = "- Select Modifier -",
				IsEnabled = false
			});
			foreach (var mod in CommandHandler.ModificationCommands)
			{
				supportedModifiers.Add(mod);
				var cbi = new ComboBoxItem()
				{
					Content = mod.command
				};
				dropDownItems.Add(cbi);
			}

			modificatorDropDown.Items.Clear();
			modificatorDropDown.ItemsSource = dropDownItems;
		}

		private void OnAddFile(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog()
			{
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				Multiselect = false,
				Title = "Add file to import"
			};
			if (dialog.ShowDialog() == true)
			{
				string path = dialog.FileName;
				formats.Add(new FileFormat(Path.GetExtension(path), "x", Path.GetExtension(path), path, (HMConExportHandler)null));
				InputList.SelectedIndex = formats.Count - 1;
			}
		}

		private void OnRemoveSelectedFile(object sender, RoutedEventArgs e)
		{
			if (InputList.SelectedIndex >= 0)
			{
				formats.RemoveAt(InputList.SelectedIndex);
			}
		}

		private void OnInputSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool b = InputList.SelectedIndex >= 0;
			RemoveFileButton.IsEnabled = b;
			PreviewFileButton.IsEnabled = b;
		}

		private void OnModificatorDropdownClose(object sender, EventArgs e)
		{
			int i = modificatorDropDown.SelectedIndex;
			if (i > 0)
			{
				i--;
				ConsoleOutput.WriteLine("You selected " + modificatorDropDown.Items[i]);
				var m = supportedModifiers[i].CreateModifier();
				job.exportSettings.AddModifierToChain(m, false);
				AddModifierComposite(m);
			}
			modificatorDropDown.SelectedIndex = 0;
		}

		void AddModifierComposite(Modifier mod)
		{
			var type = mod.GetType();
			var group = new GroupBox()
			{
				Header = mod.GetType().Name
			};
			StackPanel stack = new StackPanel();
			group.Content = stack;
			foreach (var fi in GetExposedFields(type))
			{
				UIElement elem;
				//if(fi.FieldType == typeof(int))
				//{
				elem = new TextBox()
				{
					Text = fi.GetValue(mod).ToString()
				};
				//}
				stack.Children.Add(elem);
			}
			ModificationStack.Children.Add(group);
		}

		FieldInfo[] GetExposedFields(Type t)
		{
			List<FieldInfo> exposed = new List<FieldInfo>();
			var fields = t.GetFields();
			foreach (var f in fields)
			{
				var attr = f.GetCustomAttribute<DrawInInspectorAttribute>();
				if (attr != null)
				{
					exposed.Add(f);
				}
			}
			return exposed.ToArray();
		}
	}
}
