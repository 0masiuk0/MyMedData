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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для EnterPasswordWindow.xaml
	/// </summary>
	public partial class EnterPasswordWindow : Window
	{
		User _user;

		public EnterPasswordWindow(User user)
		{
			InitializeComponent();
			_user = user;
		}

		public string? Password{ get; private set; }

		private void OKbutton_Click(object sender, RoutedEventArgs e)
		{
			if (_user.CheckPassword(PasswrodBox.Password))
			{
				Password = PasswrodBox.Password;
				Close();
			}
			else
			{
				Password = null;
				MessageBox.Show("Неверный пароль.", "Неудача.", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Password = null;
			Close();
		}
	}
}
