using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using MyMedData.Classes;
using static MyMedData.Classes.SettingsManager;
using System.Linq;

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

		private void TabItem_Loaded(object sender, RoutedEventArgs e)
		{
			ScannerCombBox.Items.Clear();
			foreach(var scannerName in ScannerManager.GetDeviceNames()) 
			{
				ScannerCombBox.Items.Add(scannerName);
			}

			int index = ScannerCombBox.Items.IndexOf(ScannerManager.GetSetScannerName());
			if (index > 0)
			{
				ScannerCombBox.SelectedIndex = index;
			}
			else if (ScannerCombBox.Items.Count > 0) 
			{ 
				ScannerCombBox.SelectedIndex = 0;
			}

			var dpi = AppSettings[ScannerManager.DPI_SETTING_KEY]?.Value;
			var dpi_list = new string[] { "100", "150", "200", "300" }.ToList();
			dpi_list.ForEach(dpi => DPI_CombBox.Items.Add(dpi));
			
			if (dpi != null && dpi_list.Contains(dpi))
				DPI_CombBox.SelectedItem = dpi;
        }

		private void ScannerCombBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ScannerCombBox.SelectedItem.ToString() is string newScannerName && newScannerName != AppSettings[ScannerManager.DEFAULT_SCANNER_NAME_SETTING_KEY]?.Value)
				_changedSettings[ScannerManager.DEFAULT_SCANNER_NAME_SETTING_KEY] = newScannerName;
			else
				_changedSettings.Remove(ScannerManager.DEFAULT_SCANNER_NAME_SETTING_KEY);
        }

		private void DPI_CombBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DPI_CombBox.SelectedItem is string newDPI
				&& newDPI != AppSettings[ScannerManager.DPI_SETTING_KEY]?.Value)
				_changedSettings[ScannerManager.DPI_SETTING_KEY] = newDPI;
			else
				_changedSettings.Remove(ScannerManager.DPI_SETTING_KEY);
		}
    }
}
