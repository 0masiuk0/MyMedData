﻿using System;
using System.CodeDom;
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
		}

		private void RecordsDataGrid_Loaded(object sender, RoutedEventArgs e)
		{
			foreach(var column in RecordsDataGrid.Columns) 
			{
				column.CanUserResize = true;
				column.CanUserSort = true;
			}
		
		}

		
	}
}
