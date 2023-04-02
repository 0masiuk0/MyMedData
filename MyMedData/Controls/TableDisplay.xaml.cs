using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
	/// Логика взаимодействия для TableDisplay.xaml
	/// </summary>
	public partial class TableDisplay : UserControl
	{
		public TableDisplay()
		{
			InitializeComponent();

			recordsCollectionsViewSource = (CollectionViewSource)Resources["RecordsCollectionViewSource"];

			// Get a reference to the FrameworkElement whose DataContext you want to monitor
			FrameworkElement element = RecordsDataGrid;

			// Use the DependencyPropertyDescriptor to get a descriptor for the DataContext property
			DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(
				FrameworkElement.DataContextProperty, typeof(FrameworkElement));

			// Add a PropertyChangedCallback to the descriptor's AddValueChanged method
			dpd.AddValueChanged(element, OnDataContextChanged);
		}

		bool dataContextIsSet = false;
		TextChangedEventHandler TitleFilterChagedHandler => (o, e) => recordsCollectionsViewSource.View.Refresh();
		EventHandler<System.Windows.Controls.SelectionChangedEventArgs> fromDateEventHandler => (o, e) => recordsCollectionsViewSource.View.Refresh();
		EventHandler<System.Windows.Controls.SelectionChangedEventArgs> toDateEventHandler => (o, e) => recordsCollectionsViewSource.View.Refresh();
		TextChangedEventHandler CommentFilterChagedHandler => (o, e) => recordsCollectionsViewSource.View.Refresh();

		private void OnDataContextChanged(object? sender, EventArgs e)
		{
			if (!dataContextIsSet && DataContext is ICollection<ExaminationRecord> recordsColelction)
			{
				TitleFilterTextBox.TextChanged += TitleFilterChagedHandler;
				fromDateFilterDatePicker.SelectedDateChanged += fromDateEventHandler;
				toDateFilterDatePicker.SelectedDateChanged += toDateEventHandler;
				CommentFilterTextBox.TextChanged += CommentFilterChagedHandler;
				dataContextIsSet = true;
			}
			else
			{
				TitleFilterTextBox.TextChanged -= TitleFilterChagedHandler;
				fromDateFilterDatePicker.SelectedDateChanged -= fromDateEventHandler;
				toDateFilterDatePicker.SelectedDateChanged -= toDateEventHandler;
				CommentFilterTextBox.TextChanged -= CommentFilterChagedHandler;
				dataContextIsSet = false;
			}

		}

		private void RecordsTableDisplay_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			
		}

		CollectionViewSource recordsCollectionsViewSource;

		private void RecordsDataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			foreach(var column in RecordsDataGrid.Columns) 
			{
				column.CanUserResize = true;
				column.CanUserSort = true;
			}
		
		}

		private void RecordsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void RecordsCollectionViewSource_Filter(object sender, FilterEventArgs e)
		{	
			if (e.Item is ExaminationRecord record)
			{
				if (TitleFilterTextBox.Text is string titleFilter && titleFilter.Length > 0)
				{
					if (!record.Title.ToLower().Contains(titleFilter.ToLower()))
					{
						e.Accepted = false;
						return;
					}
				}

				if (fromDateFilterDatePicker.SelectedDate is DateTime fromDate)
				{
					if(!(record.Date < fromDate.ToDateOnly()))
					{
						e.Accepted = false;
						return;
					}
				}

				if (toDateFilterDatePicker.SelectedDate is DateTime toDate)
				{
					if (!(record.Date < toDate.ToDateOnly()))
					{
						e.Accepted = false;
						return;
					}
				}

				if(CommentFilterTextBox.Text is string commentFilter && commentFilter.Length > 0)
				{
					if(!record.Comment.ToLower().Contains(commentFilter.ToLower())) 
					{
						e.Accepted = false; 
						return; 
					}
				}

				e.Accepted = true;
			}
			else
			{
				throw new Exception("Source item in table datagrid is not ExaminationRecord somehow.");
			}
		}		
	}
}
