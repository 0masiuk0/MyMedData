using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

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

	public sealed class TrulyObservableCollection<T> : ObservableCollection<T>
	where T : INotifyPropertyChanged
	{
		public TrulyObservableCollection()
		{
			CollectionChanged += FullObservableCollectionCollectionChanged;
		}

		public TrulyObservableCollection(IEnumerable<T> pItems) : this()
		{
			foreach (var item in pItems)
			{
				this.Add(item);
			}
		}

		private void FullObservableCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (Object item in e.NewItems)
				{
					((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
				}
			}
			if (e.OldItems != null)
			{
				foreach (Object item in e.OldItems)
				{
					((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
				}
			}
		}

		private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T)sender));
			OnCollectionChanged(args);
		}
	}
}
