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
	/// Interaction logic for IdAndCommentPlaque.xaml
	/// </summary>
	public partial class IdAndCommentPlaque : UserControl
	{
		public IdAndCommentPlaque()
		{
			InitializeComponent();
		}

		public static readonly RoutedEvent DeletionRequestedEvent = EventManager.RegisterRoutedEvent(
			"DeletionRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IdAndCommentPlaque));

		// Provide CLR accessors for the event
		public event RoutedEventHandler DeletionRequested
		{
			add { AddHandler(DeletionRequestedEvent, value); }
			remove { RemoveHandler(DeletionRequestedEvent, value); }
		}

		void RaiseDeletionRequestedRoutedEvent()
		{
			RoutedEventArgs eArgs = new RoutedEventArgs(DeletionRequestedEvent, this);
			RaiseEvent(eArgs);
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			RaiseDeletionRequestedRoutedEvent();
		}
	}

	[ValueConversion(typeof(bool), typeof(SolidColorBrush))]
	public class BoolToRedColorConverter : IValueConverter
	{
		static SolidColorBrush normal;
		static SolidColorBrush red;

		static BoolToRedColorConverter()
		{
			normal = (SolidColorBrush)Application.Current.FindResource("DarkThemeRedColor");
			red = (SolidColorBrush)Application.Current.FindResource("DarkThemeFontColor");
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? red : normal;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
