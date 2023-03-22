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

			textBlockDefaultBrush = (Brush)TryFindResource("DarkThemeFontColor");
			ValidateData();

			Binding userPlaqContextBinding = new();
			userPlaqContextBinding.Source = NewUser;
			userPlaqContextBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			BindingOperations.SetBinding(userPlaque, DataContextProperty, userPlaqContextBinding);
		}

		public User NewUser = new("", Colors.White, "", "", false);
	
		private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			NewUser.Name = UserNameTextBox.Text;
			ValidateData();
		}
	
		private void PickColorButton_Click(object sender, RoutedEventArgs e)
		{
			var colorPickerWindow = new ColorPickerWindow();
			colorPickerWindow.ShowDialog();
			NewUser.AccountColor = colorPickerWindow.SelectedColor;
		}

		private void EditDataFileButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new Microsoft.Win32.OpenFileDialog();
			openFileDialog.Multiselect = false;
			openFileDialog.CheckFileExists = false;
			openFileDialog.Filter = "LiteDB Data Base|*.db";
			if (openFileDialog.ShowDialog() ?? false)
			{
				NewUser.DatabaseFile = openFileDialog.FileName;
				DataFileTextBox.Text = openFileDialog.FileName;
			}
			else
			{
				NewUser.DatabaseFile = "";
				DataFileTextBox.Text = null;
			}
			ValidateData();
		}

		private void OKbutton_Click(object sender, RoutedEventArgs e)
		{
			if (!validData) return; // в теории невозможно
			var password = PasswordTextBox1.Password;

			NewUser.SetPassword(password);
			if (!RecordsDataBase.CreateUserDocumnetDb(NewUser, password, DbCreationOptions.Ask))
			{
				//неудача создания базы для пользователя		
				MessageBox.Show("Новый пользователь создан не был.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			DialogResult = true;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		bool validUser => User.IsValidUserName(NewUser.Name);
		bool validPassword => User.IsValidPassword(PasswordTextBox1.Password);
		bool passwordsMatch => PasswordTextBox1.Password == PasswordTextBox2.Password;
		bool validDataFile
		{
			get
			{
				try
				{
					System.IO.Path.GetFullPath(NewUser.DatabaseFile);
					return true;
				}
				catch { return false; }
			}
		}
		bool validData => validUser && validPassword && validDataFile && passwordsMatch;
					
		private void ValidateData(object sender, RoutedEventArgs eventArgs)
		{
			ValidateData();		
		}

		Brush textBlockDefaultBrush; 
		private void ValidateData()
		{

			UserNameTextBlock.Foreground = validUser ? textBlockDefaultBrush : Brushes.Red;
			PasswordTextBlock.Foreground = validPassword ? textBlockDefaultBrush : Brushes.Red;
			RepeatPasswordTextBlock.Foreground = passwordsMatch ? textBlockDefaultBrush : Brushes.Red;
			DataFileTextBlock.Foreground = validDataFile ? textBlockDefaultBrush : Brushes.Red;
			OKbutton.IsEnabled = validData;
		}

		private void OwnDatabaseCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Эту настройку нельзя изменить после создания пользователя.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
		}	
	}
}
