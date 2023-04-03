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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyMedData.Controls
{
	/// <summary>
	/// Логика взаимодействия для RecordDisplay.xaml
	/// </summary>
	public partial class RecordDisplay : UserControl
	{
		public RecordDisplay()
		{
			InitializeComponent();
		}

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{

		}
	}

	public class DateTimeToDateOnlyConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime dateTime)
			{
				return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
			}

			return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateOnly dateOnly)
			{
				return new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);
			}

			return DependencyProperty.UnsetValue;
		}
	}

}
