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

		private void OkButtonClick(object sender, RoutedEventArgs e)
		{
			if (PasswordBox1.Password != PasswordBox2.Password)
			{
				MessageBox.Show("Пароли не совпадают.", "Неверный ввод.", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (!Authorizator.IsValidPassword(PasswordBox1.Password))
			{
				MessageBox.Show("Пароль должен содержать только латинские буквы цифры и \"_\".", "Неверный ввод.", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			bool success= _user.ChangePasswordAndRebuildDb(_oldPassword, PasswordBox1.Password);
			_newPassword = success ? PasswordBox1.Password : null;
			MessageBox.Show("Пароль изменен.", "Успех операции", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			DialogResult = success;				
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
