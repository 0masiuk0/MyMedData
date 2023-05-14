using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
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
using MyMedData.Classes;
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
		Image AutoLogOn_On_Icon;
		Image AutoLogOn_Off_Icon;

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

			AutoLogOn_Off_Icon = (Image)TryFindResource("AutoLockOffIcon");
			AutoLogOn_On_Icon = (Image)TryFindResource("AutoLockOnIcon");
			var autologinParam = SettingsManager.GetOrInsertDefaultValue("auto_log_in", "False");
			autoLogin = autologinParam == "True" ? true : false;
		}		

		private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
		{
			NewDocExaminationButton.Visibility = Visibility.Collapsed;
			NewLabAnalysisButon.Visibility = Visibility.Collapsed;
			RecordsTableDisplay.RecordsDataGrid.SelectionChanged += RecordsDataGrid_SelectionChanged;

			if (autoLogin)
			{
				Authorizator.AuthorizeLastUser(this);
			}
		}		

		private void OpenUsersDialog()
		{
			Window usersWindow = new UsersWindow();
			usersWindow.Owner = this;
			usersWindow.ShowDialog();
		}

		public void LogIn(Session session)
		{
			try
			{
				ActiveSession = session;
				UsernameTextBlock.Text = ActiveUser.Name;
				DataContext = session;

				NewDocExaminationButton.Visibility = Visibility.Visible;
				NewLabAnalysisButon.Visibility = Visibility.Visible;
			}
			catch(Exception ex) 
			{
				MessageBox.Show("Не удалось прочитать базу данных.\n" + ex.Message, "Ошибка чтения базы", MessageBoxButton.OK, MessageBoxImage.Error);
				LogOff();
			}
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

		

		bool _autologin;
		private bool autoLogin
		{
			get => _autologin;
			set
			{
				_autologin = value;
				if (value) 
					AutlogInButton.Content = AutoLogOn_On_Icon;
				else
					AutlogInButton.Content = AutoLogOn_Off_Icon;
			}
		}

		private void NewAppointmentRecordData_Click(object sender, RoutedEventArgs e)
		{
			if (ActiveSession is Session session)
			{
				AddExaminationWindow AddWindow = new AddExaminationWindow(session, DocOrLabExamination.Doc);
				AddWindow.ShowDialog();
			}
		}

		private void NewLabAnalysisButon_Click(object sender, RoutedEventArgs e)
		{
			if (ActiveSession is Session session)
			{
				AddExaminationWindow AddWindow = new AddExaminationWindow(session, DocOrLabExamination.Lab);
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

		private void AutlogInButton_Click(object sender, RoutedEventArgs e)
		{
			autoLogin = !autoLogin;
			SettingsManager.UpsertSetting("auto_log_in", autoLogin.ToString());
		}
	}

}
