using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace MyMedData.Controls
{
	[ValueConversion(typeof(DateTime), typeof(DateOnly))]
	public class DateTimeToDateOnlyConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime dateTime)
			{
				return DateOnly.FromDateTime(dateTime);
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

	[ValueConversion(typeof(DateOnly), typeof(DateTime))]
	public class DateOnlyToDateTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateOnly dateOnly)
			{
				return new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);
			}

			return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime dateTime)
			{
				return DateOnly.FromDateTime(dateTime);
			}

			return DependencyProperty.UnsetValue;
		}
	}
}
