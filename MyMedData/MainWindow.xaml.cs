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

namespace MyMedData
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public Session? ActiveSession { get; private set; }
		User? ActiveUser => ActiveSession != null ? ActiveSession.ActiveUser : null;

		public MainWindow()
		{
			InitializeComponent();
			UsernameTextBlock.MouseDown += new MouseButtonEventHandler(
				(o, MouseEventArgs) =>
				{
					if (MouseEventArgs.ClickCount == 2)				
						OpenUsersDialog();				
				});

		
		}

		private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
		{

		}		

		private void OpenUsersDialog()
		{
			Window usersWindow = new UsersWindow();
			usersWindow.Owner = this;
			usersWindow.ShowDialog();
		}

		public void LogIn(Session session)
		{
			ActiveSession = session;
			UsernameTextBlock.Text = ActiveUser.Name;
		}

		private void LogOffButton_Click(object sender, RoutedEventArgs e)
		{	
			LogOff();
		}

		public void LogOff()
		{
			if (ActiveSession != null)
			{
				ActiveSession.Dispose();
				ActiveSession = null;
				UsernameTextBlock.Text = string.Empty;
			}
		}

		private void AuthorizationButton_Click(object sender, RoutedEventArgs e)
		{
			OpenUsersDialog();
		}



		private void SettingsdButton_Click(object sender, RoutedEventArgs e)
		{
			SettingsWindow settingsWindow = new();
			settingsWindow.Owner = this;
			settingsWindow.ShowDialog();
		}
	}
}
