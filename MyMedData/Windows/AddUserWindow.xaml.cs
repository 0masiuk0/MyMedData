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
using MyMedData.Classes;

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

			_textBlockDefaultBrush = (Brush)TryFindResource("DarkThemeFontColor");
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
			_validDataFile = null;

			var openFileDialog = new Microsoft.Win32.OpenFileDialog();
			openFileDialog.Multiselect = false;
			openFileDialog.CheckFileExists = false;
			openFileDialog.Filter = "LiteDB Data Base|*.db";
			if (openFileDialog.ShowDialog() ?? false)
			{
				if (File.Exists(openFileDialog.FileName)) 
				{
					if (MessageBox.Show("Выбран существующий файл. Его использование возможно только при совпадении пароля этой учетной записи и пароля," +
						"использванного при создании базы данных.",
						"Предупреждение о шифровании.",
						MessageBoxButton.OKCancel, MessageBoxImage.Exclamation)
						!= MessageBoxResult.OK)
						return;
				}

				NewUser.DatabaseFile = openFileDialog.FileName;
				DataFileTextBox.Text = openFileDialog.FileName;
			}
			
			ValidateData();
		}

		private void OKbutton_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidData) return; // в теории невозможно
			var password = PasswordTextBox1.Password;

			NewUser.SetPassword(password);
			try
			{
				if (!RecordsDataBase.CreateUserDocumnetDb(NewUser, password, DbCreationOptions.Ask))
				{
					//неудача создания базы для пользователя		
					MessageBox.Show("Новый пользователь создан не был.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
					DialogResult = false;
					Close();
				}
				DialogResult = true;
				Close();
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "Новый пользователь создан не был.", MessageBoxButton.OK, MessageBoxImage.Warning);
				DialogResult = true;
				Close();
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private bool ValidUser => Authorizator.IsValidUserName(NewUser.Name);
		private bool ValidPassword => Authorizator.IsValidPassword(PasswordTextBox1.Password);
		private bool PasswordsMatch => PasswordTextBox1.Password == PasswordTextBox2.Password;
		
		//IO call and exception processing is too slow to do each TextboxChange.
		//Hence lazy implementation. 
		//_validDataFile = null; is invalidates value and is called each time DataFile is edited;
		private bool? _validDataFile;

		private bool   ValidDataFile
		{
			get
			{
				if (_validDataFile == null)
				{
					try
					{					
						System.IO.Path.GetFullPath(NewUser.DatabaseFile);
						_validDataFile = true;
					}
					catch 
					{ _validDataFile = false; }

					return (bool)_validDataFile;
				}
				else 
					return (bool)_validDataFile;
			}
		}

		private bool ValidData => ValidUser && ValidPassword && ValidDataFile && PasswordsMatch;
					
		private void ValidateData(object sender, RoutedEventArgs eventArgs)
		{
			ValidateData();		
		}

		private Brush _textBlockDefaultBrush; 
		private void ValidateData()
		{

			UserNameTextBlock.Foreground = ValidUser ? _textBlockDefaultBrush : Brushes.Red;
			PasswordTextBlock.Foreground = ValidPassword ? _textBlockDefaultBrush : Brushes.Red;
			RepeatPasswordTextBlock.Foreground = PasswordsMatch ? _textBlockDefaultBrush : Brushes.Red;
			DataFileTextBlock.Foreground = ValidDataFile ? _textBlockDefaultBrush : Brushes.Red;
			OKbutton.IsEnabled = ValidData;
		}		

		private void AddUserWindowInstance_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Close();
		}
	}
}
