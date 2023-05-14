using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using MyMedData.Classes;
using static MyMedData.Classes.SettingsManager;

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

		private Dictionary<string, string> _changedSettings = new();		

		private MainWindow MainWindow => (MainWindow)this.Owner;

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string? userDbFileName = SettingsManager.AppSettings["UserDbName"]?.Value;
			if (userDbFileName != null)
			{
				if (File.Exists(userDbFileName) && UsersDataBase.FastCheckUserDvValidity(userDbFileName))
				{
					UsersDbFileNameTextBox.Text = userDbFileName;
				}
				else
				{
					UpsertSetting("UserDbName", "");
				}
			}

			DBPasswordTextBox.Text = RecordsDataBase.HashString16("");
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

		private void AddUpdateAppSettings()
		{
			foreach (var settingKeyValue in _changedSettings)
			{
				UpsertSetting(settingKeyValue.Key, settingKeyValue.Value);
			}
		}

		private void EditUserDBFileButton_Click(object sender, RoutedEventArgs e)
		{
			MainWindow.LogOff();
			Microsoft.Win32.OpenFileDialog openFileDialog = new ();
			openFileDialog.Filter = "LiteDB database|*.db";
			openFileDialog.DefaultExt = ".db";
			openFileDialog.CheckFileExists = false;
			if (openFileDialog.ShowDialog() ?? false)
			{
				string newPath = openFileDialog.FileName;

				if (!UsersDataBase.CreateNewUsersDb(newPath))
					System.Windows.MessageBox.Show("Создание/перезапись базы дапнных отменена.", "Отмена операции.",
						MessageBoxButton.OK, MessageBoxImage.Information);

				UpsertSetting("UserDbName", newPath);
				UsersDbFileNameTextBox.Text = newPath;
			}
		}

		private void AccountPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string accPassword = AccPasswordTextBox.Text ?? "";			
			DBPasswordTextBox.Text = RecordsDataBase.HashString16(accPassword);			
		}
	}
}
