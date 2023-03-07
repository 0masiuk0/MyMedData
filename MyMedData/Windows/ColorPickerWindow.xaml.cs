using Microsoft.Windows.Themes;
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
	/// Логика взаимодействия для ColorPickerWindow.xaml
	/// </summary>
	public partial class ColorPickerWindow : Window
	{			
		public ColorPickerWindow()
		{
			InitializeComponent();
			initalColor = TheСolorPicker.SelectedColor = Colors.White;
		}

		public ColorPickerWindow(Color color)
		{
			InitializeComponent();
			initalColor = TheСolorPicker.SelectedColor = color;
		}

		readonly Color initalColor;
		public Color SelectedColor { get; private set; }

		private void TheСolorPicker_ColorChanged(object sender, RoutedEventArgs e)
		{
			SelectedColor = TheСolorPicker.SelectedColor;
		}

		private void OKbutton_Click(object sender, RoutedEventArgs e)
		{
			SelectedColor = TheСolorPicker.SelectedColor;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedColor = initalColor;
			Close();
		}		
	}
}
