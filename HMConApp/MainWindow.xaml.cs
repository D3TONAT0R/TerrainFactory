using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Modification;
using HMCon.Util;
using HMConApp.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace HMConApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		ConsoleWindowHandler console;

		public static ObservableCollection<ModificationCommand> supportedModifiers = new ObservableCollection<ModificationCommand>();

		public Job job = new Job();

		Dictionary<Modifier, ModifierStackEntry> stackEntries = new Dictionary<Modifier, ModifierStackEntry>();

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

			InputList.ItemsSource = job.InputFileList;

			RemoveFileButton.IsEnabled = false;
			PreviewFileButton.IsEnabled = false;

			foreach (var mod in CommandHandler.ModificationCommands)
			{
				supportedModifiers.Add(mod);
				var cbi = new ComboBoxItem()
				{
					Content = mod.command
				};
				modificatorDropDown.Items.Add(cbi);
			}

			foreach (var ff in FileFormatManager.GetSupportedFormats())
			{
				var toggle = new CheckBox()
				{
					Content = ff.Identifier,
					Tag = ff,
				};
				toggle.Checked += OnExportFormatChecked;
				toggle.Unchecked += OnExportFormatUnchecked;
				exportFormatToggleList.Children.Add(toggle);
			}

			UpdateModificationStack();
		}

		internal void UpdateModificationStack()
		{
			ModificationStack.Children.Clear();
			for (int i = 0; i < job.exportSettings.modificationChain.Count; i++)
			{
				var m = job.exportSettings.modificationChain[i];
				stackEntries[m].StackIndex = i;
				ModificationStack.Children.Add(stackEntries[m]);
			}
		}

		internal void OnModifierAdded(Modifier mod)
		{
			var entry = new ModifierStackEntry(this, mod, job.exportSettings, job.exportSettings.modificationChain.Count - 1);
			stackEntries.Add(mod, entry);
			ModificationStack.Children.Add(entry);
		}

		internal void OnModifierRemoved(ModifierStackEntry entry)
		{
			job.exportSettings.modificationChain.Remove(entry.mod);
			stackEntries.Remove(entry.mod);
			ModificationStack.Children.Remove(entry);
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
				job.InputFileList.Add(path);
				InputList.SelectedIndex = job.InputFileList.Count - 1;
			}
		}

		private void OnRemoveSelectedFile(object sender, RoutedEventArgs e)
		{
			if (InputList.SelectedIndex >= 0)
			{
				job.InputFileList.RemoveAt(InputList.SelectedIndex);
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
				m.sourceCommand = supportedModifiers[i];
				job.exportSettings.AddModifierToChain(m, false);
				OnModifierAdded(m);
				//AddModifierComposite(m);
			}
			modificatorDropDown.SelectedIndex = 0;
		}

		private void OnExportFormatChecked(object sender, RoutedEventArgs args)
		{
			var ff = (FileFormat)((CheckBox)sender).Tag;
			job.exportSettings.outputFormats.Add(ff);
		}

		private void OnExportFormatUnchecked(object sender, RoutedEventArgs args)
		{
			var ff = (FileFormat)((CheckBox)sender).Tag;
			for (int i = 0; i < job.exportSettings.outputFormats.Count; i++)
			{
				if (job.exportSettings.outputFormats[i].GetType() == ff.GetType())
				{
					job.exportSettings.outputFormats.RemoveAt(i);
					return;
				}
			}
		}

		private void OnExportClick(object sender, RoutedEventArgs e)
		{

			if (Directory.Exists(Path.GetDirectoryName(outputPathBox.Text)))
			{
				job.outputPath = outputPathBox.Text;
				job.ExportAll();
			}
			else
			{
				MessageBox.Show($"Directory does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
