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
	/// Логика взаимодействия для EditUserNameWindow.xaml
	/// </summary>
	public partial class EditUserNameWindow : Window
	{
		public EditUserNameWindow(User user)
		{
			InitializeComponent();
			UsernameTextBox.Text = user.Name;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{			
			UsernameTextBox.Focus();
		}

		public string? UserName { get; private set; }

		private void ProcessAndClose()
		{
			var newName = UsernameTextBox.Text;

			if (User.IsValidUserName(newName))
			{
				UserName = newName;
				this.DialogResult = true;
				Close();
			}
			else
			{
				MessageBox.Show("Разрешены непустые имена из букв, цифр и _. ", "Недопустимое имя!", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OKbutton_click(object sender, RoutedEventArgs e)
		{
			ProcessAndClose();
		}

		private void CancelButton_click(object sender, RoutedEventArgs e)
		{
			UserName = null;
			this.DialogResult = false;
			Close();
		}

		private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter) { ProcessAndClose(); }
        }		
	}
}
