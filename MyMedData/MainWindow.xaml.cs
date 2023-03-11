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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyMedData.Windows;
using MyMedData.Windows;

namespace MyMedData
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
		{

		}

		private void AuthorizationButton_Click(object sender, RoutedEventArgs e)
		{
			Window usersWindow = new UsersWindow();
			usersWindow.Owner = this;
			usersWindow.ShowDialog();
			if (User.ActiveUser != null)
			{
				LoadUserData();
			}
			else
			{
				LogOff();
			}
		}

		private void LogOffButton_Click(object sender, RoutedEventArgs e)
		{	
			LogOff();
		}

		public void LogOff()
		{
			User.LogOff();
			UsernameTextBlock.Text = string.Empty;
		}

		private void LoadUserData()
		{
			throw new NotImplementedException();
		}		

		private void SettingsdButton_Click(object sender, RoutedEventArgs e)
		{
			SettingsWindow settingsWindow = new();
			settingsWindow.Owner = this;
			settingsWindow.ShowDialog();
		}
	}
}
