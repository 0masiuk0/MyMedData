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
		User _user;

		public ChangePasswordWindow(User user)
		{
			InitializeComponent();
			DialogResult = false;
			_user = user;
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

			_user.SetPassword(PasswordBox1.Password);
			DialogResult = true;
			Close();
			
		}

		private void CancelButtonClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
