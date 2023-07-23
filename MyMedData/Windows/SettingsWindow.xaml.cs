using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using MyMedData.Classes;
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

		private MainWindow MainWindow => (MainWindow)this.Owner;

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			DBPasswordTextBox.Text = RecordsDataBase.HashString16("");
		}		

		private void OKbutton_Click(object sender, RoutedEventArgs e)
		{
			AppConfigDatabase.Settings.SaveSettings();
			Close();
		}

		private void Applybutton_Click(object sender, RoutedEventArgs e)
		{
			AppConfigDatabase.Settings.SaveSettings();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			AppConfigDatabase.Settings.CancelSettingUpdate();
			Close();
		}

		private void AccountPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string accPassword = AccPasswordTextBox.Text ?? "";			
			DBPasswordTextBox.Text = RecordsDataBase.HashString16(accPassword);			
		}

		private void ScannerTabItem_Loaded(object sender, RoutedEventArgs e)
		{
			ScannerCombBox.Items.Clear();
			foreach(var scannerName in ScannerManager.GetDeviceNames()) 
			{
				ScannerCombBox.Items.Add(scannerName);
			}

			int index = ScannerCombBox.Items.IndexOf(AppConfigDatabase.Settings.DefaultScannerName);
			if (index > 0)
			{
				ScannerCombBox.SelectedIndex = index;
			}
			else if (ScannerCombBox.Items.Count > 0) 
			{ 
				ScannerCombBox.SelectedIndex = 0;
			}

			var dpi = AppConfigDatabase.Settings.DPI;
			var dpi_list = ScannerManager.AVALIBLE_DPI.ToList();
			dpi_list.ForEach(dpi => DPI_CombBox.Items.Add(dpi));
			
			if (dpi != null && dpi_list.Contains(dpi))
				DPI_CombBox.SelectedItem = dpi;
        }

		private void ScannerCombBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ScannerCombBox.SelectedItem.ToString() is string newScannerName)
				AppConfigDatabase.Settings.DefaultScannerName = newScannerName;
			else
				AppConfigDatabase.Settings.CancelSettingUpdate(nameof(AppConfigDatabase.Settings.DefaultScannerName));
        }

		private void DPI_CombBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DPI_CombBox.SelectedItem is string newDPI)
				AppConfigDatabase.Settings.DPI = newDPI;
			else
				AppConfigDatabase.Settings.CancelSettingUpdate(nameof(AppConfigDatabase.Settings.DPI));
		}
    }
}
