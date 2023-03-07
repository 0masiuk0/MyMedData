using Microsoft.Win32;
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
using Ookii.Dialogs;
using System.IO;

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для AddUserWindow.xaml
	/// </summary>
	public partial class AddUserWindow : Window
	{
		public AddUserWindow()
		{
			InitializeComponent();
			UserNameTextBox.TextChanged += ValidateData;
			PasswordTextBox1.PasswordChanged += ValidateData;
			PasswordTextBox2.PasswordChanged += ValidateData;
			selectedColor = userPlaque.Foreground;
			username = "";
		}

		string username;
		private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			username = userPlaque.Text = UserNameTextBox.Text;
		}

		Brush selectedColor;
		private void PickColorButton_Click(object sender, RoutedEventArgs e)
		{
			var colorPickerWindow = new ColorPickerWindow();
			colorPickerWindow.ShowDialog();
			userPlaque.Foreground = selectedColor = new SolidColorBrush(colorPickerWindow.SelectedColor);
		}

		string? dataFolder;
		private void EditDataFolderButton_Click(object sender, RoutedEventArgs e)
		{
			var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
			openFolderDialog.Multiselect = false;			
			if (openFolderDialog.ShowDialog() ?? false)
			{
				dataFolder = openFolderDialog.SelectedPath;
			}
			else
				dataFolder = null;
			ValidateData();
		}

		public User? NewUser;

		private void OKbutton_Click(object sender, RoutedEventArgs e)
		{
			NewUser = new User(username, selectedColor, PasswordTextBox1.Password, dataFolder);
			
			if (!DataBase.CreateUserDocumnetDb(NewUser, RecordsDbCreationOptions.Ask))
			{
				//неудача создания базы для пользователя
				NewUser = null;
			}
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			NewUser = null;
			Close();
		}

		private void ValidateData()
		{
			bool validData = User.IsValidUserName(username)
				&& User.IsValidPassword(PasswordTextBox1.Password)
				&& PasswordTextBox1.Password == PasswordTextBox2.Password
				&& dataFolder != null;
			
			OKbutton.IsEnabled = validData;
		}

		private void ValidateData(object sender, RoutedEventArgs eventArgs)
		{
			ValidateData();
		}


	}
}
