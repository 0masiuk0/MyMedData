using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;
using System.Collections.Specialized;
using System.Threading.Channels;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.IO;
using LiteDB;

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();			
		}

		Dictionary<string, string> _changedSettings = new();
		NameValueCollection appSettings => ConfigurationManager.AppSettings;

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string? userDbFileName = appSettings["UserDbName"];
			if (userDbFileName != null)
			{
				UsersDbFileNameTextBox.Text = userDbFileName;
			}
		}		

		private void OKbutton_Click(object sender, RoutedEventArgs e)
		{
			AddUpdateAppSettings();
			Close();
		}

		private void Applybutton_Click(object sender, RoutedEventArgs e)
		{
			AddUpdateAppSettings();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			_changedSettings.Clear();
			Close();
		}

		void AddUpdateAppSettings()
		{
			try
			{
				var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				var settings = configFile.AppSettings.Settings;
				foreach (var settingKeyValue in _changedSettings)
				{
					string key = settingKeyValue.Key;
					string value = settingKeyValue.Value;

					if (settings[key] == null)
					{
						settings.Add(key, value);
					}
					else
					{
						settings[key].Value = value;
					}
				}
				configFile.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
			}
			catch (ConfigurationErrorsException)
			{
				MessageBox.Show("Ошибка сохранения настроек!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				Close();
			}
		}

		private void EditUserDBFileButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "LiteDB database|*.db";
			openFileDialog.DefaultExt = ".db";
			openFileDialog.CheckFileExists = false;
			if (openFileDialog.ShowDialog() ?? false)
			{
				string newPath = openFileDialog.FileName;

				if (!File.Exists(newPath))
				{

				}
				else
				{
					sdsadasd
				}


				if (newPath != appSettings["UserDbName"])
				{
					_changedSettings["UserDbName"] = newPath;
				}
				else if (_changedSettings.ContainsKey("UserDbName"))
				{
					_changedSettings.Remove("UserDbName");
				}

				

				UsersDbFileNameTextBox.Text = newPath;
			}
		}
	}
}
