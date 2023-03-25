using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Xml.Serialization;
using LiteDB;
using Microsoft.Win32;
using MyMedData.Windows;

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для EditUserWindow.xaml
	/// </summary>
	public partial class EditUserWindow : Window
	{
		public EditUserWindow(User user, string password)
		{
			InitializeComponent();
			
			EditedUser = user;
			this.Password = password;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			usersDbFileName = (Owner as UsersWindow).UsersDbFileName;
		}

		public User EditedUser { get; private set; }
		public string Password { get; private set; }
		string usersDbFileName;

		private void EditNameButton_Click(object sender, RoutedEventArgs e)
		{
			var editUserNameWindow = new EditUserNameWindow(EditedUser);
			if (editUserNameWindow.ShowDialog() ?? false)
			{
				EditedUser.Name = editUserNameWindow.UserName;
				UsersDataBase.UpdateUser(EditedUser, usersDbFileName);
			}
		}

		private void EditColorButton_Click(object sender, RoutedEventArgs e)
		{
			var colorPickerWindow = new ColorPickerWindow(EditedUser.AccountColor);
			colorPickerWindow.ShowDialog();
			EditedUser.AccountColor = colorPickerWindow.SelectedColor;
			UsersDataBase.UpdateUser(EditedUser, usersDbFileName);
		}

		private void EditPasswordButton_Click(object sender, RoutedEventArgs e)
		{
			ChangePasswordWindow changePasswordWindow = new(EditedUser, Password);
			changePasswordWindow.ShowDialog();
			if (changePasswordWindow.DialogResult ?? false)
			{
				Password = changePasswordWindow.NewPassword ?? "";				
			}
		}

		private void EditDBFileNameButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "LiteDB Data Base|*.db";
			openFileDialog.Multiselect = false;
			if (openFileDialog.ShowDialog() ?? false)
			{
				EditedUser.DatabaseFile = openFileDialog.FileName;
				if (!RecordsDataBase.CreateUserDocumnetDb(EditedUser, Password))
				{
					MessageBox.Show("Что-то пошло не так при изменении базы данных этого пользователя.", "Ошибка",
								MessageBoxButton.OK, MessageBoxImage.Error);					
				}
			}
			
		}
	
	}
}
