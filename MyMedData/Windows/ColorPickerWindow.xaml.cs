using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
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
			_initalColor = TheСolorPicker.SelectedColor = Colors.White;
		}

		public ColorPickerWindow(Color color)
		{
			InitializeComponent();
			_initalColor = TheСolorPicker.SelectedColor = color;
		}

		private readonly Color _initalColor;
		public Color SelectedColor { get; private set; }		

		private void OKbutton_Click(object sender, RoutedEventArgs e)
		{
			SelectedColor = TheСolorPicker.SelectedColor;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			SelectedColor = _initalColor;
			Close();
		}

		[ValueConversion(typeof(Color), typeof(SolidColorBrush))]
		public class ColorToBrushConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return new SolidColorBrush((Color)value);
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return ((SolidColorBrush)value).Color;
			}
		}
	}	
}
