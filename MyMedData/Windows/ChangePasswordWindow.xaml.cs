using MyMedData.Classes;
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

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для ChangePasswordWindow.xaml
	/// </summary>
	public partial class ChangePasswordWindow : Window
	{
		private User _user;
		private string? _newPassword;
		private readonly string _oldPassword;
		public string? NewPassword => _newPassword;

		public ChangePasswordWindow(User user, string oldPassword)
		{
			InitializeComponent();			
			this._user = user;
			this._oldPassword = oldPassword;
		}

		private void AsquireNewPassword()
		{
			if (PasswordBox1.Password != PasswordBox2.Password)
			{
				MessageBox.Show("Пароли не совпадают.", "Неверный ввод.", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (!Authorizator.IsValidPassword(PasswordBox1.Password))
			{
				MessageBox.Show("Пароль должен содержать только латинские буквы цифры и \"_\" или быть пустым", "Неверный ввод.", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			bool success = _user.ChangePasswordAndRebuildDb(_oldPassword, PasswordBox1.Password);
			_newPassword = success ? PasswordBox1.Password : null;
			MessageBox.Show("Пароль изменен.", "Успех операции", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			DialogResult = success;
		}

		private void Cancel()
		{
			DialogResult = false;
			Close();
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			Cancel();
		}

		private void OkButtonClick(object sender, RoutedEventArgs e)
		{
			AsquireNewPassword();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter) 
				AsquireNewPassword();
			else if (e.Key == Key.Escape) 
				Cancel(); 
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			PasswordBox1.Focus();
		}
	}
}
