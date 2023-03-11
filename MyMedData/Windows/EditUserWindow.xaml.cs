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
		public EditUserWindow()
		{
			InitializeComponent();
			EditedUser = new();
			_editedCopyOfUser = new();
		}

		public EditUserWindow(User user)
		{
			InitializeComponent();
			EditedUser = user;
			_editedCopyOfUser = User.Copy(user);
		}

		public User EditedUser { get; private set; }
		User _editedCopyOfUser;

		private void EditNameButton_Click(object sender, RoutedEventArgs e)
		{
			var editUserNameWindow = new EditUserNameWindow();
			if (editUserNameWindow.ShowDialog() ?? false)
			{
				EditedUserPlaque.Text = _editedCopyOfUser.Name = editUserNameWindow.Name;
			}
		}

		private void EditColorButton_Click(object sender, RoutedEventArgs e)
		{
			var colorPickerWindow = new ColorPickerWindow(_editedCopyOfUser.AccountColor);
			colorPickerWindow.ShowDialog();
			_editedCopyOfUser.AccountColor = colorPickerWindow.SelectedColor;
			EditedUserPlaque.Foreground = _editedCopyOfUser.AccountColoredBrush;
		}

		private void EditPasswordButton_Click(object sender, RoutedEventArgs e)
		{
			ChangePasswordWindow changePasswordWindow = new(_editedCopyOfUser);
			changePasswordWindow.ShowDialog();
		}

		private void EditDBFileNameButton_Click(object sender, RoutedEventArgs e)
		{
			var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
			openFolderDialog.Multiselect = false;
			if (openFolderDialog.ShowDialog() ?? false)
			{
				_editedCopyOfUser.RecordsFolder = openFolderDialog.SelectedPath;				
			}
			
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			if (EditedUser.RecordsFolder != _editedCopyOfUser.RecordsFolder)
			{
				if (!DataBase.CreateUserDocumnetDb(_editedCopyOfUser))				
				{
					MessageBox.Show("Что-то пошло не так при изменении базы данных этого пользователя.", "Ошибка",
								MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			if (User.UpdateUser(_editedCopyOfUser))
			{
				EditedUser = _editedCopyOfUser;
			}
			else
			{
				MessageBox.Show("Что-то пошло не так при обновлении базы данных пользователей.", "Ошибка",
				MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}	

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
