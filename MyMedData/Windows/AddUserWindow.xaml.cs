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
			if (userPlaque.Foreground is SolidColorBrush)
			{
				selectedColor = (SolidColorBrush)userPlaque.Foreground;
			}
			else
			{
				throw new Exception("Этого не должно было произойти. Цвет плашки пользователя не выражен одним цветом. " +
					"(UserPlaque.Foreground is not SolidColorBrush)");
			}
				
			username = "";
			textBlockDefaultBrush = (Brush)TryFindResource("DarkThemeFontColor");
			ValidateData();
		}

		string username;
		private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			username = userPlaque.Text = UserNameTextBox.Text;
			ValidateData();
		}

		SolidColorBrush selectedColor;
		private void PickColorButton_Click(object sender, RoutedEventArgs e)
		{
			var colorPickerWindow = new ColorPickerWindow();
			colorPickerWindow.ShowDialog();
			userPlaque.Foreground = selectedColor = new SolidColorBrush(colorPickerWindow.SelectedColor);
		}

		string? dataFile;
		private void EditDataFileButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new Microsoft.Win32.OpenFileDialog();
			openFileDialog.Multiselect = false;
			openFileDialog.CheckFileExists = false;
			openFileDialog.Filter = "LiteDB Data Base|*.db";
			if (openFileDialog.ShowDialog() ?? false)
			{
				dataFile = openFileDialog.FileName;
				DataFileTextBox.Text = dataFile;
			}
			else
			{
				dataFile = null;
				DataFileTextBox.Text = null;
			}
			ValidateData();
		}

		public User? NewUser;

		private void OKbutton_Click(object sender, RoutedEventArgs e)
		{

			NewUser = new User(username, selectedColor, PasswordTextBox1.Password, dataFile, OwnDatabaseCheckBox.IsChecked ?? false);
			
			if (!RecordsDataBase.CreateUserDocumnetDb(NewUser, PasswordTextBox1.Password, DbCreationOptions.Ask))
			{
				//неудача создания базы для пользователя
				NewUser = null;
				MessageBox.Show("Новый пользователь создан не был.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
			}			
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			NewUser = null;
			Close();
		}

		bool validUser => User.IsValidUserName(username);
		bool validPassword => User.IsValidPassword(PasswordTextBox1.Password);
		bool passwordsMatch => PasswordTextBox1.Password == PasswordTextBox2.Password;
		bool validDataFile => dataFile != null;
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
