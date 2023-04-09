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
		private User _user;

		public EnterPasswordWindow(User user)
		{
			InitializeComponent();
			_user = user;
			Loaded += (o, e) => PasswrodBox.Focus();
		}

		public string? Password{ get; private set; }

		private void TryAuthorize(object sender, RoutedEventArgs e) => TryAuthorize();

		private void TryAuthorize()
		{
			if (PasswrodBox.Password is string pswrd)
			{
				if (_user.CheckPassword(pswrd))
				{
					Password = pswrd;
					Close();
				}
				else
				{
					Password = null;
					MessageBox.Show("Неверный пароль.", "Неудача.", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Password = null;
			Close();
		}

		private void PasswrodBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter) 
				TryAuthorize();
		}
	}
}
