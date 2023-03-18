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
		User user;
		string? newPassword;
		readonly string oldPassword;
		public string? NewPassword => newPassword;

		public ChangePasswordWindow(User user, string oldPassword)
		{
			InitializeComponent();
			DialogResult = false;
			this.user = user;
			this.oldPassword = oldPassword;
		}

		private void OKButtonClick(object sender, RoutedEventArgs e)
		{
			if (PasswordBox1.Password != PasswordBox2.Password)
			{
				MessageBox.Show("Пароли не совпадают.", "Неверный ввод.", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (!User.IsValidPassword(PasswordBox1.Password))
			{
				MessageBox.Show("Пароль должен содержать только латинские буквы цифры и \"_\".", "Неверный ввод.", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			DialogResult = user.ChangePasswordAndRebuildDb(oldPassword, PasswordBox1.Password);
			Close();			
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
