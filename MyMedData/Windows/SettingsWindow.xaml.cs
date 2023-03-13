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
using System.Windows.Forms;

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

		MainWindow mainWindow => (MainWindow)this.Owner;

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
			foreach (var settingKeyValue in _changedSettings)
			{
				UpdateSetting(settingKeyValue.Key, settingKeyValue.Value);
			}
		}

		void UpdateSetting(string settingName, string newValue)
		{
			try
			{
				var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				var settings = configFile.AppSettings.Settings;

				if (settings[settingName] == null)
				{
					settings.Add(settingName, newValue);
				}
				else
				{
					settings[settingName].Value = newValue;
				}

				configFile.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
			}
			catch (ConfigurationErrorsException)
			{
				System.Windows.MessageBox.Show("Ошибка сохранения настроек!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);				
			}
		}

		private void EditUserDBFileButton_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.LogOff();
			System.Windows.Forms.OpenFileDialog openFileDialog = new ();
			openFileDialog.Filter = "LiteDB database|*.db";
			openFileDialog.DefaultExt = ".db";
			openFileDialog.CheckFileExists = false;
			if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
			{
				string newPath = openFileDialog.FileName;

				if (UsersDataBase.CreateNewUsersDb(newPath))
					System.Windows.MessageBox.Show("Создание/перезапись базы данных произвдена.", "",
						MessageBoxButton.OK, MessageBoxImage.Information);
				else
					System.Windows.MessageBox.Show("Создание/перезапись базы дапнных отменена.", "Отмена операции.",
						MessageBoxButton.OK, MessageBoxImage.Information);

				UpdateSetting("UserDbName", newPath);
				UsersDbFileNameTextBox.Text = newPath;
			}
		}
	}
}
