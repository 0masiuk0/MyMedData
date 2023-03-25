using System;
using System.Collections.Generic;
using System.Drawing;
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
		public User? ActiveUser => ActiveSession != null ? ActiveSession.ActiveUser : null;

		string _statusText = "";
		public string StatusText
		{
			get => _statusText;
			set
			{
				StatusTextBlock.Text = _statusText = value;				 
			}
		}

		public MainWindow()
		{
			InitializeComponent();
			UsernameTextBlock.MouseDown += new MouseButtonEventHandler(
				(o, MouseEventArgs) =>
				{
					if (MouseEventArgs.ClickCount == 2)				
						OpenUsersDialog();				
				});

			StateChanged += MainWindowStateChangeRaised;
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
			DataContext = RecordsDataBase.GenerateSampleExaminationRecordList(10);
		}

		private void LogOffButton_Click(object sender, RoutedEventArgs e)
		{	
			LogOff();
			DataContext = null;
		}

		public void LogOff()
		{
			if (ActiveSession != null)
			{
				ActiveSession.Dispose();
				ActiveSession = null;
				UsernameTextBlock.Text = "";
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

		#region BoringChromeOverrideStuff
		// Can execute
		private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		// Minimize
		private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(this);
		}

		// Maximize
		private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MaximizeWindow(this);
		}

		// Restore
		private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.RestoreWindow(this);
		}

		// Close
		private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		// State change
		private void MainWindowStateChangeRaised(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Maximized)
			{
				BorderThickness = new Thickness(8);
				RestoreButton.Visibility = Visibility.Visible;
				MaximizeButton.Visibility = Visibility.Collapsed;
			}
			else
			{
				BorderThickness = new Thickness(0);
				RestoreButton.Visibility = Visibility.Collapsed;
				MaximizeButton.Visibility = Visibility.Visible;
			}
		}
		#endregion
	}


}
