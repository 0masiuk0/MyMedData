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
		}

		public EditUserWindow(User user)
		{
			InitializeComponent();
			EditedUser = user;
		}

		public readonly User EditedUser;

		private void EditNameButton_Click(object sender, RoutedEventArgs e)
		{
			var editUserNameWindow = new EditUserNameWindow();
			if (editUserNameWindow.ShowDialog() ?? false)
			{
				EditedUser.Name = editUserNameWindow.Name;
			}
		}

		private void EditDBFileNameButton_Click(object sender, RoutedEventArgs e)
		{
			Li
		}
	}
}
