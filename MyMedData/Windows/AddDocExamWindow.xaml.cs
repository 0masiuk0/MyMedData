using MyMedData.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Microsoft.Win32;

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для AddDocExamWindow.xaml
	/// </summary>
	public partial class AddDocExamWindow : Window
	{
		public AddDocExamWindow(Session session)
		{
			InitializeComponent();
			this.session = session;
		}

		readonly Session session;

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
        }
    }
}
