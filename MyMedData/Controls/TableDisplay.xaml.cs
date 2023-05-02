﻿using System;
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

			_recordsCollectionsViewSource = (CollectionViewSource)Resources["RecordsCollectionViewSource"];
		}

		private bool _dataContextIsSet = false;
		private TextChangedEventHandler TitleFilterChagedHandler => (o, e) => _recordsCollectionsViewSource.View.Refresh();
		private EventHandler<System.Windows.Controls.SelectionChangedEventArgs> FromDateEventHandler => (o, e) => _recordsCollectionsViewSource.View.Refresh();
		private EventHandler<System.Windows.Controls.SelectionChangedEventArgs> ToDateEventHandler => (o, e) => _recordsCollectionsViewSource.View.Refresh();
		private TextChangedEventHandler CommentFilterChagedHandler => (o, e) => _recordsCollectionsViewSource.View.Refresh();

		private void RecordsTableDisplay_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!_dataContextIsSet && DataContext is Session session)
			{
				TitleFilterTextBox.TextChanged += TitleFilterChagedHandler;
				fromDateFilterDatePicker.SelectedDateChanged += FromDateEventHandler;
				toDateFilterDatePicker.SelectedDateChanged += ToDateEventHandler;
				CommentFilterTextBox.TextChanged += CommentFilterChagedHandler;
				_dataContextIsSet = true;
				//RecordDisplay.DataContext = session;
			}
			else
			{
				TitleFilterTextBox.TextChanged -= TitleFilterChagedHandler;
				fromDateFilterDatePicker.SelectedDateChanged -= FromDateEventHandler;
				toDateFilterDatePicker.SelectedDateChanged -= ToDateEventHandler;
				CommentFilterTextBox.TextChanged -= CommentFilterChagedHandler;
				_dataContextIsSet = false;
				//RecordDisplay.DataContext = null;
			}
		}

		private readonly CollectionViewSource _recordsCollectionsViewSource;

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
