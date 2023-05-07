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

		public MainWindow()
		{
			InitializeComponent();
			UsernameTextBlock.MouseDown += new MouseButtonEventHandler(
				(o, mouseEventArgs) =>
				{
					if (mouseEventArgs.ClickCount == 2)				
						OpenUsersDialog();				
				});

			StateChanged += MainWindowStateChangeRaised;
		}		

		private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
		{
			NewDocExaminationButton.Visibility = Visibility.Collapsed;
			NewLabAnalysisButon.Visibility = Visibility.Collapsed;
			RecordsTableDisplay.RecordsDataGrid.SelectionChanged += RecordsDataGrid_SelectionChanged;
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
			DataContext = session;

			NewDocExaminationButton.Visibility = Visibility.Visible;
			NewLabAnalysisButon.Visibility = Visibility.Visible;
		}

		public void LogOff()
		{
			DataContext = null;
			if (ActiveSession != null)
			{
				ActiveSession.Dispose();
				ActiveSession = null;
				UsernameTextBlock.Text = "АВТОРИЗУЙТЕСЬ";
			}
			NewDocExaminationButton.Visibility = Visibility.Collapsed;
			NewLabAnalysisButon.Visibility = Visibility.Collapsed;
		}

		private void AuthorizationButton_Click(object sender, RoutedEventArgs e)
		{
			OpenUsersDialog();
		}

		private void LogOffButton_Click(object sender, RoutedEventArgs e)
		{
			LogOff();
		}

		private void NewAppointmentRecordData_Click(object sender, RoutedEventArgs e)
		{
			if (ActiveSession is Session session)
			{
				AddExaminationWindow AddWindow = new AddExaminationWindow(session, DocOrLabExamination.Doc);
				AddWindow.ShowDialog();
			}
		}

		private void SettingsdButton_Click(object sender, RoutedEventArgs e)
		{
			SettingsWindow settingsWindow = new();
			settingsWindow.Owner = this;
			settingsWindow.ShowDialog();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MainWindowStateChangeRaised(object? sender, EventArgs e)
		{
			switch(WindowState)
			{
				case WindowState.Normal:
					RestoreButton.Visibility = Visibility.Collapsed;
					MaximizeButton.Visibility = Visibility.Visible;
					MainDockPanel.Margin = new Thickness(2);
					break;

				case WindowState.Maximized:
					MaximizeButton.Visibility = Visibility.Collapsed;
					RestoreButton.Visibility = Visibility.Visible;
					MainDockPanel.Margin = new Thickness(8.0);
					break;
			}
		}

		private void RecordsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ActiveSession != null && RecordsTableDisplay.RecordsDataGrid.SelectedItem is ExaminationRecord)
				DeleteRecordButton.Visibility = Visibility.Visible;
			else
				DeleteRecordButton.Visibility = Visibility.Collapsed;
		}

		private void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
		{
			if (ActiveSession != null && RecordsTableDisplay.RecordsDataGrid.SelectedItem is ExaminationRecord record)
			{
				ActiveSession.DeleteRecord(record);
			}			
		}

		private void RestoreButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Normal;			
		}

		private void MaximizeButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Maximized;			
		}

		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}		
	}

}
